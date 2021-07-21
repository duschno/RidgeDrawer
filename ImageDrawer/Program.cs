using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace ImageDrawer
{
	public class Program
	{
		/// <summary>
		/// Gets backend drawer implementation
		/// </summary>
		/// <param name="type">Backend to use</param>
		/// <returns>Drawer instance</returns>
		private static IBackendDrawer GetBackendDrawer(BackendType type)
		{
			try
			{
				Type t = typeof(IBackendDrawer);
				t = Type.GetType($"{t.Namespace}.{type}");
				return (IBackendDrawer)Activator.CreateInstance(t);
			}
			catch (Exception e)
			{
				throw new Exception($"{type} backend is not supported", e);
			}
		}

		/// <summary>
		/// Renders whole image
		/// </summary>
		/// <param name="width">Image width</param>
		/// <param name="height">Image height</param>
		/// <param name="param">Render params</param>
		/// <returns>Rendered bitmap</returns>
		private static Bitmap RenderImage(Bitmap loadedBitmap, RenderParams param)
		{
			Bitmap bitmap = new Bitmap(loadedBitmap.Width, loadedBitmap.Height);

			using (var graphics = Graphics.FromImage(bitmap))
			{
				using (SolidBrush brush = new SolidBrush(System.Drawing.Color.White))
					graphics.FillRectangle(brush, 0, 0, bitmap.Width, bitmap.Height);

				IBackendDrawer drawer = GetBackendDrawer(param.Backend);
				drawer.Draw(graphics, loadedBitmap, param);
			}

			return bitmap;
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
				Smoothing = SmoothingType.Antialias,
				LineType = LineType.Curve,
				Method = MethodType.Squiggle,
				Backend = BackendType.GDIPlus
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
