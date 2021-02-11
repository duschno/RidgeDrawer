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
using System.Reflection;
using System.Runtime.InteropServices;

namespace preview_test
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{

		double scaleFactor = 2;
		Point startPos;
		Thickness oldMargin;
		public MainWindow()
		{
			InitializeComponent();
			image.Source = ConvertToNativeDpi((BitmapSource)image.Source);
			image.MaxWidth = image.Source.Width * scaleFactor * 8;
			image.MaxHeight = image.Source.Height * scaleFactor * 8;
		}

		BitmapSource ConvertToNativeDpi(BitmapSource bitmapSource)
		{
			DpiScale dpiScale = VisualTreeHelper.GetDpi(this);
			int width = bitmapSource.PixelWidth;
			int height = bitmapSource.PixelHeight;

			int stride = width * bitmapSource.Format.BitsPerPixel;
			byte[] pixelData = new byte[stride * height];
			bitmapSource.CopyPixels(pixelData, stride, 0);

			return BitmapSource.Create(width, height,
				dpiScale.PixelsPerInchX, dpiScale.PixelsPerInchY,
				bitmapSource.Format, bitmapSource.Palette,
				pixelData, stride);
		}

		bool IsFitsGrid
		{
			get
			{
				return image.ActualWidth <= ImageGrid.ActualWidth &&
					image.ActualHeight <= ImageGrid.ActualHeight;
			}
		}

		private bool IsNonScaled
		{
			get
			{
				return double.IsNaN(image.Width) || image.Source.Width == image.Width;
			}
		}

		private void SetNonScaled()
		{
			image.Width = image.Height = double.NaN;
		}

		private void ChangeScale(bool zoomIn)
		{
			if (zoomIn)
			{
				image.Width = image.ActualWidth * scaleFactor;
				image.Height = image.ActualHeight * scaleFactor;
			}
			else
			{
				if (IsNonScaled)
					return;

				if (image.Width <= image.Source.Width)
					SetNonScaled();
				else
				{
					image.Width = image.ActualWidth / scaleFactor;
					image.Height = image.ActualHeight / scaleFactor;
					if (image.Width <= image.Source.Width)
						SetNonScaled();
				}

				if (!IsNonScaled)
					image.Margin = CheckBoundaries(image.Margin);
			}
		}

		private void ChangeZoom(bool zoomIn)
		{
			ChangeScale(zoomIn);
			ChangeUIProps();
			Debug.WriteLine("width = {0}, height = {1}, ac. width = {2}, ac. height = {3}", 
				image.Width, image.Height, image.ActualWidth, image.ActualHeight);
		}

		private void ChangeUIProps()
		{
			if (IsNonScaled)
			{
				if (image.Source.Width < ImageGrid.ActualWidth &&
					image.Source.Height < ImageGrid.ActualHeight)
					image.Stretch = Stretch.None;
				else
					image.Stretch = Stretch.Uniform;

				ImageGrid.Cursor = Cursors.Arrow;
				RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.HighQuality);
				image.Margin = new Thickness();
				oldMargin = new Thickness();
			}
			else
			{
				if (image.Width <= ImageGrid.ActualWidth &&
					image.Height <= ImageGrid.ActualHeight)
				{
					image.Margin = new Thickness();
					oldMargin = new Thickness();
					ImageGrid.Cursor = Cursors.Arrow;
				}
				else
				{
					ImageGrid.Cursor = Cursors.SizeAll;
				}

				image.Stretch = Stretch.Uniform;
				RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.NearestNeighbor);
			}
		}

		private void image_MouseWheel(object sender, MouseWheelEventArgs e)
		{
			ChangeZoom(e.Delta > 0);
		}

		private Thickness CheckBoundaries(Thickness margin)
		{
			double widthOver = ImageGrid.ActualWidth - image.Width;
			double heightOver = ImageGrid.ActualHeight - image.Height;

			if (widthOver >= 0 || margin.Left > 0)
				margin.Left = 0;
			if (heightOver >= 0 || margin.Top > 0)
				margin.Top = 0;
			if (widthOver < 0 && margin.Left < widthOver)
				margin.Left = widthOver;
			if (heightOver < 0 && margin.Top < heightOver)
				margin.Top = heightOver;

			return margin;
		}

		private void image_MouseMove(object sender, MouseEventArgs e)
		{
			if (e.LeftButton == MouseButtonState.Pressed && !IsNonScaled)
			{
				Point newPos = Mouse.GetPosition(Window);
				Thickness margin = image.Margin;
				if (ImageGrid.ActualHeight < image.ActualHeight)
					margin.Top = oldMargin.Top - (startPos - newPos).Y;
				if (ImageGrid.ActualWidth < image.ActualWidth)
					margin.Left = oldMargin.Left - (startPos - newPos).X;

				image.Margin = CheckBoundaries(margin);

				Debug.WriteLine("* margin.left = {0}, margin.top = {1}", image.Margin.Left, image.Margin.Top);
				Debug.WriteLine("* width = {0}, height = {1}, ac. width = {2}, ac. height = {3}",
					image.Width, image.Height, image.ActualWidth, image.ActualHeight);
				Debug.WriteLine("* grid.width = {0}, grid.height = {1}, grid.ac. width = {2}, grid.ac. height = {3}\n",
					ImageGrid.Width, ImageGrid.Height, ImageGrid.ActualWidth, ImageGrid.ActualHeight);
			}
		}

		private void image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			startPos = Mouse.GetPosition(Window);
			Mouse.Capture(image);
		}

		private void image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			oldMargin = image.Margin;
			Mouse.Capture(null);
		}

		private void Window_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Add || e.Key == Key.OemPlus)
				ChangeZoom(true);
			if (e.Key == Key.Subtract || e.Key == Key.OemMinus)
				ChangeZoom(false);
		}

		private void ImageGrid_Loaded(object sender, RoutedEventArgs e)
		{
			if (!IsFitsGrid)
			{
				image.Stretch = Stretch.Uniform;
				RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.HighQuality);
			}
		}

		private void ImageGrid_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			if (ImageGrid.IsLoaded)
				ChangeUIProps();
		}

		private void button_Click(object sender, RoutedEventArgs e)
		{

		}
	}
}
