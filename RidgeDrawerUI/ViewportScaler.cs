using System;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Media;

namespace RidgeDrawerUI
{
	public enum ScaleType
	{
		/// <summary>
		/// Scale is less than 100%, image is fit to viewport.
		/// Is applied only if image 100% size is bigger then viewport
		/// </summary>
		FitToViewport,

		/// <summary>
		/// Scale is 100%
		/// Image is smaller than viewport and completely visible
		/// </summary>
		NonScaledSmallerThanViewport,

		/// <summary>
		/// Scale is 100%
		/// Image is bigger than viewport and partly visible
		/// </summary>
		NonScaledBiggerThanViewport,

		/// <summary>
		/// Integer scale factor, e.g. 2, 3 and so on. 
		///Image is smaller than viewport and completely visible
		/// </summary>
		FactorSmallerThanViewport,

		/// <summary>
		/// Integer scale factor, e.g. 2, 3 and so on.
		/// Image is bigger than viewport and partly visible
		/// </summary>
		FactorScaledBiggerThanViewport
	}

	public class ViewportScaler
	{
		private int currentFactor;
		private int maxFactor;
		private Image image;
		private Grid viewport;
		private ScaleType currentScaleType;

		public ScaleType CurrentScaleType
		{
			get
			{
				return currentScaleType;
			}

			private set
			{
				if (currentScaleType == value &&
					value != ScaleType.FactorScaledBiggerThanViewport &&
					value != ScaleType.FactorSmallerThanViewport)
					return;

				currentScaleType = value;
				switch (value)
				{
					case ScaleType.FitToViewport:
						currentFactor = 1;
						image.Width = double.NaN;
						image.Height = double.NaN;
						image.Stretch = Stretch.Uniform;
						RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.Linear);
						break;
					case ScaleType.NonScaledSmallerThanViewport:
						currentFactor = 1;
						image.Width = image.Source.Width;
						image.Height = image.Source.Height;
						image.Stretch = Stretch.None;
						RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.Linear);
						break;
					case ScaleType.NonScaledBiggerThanViewport:
						currentFactor = 1;
						image.Width = image.Source.Width;
						image.Height = image.Source.Height;
						image.Stretch = Stretch.Uniform;
						RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.Linear);
						break;
					case ScaleType.FactorSmallerThanViewport:
					case ScaleType.FactorScaledBiggerThanViewport:
						image.Width = image.Source.Width * currentFactor;
						image.Height = image.Source.Height * currentFactor;
						image.Stretch = Stretch.Uniform;
						RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.NearestNeighbor);
						break;
					default:
						break;
				}
			}
		}

		public ViewportScaler(Image image, Grid viewport, int maxFactor)
		{
			this.image = image;
			this.viewport = viewport;
			this.maxFactor = maxFactor;
			currentFactor = 1;
		}

		public void ChangeFactor(bool zoomIn)
		{
			if (zoomIn && currentFactor == maxFactor)
				return;
			if (!zoomIn && currentFactor == 1)
				return;

			currentFactor += zoomIn ? 1 : -1;

			if (currentFactor == 1)
				SetOriginalSize();
			else
			{
				if (image.Source.Width * currentFactor < viewport.ActualWidth &&
					image.Source.Height * currentFactor < viewport.ActualHeight)
					CurrentScaleType = ScaleType.FactorSmallerThanViewport;
				else
					CurrentScaleType = ScaleType.FactorScaledBiggerThanViewport;
			}
		}

		internal void ChangeScale(bool zoomIn)
		{
			if (zoomIn)
			{
				if (CurrentScaleType == ScaleType.FitToViewport)
					SetOriginalSize();
				else
					ChangeFactor(zoomIn);
			}
			else
			{
				if (CurrentScaleType == ScaleType.FitToViewport ||
					CurrentScaleType == ScaleType.NonScaledSmallerThanViewport)
					return;

				if (CurrentScaleType == ScaleType.FactorSmallerThanViewport)
				{
					ChangeFactor(zoomIn);
					return;
				}

				if (image.Width <= image.Source.Width)
					CurrentScaleType = ScaleType.FitToViewport;
				else
				{
					ChangeFactor(zoomIn);
					if (image.Width < image.Source.Width)
						CurrentScaleType = ScaleType.FitToViewport;
				}
			}
		}

		internal void SetOriginalSize()
		{
			if (IsOriginalSmallerThanViewport)
				CurrentScaleType = ScaleType.NonScaledSmallerThanViewport;
			else
				CurrentScaleType = ScaleType.NonScaledBiggerThanViewport;
		}

		internal void CheckScale()
		{
			switch (CurrentScaleType)
			{
				case ScaleType.FitToViewport:
					if (IsOriginalSmallerThanViewport)
						SetOriginalSize();
					break;
				case ScaleType.NonScaledSmallerThanViewport:
					if (!IsCurrentSmallerThanViewport)
						CurrentScaleType = ScaleType.FitToViewport;
					break;
				case ScaleType.NonScaledBiggerThanViewport:
					if (IsCurrentSmallerThanViewport)
						CurrentScaleType = ScaleType.NonScaledSmallerThanViewport;
					break;
			}
		}

		private bool IsOriginalSmallerThanViewport
		{
			get
			{
				return image.Source.Width < viewport.ActualWidth &&
					image.Source.Height < viewport.ActualHeight;
			}
		}

		private bool IsCurrentSmallerThanViewport
		{
			get
			{
				return image.Width < viewport.ActualWidth &&
					image.Height < viewport.ActualHeight;
			}
		}

		internal void Initialize()
		{
			image.Width = image.Source.Width;
			image.Height = image.Source.Height;

			if (image.ActualWidth > viewport.ActualWidth ||
				image.ActualHeight > viewport.ActualHeight)
			{
				CurrentScaleType = ScaleType.FitToViewport;
			}
			else
			{
				SetOriginalSize();
			}
		}
	}
}
