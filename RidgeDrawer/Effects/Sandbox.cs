using RidgeDrawer;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace RidgeDrawer
{
	public class Sandbox : BackendDrawerBase
	{

		private void Play()
		{
			using (Graphics graphics = Graphics.FromImage(newBitmap))
			{
				graphics.SmoothingMode = param.Smoothing == SmoothingType.Antialias ?
					SmoothingMode.AntiAlias : SmoothingMode.None;
				int w1 = 20;
				int w2 = 80;
				graphics.FillVariableLine(new SolidBrush(Color.Red), 380, 380, 280, 280, w1, w2);
				graphics.FillVariableLine(new SolidBrush(Color.Orange), 380, 420, 280, 520, w1, w2);
				graphics.FillVariableLine(new SolidBrush(Color.Yellow), 420, 380, 520, 280, w1, w2);
				graphics.FillVariableLine(new SolidBrush(Color.DarkOliveGreen), 420, 420, 520, 520, w1, w2);

				graphics.FillVariableLine(new SolidBrush(Color.Blue), 400, 380, 400, 280, w1, w2);
				graphics.FillVariableLine(new SolidBrush(Color.LightBlue), 380, 400, 280, 400, w1, w2);
				graphics.FillVariableLine(new SolidBrush(Color.Violet), 400, 420, 400, 520, w1, w2);
				graphics.FillVariableLine(new SolidBrush(Color.Black), 420, 400, 520, 400, w1, w2);


				graphics.DrawCurve(new Pen(Color.Black), new Point[] { new Point(100, 100), new Point(200, 200), new Point(200, 100), new Point(100, 200) }, .5f);
			}
		}

		protected override void DrawCurve(MyPoint[] coords)
		{
			Play();
		}

		protected override void DrawDots(MyPoint[] coords)
		{
			Play();
		}

		protected override void DrawLines(MyPoint[] coords)
		{
			Play();
		}

		protected override void DrawVariableLines(MyPoint[] coords, int y)
		{
			Play();
		}

		protected override void DrawBezier(MyPoint[] coords)
		{
			Play();
		}

		protected override void DrawDebugInfo()
		{
			throw new NotImplementedException();
		}
	}
}
