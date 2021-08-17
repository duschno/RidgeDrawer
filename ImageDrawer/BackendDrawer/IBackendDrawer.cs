using System.Drawing;

namespace ImageDrawer
{
	public interface IBackendDrawer
	{
		void Draw(Bitmap newBitmap, Bitmap origBitmap, RenderParams param);
	}
}
