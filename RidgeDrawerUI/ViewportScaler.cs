using System;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Media;

namespace RidgeDrawerUI
{
	public enum ScaleType
	{
		/// <summary>
		/// Image is reduced and fits to viewport
		/// Is used only if image original size is bigger then viewport
		/// </summary>
		Reduced,

		/// <summary>
		/// Original size
		/// Image is smaller than viewport and completely visible
		/// </summary>
		OriginalSmallerThanViewport,

		/// <summary>
		/// Original size
		/// Image is bigger than viewport and partly visible
		/// </summary>
		OriginalBiggerThanViewport,

		/// <summary>
		/// Integer scale factor, e.g. 2, 3 and so on
		/// Image is smaller than viewport and completely visible
		/// </summary>
		EnlargedSmallerThanViewport,

		/// <summary>
		/// Integer scale factor, e.g. 2, 3 and so on
		/// Image is bigger than viewport and partly visible
		/// </summary>
		EnlargedBiggerThanViewport
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
					value != ScaleType.EnlargedBiggerThanViewport &&
					value != ScaleType.EnlargedSmallerThanViewport)
					return;

				currentScaleType = value;
				switch (value)
				{
					case ScaleType.Reduced:
						currentFactor = 1;
						image.Width = double.NaN;
						image.Height = double.NaN;
						image.Stretch = Stretch.Uniform;
						RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.Linear);
						break;
					case ScaleType.OriginalSmallerThanViewport:
						currentFactor = 1;
						image.Width = image.Source.Width;
						image.Height = image.Source.Height;
						image.Stretch = Stretch.None;
						RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.Linear);
						break;
					case ScaleType.OriginalBiggerThanViewport:
						currentFactor = 1;
						image.Width = image.Source.Width;
						image.Height = image.Source.Height;
						image.Stretch = Stretch.Uniform;
						RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.Linear);
						break;
					case ScaleType.EnlargedSmallerThanViewport:
					case ScaleType.EnlargedBiggerThanViewport:
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
					CurrentScaleType = ScaleType.EnlargedSmallerThanViewport;
				else
					CurrentScaleType = ScaleType.EnlargedBiggerThanViewport;
			}
		}

		internal void ChangeScale(bool zoomIn)
		{
			if (zoomIn)
			{
				if (CurrentScaleType == ScaleType.Reduced)
					SetOriginalSize();
				else
					ChangeFactor(zoomIn);
			}
			else
			{
				if (CurrentScaleType == ScaleType.Reduced ||
					CurrentScaleType == ScaleType.OriginalSmallerThanViewport)
					return;

				if (CurrentScaleType == ScaleType.EnlargedSmallerThanViewport)
				{
					ChangeFactor(zoomIn);
					return;
				}

				if (image.Width <= image.Source.Width)
					CurrentScaleType = ScaleType.Reduced;
				else
				{
					ChangeFactor(zoomIn);
					if (image.Width < image.Source.Width)
						CurrentScaleType = ScaleType.Reduced;
				}
			}
		}

		internal void SetOriginalSize()
		{
			if (OriginalFitsToViewport)
				CurrentScaleType = ScaleType.OriginalSmallerThanViewport;
			else
				CurrentScaleType = ScaleType.OriginalBiggerThanViewport;
		}

		internal void CheckScale()
		{
			switch (CurrentScaleType)
			{
				case ScaleType.Reduced:
					if (OriginalFitsToViewport)
						SetOriginalSize();
					break;
				case ScaleType.OriginalSmallerThanViewport:
					if (!OriginalFitsToViewport)
						CurrentScaleType = ScaleType.Reduced;
					break;
				case ScaleType.OriginalBiggerThanViewport:
					if (OriginalFitsToViewport)
						CurrentScaleType = ScaleType.OriginalSmallerThanViewport;
					break;
			}
		}

		private bool OriginalFitsToViewport
		{
			get
			{
				return image.Source.Width < viewport.ActualWidth &&
					image.Source.Height < viewport.ActualHeight;
			}
		}

		internal void Initialize()
		{
			image.Width = image.Source.Width;
			image.Height = image.Source.Height;

			if (image.Width > viewport.ActualWidth ||
				image.Height > viewport.ActualHeight)
			{
				CurrentScaleType = ScaleType.Reduced;
			}
			else
			{
				SetOriginalSize();
			}
		}
	}
}
