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
				if (currentScaleType == value &&
					value != ScaleType.FactorScaledBiggerThanViewport &&
					value != ScaleType.FactorSmallerThanViewport)
					return;

				currentScaleType = value;
				switch (value)
				{
					case ScaleType.FitToViewport:
						CurrentFactor = 1;
						Image.Width = double.NaN;
						Image.Height = double.NaN;
						Image.Stretch = Stretch.Uniform;
						RenderOptions.SetBitmapScalingMode(Image, BitmapScalingMode.Linear);
						break;
					case ScaleType.NonScaledSmallerThanViewport:
						CurrentFactor = 1;
						Image.Width = Image.Source.Width;
						Image.Height = Image.Source.Height;
						Image.Stretch = Stretch.None;
						RenderOptions.SetBitmapScalingMode(Image, BitmapScalingMode.Linear);
						break;
					case ScaleType.NonScaledBiggerThanViewport:
						CurrentFactor = 1;
						Image.Width = Image.Source.Width;
						Image.Height = Image.Source.Height;
						Image.Stretch = Stretch.Uniform;
						RenderOptions.SetBitmapScalingMode(Image, BitmapScalingMode.Linear);
						break;
					case ScaleType.FactorSmallerThanViewport:
					case ScaleType.FactorScaledBiggerThanViewport:
						Image.Width = Image.Source.Width * CurrentFactor;
						Image.Height = Image.Source.Height * CurrentFactor;
						Image.Stretch = Stretch.Uniform;
						RenderOptions.SetBitmapScalingMode(Image, BitmapScalingMode.NearestNeighbor);
						break;
					default:
						break;
				}

				Debug.WriteLine($"{CurrentScaleType}, scale={CurrentFactor}");
			}
		}

		private Image Image { get; set; }
		private Grid Viewport { get; set; }

		public ViewportScaler(Image image, Grid viewport, int maxFactor)
		{
			Image = image;
			Viewport = viewport;
			CurrentFactor = 1;
			MaxFactor = maxFactor;
		}

		public void ChangeFactor(bool zoomIn)
		{
			if (zoomIn && CurrentFactor == MaxFactor)
				return;
			if (!zoomIn && CurrentFactor == 1)
				return;

			CurrentFactor += zoomIn ? 1 : -1;

			if (CurrentFactor == 1)
				SetOriginalSize();
			else
			{
				if (Image.Source.Width * CurrentFactor < Viewport.ActualWidth &&
					Image.Source.Height * CurrentFactor < Viewport.ActualHeight)
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

				if (Image.Width <= Image.Source.Width)
					CurrentScaleType = ScaleType.FitToViewport;
				else
				{
					ChangeFactor(zoomIn);
					if (Image.Width < Image.Source.Width)
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
				return Image.Source.Width < Viewport.ActualWidth &&
					Image.Source.Height < Viewport.ActualHeight;
			}
		}

		private bool IsCurrentSmallerThanViewport
		{
			get
			{
				return Image.Width < Viewport.ActualWidth &&
					Image.Height < Viewport.ActualHeight;
			}
		}

		internal void Initialize()
		{
			Image.Width = Image.Source.Width;
			Image.Height = Image.Source.Height;

			if (Image.ActualWidth > Viewport.ActualWidth ||
				Image.ActualHeight > Viewport.ActualHeight)
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
