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
	////public static class Render
	////{
	////	static Render()
	////	{
	////		var flags = BindingFlags.NonPublic | BindingFlags.Static;
	////		var dpiProperty = typeof(SystemParameters).GetProperty("Dpi", flags);

	////		Dpi = (int)dpiProperty.GetValue(null, null);
	////		PixelSize = 96.0 / Dpi;
	////	}

	////	//Размер физического пикселя в виртуальных единицах
	////	public static double PixelSize { get; private set; }

	////	//Текущее разрешение
	////	public static int Dpi { get; private set; }
	////}


	////public class StaticImage : Image
	////{
	////	static StaticImage()
	////	{
	////		//Отслеживание смены исходной картинки
	////		Image.SourceProperty.OverrideMetadata(
	////			typeof(StaticImage),
	////			new FrameworkPropertyMetadata(SourceChanged));
	////	}

	////	private static void SourceChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
	////	{
	////		var image = obj as StaticImage;
	////		if (image == null) return;

	////		//Поправка размера картинки под текущее разрешение
	////		image.Width = image.Source.Width * Render.PixelSize;
	////		image.Height = image.Source.Height * Render.PixelSize;
	////	}
	////}


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
			image.Source = PixelToPixelConvert((BitmapSource)image.Source);
			
		}

		BitmapSource PixelToPixelConvert(BitmapSource bitmapSource)
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

		bool NeedToUniform
		{
			get
			{
				return double.IsNaN(image.Width) &&
					(image.ActualWidth > ImageGrid.ActualWidth || image.ActualHeight > ImageGrid.ActualHeight);
			}
		}

		private void ChangeZoom(bool zoomIn)
		{
			if (zoomIn)
			{
				var c = VisualTreeHelper.GetDpi(this);
				image.Width = image.ActualWidth * scaleFactor;
				image.Height = image.ActualHeight * scaleFactor;
			}
			else
			{
				if (NoScale)
					if (NeedToUniform)
					{
						image.Stretch = Stretch.Uniform;
						RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.HighQuality);
						return;
					}
					else
						return;

				if (image.Width <= ImageGrid.ActualWidth || image.Height <= ImageGrid.ActualHeight)
					image.Width = image.Height = double.NaN;
				else
				{
					image.Width = image.Width / scaleFactor;
					image.Height = image.Height / scaleFactor;
					if (image.Width <= ImageGrid.ActualWidth)
						image.Width = image.Height = double.NaN;
				}
				image.Stretch = NeedToUniform ? Stretch.Uniform : Stretch.None;
			}

			if (NoScale)
			{
				image.Stretch = NeedToUniform ? Stretch.Uniform : Stretch.None;
				ImageGrid.Cursor = Cursors.Arrow;
				RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.Linear);
				image.Margin = new Thickness();
				oldMargin = new Thickness();
			}
			else
			{
				image.Stretch = Stretch.Uniform;
				ImageGrid.Cursor = Cursors.SizeAll;
				RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.NearestNeighbor);
			}

			Debug.WriteLine("width = {0}, height = {1}, ac. width = {2}, ac. height = {3}", 
				image.Width, image.Height, image.ActualWidth, image.ActualHeight);
		}

		private void image_MouseWheel(object sender, MouseWheelEventArgs e)
		{
			ChangeZoom(e.Delta > 0);
		}

		private void image_MouseMove(object sender, MouseEventArgs e)
		{
			if (e.LeftButton == MouseButtonState.Pressed && !NoScale)
			{
				Point newPos = Mouse.GetPosition(Window);

				Thickness margin = image.Margin;
				if (ImageGrid.ActualHeight < image.ActualHeight)
					margin.Top = oldMargin.Top - (startPos - newPos).Y;
				if (ImageGrid.ActualWidth < image.ActualWidth)
					margin.Left = oldMargin.Left - (startPos - newPos).X;

				if (margin.Left > 0)
					margin.Left = 0;
				if (margin.Top > 0)
					margin.Top = 0;
				if (ImageGrid.ActualHeight < image.ActualHeight && 
					margin.Top < ImageGrid.ActualHeight - image.ActualHeight)
					margin.Top = ImageGrid.ActualHeight - image.ActualHeight;
				if (ImageGrid.ActualWidth < image.ActualWidth && 
					margin.Left < ImageGrid.ActualWidth - image.ActualWidth)
					margin.Left = ImageGrid.ActualWidth - image.ActualWidth;
				image.Margin = margin;

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

		private bool NoScale
		{
			get
			{
				return double.IsNaN(image.Width);
			}
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
			if (NeedToUniform)
			{
				image.Stretch = Stretch.Uniform;
				RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.HighQuality);
			}
		}
	}
}
