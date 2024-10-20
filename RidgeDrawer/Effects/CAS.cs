using System.Drawing;
using System.Numerics;
using System.Threading.Tasks;

namespace RidgeDrawer
{
	internal class CAS : IEffect
	{
		float Sharpening = 1.0f;
		float Contrast = 0.0f;
		int height;
		int width;
		Bitmap newBitmap;
		Bitmap origBitmap;

		public void Apply(BackendBase backend, Bitmap newBitmap, Bitmap origBitmap, RenderParams param)
		{
			height = newBitmap.Height;
			width = newBitmap.Width;
			this.newBitmap = newBitmap;
			this.origBitmap = origBitmap;
			using (Graphics graphics = Graphics.FromImage(newBitmap))
			{
				Parallel.For(0, width, Sharpen);
			}
		}

		private Color GetPixelLock(Bitmap bm, int x, int y)
		{
			lock (bm)
			{
				return bm.GetPixel(x, y);
			}
		}

		private void SetPixelLock(Bitmap bm, int x, int y, Color c)
		{
			lock (bm)
			{
				bm.SetPixel(x, y, c);
			}
		}

		private void Sharpen(int x)
		{
			for (int y = 0; y < height; y++)
			{
				if (x == 0 || y == 0 ||
					x == width - 1 || y == height - 1)
				{
					SetPixelLock(newBitmap, x, y, GetPixelLock(origBitmap, x, y));
					continue;
				}

				// fetch a 3x3 neighborhood around the pixel 'e',
				// a b c
				// d(e)f
				// g h i

				Vector3 a, b, c, d, e, f, g, h, i;
				lock (origBitmap)
				{
					a = ToVector3(origBitmap.GetPixel(x - 1, y - 1));
					b = ToVector3(origBitmap.GetPixel(x, y - 1));
					c = ToVector3(origBitmap.GetPixel(x + 1, y - 1));
					d = ToVector3(origBitmap.GetPixel(x - 1, y));
					e = ToVector3(origBitmap.GetPixel(x, y));
					f = ToVector3(origBitmap.GetPixel(x + 1, y));
					g = ToVector3(origBitmap.GetPixel(x - 1, y + 1));
					h = ToVector3(origBitmap.GetPixel(x, y + 1));
					i = ToVector3(origBitmap.GetPixel(x + 1, y + 1));
				}

				// Soft min and max.
				// a b c			 b
				// d e f * 0.5  +  d e f * 0.5
				// g h i			 h
				// These are 2.0x bigger (factored out the extra multiply).
				Vector3 mnRGB = Vector3.Min(Vector3.Min(Vector3.Min(d, e), Vector3.Min(f, b)), h);
				Vector3 mnRGB2 = Vector3.Min(mnRGB, Vector3.Min(Vector3.Min(a, c), Vector3.Min(g, i)));
				mnRGB += mnRGB2;

				Vector3 mxRGB = Vector3.Max(Vector3.Max(Vector3.Max(d, e), Vector3.Max(f, b)), h);
				Vector3 mxRGB2 = Vector3.Max(mxRGB, Vector3.Max(Vector3.Max(a, c), Vector3.Max(g, i)));
				mxRGB += mxRGB2;

				// Smooth Minimum distance to signal limit divided by smooth Max.
				Vector3 rcpMRGB = Rcp(mxRGB);
				Vector3 ampRGB = Saturate(Vector3.Min(mnRGB, new Vector3(2) - mxRGB) * rcpMRGB);

				// Shaping amount of sharpening.
				ampRGB = Rsqrt(ampRGB);

				float peak = -3.0f * Contrast + 8.0f;
				Vector3 wRGB = -Rcp(ampRGB * peak);

				Vector3 rcpWeightRGB = Rcp(4f * wRGB + new Vector3(1));

				//						  0 w 0
				//  Filter shape:		  w 1 w
				//						  0 w 0  
				Vector3 window = (b + d) + (f + h);
				Vector3 outColor = Saturate((window * wRGB + e) * rcpWeightRGB);

				SetPixelLock(newBitmap, x, y, ToColor(Vector3.Lerp(e, outColor, Sharpening)));
			}
		}

		private Vector3 Saturate(Vector3 c)
		{
			if (c.X > 1)
				c.X = 1;
			if (c.Y > 1)
				c.Y = 1;
			if (c.Z > 1)
				c.Z = 1;

			if (c.X < 0)
				c.X = 0;
			if (c.Y < 0)
				c.Y = 0;
			if (c.Z < 0)
				c.Z = 0;

			return c;
		}

		private Vector3 Rcp(Vector3 c)
		{
			c.X = c.X == 0 ? 1 : 1 / c.X;
			c.Y = c.Y == 0 ? 1 : 1 / c.Y;
			c.Z = c.Z == 0 ? 1 : 1 / c.Z;
			return c;
		}

		private Vector3 Rsqrt(Vector3 c)
		{
			return Rcp(Vector3.SquareRoot(c));
		}

		private Vector3 ToVector3(Color c)
		{
			return new Vector3(c.R / 255f,
							   c.G / 255f,
							   c.B / 255f);
		}
		private Color ToColor(Vector3 c)
		{
			return Color.FromArgb(
				(int)(c.X * 255),
				(int)(c.Y * 255),
				(int)(c.Z * 255)
			);
		}
	}
}
