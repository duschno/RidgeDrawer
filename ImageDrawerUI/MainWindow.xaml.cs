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
		private readonly double scaleFactor = 2;
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
			FillComboBox(Backend, typeof(BackendDrawerBase));
			LinesCount.Text = 50.ToString();
			Stroke.Text = 1.ToString();
			Factor.Text = 30.ToString();
			ChunkSize.Text = 5.ToString();
			GreyLevel.Text = 127.ToString();
			Angle.Text = 0.ToString();
			DrawOnSides.IsChecked = true;
			Backend.SelectedItem = typeof(GDIPlus);
			Method.SelectedItem = ImageDrawer.MethodType.Squiggle;
			LineType.SelectedItem = ImageDrawer.LineType.Curve;

			filename = @"D:\Fraps Pictures\iw3mp 2020-05-06 01-39-35-97 — копия.bmp";
			if (!string.IsNullOrEmpty(filename))
				Render(filename);
		}

		private void FillComboBox(ComboBox comboBox, Type type)
		{
			if (type.IsEnum)
				comboBox.ItemsSource = Enum.GetValues(type);
			if (type.IsAbstract)
				comboBox.ItemsSource = Logic.GetImplementations(type);
			comboBox.SelectedItem = comboBox.Items[0];
		}

		private void Render(string filename)
		{
			NotImplementedLabel.Visibility = Visibility.Collapsed;
			RenderParams param = new RenderParams
			{
				LinesCount = Convert.ToInt32(LinesCount.Text),
				Stroke = Convert.ToInt32(Stroke.Text),
				Factor = Convert.ToInt32(Factor.Text),
				ChunkSize = Convert.ToInt32(ChunkSize.Text),
				GreyLevel = Convert.ToInt32(GreyLevel.Text),
				Angle = Convert.ToInt32(Angle.Text),
				Smoothing = (SmoothingType)Smoothing.Items[Smoothing.SelectedIndex],
				LineType = (LineType)LineType.Items[LineType.SelectedIndex],
				Method = (MethodType)Method.Items[Method.SelectedIndex],
				DrawOnSides = DrawOnSides.IsChecked ?? false,
				FillInside = FillInside.IsChecked ?? false,
				Backend = (Type)Backend.Items[Backend.SelectedIndex]
			};

			Cursor = Cursors.Wait;
			original = ConvertToNativeDpi(new Bitmap(filename));
			try
			{
				processed = ConvertToNativeDpi(Logic.ProcessByFilename(filename, param));
			}
			catch (NotImplementedException)
			{
				NotImplementedLabel.Visibility = Visibility.Visible;
			}

			image.Source = processed;
			image.MaxWidth = image.Source.Width * scaleFactor * 8;
			image.MaxHeight = image.Source.Height * scaleFactor * 8;
			Cursor = Cursors.Arrow;
		}

		private BitmapSource ConvertToNativeDpi(Bitmap bitmap)
		{
			BitmapSource bitmapSource = Logic.BitmapToBitmapSource(bitmap);
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
			string text = GetTextForValidation(sender as TextBox, e);

			Regex regex = new Regex("^[0-9]+$");
			e.Handled = !regex.IsMatch(text);
		}

		private void AngleValidation(object sender, TextCompositionEventArgs e)
		{
			string text = GetTextForValidation(sender as TextBox, e);

			Regex regex = new Regex("^-?[0-9]*$");
			e.Handled = !regex.IsMatch(text);
		}

		private void ColorRangeValidation(object sender, TextCompositionEventArgs e)
		{
			string text = GetTextForValidation(sender as TextBox, e);

			int value;
			e.Handled = !(int.TryParse(text, out value) && value > 0 && value < 256);
		}

		private string GetTextForValidation(TextBox textBox, TextCompositionEventArgs e)
		{
			string text;
			if (textBox.SelectionLength == 0)
				text = textBox.Text + e.Text;
			else
			{
				text = textBox.Text.Substring(0, textBox.SelectionStart) +
					   e.Text +
					   textBox.Text.Substring(textBox.SelectionStart + textBox.SelectionLength);
			}

			return text;
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

			Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog
			{
				Filter = "Image files (*.png) | *.png"
			};

			bool? result = dlg.ShowDialog();
			if (result == true)
				Logic.Save(processed as BitmapSource, dlg.FileName);
		}

		private void ParametersChanged(object sender, SelectionChangedEventArgs e)
		{
			if (filename != null)
				Render(filename);
		}

		private void Image_MouseWheel(object sender, MouseWheelEventArgs e)
		{
			ChangeZoom(e.Delta > 0);
		}

		private void Image_MouseMove(object sender, MouseEventArgs e)
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

		private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			Keyboard.ClearFocus();
			startPos = Mouse.GetPosition(Window);
			Mouse.Capture(image);
		}

		private void Image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
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
			Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog
			{
				Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png, *.bmp) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png; *.bmp"
			};

			bool? result = dlg.ShowDialog();

			if (result == true)
			{
				filename = dlg.FileName;
				Render(filename);
			}
		}

		#endregion

		private void ParametersChanged(object sender, RoutedEventArgs e)
		{
			ParametersChanged(null, null);
		}

		private void ParamChange_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Space)
				e.Handled = true;
		}
	}
}
