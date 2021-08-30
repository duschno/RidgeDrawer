using System.Drawing;

namespace ImageDrawer
{
	public abstract class BackendDrawerBase
	{
		public abstract void Draw(Bitmap newBitmap, Bitmap origBitmap, RenderParams param);
	}
}
