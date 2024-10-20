using RidgeDrawer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace RidgeDrawer
{
	public class PixelManual : BackendBase
	{
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
			foreach (var coord in coords)
			{
				var x = coord.X;
				var y = coord.Y;
				for (int i = 0; i < param.Stroke; i++)
				{
					for (int j = 0; j < param.Stroke; j++)
					{
						if (param.Stroke > 2 && ((j == 0 && i == 0) || (j == param.Stroke-1 && i == param.Stroke-1) || (j == param.Stroke-1 && i == 0) || (j == 0 && i == param.Stroke-1))) // do not draw rectangle corner pixels
						{
							// TODO: рисовать круг вместо точки. в случае антиалиасинга хз пока
							continue;
						}
						if (x+i > 0 && y+j > 0 && x+i < newBitmap.Width && y+j < newBitmap.Height) // do not draw on image bounds
							newBitmap.SetPixel(x+i, y+j, Color.Black);
					}
				}
				
			}
		}

		private void DrawNonAALine(int x0, int y0, int x1, int y1)
		{
			int dx = Math.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
			int dy = -Math.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
			int err = dx + dy, e2; /* error value e_xy */

			for (; ; )
			{  /* loop */
				SetPixel(x0, y0, Color.Black);
				if (x0 == x1 && y0 == y1) break;
				e2 = 2 * err;
				if (e2 >= dy) { err += dy; x0 += sx; } /* e_xy+e_x > 0 */
				if (e2 <= dx) { err += dx; y0 += sy; } /* e_xy+e_y < 0 */
			}
		}

		private void DrawAALine(int x0, int y0, int x1, int y1)
		{
			int dx = Math.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
			int dy = Math.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
			int err = dx - dy, e2, x2;                       /* error value e_xy */
			int ed = dx + dy == 0 ? 1 : (int)Math.Sqrt(dx * dx + dy * dy);

			for (; ; )
			{                                         /* pixel loop */
				SetPixel(x0, y0, Color.Black, 255 * Math.Abs(err - dx + dy) / ed);
				e2 = err; x2 = x0;
				if (2 * e2 >= -dx)
				{                                    /* x step */
					if (x0 == x1) break;
					if (e2 + dy < ed) SetPixel(x0, y0 + sy, Color.Black, 255 * (e2 + dy) / ed);
					err -= dy; x0 += sx;
				}
				if (2 * e2 <= dy)
				{                                     /* y step */
					if (y0 == y1) break;
					if (dx - e2 < ed) SetPixel(x2 + sx, y0, Color.Black, 255 * (dx - e2) / ed);
					err += dx; y0 += sy;
				}
			}
		}

		private void SetPixel(int x, int y, Color color, int alpha = 0)
		{
			if (x > 0 && y > 0 && x < newBitmap.Width && y < newBitmap.Height)
				newBitmap.SetPixel(x, y, Color.FromArgb(255 - alpha, color));
		}

		public override void DrawLines(MyPoint[] coords)
		{
			for (int i = 0; i < coords.Length - 1; i++)
				if (param.Smoothing == SmoothingType.Antialias)
					DrawAALine(coords[i].X, coords[i].Y, coords[i + 1].X, coords[i + 1].Y);
				else
					DrawNonAALine(coords[i].X, coords[i].Y, coords[i + 1].X, coords[i + 1].Y);
		}

		public override void DrawVariableLines(MyPoint[] coords, int y)
		{
			throw new NotImplementedException();
		}

		public override void DrawDebugInfo()
		{
			throw new NotImplementedException();
		}
	}
}
