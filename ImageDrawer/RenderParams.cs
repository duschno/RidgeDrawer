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
		// TODO: коэф стягивания к указанной точке (типа как блюр по z индексу в афтере)
	}

	public struct RenderParams
	{
		[ConsoleArgument("l")]
		public int LinesCount;

		[ConsoleArgument("s")]
		public int Stroke; // в конце прямо перед отрисовкой

		[ConsoleArgument("f")]
		public int Factor;

		[ConsoleArgument("c")]
		public int ChunkSize;

		[ConsoleArgument("g")]
		public int GreyPoint;

		[ConsoleArgument("b")]
		public int BlackPoint;

		[ConsoleArgument("w")]
		public int WhitePoint;

		[ConsoleArgument("a")]
		public int Angle;

		[ConsoleArgument("st")]
		public SmoothingType Smoothing; // в начале

		[ConsoleArgument("lt")]
		public LineType LineType; // в конце

		[ConsoleArgument("mt")]
		public MethodType Method; // в начале

		[ConsoleArgument("bt")]
		public Type Backend;

		[ConsoleArgument("dos")]
		public bool DrawOnSides;

		[ConsoleArgument("pap")]
		public int PointsAroundPeak;

		[ConsoleArgument("fi")]
		public bool FillInside;

		[ConsoleArgument("inv")]
		public bool Invert;

		[ConsoleArgument("d")]
		public bool Debug;

		[ConsoleArgument("ptx")]
		public int PullPointX;

		[ConsoleArgument("pty")]
		public int PullPointY;

		//TODO: рендерится не все сразу, а можно отрендерить лишь одну вертиальную линию например, чтобы можно было это анимировать. ниче не понял
	}

	[AttributeUsage(AttributeTargets.Field)]
	public class ConsoleArgumentAttribute : Attribute
	{
		public ConsoleArgumentAttribute(string name)
		{
			Name = name;
		}

		public virtual string Name { get; set; }
	}
}
