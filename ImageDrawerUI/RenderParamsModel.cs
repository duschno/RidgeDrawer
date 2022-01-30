using ImageDrawer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace ImageDrawerUI
{
	public class RenderParamsModel : INotifyPropertyChanged
	{
		public RenderParamsModel(RenderParams p, string filename, Action renderAction)
		{
			param = p;
			Filename = filename;
			action = renderAction;
		}

		private Action action;
		private RenderParams param;
		public string Filename { get; set; }
		public RenderParams Param { get { return param; } set { } }

		public Bitmap OriginalBitmap { get; set; }
		public ImageSource Original { get; set; }
		public ImageSource Processed { get; set; }

		public int LinesCount { get { return param.LinesCount; } set { SetField(ref param.LinesCount, value); } }

		public int Stroke { get { return param.Stroke; } set { SetField(ref param.Stroke, value); } }

		public int Factor { get { return param.Factor; } set { SetField(ref param.Factor, value); } }

		public int ChunkSize { get { return param.ChunkSize; } set { SetField(ref param.ChunkSize, value); } }

		public int GreyPoint { get { return param.GreyPoint; } set { SetField(ref param.GreyPoint, value); } }

		public int BlackPoint { get { return param.BlackPoint; } set { SetField(ref param.BlackPoint, value); } }

		public int WhitePoint { get { return param.WhitePoint; } set { SetField(ref param.WhitePoint, value); } }

		public int Angle { get { return param.Angle; } set { SetField(ref param.Angle, value); } }

		public SmoothingType Smoothing { get { return param.Smoothing; } set { SetField(ref param.Smoothing, value); } }

		public LineType LineType { get { return param.LineType; } set { SetField(ref param.LineType, value); } }

		public MethodType Method { get { return param.Method; } set { SetField(ref param.Method, value); } }

		public Type Backend { get { return param.Backend; } set { SetField(ref param.Backend, value); } }

		public bool DrawOnSides { get { return param.DrawOnSides; } set { SetField(ref param.DrawOnSides, value); } }

		public int PointsAroundPeak { get { return param.PointsAroundPeak; } set { SetField(ref param.PointsAroundPeak, value); } }

		public bool FillInside { get { return param.FillInside; } set { SetField(ref param.FillInside, value); } }

		public bool Invert { get { return param.Invert; } set { SetField(ref param.Invert, value); } }

		public bool Debug { get { return param.Debug; } set { SetField(ref param.Debug, value); } }

		public int PullPointX { get { return param.PullPointX; } set { SetField(ref param.PullPointX, value); } }

		public int PullPointY { get { return param.PullPointY; } set { SetField(ref param.PullPointY, value); } }

		#region INotifyPropertyChanged Members

		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Raises this object's PropertyChanged event.
		/// </summary>
		/// <param name="propertyName">The property that has a new value.</param>
		private void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null)
			{
				PropertyChangedEventArgs e = new PropertyChangedEventArgs(propertyName);
				handler(this, e);
			}
		}
		#endregion

		private void SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
		{
			if (EqualityComparer<T>.Default.Equals(field, value))
				return;
			field = value;
			OnPropertyChanged(propertyName);
			action?.Invoke();
		}
	}
}
