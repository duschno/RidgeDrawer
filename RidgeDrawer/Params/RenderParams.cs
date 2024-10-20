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

	public class LogicParams
	{
		[ConsoleArgument("input_image", "Input image destination path",
			null, typeof(string), true, false)]
		public string InputFilename { get; set; }

		[ConsoleArgument("output_image", "Output image destination path, input_image path by default",
			null, typeof(string), false, false)]
		public string OutputFilename { get; set; }

		public RenderParams RenderParams { get; set; }
	}

	public class RenderParams : ParamsBase
	{
		//добавить еще 2 параметра - сколько процентов от общего надо рисовать. если 0 и 100, то рисовать все, если 10 и 90, то с боков не будет
		//TODO: рендерится не все сразу, а можно отрендерить лишь одну вертиальную линию например, чтобы можно было это анимировать. ниче не понял

		[ConsoleArgument("l", "Amount of lines", "Lines count")]
		public int LinesCount { get; set; }

		[ConsoleArgument("s", "Line stroke width", "Stroke")]
		public int Stroke { get; set; } // в конце прямо перед отрисовкой

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

		[ConsoleArgument("lt", "Line type", type: typeof(LineType))]
		public LineType LineType { get; set; } // в конце

		[ConsoleArgument("dos", "Draw on line sides", "Draw on sides", type: typeof(bool))]
		public bool DrawOnSides { get; set; }

		[ConsoleArgument("pap", "Amount of points around peaks\nDoes not work at the moment", "Points around peak")]
		public int PointsAroundPeak { get; set; }

		[ConsoleArgument("fi", "Prevent lines overlapping", "Fill inside", type: typeof(bool))]
		public bool FillInside { get; set; }

		[ConsoleArgument("inv", "Invert", type: typeof(bool))]
		public bool Invert { get; set; }

		[ConsoleArgument("ptx", "Pull lines to point (X axis)\nDoes not work at the moment", "Pull to X")]
		public int PullPointX { get; set; } // todo: make so that it is image center if this param is not specified // \d+(.\d+)?(,\d+(.\d+)?)?

		[ConsoleArgument("pty", "Pull lines to point (Y axis)\nDoes not work at the moment", "Pull to Y")]
		public int PullPointY { get; set; }

		public RenderParams Clone()
		{
			RenderParams clone = new RenderParams();
			foreach (PropertyInfo prop in typeof(RenderParams).GetProperties())
				prop.SetValue(clone, prop.GetValue(this));
			return clone;
		}

		public bool Equals(RenderParams other)
		{
			if (other == null)
				return false;
			foreach (PropertyInfo prop in typeof(RenderParams).GetProperties())
			{
				var v1 = prop.GetValue(this);
				var v2 = prop.GetValue(other);
				if (!Equals(v1, v2))
					return false;
			}

			return true;
		}
	}
}
