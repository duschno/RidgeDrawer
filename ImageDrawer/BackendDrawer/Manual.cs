using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace ImageDrawer
{
	public class Manual : BackendDrawerBase
	{
		protected override void DrawCurve(Point[] coords)
		{
			throw new NotImplementedException();
		}

		protected override void DrawDots(Point[] coords)
		{
			foreach (var coord in coords)
			{
				var x = coord.X;
				var y = coord.Y;
				for (int i = 0; i < param.Stroke; i++)
				{
					for (int j = 0; j < param.Stroke; j++)
					{
						if (param.Stroke > 2 && ((j == 0 && i == 0) || (j == param.Stroke-1 && i == param.Stroke-1) || (j == param.Stroke-1 && i == 0) || (j == 0 && i == param.Stroke-1)))
						{
							//TODO: рисовать круг вместо точки. в слачае антиалиасинга хз пока
							continue;
						}
						if (x+i > 0 && y+j > 0 && x+i < newBitmap.Width && y+j < newBitmap.Height)
							newBitmap.SetPixel(x+i, y+j, Color.Black);
					}
				}
				
			}
		}

		protected override void DrawLines(Point[] coords)
		{
			Color[] colors = new Color[] { Color.Black, Color.Black, Color.Black };

			for (int i = 0; i < coords.Length - 1; i++)
			{
				int x0 = coords[i].X;
				int y0 = coords[i].Y;
				int x1 = coords[i + 1].X;
				int y1 = coords[i + 1].Y;

				int dx = Math.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
				int dy = -Math.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
				int err = dx + dy, e2; /* error value e_xy */

				for (; ; )
				{  /* loop */
					if (x0 > 0 && y0 > 0 && x0 < newBitmap.Width && y0 < newBitmap.Height)
						newBitmap.SetPixel(x0, y0, colors[i % 3]);
					if (x0 == x1 && y0 == y1) break;
					e2 = 2 * err;
					if (e2 >= dy) { err += dy; x0 += sx; } /* e_xy+e_x > 0 */
					if (e2 <= dx) { err += dx; y0 += sy; } /* e_xy+e_y < 0 */
				}
			}

			//for (int i = 0; i < coords.Length - 1; i++)
			//{
			//	int x = coords[i].X;
			//	int y = coords[i].Y;
			//	int x2 = coords[i + 1].X;
			//	int y2 = coords[i + 1].Y;

			//	int w = x2 - x;
			//	int h = y2 - y;
			//	int dx1 = 0, dy1 = 0, dx2 = 0, dy2 = 0;
			//	if (w < 0) dx1 = -1; else if (w > 0) dx1 = 1;
			//	if (h < 0) dy1 = -1; else if (h > 0) dy1 = 1;
			//	if (w < 0) dx2 = -1; else if (w > 0) dx2 = 1;
			//	int longest = Math.Abs(w);
			//	int shortest = Math.Abs(h);
			//	if (!(longest > shortest))
			//	{
			//		longest = Math.Abs(h);
			//		shortest = Math.Abs(w);
			//		if (h < 0) dy2 = -1; else if (h > 0) dy2 = 1;
			//		dx2 = 0;
			//	}
			//	int numerator = longest >> 1;
			//	for (int j = 0; j <= longest; j++)
			//	{
			//		if (x > 0 && y > 0 && x < newBitmap.Width && y < newBitmap.Height)
			//			newBitmap.SetPixel(x, y, colors[i % 3]);
			//		numerator += shortest;
			//		if (!(numerator < longest))
			//		{
			//			numerator -= longest;
			//			x += dx1;
			//			y += dy1;
			//		}
			//		else
			//		{
			//			x += dx2;
			//			y += dy2;
			//		}
			//	}
			//}
		}

		protected override void DrawVariableLines(Point[] coords, int y)
		{
			throw new NotImplementedException();
		}
	}
}
