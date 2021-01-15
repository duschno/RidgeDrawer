using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageDrawer
{
	class Program
	{
		static void MethodRidge(Graphics graphics, Bitmap bmp, RenderParams param)
		{
			int lineNumber = 0;
			while (lineNumber < param.LinesCount)
			{
				List<Point> coords = new List<Point>();
				int y = bmp.Height * lineNumber / param.LinesCount + bmp.Height / (param.LinesCount * 2);
				coords.Add(new Point(0, y));
				for (int x = 1; x < bmp.Width; x += param.ChunkSize)
				{
					Color pixel = bmp.GetPixel(x, y);
					int grayscale = (pixel.R + pixel.G + pixel.B) / 3;
					int factor = param.Factor * (grayscale - 127) / 127;
					coords.Add(new Point(x, y + factor));
				}

				RenderLine(graphics, coords, param);
				lineNumber++;
			}
		}

		static void MethodSquiggle(Graphics graphics, Bitmap bmp, RenderParams param)
		{
			int lineNumber = 0;
			while (lineNumber < param.LinesCount)
			{
				List<Point> coords = new List<Point>();
				int sign = -1;
				int y = bmp.Height * lineNumber / param.LinesCount + bmp.Height / (param.LinesCount * 2);
				coords.Add(new Point(0, y));
				int accumulator = 0;
				int xStepsCount = 0;
				//int xPossibleSteps = (bmp.Width - 1) / param.ChunkSize;
				for (int x = 1; x < bmp.Width; x += param.ChunkSize)
				{
					xStepsCount++;
					Color pixel = bmp.GetPixel(x, y);
					int grayscale = 255 - (pixel.R + pixel.G + pixel.B) / 3;
					accumulator += grayscale;
					if (accumulator > 127)
					{
						int factor = param.Factor;
						coords.Add(new Point(
							x, y + sign * factor / xStepsCount));
						sign *= -1;
						accumulator = 0;
						xStepsCount = 0;
					}
				}

				RenderLine(graphics, coords, param);
				lineNumber++;
			}
		}

		static Bitmap RenderImage(Bitmap bmp, RenderParams param)
		{
			Bitmap empty = new Bitmap(bmp.Width, bmp.Height);

			using (var graphics = Graphics.FromImage(empty))
			{
				graphics.SmoothingMode = param.Smoothing;
				switch (param.Method)
				{
					case RenderMethod.Ridge:
						MethodRidge(graphics, bmp, param);
						break;
					case RenderMethod.Squiggle:
						MethodSquiggle(graphics, bmp, param);
						break;
					default:
						break;
				}
			}

			return empty;
		}

		private static void RenderLine(Graphics graphics, List<Point> coords, RenderParams param)
		{
			Pen pen = new Pen(Color.Black, param.Width);

			switch (param.LineType)
			{
				case RenderType.Line:
					graphics.DrawLines(pen, coords.ToArray());
					break;
				case RenderType.Curve:
					graphics.DrawCurve(pen, coords.ToArray());
					break;
				case RenderType.Dot:
					foreach (Point coord in coords)
						graphics.DrawRectangle(pen, coord.X, coord.Y, 1f, 1f);
					break;
				default:
					break;
			}
		}

		//private static void Grayscale(Bitmap bmp)
		//{
		//	for (int i = 0; i < bmp.Width; i++)
		//	{
		//		for (int j = 0; j < bmp.Height; j++)
		//		{
		//			Color color = bmp.GetPixel(i, j);
		//			int grey = (color.R + color.G + color.B) / 3;
		//			Color t = Color.FromArgb(grey, grey, grey);
		//			bmp.SetPixel(i, j, t);
		//		}
		//	}
		//}

		private static void Draw(string inputFileName, string outputFileName, RenderParams param)
		{
			Bitmap bmp = new Bitmap(inputFileName);

			bmp = RenderImage(bmp, param);

			EncoderParameters parameters = new EncoderParameters(1);
			parameters.Param[0] = new EncoderParameter(
				System.Drawing.Imaging.Encoder.Quality, 100L);
			bmp.Save(outputFileName,
				ImageCodecInfo.GetImageEncoders().FirstOrDefault(codec => codec.FormatID == ImageFormat.Png.Guid),
				parameters);
		}

		static void Main(string[] args)
		{
			RenderParams param = new RenderParams
			{
				LinesCount = 120,
				Width = 1,
				Factor = 5,
				ChunkSize = 5,
				Smoothing = SmoothingMode.AntiAlias,
				LineType = RenderType.Curve,
				Method = RenderMethod.Squiggle
			};

			string imageName = GetImageName("Rachel-Carson.jpg", args);
			Draw(imageName, AddPostfix(imageName), param);
		}

		private static string GetImageName(string defaultName, string[] args)
		{
			string imageName = defaultName;
			if (args.Length > 0)
				imageName = args[0];

			return imageName;
		}

		private static string AddPostfix(string imageName)
		{
			string filename = Path.GetFileNameWithoutExtension(imageName);
			string outputExt = ImageFormat.Png.ToString().ToLower();
			return filename + "_processed." + outputExt; 
		}
	}
}
