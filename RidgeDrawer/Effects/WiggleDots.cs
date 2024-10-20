using System;
using System.Drawing;

namespace RidgeDrawer
{
	public class WiggleDots : IEffect
	{
		int height;
		int width;
		Color[] colors = new Color[] { Color.Black };

		public void Apply(BackendBase backend, Bitmap newBitmap, Bitmap origBitmap, RenderParams param)
		{
			/// go through every <see cref="reduceMltp"/>th pixel imitating imdage downsize
			/// if pixel is key color set pixel to the x+random, y+random

			int reduceMltp = param.ChunkSize;
			int maxShift = param.Factor;
			Color keyColor = Color.Black;
			height = newBitmap.Height / reduceMltp;
			width = newBitmap.Width / reduceMltp;
			Func<int, int, int> get = (int x, int y) => origBitmap.GetPixel(x * reduceMltp, y * reduceMltp).ToArgb();

			if (param.Debug)
				colors = new Color[] { Color.Red, Color.Blue, Color.Green, Color.Gray, Color.Magenta, Color.Brown, Color.Black, Color.Orange };

			int q = 0;
			Random r = new Random(param.Angle);
			for (int x = 1; x < width - 1; x++)
			{
				for (int y = 1; y < height - 1; y++)
				{
					var pixel = get(x, y);
					if (pixel == keyColor.ToArgb())
					{
						int shift = r.Next(0, maxShift) - maxShift / 3;
						int x1 = x * reduceMltp;
						int y1 = y * reduceMltp;
						int x2 = (x + 1) * reduceMltp;
						int y2 = (y + 1) * reduceMltp;

						var left = get(x - 1, y);
						var right = get(x + 1, y);
						var top = get(x, y - 1);
						var bottom = get(x, y + 1);

						if (param.Invert)
						{
							if (left != keyColor.ToArgb())
								x1 -= shift;
							if (right != keyColor.ToArgb())
								x2 += shift;
							if (top != keyColor.ToArgb())
								y1 -= shift;
							if (bottom != keyColor.ToArgb())
								y2 += shift;
						}
						else
						{
							if (left != keyColor.ToArgb() || right != keyColor.ToArgb() || top != keyColor.ToArgb() || bottom != keyColor.ToArgb())
							{
								int shift2 = r.Next(0, maxShift);
								x1 -= shift2;
								y1 -= shift2;
								x2 += shift2;
								y2 += shift2;
							}
						}

						DrawRect(newBitmap, x1, y1, x2, y2, colors[q++ % colors.Length]);
					}
				}
			}

		}

		private void DrawRect(Bitmap newBitmap, int x1, int y1, int x2, int y2, Color color)
		{
			for (int x = x1; x < x2; x++)
			{
				for (int y = y1; y < y2; y++)
				{
					if (x > 0 && y > 0 && x < newBitmap.Width && y < newBitmap.Height)
					{
						newBitmap.SetPixel(x, y, color);
					}
				}
			}
		}
	}
}
