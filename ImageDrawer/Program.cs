using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace ImageDrawer
{
	public class Program
	{
		#region Methods
		/// <summary>
		/// Rigde type of render
		/// </summary>
		/// <param name="graphics"></param>
		/// <param name="bmp"></param>
		/// <param name="param"></param>
		//static void MethodRidge(Graphics graphics, Bitmap bmp, RenderParams param)
		//{
		//	int lineNumber = 0;
		//	while (lineNumber < param.LinesCount)
		//	{
		//		List<System.Drawing.Point> coords = new List<System.Drawing.Point>();
		//		int y = bmp.Height * lineNumber / param.LinesCount + bmp.Height / (param.LinesCount * 2);
		//		coords.Add(new System.Drawing.Point(0, y));
		//		for (int x = 1; x < bmp.Width; x += param.ChunkSize)
		//		{
		//			System.Drawing.Color pixel = bmp.GetPixel(x, y);
		//			int grayscale = (pixel.R + pixel.G + pixel.B) / 3;
		//			int factor = param.Factor * (grayscale - 127) / 127;
		//			coords.Add(new System.Drawing.Point(x, y + factor));
		//		}

		//		RenderLine(graphics, coords, param);
		//		lineNumber++;
		//	}
		//}

		///// <summary>
		///// Squiggle type of render
		///// </summary>
		///// <param name="graphics"></param>
		///// <param name="bmp"></param>
		///// <param name="param"></param>
		//static void MethodSquiggle(Graphics graphics, Bitmap bmp, RenderParams param)
		//{
		//	int lineNumber = 0;
		//	while (lineNumber < param.LinesCount)
		//	{
		//		List<System.Drawing.Point> coords = new List<System.Drawing.Point>();
		//		int sign = -1;
		//		int y = bmp.Height * lineNumber / param.LinesCount + bmp.Height / (param.LinesCount * 2);
		//		coords.Add(new System.Drawing.Point(0, y));
		//		int accumulator = param.ChunkSize;
		//		for (int x = 1; x < bmp.Width; x += accumulator)
		//		{
		//			System.Drawing.Color pixel = bmp.GetPixel(x, y);
		//			int grayscale = 255 - (pixel.R + pixel.G + pixel.B) / 3;
		//			accumulator = (param.ChunkSize * (255 - grayscale) + 10) / 10;
		//			int factor = param.Factor;
					
		//			grayscale = grayscale == 0 ? 1 : grayscale;
		//			coords.Add(new System.Drawing.Point(
		//				x + accumulator, y + (sign * factor * grayscale / 80)));
		//			sign *= -1;
		//		}

		//		RenderLine(graphics, coords, param);
		//		lineNumber++;
		//	}
		//}

		#endregion

		static IBackendDrawer GetBackendDrawer(BackendType type)
		{
			switch (type)
			{
				case BackendType.GDIPlus:
					return new GDIPlus();
				case BackendType.Cairo:
					return new Cairo();
				default:
					throw new Exception("Given backend is not supported");
			}
		}

		static Bitmap RenderImage(Bitmap bmp, RenderParams param)
		{
			Bitmap bitmap = new Bitmap(bmp.Width, bmp.Height);

			using (var graphics = Graphics.FromImage(bitmap))
			{
				using (SolidBrush brush = new SolidBrush(System.Drawing.Color.White))
					graphics.FillRectangle(brush, 0, 0, bmp.Width, bmp.Height);

				IBackendDrawer drawer = GetBackendDrawer(param.Backend);
				drawer.Draw(graphics, bitmap, param);
			}

			return bitmap;
		}

		//private static void RenderLine(Graphics graphics, List<System.Drawing.Point> coords, RenderParams param)
		//{
		//	if (coords.Count == 1)
		//		return;
		//	System.Drawing.Brush brush = new SolidBrush(System.Drawing.Color.Black);
		//	System.Drawing.Pen pen = new System.Drawing.Pen(brush, param.Width);

		//	switch (param.LineType)
		//	{
		//		case LineType.Line:
		//			graphics.DrawLines(pen, coords.ToArray());
		//			break;
		//		case LineType.Curve:
		//			graphics.DrawCurve(pen, coords.ToArray());
		//			break;
		//		case LineType.Dot:
		//			foreach (System.Drawing.Point coord in coords)
		//				graphics.FillRectangle(brush, coord.X, coord.Y, param.Width, param.Width);
		//			break;
		//		default:
		//			break;
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

		public static Bitmap DrawUI(string inputFileName, RenderParams param)
		{
			Bitmap bmp = new Bitmap(inputFileName);
			return RenderImage(bmp, param);
		}

		[DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool DeleteObject([In] IntPtr hObject);

		public static BitmapSource ImageSourceFromBitmap(Bitmap bmp)
		{
			var handle = bmp.GetHbitmap();
			try
			{
				return Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
			}
			finally { DeleteObject(handle); }
		}

		#region Console app stuff

		static void Main(string[] args)
		{
			RenderParams param = new RenderParams
			{
				LinesCount = 120,
				Width = 1,
				Factor = 5,
				ChunkSize = 5,
				Smoothing = SmoothingType.Antialias,
				LineType = LineType.Curve,
				Method = MethodType.Squiggle,
				Backend = BackendType.GDIPlus
			};

			string imageName = GetImageName("Rachel-Carson.jpg", args);
			Draw(imageName, AddPostfix(imageName), param);
		}

		public static string GetImageName(string defaultName, string[] args = null)
		{
			string imageName = defaultName;
			if (args != null && args.Length > 0)
				imageName = args[0];

			return imageName;
		}

		public static string AddPostfix(string imageName)
		{
			string filename = Path.GetFileNameWithoutExtension(imageName);
			string outputExt = ImageFormat.Png.ToString().ToLower();
			return filename + "_processed." + outputExt; 
		}

		#endregion
	}
}
