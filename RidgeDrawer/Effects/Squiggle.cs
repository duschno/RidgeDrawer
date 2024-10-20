using System;
using System.Drawing;
using System.Linq;

namespace RidgeDrawer.Effects
{
	//internal class Squiggle : IEffect
	//{
	//	private extern static void Draw(IntPtr hdc, int[] x, int[] y, int size);

	//	protected override void DrawBezier(MyPoint[] coords)
	//	{
	//		throw new NotImplementedException();
	//	}

	//	protected override void DrawCurve(MyPoint[] coords)
	//	{
	//		throw new NotImplementedException();
	//	}

	//	protected override void DrawDots(MyPoint[] coords)
	//	{
	//		throw new NotImplementedException();
	//	}

	//	protected override void DrawLines(MyPoint[] coords)
	//	{
	//		using (Graphics graphics = Graphics.FromImage(newBitmap))
	//		{
	//			IntPtr hdc = graphics.GetHdc();
	//			Draw(hdc, coords.Select(e => e.X).ToArray(), coords.Select(e => e.Y).ToArray(), coords.Count());
	//			graphics.ReleaseHdc();
	//		}
	//	}

	//	protected override void DrawVariableLines(MyPoint[] coords, int y)
	//	{
	//		throw new NotImplementedException();
	//	}

	//	protected override void DrawDebugInfo()
	//	{
	//		throw new NotImplementedException();
	//	}
	//}
}
