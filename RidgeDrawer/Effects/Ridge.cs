using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace RidgeDrawer
{
	public class Ridge : Common, IEffect
	{
		public void Apply(BackendBase backend, Bitmap newBitmap, Bitmap origBitmap, RenderParams param)
		{
			// TODO: чекни скрин на телефоне с женщиной, там линии объемно смещаются от центра
			int lineNumber = 0; // сейчас он считает так: насколько относительно серого цвета сместить вверх или вниз линию. надо переделать от белого
			while (lineNumber < param.LinesCount)
			{
				List<MyPoint> coords = new List<MyPoint>();
				int y = GetLineY(lineNumber, origBitmap, param);

				for (int x = origBitmap.Width / 2 % param.ChunkSize; x < origBitmap.Width; x += param.ChunkSize) // TODO: чанки распределять на оси Х не равномерно, а без сдвига, что бы при некратных значениях (50 и 51 наприм) не было фликеринга, а просто добавлялась новая координата
					coords.Add(CalculatePoint(origBitmap, x, y, param));
				if (param.DrawOnSides)
				{
					coords.Insert(0, CalculatePoint(origBitmap, 0, y, param));
					coords.Add(CalculatePoint(origBitmap, origBitmap.Width - 1, y, param));
				}

				foreach (List<MyPoint> coordsPart in GetAffectedPoints(coords, y, param))
					RenderLine(backend, coordsPart, param, y);
				lineNumber++;
			}
		}
	}
}
