using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RidgeDrawer
{
	public enum SmoothingType
	{
		None,
		Antialias,
	}

	public class ParamsBase
	{
		[ConsoleArgument("bt", "Render backend", type: typeof(BackendBase))]
		public Type Backend { get; set; }

		[ConsoleArgument("st", "Line smoothing", type: typeof(SmoothingType))]
		public SmoothingType Smoothing { get; set; } // в начале

		[ConsoleArgument("d", "Render with debug info", type: typeof(bool))]
		public bool Debug { get; set; }

		[ConsoleArgument("ef", "Effect", type: typeof(IEffect))]
		public Type Effect { get; set; } // в начале
	}
}
