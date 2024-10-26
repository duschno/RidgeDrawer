namespace RidgeDrawer
{
	public interface IBackend
	{
		void DrawLines(MyPoint[] coords);
		void DrawDots(MyPoint[] coords);
		void DrawVariableLines(MyPoint[] coords, int y);
		void DrawCurve(MyPoint[] coords);
		void DrawBezier(MyPoint[] coords);
		void FillRect(int x1, int y1, int x2, int y2);
		void DrawDebugInfo();
	}
}
