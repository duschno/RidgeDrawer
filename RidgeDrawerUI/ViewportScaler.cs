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

		public ViewportScaler(int maxFactor)
		{
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
	}
}
