using ImageDrawer;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.Text.RegularExpressions;

namespace ImageDrawerUI
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private string filename;
		private ImageSource original;
		private ImageSource processed;
		private double scaleFactor = 2;
		private System.Windows.Point startPos;
		private Thickness oldMargin;

		private bool IsFitsGrid
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

		public MainWindow()
		{
			InitializeComponent();

			FillComboBox(Smoothing, typeof(SmoothingType));
			FillComboBox(LineType, typeof(LineType));
			FillComboBox(Method, typeof(MethodType));
			FillComboBox(Backend, typeof(BackendType));
			LinesCount.Text = 1.ToString();
			Stroke.Text = 1.ToString();
			Factor.Text = 30.ToString();
			ChunkSize.Text = 5.ToString();
			Angle.Text = 0.ToString();
			Backend.SelectedIndex += 2;
		}

		private void FillComboBox(ComboBox comboBox, Type type)
		{
			comboBox.ItemsSource = Enum.GetValues(type);
			comboBox.SelectedItem = comboBox.Items[0];
		}

		private void Render(string filename)
		{
			RenderParams param = new RenderParams
			{
				LinesCount = Convert.ToInt32(LinesCount.Text),
				Stroke = Convert.ToInt32(Stroke.Text),
				Factor = Convert.ToInt32(Factor.Text),
				ChunkSize = Convert.ToInt32(ChunkSize.Text),
				Angle = Convert.ToInt32(Angle.Text),
				Smoothing = (SmoothingType)Smoothing.Items[Smoothing.SelectedIndex],
				LineType = (LineType)LineType.Items[LineType.SelectedIndex],
				Method = (MethodType)Method.Items[Method.SelectedIndex],
				Backend = (BackendType)Backend.Items[Backend.SelectedIndex]
			};

			Cursor = Cursors.Wait;
			original = ConvertToNativeDpi(new Bitmap(filename));
			processed = ConvertToNativeDpi(Program.ProcessByFilename(filename, param));
			image.Source = processed;
			image.MaxWidth = image.Source.Width * scaleFactor * 8;
			image.MaxHeight = image.Source.Height * scaleFactor * 8;
			Cursor = Cursors.Arrow;
		}

		private BitmapSource ConvertToNativeDpi(Bitmap bitmap)
		{
			BitmapSource bitmapSource = Program.BitmapToBitmapSource(bitmap);
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

		private void SetFullsize()
		{
			if (image.Source == null)
				return;

			image.Width = image.Source.Width;
			image.Height = image.Source.Height;
			ChangeUIProps();
		}

		private void PositiveNumberValidation(object sender, TextCompositionEventArgs e)
		{
			Regex regex = new Regex("[^0-9]+");
			e.Handled = regex.IsMatch(e.Text);
		}

		private void AngleValidation(object sender, TextCompositionEventArgs e)
		{
			Regex regex = new Regex("[\\-0-9]+");
			e.Handled = !regex.IsMatch(e.Text);
		}

		#region Event handlers

		private void Compare_Click(object sender, RoutedEventArgs e)
		{
			ImageSource temp = original;
			original = image.Source;
			image.Source = temp;
		}

		private void Save_Click(object sender, RoutedEventArgs e)
		{
			if (processed == null)
				return;

			Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
			dlg.Filter = "Image files (*.bmp) | *.bmp";

			bool? result = dlg.ShowDialog();
			if (result == true)
				Program.Save(processed as BitmapSource, dlg.FileName);
		}

		private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (filename != null)
				Render(filename);
		}

		private void image_MouseWheel(object sender, MouseWheelEventArgs e)
		{
			ChangeZoom(e.Delta > 0);
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
			Keyboard.ClearFocus();
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
			if (e.OriginalSource is TextBox &&
				!(e.Key == Key.O || e.Key == Key.C || e.Key == Key.S))
				return;

			if (e.Key == Key.O)
				Open_Click(null, null);
			if (e.Key == Key.C)
				Compare_Click(null, null);
			if (e.Key == Key.S)
				Save_Click(null, null);
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

		private void ParamChange_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key != Key.Enter)
				return;
			if (string.IsNullOrWhiteSpace(((TextBox)e.Source).Text) ||
				!int.TryParse(((TextBox)e.Source).Text, out _))
				return;
			if (filename != null)
				Render(filename);
		}
		private void Open_Click(object sender, RoutedEventArgs e)
		{
			Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
			dlg.Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png, *.bmp) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png; *.bmp";

			bool? result = dlg.ShowDialog();

			if (result == true)
			{
				filename = dlg.FileName;
				Render(filename);
			}
		}

		#endregion
	}
}
