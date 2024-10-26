using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.InteropServices;

namespace RidgeDrawer
{
	public class Cairo : BackendBase
	{
		public override string OutputTypeDescription => "PNG";
		public override string OutputTypeExtension => "png";

		[DllImport(@"PlusPlus.dll", CallingConvention = CallingConvention.Cdecl)]
		private extern static void Draw(IntPtr hdc, int[] x, int[] y, int size);

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
			throw new NotImplementedException();
		}

		public override void DrawLines(MyPoint[] coords)
		{
			using (Graphics graphics = Graphics.FromImage(newBitmap))
			{
				IntPtr hdc = graphics.GetHdc();
				Draw(hdc, coords.Select(e => e.X).ToArray(), coords.Select(e => e.Y).ToArray(), coords.Count());
				graphics.ReleaseHdc();
			}
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
			throw new NotImplementedException();
		}

		public override void FillRect(int x1, int y1, int x2, int y2)
		{
			throw new NotImplementedException();
		}
	}
}
