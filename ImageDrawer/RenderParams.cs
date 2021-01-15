using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageDrawer
{
	enum RenderType
	{
		Line,
		Dot,
		Curve
	}

	enum RenderMethod
	{
		Ridge,
		Squiggle
	}

	struct RenderParams
	{
		public int LinesCount;
		public int Width;
		public int Factor;
		public int ChunkSize;
		public SmoothingMode Smoothing;
		public RenderType LineType;
		public RenderMethod Method;
	}
}
