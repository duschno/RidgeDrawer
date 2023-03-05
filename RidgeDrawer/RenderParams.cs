using System;
using System.ComponentModel;
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
		public string InputFilename;
		public string OutputFilename;
		public RenderParams RenderParams;
	}

	public class RenderParams : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged; // it is not related to ui, why should it use propertychanged then?

		private int linesCount;
		[ConsoleArgument("l")]
		public int LinesCount
		{
			get => linesCount;
			set
			{
				linesCount = value;
				if (PropertyChanged != null)
					PropertyChanged(this, new PropertyChangedEventArgs(nameof(LinesCount)));
			}
		}

		private int stroke;
		[ConsoleArgument("s")]
		public int Stroke
		{
			get => stroke;
			set
			{
				stroke = value;
				if (PropertyChanged != null)
					PropertyChanged(this, new PropertyChangedEventArgs(nameof(Stroke)));
			}
		} // в конце прямо перед отрисовкой

		private int factor;
		[ConsoleArgument("f")]
		public int Factor
		{
			get => factor;
			set
			{
				factor = value;
				if (PropertyChanged != null)
					PropertyChanged(this, new PropertyChangedEventArgs(nameof(Factor)));
			}
		}

		private int chunkSize;
		[ConsoleArgument("c")]
		public int ChunkSize
		{
			get => chunkSize;
			set
			{
				chunkSize = value;
				if (PropertyChanged != null)
					PropertyChanged(this, new PropertyChangedEventArgs(nameof(ChunkSize)));
			}
		}

		private int greyPoint;
		[ConsoleArgument("g")]
		public int GreyPoint
		{
			get => greyPoint;
			set
			{
				greyPoint = value;
				if (PropertyChanged != null)
					PropertyChanged(this, new PropertyChangedEventArgs(nameof(GreyPoint)));
			}
		}

		private int blackPoint;
		[ConsoleArgument("b")]
		public int BlackPoint
		{
			get => blackPoint;
			set
			{
				blackPoint = value;
				if (PropertyChanged != null)
					PropertyChanged(this, new PropertyChangedEventArgs(nameof(BlackPoint)));
			}
		}

		private int whitePoint;
		[ConsoleArgument("w")]
		public int WhitePoint
		{
			get => whitePoint;
			set
			{
				whitePoint = value;
				if (PropertyChanged != null)
					PropertyChanged(this, new PropertyChangedEventArgs(nameof(WhitePoint)));
			}
		}

		private int angle;
		[ConsoleArgument("a")]
		public int Angle
		{
			get => angle;
			set
			{
				angle = value;
				if (PropertyChanged != null)
					PropertyChanged(this, new PropertyChangedEventArgs(nameof(Angle)));
			}
		}

		private SmoothingType smoothing;
		[ConsoleArgument("st")]
		public SmoothingType Smoothing
		{
			get => smoothing;
			set
			{
				smoothing = value;
				if (PropertyChanged != null)
					PropertyChanged(this, new PropertyChangedEventArgs(nameof(Smoothing)));
			}
		} // в начале

		private LineType lineType;
		[ConsoleArgument("lt")]
		public LineType LineType
		{
			get => lineType;
			set
			{
				lineType = value;
				if (PropertyChanged != null)
					PropertyChanged(this, new PropertyChangedEventArgs(nameof(LineType)));
			}
		} // в конце

		private MethodType method;
		[ConsoleArgument("mt")]
		public MethodType Method
		{
			get => method;
			set
			{
				method = value;
				if (PropertyChanged != null)
					PropertyChanged(this, new PropertyChangedEventArgs(nameof(Method)));
			}
		} // в начале

		private Type backend;
		[ConsoleArgument("bt")]
		public Type Backend
		{
			get => backend;
			set
			{
				backend = value;
				if (PropertyChanged != null)
					PropertyChanged(this, new PropertyChangedEventArgs(nameof(Backend)));
			}
		}

		private bool drawOnSides;
		[ConsoleArgument("dos")]
		public bool DrawOnSides
		{
			get => drawOnSides;
			set
			{
				drawOnSides = value;
				if (PropertyChanged != null)
					PropertyChanged(this, new PropertyChangedEventArgs(nameof(DrawOnSides)));
			}
		}

		private int pointsAroundPeak;
		[ConsoleArgument("pap")]
		public int PointsAroundPeak
		{
			get => pointsAroundPeak;
			set
			{
				pointsAroundPeak = value;
				if (PropertyChanged != null)
					PropertyChanged(this, new PropertyChangedEventArgs(nameof(PointsAroundPeak)));
			}
		}

		private bool fillInside;
		[ConsoleArgument("fi")]
		public bool FillInside
		{
			get => fillInside;
			set
			{
				fillInside = value;
				if (PropertyChanged != null)
					PropertyChanged(this, new PropertyChangedEventArgs(nameof(FillInside)));
			}
		}

		private bool invert;
		[ConsoleArgument("inv")]
		public bool Invert
		{
			get => invert;
			set
			{
				invert = value;
				if (PropertyChanged != null)
					PropertyChanged(this, new PropertyChangedEventArgs(nameof(Invert)));
			}
		}

		private bool debug;
		[ConsoleArgument("d")]
		public bool Debug
		{
			get => debug;
			set
			{
				debug = value;
				if (PropertyChanged != null)
					PropertyChanged(this, new PropertyChangedEventArgs(nameof(Debug)));
			}
		}

		private int pullPointX;
		[ConsoleArgument("ptx")]
		public int PullPointX
		{
			get => pullPointX;
			set
			{
				pullPointX = value;
				if (PropertyChanged != null)
					PropertyChanged(this, new PropertyChangedEventArgs(nameof(PullPointX)));
			}
		}

		private int pullPointY;
		[ConsoleArgument("pty")]
		public int PullPointY
		{
			get => pullPointY;
			set
			{
				pullPointY = value;
				if (PropertyChanged != null)
					PropertyChanged(this, new PropertyChangedEventArgs(nameof(PullPointY)));
			}
		}

		//добавить еще 2 паарметра - сколько процентов от общего надо рисовать. если 0 и 100, то рисовать все, если 10 и 90, то с боков не будет

		public RenderParams Clone()
		{
			RenderParams clone = new RenderParams();
			foreach (PropertyInfo prop in typeof(RenderParams).GetProperties())
				prop.SetValue(clone, prop.GetValue(this));
			return clone;
		}

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
