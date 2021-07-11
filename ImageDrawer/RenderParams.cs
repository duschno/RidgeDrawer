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
		Dot,
		Curve
	}

	public enum MethodType
	{
		Ridge,
		Squiggle
	}

	public enum BackendType
	{
		GDIPlus,
		Cairo
	}

	public struct RenderParams
	{
		public int LinesCount;
		public int Stroke; // в конце прямо перед отрисовкой
		public int Factor;
		public int ChunkSize;
		public int Angle;
		public SmoothingType Smoothing; // в начале
		public LineType LineType; // в конце
		public MethodType Method; // в начале
		public BackendType Backend;
	}
}
