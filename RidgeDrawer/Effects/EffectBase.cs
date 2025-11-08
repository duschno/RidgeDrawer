using System.Drawing;

namespace RidgeDrawer
{
	public abstract class EffectBase<T> : IEffect where T : ParamsBase
	{
		protected BackendBase backend;
		protected Bitmap newBitmap;
		protected Bitmap origBitmap;
		protected T param;

		public EffectBase(BackendBase backend, Bitmap newBitmap, Bitmap origBitmap, T param)
		{
			this.backend = backend;
			this.newBitmap = newBitmap;
			this.origBitmap = origBitmap;
			this.param = param;
		}

		public abstract void Apply();
	}
}
