using System.Windows.Controls;
using System.Windows.Media;

namespace RidgeDrawerUI
{
	public enum ScaleType
	{
		/// <summary>
		/// Scale is less than 100%, image is fit to viewport
		/// </summary>
		FitToViewport,

		/// <summary>
		/// Scale is 100%, image is smaller than viewport and completely visible
		/// </summary>
		NoScaleSmallerThanViewport,

		/// <summary>
		/// Scale is 100%, image is bigger than viewport and partly visible
		/// </summary>
		NoScaleBiggerThanViewport,

		/// <summary>
		/// Integer scale factor, e.g. 2, 3 and so on.
		/// </summary>
		Factor
	}

	public class ViewportScaler
	{
		public int CurrentFactor { get; private set; }
		private int MaxFactor { get; set; }

		private ScaleType currentScaleType;

		public ScaleType CurrentScaleType
		{
			get
			{
				return currentScaleType;
			}

			private set
			{
				switch (value)
				{
					case ScaleType.FitToViewport:
						CurrentFactor = 1;
						Image.Width = Image.Height = double.NaN;
						Image.Stretch = Stretch.Uniform;
						RenderOptions.SetBitmapScalingMode(Image, BitmapScalingMode.Linear);
						break;
					case ScaleType.NoScaleSmallerThanViewport:
						CurrentFactor = 1;
						Image.Stretch = Stretch.None;
						RenderOptions.SetBitmapScalingMode(Image, BitmapScalingMode.Linear);
						break;
					case ScaleType.NoScaleBiggerThanViewport:
						CurrentFactor = 1;
						Image.Stretch = Stretch.Uniform;
						RenderOptions.SetBitmapScalingMode(Image, BitmapScalingMode.Linear);
						break;
					case ScaleType.Factor:
						Image.Stretch = Stretch.Uniform;
						RenderOptions.SetBitmapScalingMode(Image, BitmapScalingMode.NearestNeighbor);
						break;
					default:
						break;
				}
			}
		}

		private Image Image { get; set; }

		public ViewportScaler(Image image, int maxFactor)
		{
			Image = image;
			CurrentFactor = 1;
			MaxFactor = maxFactor;
		}

		public void SetNextFactor(bool zoomIn)
		{
			if (zoomIn && CurrentFactor == MaxFactor)
				return;
			if (!zoomIn && CurrentFactor == 1)
				return;

			CurrentFactor += zoomIn ? 1 : -1;
		}

		internal void ChangeScale(bool zoomIn)
		{
			if (Image.Source == null)
				return;

			if (zoomIn)
			{
				if (Image.ActualWidth < Image.Source.Width)
				{
					Image.Width = Image.Source.Width;
					Image.Height = Image.Source.Height;
				}
				else
				{
					SetNextFactor(zoomIn);
					Image.Width = Image.Source.Width * CurrentFactor;
					Image.Height = Image.Source.Height * CurrentFactor;
				}
			}
			else
			{
				if (CurrentScaleType == ScaleType.NoScaleSmallerThanViewport)
					return;

				if (Image.Width <= Image.Source.Width)
					CurrentScaleType = ScaleType.FitToViewport;
				else
				{
					SetNextFactor(zoomIn);
					Image.Width = Image.Source.Width * CurrentFactor;
					Image.Height = Image.Source.Height * CurrentFactor;
					if (Image.Width < Image.Source.Width)
						CurrentScaleType = ScaleType.FitToViewport;
				}
			}
		}

		internal void SetOriginalSize()
		{
			CurrentFactor = 1;
			Image.Width = Image.Source.Width;
			Image.Height = Image.Source.Height;
		}

		internal void ChangeUIProps(Grid viewport)
		{
			if (IsOriginalSize)
			{
				if (Image.Source.Width < viewport.ActualWidth &&
					Image.Source.Height < viewport.ActualHeight)
				{
					CurrentScaleType = ScaleType.NoScaleSmallerThanViewport;
				}
				else
				{
					CurrentScaleType = ScaleType.NoScaleBiggerThanViewport;
				}
			}
			else
			{
				CurrentScaleType = ScaleType.Factor;
			}
		}



		public bool IsOriginalSize
		{
			get
			{
				return double.IsNaN(Image.Width) || Image.Width == Image.Source.Width;
			}
		}

		internal void Initialize(Grid viewport)
		{
			if (Image.ActualWidth > viewport.ActualWidth ||
				Image.ActualHeight > viewport.ActualHeight)
			{
				CurrentScaleType = ScaleType.FitToViewport;
			}
		}
	}
}
