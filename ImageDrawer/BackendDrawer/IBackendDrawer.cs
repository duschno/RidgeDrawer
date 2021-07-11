using System.Drawing;

namespace ImageDrawer
{
	interface IBackendDrawer
	{
		void Draw(Graphics g, Bitmap bmp, RenderParams param);
	}
}
