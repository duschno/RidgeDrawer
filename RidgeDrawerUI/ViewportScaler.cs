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

		public ScaleType CurrentScaleType { get; private set; }

		private Image Image { get; set; }

		public ViewportScaler(Image image, int maxFactor)
		{
			Image = image;
			CurrentFactor = 1;
			MaxFactor = maxFactor;
		}

		private void ChangeScaleType(ScaleType scaleType)
		{
			CurrentScaleType = scaleType;
		}

		public void SetNextFactor(bool zoomIn)
		{
			if (zoomIn && CurrentFactor == MaxFactor)
				return;
			if (!zoomIn && CurrentFactor == 1)
				return;

			CurrentFactor += zoomIn ? 1 : -1;
		}

		private void SetToFitGrid()
		{
			CurrentFactor = 1;
			Image.Width = Image.Height = double.NaN;
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
				if (Image.Stretch == Stretch.None)
					return;

				if (Image.Width <= Image.Source.Width)
					SetToFitGrid();
				else
				{
					SetNextFactor(zoomIn);
					Image.Width = Image.Source.Width * CurrentFactor;
					Image.Height = Image.Source.Height * CurrentFactor;
					if (Image.Width < Image.Source.Width)
						SetToFitGrid();
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
					Image.Stretch = Stretch.None;
				else
					Image.Stretch = Stretch.Uniform;

				RenderOptions.SetBitmapScalingMode(Image, BitmapScalingMode.Linear);
			}
			else
			{
				Image.Stretch = Stretch.Uniform;
				RenderOptions.SetBitmapScalingMode(Image, BitmapScalingMode.NearestNeighbor);
			}
		}



		public double OriginalHeight
		{
			get
			{
				return Image.Source.Height;
			}
		}

		public double OriginalWidth
		{
			get
			{
				return Image.Source.Width;
			}
		}

		public bool IsOriginalSize
		{
			get
			{
				return double.IsNaN(Image.Width) || Image.Width == Image.Source.Width;
			}
		}

		internal bool IsFittingGrid(Grid viewport)
		{
			return Image.ActualWidth <= viewport.ActualWidth &&
					Image.ActualHeight <= viewport.ActualHeight;
		}

		internal void Initialize(Grid viewport)
		{
			if (!IsFittingGrid(viewport))
			{
				Image.Stretch = Stretch.Uniform;
				RenderOptions.SetBitmapScalingMode(Image, BitmapScalingMode.Linear);
			}
		}
	}
}
