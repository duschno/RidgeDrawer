
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace RidgeDrawerUI
{
	enum ViewportScale
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
		public int CurrentFactor { get; set; }
		public int MaxFactor { get; private set; }

		private Image image;

		public ViewportScaler(Image image, int maxFactor)
		{
			this.image = image;
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

		private void SetToFitGrid()
		{
			CurrentFactor = 1;
			image.Width = image.Height = double.NaN;
		}

		internal void ChangeScale(bool zoomIn)
		{
			if (image.Source == null)
				return;

			if (zoomIn)
			{
				if (image.ActualWidth < image.Source.Width)
				{
					image.Width = image.Source.Width;
					image.Height = image.Source.Height;
				}
				else
				{
					SetNextFactor(zoomIn);
					image.Width = image.Source.Width * CurrentFactor;
					image.Height = image.Source.Height * CurrentFactor;
				}
			}
			else
			{
				if (image.Stretch == Stretch.None)
					return;

				if (image.Width <= image.Source.Width)
					SetToFitGrid();
				else
				{
					SetNextFactor(zoomIn);
					image.Width = image.Source.Width * CurrentFactor;
					image.Height = image.Source.Height * CurrentFactor;
					if (image.Width < image.Source.Width)
						SetToFitGrid();
				}
			}
		}

		internal void SetOriginalSize()
		{
			CurrentFactor = 1;
			image.Width = image.Source.Width;
			image.Height = image.Source.Height;
		}

		internal void ChangeUIProps(Grid viewport)
		{
			if (IsOriginalSize)
			{
				if (image.Source.Width < viewport.ActualWidth &&
					image.Source.Height < viewport.ActualHeight)
					image.Stretch = Stretch.None;
				else
					image.Stretch = Stretch.Uniform;

				RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.Linear);
			}
			else
			{
				image.Stretch = Stretch.Uniform;
				RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.NearestNeighbor);
			}
		}

		public double OriginalHeight
		{
			get
			{
				return image.Source.Height;
			}
		}

		public double OriginalWidth
		{
			get
			{
				return image.Source.Width;
			}
		}

		public bool IsOriginalSize
		{
			get
			{
				return double.IsNaN(image.Width) || image.Width == image.Source.Width;
			}
		}

		internal bool IsFittingGrid(Grid viewport)
		{
			return image.ActualWidth <= viewport.ActualWidth &&
					image.ActualHeight <= viewport.ActualHeight;
		}

		internal void Initialize(Grid viewport)
		{
			if (!IsFittingGrid(viewport))
			{
				image.Stretch = Stretch.Uniform;
				RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.Linear);
			}
		}
	}
}
