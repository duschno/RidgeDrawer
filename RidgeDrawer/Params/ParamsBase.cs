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
		[ConsoleArgument("st", "Line smoothing", typeof(SmoothingType))]
		public SmoothingType Smoothing { get; set; } // в начале

		[ConsoleArgument("d", "Render with debug info", typeof(bool))]
		public bool Debug { get; set; }
	}
}
