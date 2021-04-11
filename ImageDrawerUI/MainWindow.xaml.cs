using ImageDrawer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Drawing.Drawing2D;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Reflection;
using System.Drawing;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.IO;

namespace ImageDrawerUI
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		string filename;
		ImageSource original;
		ImageSource processed;
		double scaleFactor = 2;
		System.Windows.Point startPos;
		Thickness oldMargin;

		public MainWindow()
		{
			InitializeComponent();
			RenderParams param = new RenderParams { };
			//FieldInfo[] amountField = param.GetType().GetFields();
			//foreach (var item in amountField)
			//{
			//	var someVar = Activator.CreateInstance(item.GetType(), item.GetValue(param));
			//}
			Smoothingcb.ItemsSource = Enum.GetValues(typeof(SmoothingMode)).Cast<SmoothingMode>();
			Smoothingcb.SelectedItem = Smoothingcb.Items[3];
			LineTypecb.ItemsSource = Enum.GetValues(typeof(RenderType)).Cast<RenderType>();
			LineTypecb.SelectedItem = LineTypecb.Items[0];
			Methodcb.ItemsSource = Enum.GetValues(typeof(RenderMethod)).Cast<RenderMethod>();
			Methodcb.SelectedItem = Methodcb.Items[0];

		}

		private void button_Click(object sender, RoutedEventArgs e)
		{
			Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
			dlg.Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png, *.bmp) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png; *.bmp";

			bool? result = dlg.ShowDialog();

			if (result == true)
			{
				filename = dlg.FileName;
				RenderOnUI(filename);
			}
		}

		private void RenderOnUI(string filename)
		{
			RenderParams param = new RenderParams
			{
				LinesCount = Convert.ToInt32(linescounttb.Text),
				Width = Convert.ToInt32(Widthtb.Text),
				Factor = Convert.ToInt32(Factortb.Text),
				ChunkSize = Convert.ToInt32(ChunkSizetb.Text),
				Smoothing = (SmoothingMode)Smoothingcb.Items[Smoothingcb.SelectedIndex],
				LineType = (RenderType)LineTypecb.Items[LineTypecb.SelectedIndex],
				Method = (RenderMethod)Methodcb.Items[Methodcb.SelectedIndex]
			};

			Cursor = Cursors.Wait;
			original = ConvertToNativeDpi(new Bitmap(filename));
			processed = ConvertToNativeDpi(Program.DrawUI(filename, param));
			image.Source = processed;
			image.MaxWidth = image.Source.Width * scaleFactor * 8;
			image.MaxHeight = image.Source.Height * scaleFactor * 8;
			Cursor = Cursors.Arrow;
		}

		BitmapSource ConvertToNativeDpi(Bitmap bitmap)
		{
			BitmapSource bitmapSource = Program.ImageSourceFromBitmap(bitmap);
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

		private void Grid_GiveFeedback(object sender, GiveFeedbackEventArgs e)
		{

		}

		private void comparebuton_Click(object sender, RoutedEventArgs e)
		{
			var t = original;
			original = image.Source;
			image.Source = t;
		}

		private void savebuton_Click(object sender, RoutedEventArgs e)
		{
			if (processed == null)
				return;

			Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
			dlg.Filter = "Image files (*.bmp) | *.bmp";

			bool? result = dlg.ShowDialog();

			if (result == true)
			{
				filename = dlg.FileName;
				using (var fileStream = new FileStream(filename, FileMode.Create))
				{
					BitmapEncoder encoder = new PngBitmapEncoder();
					encoder.Frames.Add(BitmapFrame.Create(processed as BitmapSource));
					encoder.Save(fileStream);
				}
			}
		}

		private void Smoothingcb_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (filename != null)
				RenderOnUI(filename);
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

			if (image.Source == null)
				return;

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
			//Debug.WriteLine("width = {0}, height = {1}, ac. width = {2}, ac. height = {3}",
			//	image.Width, image.Height, image.ActualWidth, image.ActualHeight);
		}

		private void ChangeUIProps()
		{
			if (image.Source == null)
				return;

			if (IsNonScaled)
			{
				if (image.Source.Width < ImageGrid.ActualWidth &&
					image.Source.Height < ImageGrid.ActualHeight)
					image.Stretch = Stretch.None;
				else
					image.Stretch = Stretch.Uniform;

				ImageGrid.Cursor = Cursors.Arrow;
				RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.Linear);
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
			if ((e.LeftButton == MouseButtonState.Pressed || e.MiddleButton == MouseButtonState.Pressed) && !IsNonScaled)
			{
				System.Windows.Point newPos = Mouse.GetPosition(Window);
				Thickness margin = image.Margin;
				if (ImageGrid.ActualHeight < image.ActualHeight)
					margin.Top = oldMargin.Top - (startPos - newPos).Y;
				if (ImageGrid.ActualWidth < image.ActualWidth)
					margin.Left = oldMargin.Left - (startPos - newPos).X;

				image.Margin = CheckBoundaries(margin);

				//Debug.WriteLine("* margin.left = {0}, margin.top = {1}", image.Margin.Left, image.Margin.Top);
				//Debug.WriteLine("* width = {0}, height = {1}, ac. width = {2}, ac. height = {3}",
				//	image.Width, image.Height, image.ActualWidth, image.ActualHeight);
				//Debug.WriteLine("* grid.width = {0}, grid.height = {1}, grid.ac. width = {2}, grid.ac. height = {3}\n",
				//	ImageGrid.Width, ImageGrid.Height, ImageGrid.ActualWidth, ImageGrid.ActualHeight);
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
			if (e.Key == Key.O)
				button_Click(null, null);
			if (e.Key == Key.C)
				comparebuton_Click(null, null);
			if (e.Key == Key.S)
				savebuton_Click(null, null);
			if (e.Key == Key.D0 || e.Key == Key.NumPad0 || e.Key == Key.F)
				SetFullsize();
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
				RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.Linear);
			}
		}

		private void ImageGrid_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			if (ImageGrid.IsLoaded)
				ChangeUIProps();
		}

		private void FullsizeButton_Click(object sender, RoutedEventArgs e)
		{
			SetFullsize();
		}

		private void SetFullsize()
		{
			image.Width = image.Source.Width;
			image.Height = image.Source.Height;
			ChangeUIProps();
		}

		private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
		{
			Regex regex = new Regex("[^0-9]+");
			e.Handled = regex.IsMatch(e.Text);
		}

		private void paramchange_KeyDown(object sender, KeyEventArgs e)
		{
			int t;
			if (e.Key != Key.Enter)
				return;
			if (string.IsNullOrWhiteSpace(((TextBox)e.Source).Text) || !int.TryParse(((TextBox)e.Source).Text, out t))
				return;
			if (filename != null)
				RenderOnUI(filename);
		}
	}
}
