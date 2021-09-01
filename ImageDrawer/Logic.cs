using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace ImageDrawer
{
	public class Logic
	{
		/// <summary>
		/// Gets backend drawer implementation
		/// </summary>
		/// <param name="type">Backend to use</param>
		/// <returns>Drawer instance</returns>
		private static BackendDrawerBase GetBackendDrawer(Type type)
		{
			try
			{
				return (BackendDrawerBase)Activator.CreateInstance(type);
			}
			catch (Exception e)
			{
				throw new NotImplementedException($"{type.Name} backend is not supported", e);
			}
		}

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

				BackendDrawerBase drawer = GetBackendDrawer(param.Backend);
				drawer.Construct(newBitmap, origBitmap, param);
				drawer.Draw();
			}

			return newBitmap;
		}

		public static Bitmap ProcessByFilename(string inputFileName, RenderParams param)
		{
			Bitmap bmp = new Bitmap(inputFileName);
			return RenderImage(bmp, param);
		}

		/// <summary>
		/// Saves BitmapSource by given filename
		/// </summary>
		/// <param name="bmp">Bitmap to save</param>
		/// <param name="outputFileName">Name of the file to use</param>
		public static void Save(BitmapSource bmp, string outputFileName)
		{
			using (var fileStream = new FileStream(outputFileName, FileMode.Create))
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

		/// <summary>
		/// Entry point of the console app
		/// </summary>
		/// <param name="args">Command line arguments</param>
		private static void Main(string[] args)
		{
			RenderParams param = new RenderParams
			{
				LinesCount = 120,
				Stroke = 1,
				Factor = 5,
				ChunkSize = 5,
				GreyLevel = 127,
				Smoothing = SmoothingType.Antialias,
				LineType = LineType.Curve,
				Method = MethodType.Squiggle,
				DrawOnSides = true,
				Backend = typeof(GDIPlus)
			};

			string imageName = GetImageName("Rachel-Carson.jpg", args);
			Bitmap bmp = ProcessByFilename(imageName, param);
			bmp.Save(AddPostfix(imageName), ImageFormat.Bmp);
		}

		/// <summary>
		/// Gest name of the image to process
		/// </summary>
		/// <param name="defaultName">Name of the default image to process</param>
		/// <param name="args">Command line arguments</param>
		/// <returns>Name of the image to process</returns>
		private static string GetImageName(string defaultName, string[] args = null)
		{
			string imageName = defaultName;
			if (args != null && args.Length > 0)
				imageName = args[0];

			return imageName;
		}

		/// <summary>
		/// Adds postfix to the name of processed image
		/// </summary>
		/// <param name="imageName">Original image name</param>
		/// <returns>Name with postfix</returns>
		private static string AddPostfix(string imageName)
		{
			string filename = Path.GetFileNameWithoutExtension(imageName);
			string outputExt = ImageFormat.Bmp.ToString().ToLower();
			return filename + "_processed." + outputExt; 
		}

		#endregion
	}
}
