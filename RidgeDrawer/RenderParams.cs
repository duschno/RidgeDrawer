using System;
using System.Reflection;

namespace RidgeDrawer
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
		[ConsoleArgument("input_image", "Input image destination path",
			typeof(string), true, false)]
		public string InputFilename { get; set; }

		[ConsoleArgument("output_image", "Output image destination path, input_image path by default",
			typeof(string), false, false)]
		public string OutputFilename { get; set; }

		public RenderParams RenderParams { get; set; }
	}

	public class RenderParams
	{
		//добавить еще 2 параметра - сколько процентов от общего надо рисовать. если 0 и 100, то рисовать все, если 10 и 90, то с боков не будет
		//TODO: рендерится не все сразу, а можно отрендерить лишь одну вертиальную линию например, чтобы можно было это анимировать. ниче не понял

		[ConsoleArgument("l", "Amount of lines")]
		public int LinesCount { get; set; }

		[ConsoleArgument("s", "Line stroke width")]
		public int Stroke { get; set; } // в конце прямо перед отрисовкой

		[ConsoleArgument("f", "Multiply factor")]
		public int Factor { get; set; }

		[ConsoleArgument("c", "Line chunk size")]
		public int ChunkSize { get; set; }

		[ConsoleArgument("g", "Grey point")]
		public int GreyPoint { get; set; }

		[ConsoleArgument("b", "Black point")]
		public int BlackPoint { get; set; }

		[ConsoleArgument("w", "White point")]
		public int WhitePoint { get; set; }

		[ConsoleArgument("a", "Angle")]
		public int Angle { get; set; }

		[ConsoleArgument("st", "Line smoothing", typeof(SmoothingType))]
		public SmoothingType Smoothing { get; set; } // в начале

		[ConsoleArgument("lt", "Line type", typeof(LineType))]
		public LineType LineType { get; set; } // в конце

		[ConsoleArgument("mt", "Render algorithm method", typeof(MethodType))]
		public MethodType Method { get; set; } // в начале

		[ConsoleArgument("bt", "Render backend", typeof(BackendDrawerBase))]
		public Type Backend { get; set; }

		[ConsoleArgument("dos", "Draw on line sides", typeof(bool))]
		public bool DrawOnSides { get; set; }

		[ConsoleArgument("pap", @"Amount of points around peaks
                                  Does not work at the moment")]
		public int PointsAroundPeak { get; set; }

		[ConsoleArgument("fi", "Prevent lines overlapping", typeof(bool))]
		public bool FillInside { get; set; }

		[ConsoleArgument("inv", "Invert", typeof(bool))]
		public bool Invert { get; set; }

		[ConsoleArgument("d", "Render with debug info", typeof(bool))]
		public bool Debug { get; set; }

		[ConsoleArgument("ptx", @"Pull lines to point (X axis)
					              Does not work at the moment")]
		public int PullPointX { get; set; } // todo: make so that it is image center if this param is not specified // \d+(.\d+)?(,\d+(.\d+)?)?

		[ConsoleArgument("pty", @"Pull lines to point (Y axis)
					              Does not work at the moment")]
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
