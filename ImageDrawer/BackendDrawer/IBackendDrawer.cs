using System.Drawing;

namespace ImageDrawer
{
	interface IBackendDrawer
	{
		void Draw(Bitmap newBitmap, Bitmap origBitmap, RenderParams param);
	}
}
