using System;
using System.Reflection;

namespace RidgeDrawer
{
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
		Squiggle,
		SquiggleLegacy
		// TODO: коэф стягивания к указанной точке (типа как блюр по z индексу в афтере)
	}

	public class RenderParams : ParamsBase
	{
		//добавить еще 2 параметра - сколько процентов от общего надо рисовать. если 0 и 100, то рисовать все, если 10 и 90, то с боков не будет
		//TODO: рендерится не все сразу, а можно отрендерить лишь одну вертиальную линию например, чтобы можно было это анимировать. ниче не понял

		[ConsoleArgument("l", "Amount of lines", "Lines count")]
		public int LinesCount { get; set; }

		[ConsoleArgument("f", "Multiply factor", "Factor")]
		public int Factor { get; set; }

		[ConsoleArgument("c", "Line chunk size", "Chunk size")]
		public int ChunkSize { get; set; }

		[ConsoleArgument("g", "Grey point")]
		public int GreyPoint { get; set; }

		[ConsoleArgument("b", "Black point")]
		public int BlackPoint { get; set; }

		[ConsoleArgument("w", "White point")]
		public int WhitePoint { get; set; }

		[ConsoleArgument("a", "Angle")]
		public int Angle { get; set; }


		[ConsoleArgument("mt", "Render algorithm method", "Method", typeof(MethodType))]
		public MethodType Method { get; set; } // в начале

		[ConsoleArgument("dos", "Draw on line sides", "Draw on sides", type: typeof(bool))]
		public bool DrawOnSides { get; set; }

		[ConsoleArgument("pap", "Amount of points around peaks\nDoes not work at the moment", "Points around peak")]
		public int PointsAroundPeak { get; set; }

		[ConsoleArgument("inv", "Invert", type: typeof(bool))]
		public bool Invert { get; set; }

		public override ParamsBase Clone()
		{
			return CloneImpl<RenderParams>();
		}
	}
}
