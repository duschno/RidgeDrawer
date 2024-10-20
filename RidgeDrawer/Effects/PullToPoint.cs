using RidgeDrawer;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace RidgeDrawer
{
	public class PullToPoint : IEffect
	{
		public void Apply(BackendBase backend, Bitmap newBitmap, Bitmap origBitmap, RenderParams param)
		{
			// TODO: коэф стягивания к указанной точке (типа как блюр по z индексу в афтере)
			throw new NotImplementedException();
		}
	}
}
