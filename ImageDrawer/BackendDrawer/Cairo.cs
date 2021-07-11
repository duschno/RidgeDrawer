using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace ImageDrawer
{
	public class Cairo : IBackendDrawer
	{
		[DllImport(@"C:\Users\User\Desktop\ImageDrawer\Debug\PlusPlus.dll", CallingConvention = CallingConvention.Cdecl)]
		private extern static void Draw(IntPtr hdc, int width, int height, int linesCount, int strokeWidth, int factor, int chunkSize);

		public void Draw(Graphics g, int width, int height, int linesCount, int strokeWidth, int factor, int chunkSize)
		{
			IntPtr hdc = g.GetHdc();
			Draw(hdc, width, height, linesCount, strokeWidth, factor, chunkSize);
			g.ReleaseHdc();
		}

		public void Draw(Graphics g, Bitmap bmp, RenderParams param)
		{
			throw new NotImplementedException();
		}
	}
}
