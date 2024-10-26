using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace RidgeDrawer
{
	public class BitmapHelper
	{
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
	}
}
