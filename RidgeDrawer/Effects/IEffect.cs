using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RidgeDrawer
{
	public interface IEffect
	{
		void Apply(BackendBase backend, Bitmap newBitmap, Bitmap origBitmap, RenderParams param);
	}
}
