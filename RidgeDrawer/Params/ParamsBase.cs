using System;
using System.Reflection;

namespace RidgeDrawer
{
	public enum SmoothingType
	{
		None,
		Antialias,
	}

	public class ParamsBase
	{
		//если параметр задан - надо сохранять его состояние, чтобы при сменах плагина не слетало
		//для всех параметров надо будет задат начальное значнеи

		[ConsoleArgument("bt", "Render backend", type: typeof(BackendBase))]
		public Type Backend { get; set; }

		[ConsoleArgument("st", "Line smoothing", type: typeof(SmoothingType))]
		public SmoothingType Smoothing { get; set; } // в начале

		[ConsoleArgument("d", "Render with debug info", type: typeof(bool))]
		public bool Debug { get; set; }

		[ConsoleArgument("ef", "Effect", type: typeof(IEffect))]
		public Type Effect { get; set; }

		[ConsoleArgument("fi", "Prevent lines overlapping", "Fill inside", type: typeof(bool))]
		public bool FillInside { get; set; }

		[ConsoleArgument("lt", "Line type", type: typeof(LineType))]
		public LineType LineType { get; set; } // в конце

		[ConsoleArgument("s", "Line stroke width", "Stroke")]
		public int Stroke { get; set; } // в конце прямо перед отрисовкой

		[ConsoleArgument("ptx", "Pull lines to point (X axis)\nDoes not work at the moment", "Pull to X")]
		public int PullPointX { get; set; } // todo: make so that it is image center if this param is not specified // \d+(.\d+)?(,\d+(.\d+)?)?

		[ConsoleArgument("pty", "Pull lines to point (Y axis)\nDoes not work at the moment", "Pull to Y")]
		public int PullPointY { get; set; }

		public virtual ParamsBase Clone()
		{
			return CloneImpl<ParamsBase>();
		}

		protected T CloneImpl<T>() where T : new()
		{
			T clone = new T();
			foreach (PropertyInfo prop in typeof(T).GetProperties())
				prop.SetValue(clone, prop.GetValue(this));
			return clone;
		}

		public bool Equals(ParamsBase other)
		{
			if (other == null)
				return false;
			if (GetType() != other.GetType())
				return false;
			foreach (PropertyInfo prop in GetType().GetProperties())
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
