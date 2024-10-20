using System;
using System.Drawing;

namespace RidgeDrawer
{
	public abstract class EffectBase
	{
		protected IBackend backend;
		protected Bitmap newBitmap;
		protected Bitmap origBitmap;
		protected RenderParams param;

		public void Construct(IBackend backend, Bitmap newBitmap, Bitmap origBitmap, RenderParams param)
		{
			this.backend = backend;
			this.newBitmap = newBitmap;
			this.origBitmap = origBitmap;
			this.param = param;
		}

		public abstract void Apply();
	}
}
