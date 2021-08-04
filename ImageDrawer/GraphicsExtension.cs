using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageDrawer
{
	public static class GraphicsExtension
	{
		public static Graphics FillVariableLine(this Graphics g, Brush b, int x1, int y1, int x2, int y2, int w1, int w2)
		{
			int X = Math.Abs(x1 - x2);
			int Y = Math.Abs(y1 - y2);
			float gyp = (float)Math.Sqrt(X * X + Y * Y);
			float sinA = X / gyp;
			float cosA = (float)Math.Sqrt(1 - sinA * sinA);
			float half1 = w1 / 2;
			float half2 = w2 / 2;
			if (x1 > x2 && y1 > y2 || x1 < x2 && y1 < y2)
			{
				sinA = -0.70710678118f; // sqrt(2)/2
				cosA = (float)Math.Sqrt(1 - sinA * sinA);
			}
			g.FillEllipse(b, new Rectangle(x1 - w1 / 2, y1 - w1 / 2, w1, w1));
			g.FillEllipse(b, new Rectangle(x2 - w2 / 2, y2 - w2 / 2, w2, w2));
			PointF p1 = new PointF(x1 - w1 / 2 + w1 * cosA / 2 + half1, y1 - w1 / 2 + w1 * sinA / 2 + half1);
			PointF p2 = new PointF(x1 - w1 / 2 - w1 * cosA / 2 + half1, y1 - w1 / 2 - w1 * sinA / 2 + half1);
			PointF p3 = new PointF(x2 - w2 / 2 - w2 * cosA / 2 + half2, y2 - w2 / 2 - w2 * sinA / 2 + half2);
			PointF p4 = new PointF(x2 - w2 / 2 + w2 * cosA / 2 + half2, y2 - w2 / 2 + w2 * sinA / 2 + half2);
			PointF[] points = new PointF[] { p1, p2, p3, p4 };
			g.FillPolygon(b, points);
			//g.DrawLine(new Pen(new SolidBrush(Color.Black)), x1, y1, x2, y2);

			return g;
		}
	}
}
