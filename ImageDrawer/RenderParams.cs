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

	public class LogicParams
	{
		public string InputFilename;
		public string OutputFilename;
		public RenderParams RenderParams;
	}

	public class RenderParams
	{
		[ConsoleArgument("l")]
		public int LinesCount { get; set; }

		[ConsoleArgument("s")]
		public int Stroke { get; set; } // в конце прямо перед отрисовкой

		[ConsoleArgument("f")]
		public int Factor { get; set; }

		[ConsoleArgument("c")]
		public int ChunkSize { get; set; }

		[ConsoleArgument("g")]
		public int GreyPoint { get; set; }

		[ConsoleArgument("b")]
		public int BlackPoint { get; set; }

		[ConsoleArgument("w")]
		public int WhitePoint { get; set; }

		[ConsoleArgument("a")]
		public int Angle { get; set; }

		[ConsoleArgument("st")]
		public SmoothingType Smoothing { get; set; } // в начале

		[ConsoleArgument("lt")]
		public LineType LineType { get; set; } // в конце

		[ConsoleArgument("mt")]
		public MethodType Method { get; set; } // в начале

		[ConsoleArgument("bt")]
		public Type Backend { get; set; }

		[ConsoleArgument("dos")]
		public bool DrawOnSides { get; set; }

		[ConsoleArgument("pap")]
		public int PointsAroundPeak { get; set; }

		[ConsoleArgument("fi")]
		public bool FillInside { get; set; }

		[ConsoleArgument("inv")]
		public bool Invert { get; set; }

		[ConsoleArgument("d")]
		public bool Debug { get; set; }

		[ConsoleArgument("ptx")]
		public int PullPointX { get; set; }

		[ConsoleArgument("pty")]
		public int PullPointY { get; set; }

		//TODO: рендерится не все сразу, а можно отрендерить лишь одну вертиальную линию например, чтобы можно было это анимировать. ниче не понял
	}

	[AttributeUsage(AttributeTargets.Property)]
	public class ConsoleArgumentAttribute : Attribute
	{
		public ConsoleArgumentAttribute(string name)
		{
			Name = name;
		}

		public virtual string Name { get; set; }
	}
}
