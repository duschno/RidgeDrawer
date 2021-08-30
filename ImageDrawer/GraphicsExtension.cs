using System;
using System.Drawing;

namespace ImageDrawer
{
	public static class GraphicsExtension
	{
		public static Graphics FillVariableLine(this Graphics g, SolidBrush b, int x1, int y1, int x2, int y2, int w1, int w2)
		{
			var c1 = new { r = (float)w1 / 2, x = x1, y = y1 };
			var c2 = new { r = (float)w2 / 2, x = x2, y = y2 };

			var x = Math.Abs(c1.x - c2.x);
			var y = Math.Abs(c1.y - c2.y);
			var l = (float)Math.Sqrt((x * x) + (y * y));

			var v = new { x = c2.x - c1.x, y = c2.y - c1.y};
			var uv = new { x = v.x / l, y = v.y / l };

			var ca = (c2.r - c1.r) / l;
			var sa = (float)Math.Sqrt(1 - (ca * ca));

			PointF[] points = new PointF[] {
				new PointF(c1.x - (ca * c1.r * uv.x) - (sa * c1.r * uv.y),
						   c1.y + (sa * c1.r * uv.x) - (ca * c1.r * uv.y)),
				new PointF(c1.x - (ca * c1.r * uv.x) + (sa * c1.r * uv.y),
						   c1.y - (sa * c1.r * uv.x) - (ca * c1.r * uv.y)),

				new PointF(c2.x - (ca * c2.r * uv.x) + (sa * c2.r * uv.y),
						   c2.y - (sa * c2.r * uv.x) - (ca * c2.r * uv.y)),
				new PointF(c2.x - (ca * c2.r * uv.x) - (sa * c2.r * uv.y),
						   c2.y + (sa * c2.r * uv.x) - (ca * c2.r * uv.y)),
			};

			g.FillEllipse(b, new Rectangle(x1 - w1 / 2, y1 - w1 / 2, w1, w1));
			g.FillEllipse(b, new Rectangle(x2 - w2 / 2, y2 - w2 / 2, w2, w2));
			g.FillPolygon(b, points);
			return g;
		}
	}
}
