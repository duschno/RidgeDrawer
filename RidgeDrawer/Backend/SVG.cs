using Svg;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Xml;

namespace RidgeDrawer
{
	public class SVG : BackendBase
	{
		private SvgDocument svg;

		public override string OutputTypeDescription => "SVG";
		public override string OutputTypeExtension => "svg";

		public override void Construct(Bitmap newBitmap, Bitmap origBitmap, RenderParams param)
		{
			base.Construct(newBitmap, origBitmap, param);
			svg = new SvgDocument
			{
				ViewBox = new SvgViewBox(0, 0, origBitmap.Width, origBitmap.Height)
			};
			svg.Namespaces.Clear();
			svg.Namespaces.Add(string.Empty, SvgNamespaces.SvgNamespace);
		}

		public override void Complete()
		{
			svg.Draw(newBitmap);
		}

		public override void DrawBezier(MyPoint[] coords)
		{
			throw new NotImplementedException();
		}

		public override void DrawCurve(MyPoint[] coords)
		{
			throw new NotImplementedException();
		}

		public override void DrawDots(MyPoint[] coords)
		{
			foreach (MyPoint coord in coords)
			{
				SvgCircle circle = new SvgCircle
				{
					CenterX = coord.X,
					CenterY = coord.Y,
					Radius = (float)param.Stroke / 2
				};
				svg.Children.Add(circle);
			}
		}

		private MyPoint[] GetFillCoordinates(MyPoint[] coords) // TODO: realise filling param
		{
			List<MyPoint> fillCoords = new List<MyPoint>();
			fillCoords.Add(new MyPoint(coords[0].X, newBitmap.Height));
			fillCoords.AddRange(coords);
			fillCoords.Add(new MyPoint(coords[coords.Length - 1].X, newBitmap.Height));
			return fillCoords.ToArray();
		}

		private bool DoesMidPointLaysOnLine(MyPoint startPoint, MyPoint midPoint, MyPoint endPoint)
		{
			return Distance(startPoint, midPoint) + Distance(midPoint, endPoint) == Distance(startPoint, endPoint);
		}

		private float Distance(MyPoint a, MyPoint b)
		{
			int x = Math.Abs(a.X - b.X);
			int y = Math.Abs(a.Y - b.Y);
			return (float)Math.Sqrt((x * x) + (y * y));
		}

		public override void DrawLines(MyPoint[] coords)
		{
			if (svg.Children.Count == 0)
			{
				SvgGroup group = new SvgGroup()
				{
					StrokeWidth = param.Stroke,
					Fill = SvgPaintServer.None,
					Stroke = new SvgColourServer(Color.Black)
				};
				svg.Children.Add(group);
			}
			SvgPolyline polyline = new SvgPolyline
			{
				Points = new SvgPointCollection()
			};
			svg.Children[0].Children.Add(polyline);

			MyPoint startCoord = coords[0];
			polyline.Points.AddRange(new[] { new SvgUnit(startCoord.X), new SvgUnit(startCoord.Y) });
			for (int i = 1; i < coords.Length - 1; i++)
			{
				if (!DoesMidPointLaysOnLine(coords[i - 1], coords[i], coords[i + 1]))
				{
					polyline.Points.AddRange(new[] { new SvgUnit(coords[i].X), new SvgUnit(coords[i].Y) });

				}
			}
			MyPoint endCoord = coords[coords.Length - 1];
			polyline.Points.AddRange(new[] { new SvgUnit(endCoord.X), new SvgUnit(endCoord.Y) });
		}

		public override void DrawVariableLines(MyPoint[] coords, int y)
		{
			throw new NotImplementedException();
		}

		public override void DrawDebugInfo()
		{
			throw new NotImplementedException();
		}

		public override void Save(string outputFilename)
		{
			using (var stream = File.Create(outputFilename))
			{
				var settings = new XmlWriterSettings
				{
					Indent = true,
				};

				using (var xmlWriter = XmlWriter.Create(stream, settings))
				{
					xmlWriter.WriteStartDocument();
					svg.Write(xmlWriter);
					xmlWriter.Flush();
				}
			}
		}

		public override void FillRect(int x1, int y1, int x2, int y2)
		{
			int minX = Math.Min(x1, x2);
			int minY = Math.Min(y1, y2);
			int maxX = Math.Max(x1, x2);
			int maxY = Math.Max(y1, y2);
			
			if (svg.Children.Count == 0)
			{
				SvgGroup group = new SvgGroup()
				{
					Fill = new SvgColourServer(Color.Black)
				};
				svg.Children.Add(group);
			}
			SvgRectangle rect = new SvgRectangle
			{
				X = minX,
				Y = minY,
				Width = maxX - minX,
				Height = maxY - minY
			};
			svg.Children[0].Children.Add(rect);
		}
	}
}
