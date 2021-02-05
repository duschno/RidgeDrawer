using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Diagnostics;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace preview_test
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{

		double scaleFactor = 2;
		Point startPos;
		Point endPos;
		Thickness oldMargin;
		public MainWindow()
		{
			InitializeComponent();
		}

		private void image_MouseWheel(object sender, MouseWheelEventArgs e)
		{
			if (e.Delta > 0)
			{
				image.Width = image.ActualWidth * scaleFactor;
				image.Height = image.ActualHeight * scaleFactor;
			}
			else
			{
				if (NoScale)
					return;

				if (image.Width <= ((Grid)sender).ActualWidth)
					image.Width = image.Height = double.NaN;
				else
				{
					image.Width = image.ActualWidth / scaleFactor;
					image.Height = image.ActualHeight / scaleFactor;
					if (image.Width <= ((Grid)sender).ActualWidth)
						image.Width = image.Height = double.NaN;
				}
				
			}

			if (NoScale)
			{
				((Grid)sender).Cursor = Cursors.Arrow;
				RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.Linear);
				image.Margin = new Thickness();
				oldMargin = new Thickness();
			}
			else
			{
				((Grid)sender).Cursor = Cursors.Hand;
				RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.NearestNeighbor);
			}
		}

		private void image_MouseMove(object sender, MouseEventArgs e)
		{
			if (e.LeftButton == MouseButtonState.Pressed && !NoScale)
			{
				var c = Mouse.GetPosition(Window);

				Thickness margin = image.Margin;
				margin.Left = oldMargin.Left - (startPos - c).X;
				margin.Top = oldMargin.Top - (startPos - c).Y;

				if (margin.Left > 0) margin.Left = 0;
				if (margin.Top > 0) margin.Top = 0;
				if (margin.Top < -image.Height) margin.Top = -image.Height;
				if (margin.Left < -image.Width) margin.Left = -image.Width;
				image.Margin = margin;

				Debug.WriteLine(image.Margin.Left + " " + image.Margin.Top);
			}
		}

		private void image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			startPos = Mouse.GetPosition(Window);
		}

		private void image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			oldMargin = image.Margin;
			endPos = Mouse.GetPosition(Window);
		}

		private bool NoScale
		{
			get
			{
				return double.IsNaN(image.Width);
			}
		}
	}
}
