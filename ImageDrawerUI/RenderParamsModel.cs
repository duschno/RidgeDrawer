using ImageDrawer;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Media;

namespace ImageDrawerUI
{
	public class RenderParamsModel : INotifyPropertyChanged
	{
		public RenderParamsModel(RenderParams param, string filename, Action renderAction)
		{
			Param = param;
			Filename = filename;
			render = renderAction;
		}

		private Action render;
		public RenderParams Param { get; set; }
		public string Filename { get; set; }
		public Bitmap OriginalBitmap { get; set; }
		public ImageSource Original { get; set; }
		public ImageSource Processed { get; set; }

		#region INotifyPropertyChanged Members

		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Raises this object's PropertyChanged event.
		/// </summary>
		/// <param name="propertyName">The property that has a new value.</param>
		private void OnPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
			{
				PropertyChangedEventArgs e = new PropertyChangedEventArgs(propertyName);
				PropertyChanged(this, e);
			}
		}
		#endregion

		public void UpdateView()
		{
			OnPropertyChanged(nameof(Param));
			render?.Invoke();
		}

		public static explicit operator LogicParams(RenderParamsModel model)
		{
			return new LogicParams { InputFilename = model.Filename, RenderParams = model.Param };
		}
	}
}
