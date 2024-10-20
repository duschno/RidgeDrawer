using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Text.RegularExpressions;
using RidgeDrawer;

namespace RidgeDrawer
{
	[Guid("1A91BC9C-09A1-4817-9C71-4D91A674AAF1")]
	public class Logic
	{
		/// <summary>
		/// Gets backend drawer implementation
		/// </summary>
		/// <param name="type">Backend to use</param>
		/// <returns>Drawer instance</returns>
		private static BackendBase GetBackendDrawer(Type type)
		{
			try
			{
				return (BackendBase)Activator.CreateInstance(type);
			}
			catch (Exception e)
			{
				throw new NotImplementedException($"{type.Name} backend is not supported", e);
			}
		}

		/// <summary>
		/// Gets all possible implementations
		/// </summary>
		/// <param name="interfaceType"></param>
		/// <returns></returns>
		public static IEnumerable<Type> GetImplementations(Type interfaceType)
		{
			return AppDomain.CurrentDomain.GetAssemblies()
				.SelectMany(a => a.GetTypes())
				.Where(t => interfaceType.IsAssignableFrom(t) && !t.IsAbstract);
		}

		/// <summary>
		/// Renders whole image
		/// </summary>
		/// <param name="origBitmap">Original bitmap</param>
		/// <param name="param">Render params</param>
		/// <returns>Rendered bitmap</returns>
		private static Bitmap RenderImage(Bitmap origBitmap, RenderParams param)
		{
			Bitmap newBitmap = new Bitmap(origBitmap.Width, origBitmap.Height);

			using (var graphics = Graphics.FromImage(newBitmap))
			{
				using (SolidBrush brush = new SolidBrush(Color.White))
					graphics.FillRectangle(brush, 0, 0, newBitmap.Width, newBitmap.Height);

				BackendBase drawer = GetBackendDrawer(param.Backend);
				drawer.Construct(newBitmap, origBitmap, param);
				drawer.Draw();
			}

			return newBitmap;
		}

		public static Bitmap ProcessByFilename(string inputFilename, RenderParams param)
		{
			Bitmap bmp = new Bitmap(inputFilename);
			return RenderImage(bmp, param);
		}

		/// <summary>
		/// Saves BitmapSource by given filename
		/// </summary>
		/// <param name="bmp">Bitmap to save</param>
		/// <param name="outputFilename">Name of the file to use</param>
		public static void SaveAsPng(BitmapSource bmp, string outputFilename)
		{
			using (var fileStream = new FileStream(outputFilename, FileMode.Create))
			{
				BitmapEncoder encoder = new PngBitmapEncoder();
				encoder.Frames.Add(BitmapFrame.Create(bmp));
				encoder.Save(fileStream);
			}
		}

		#region Helpers

		[DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool DeleteObject([In] IntPtr hObject);

		/// <summary>
		/// Converts Bitmap to BitmapSource
		/// </summary>
		/// <param name="bmp">Bitmap image</param>
		/// <returns>BitmapSource image</returns>
		public static BitmapSource BitmapToBitmapSource(Bitmap bmp)
		{
			var handle = bmp.GetHbitmap();
			try
			{
				return Imaging.CreateBitmapSourceFromHBitmap(handle, 
					IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
			}
			finally { DeleteObject(handle); }
		}

		#endregion

		#region Console app related stuff

		public static void ProcessByArgs(string[] args)
		{
			Main(args);
		}

		/// <summary>
		/// Entry point of the console app
		/// </summary>
		/// <param name="args">Command line arguments</param>
		private static void Main(string[] args)
		{
			// default values
			LogicParams logicParam = new LogicParams
			{
				InputFilename = null,
				OutputFilename = null,
				RenderParams = new RenderParams
				{
					LinesCount = 1,
					Stroke = 1,
					Factor = 0,
					ChunkSize = 1,
					GreyPoint = 127,
					BlackPoint = 0,
					WhitePoint = 255,
					Smoothing = SmoothingType.None,
					LineType = LineType.Line,
					Method = MethodType.Ridge,
					Effect = typeof(RidgeLines),
					DrawOnSides = false,
					PointsAroundPeak = -1,
					FillInside = false,
					Invert = false,
					Debug = false,
					Backend = typeof(GDIPlus),
					PullPointX = 960,
					PullPointY = 540
				}
			};

#if DEBUG
			//// debug values
			//logicParam = new LogicParams
			//{
			//	InputFilename = @"..\Rachel-Carson.jpg",
			//	OutputFilename = @"..\Rachel-Carson_proc.jpg",
			//	RenderParams = new RenderParams
			//	{
			//		LinesCount = 80,
			//		Stroke = 1,
			//		Factor = 5,
			//		ChunkSize = 5,
			//		BlackPoint = 0,
			//		WhitePoint = 255,
			//		Smoothing = SmoothingType.Antialias,
			//		LineType = LineType.Curve,
			//      Method = MethodType.Squiggle,
			//		Effect = EffectType.Squiggle,
			//		DrawOnSides = true,
			//		FillInside = true,
			//		Invert = false,
			//		Debug = false,
			//		Backend = typeof(GDIPlus)
			//	}
			//};
#endif
			if (args.Length == 0)
			{
				ArgsHelper.PrintUsage();
				return;
			}

			try
			{
				logicParam = ArgsHelper.ParamsFromArgs(logicParam, true, args);
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				return;
			}
			Bitmap bmp = ProcessByFilename(logicParam.InputFilename, logicParam.RenderParams);
			SaveAsPng(BitmapToBitmapSource(bmp), logicParam.OutputFilename);
		}

#endregion
	}
}
