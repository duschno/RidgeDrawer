using System;
using System.Drawing;

namespace RidgeDrawer
{
	public class DoNotRememberWhatItIs : BackendBase
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
			throw new NotImplementedException();
		}

		public override void DrawLines(MyPoint[] coords)
		{
			for (int i = 0; i < coords.Length - 1; i++)
			{
				Point a = coords[i];
				Point b = coords[i + 1];

				int deltax = Math.Abs(b.X - a.X) + 1;
				int deltay = Math.Abs(b.Y - b.Y) + 1;
				int error = 0;
				int y = a.Y;
				int diry = b.Y - a.Y;
				if (diry > 0)
					diry = 1;
				if (diry < 0)
					diry = -1;
				for (int x = a.X; x <= b.X; x++)
				{
					if (x > 0 && x < newBitmap.Width && y > 0 && y < newBitmap.Height)
						newBitmap.SetPixel(x, y, Color.Black);
					error += deltay;
					if (error >= deltax)
					{
						y += diry;
						error -= deltax;
					}
				}
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
	}
}
