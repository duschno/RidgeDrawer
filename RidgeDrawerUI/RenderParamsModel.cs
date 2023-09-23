using RidgeDrawer;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Media;

namespace RidgeDrawerUI
{
	public class RenderParamsModel : INotifyPropertyChanged
	{
		private Action render;

		public RenderParamsModel(RenderParams param, string filename, Action renderAction)
		{
			Param = param;
			DefaultParam = param.Clone();
			Filename = filename;
			render = renderAction;
		}

		public RenderParams Param { get; set; }
		public RenderParams DefaultParam { get; private set; }
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
	}
}
