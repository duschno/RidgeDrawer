using System;

namespace ImageDrawer
{

	public enum SmoothingType
	{
		None,
		Antialias,
	}

	public enum LineType
	{
		Line,
		VariableLine,
		Dot,
		Curve,
		Bezier
	}

	public enum MethodType
	{
		Ridge,
		Squiggle
	}

	public struct RenderParams
	{
		public int LinesCount;
		public int Stroke; // в конце прямо перед отрисовкой
		public int Factor;
		public int ChunkSize;
		public int BlackPoint;
		public int WhitePoint;
		public int Angle;
		public SmoothingType Smoothing; // в начале
		public LineType LineType; // в конце
		public MethodType Method; // в начале
		public bool DrawOnSides;
		public bool FillInside;
		public bool Invert;
		public bool Debug;
		public Type Backend;
		//TODO рендерится не все сразу, а можно отрендерить лишь одну вертиальную линию например, чтобы можно было это анимировать
	}
}
