using System;
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
			throw new NotImplementedException();
		}

		protected override void DrawLines(Point[] coords)
		{
			for (int i = 0; i < coords.Length - 1; i++)
			{
				int x = coords[i].X;
				int y = coords[i].Y;
				int x2 = coords[i + 1].X;
				int y2 = coords[i + 1].Y;

				int w = x2 - x;
				int h = y2 - y;
				int dx1 = 0, dy1 = 0, dx2 = 0, dy2 = 0;
				if (w < 0) dx1 = -1; else if (w > 0) dx1 = 1;
				if (h < 0) dy1 = -1; else if (h > 0) dy1 = 1;
				if (w < 0) dx2 = -1; else if (w > 0) dx2 = 1;
				int longest = Math.Abs(w);
				int shortest = Math.Abs(h);
				if (!(longest > shortest))
				{
					longest = Math.Abs(h);
					shortest = Math.Abs(w);
					if (h < 0) dy2 = -1; else if (h > 0) dy2 = 1;
					dx2 = 0;
				}
				int numerator = longest >> 1;
				for (int j = 0; j <= longest; j++)
				{
					if (x > 0 && y > 0 && x < newBitmap.Width && y < newBitmap.Height)
						newBitmap.SetPixel(x, y, Color.Black);
					numerator += shortest;
					if (!(numerator < longest))
					{
						numerator -= longest;
						x += dx1;
						y += dy1;
					}
					else
					{
						x += dx2;
						y += dy2;
					}
				}
			}
		}

		protected override void DrawVariableLines(Point[] coords, int y)
		{
			throw new NotImplementedException();
		}
	}
}
