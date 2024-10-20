using System;
using System.Drawing;

namespace RidgeDrawer // TODO: каждая линия со своими параматрами, но это уже в афтере - типа рандом такой
{
	public abstract class BackendBase : IBackend
	{
		#region Abstract methods

		public abstract void DrawLines(MyPoint[] coords);
		public abstract void DrawDots(MyPoint[] coords);
		public abstract void DrawVariableLines(MyPoint[] coords, int y);
		public abstract void DrawCurve(MyPoint[] coords);
		public abstract void DrawBezier(MyPoint[] coords);
		public abstract void DrawDebugInfo();

		#endregion

		protected Bitmap newBitmap;
		protected Bitmap origBitmap;
		protected RenderParams param;

		public virtual void Construct(Bitmap newBitmap, Bitmap origBitmap, RenderParams param)
		{
			this.newBitmap = newBitmap;
			this.origBitmap = origBitmap;
			this.param = param;
		}

		public void Draw()
		{
			EffectBase effect;
			try
			{
				// TODO: не рисовать линии, у которых не было приращения
				effect = Activator.CreateInstance(param.Effect) as EffectBase;
				effect.Construct(this, newBitmap, origBitmap, param);
			}
			catch (Exception)
			{
				throw new NotImplementedException($"{param.Effect} effect is not supported");
			}
			
			effect.Apply();
			if (param.Debug)
				DrawDebugInfo();
		}
	}
}