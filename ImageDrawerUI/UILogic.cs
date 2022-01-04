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
	public partial class MainWindow : Window
	{
		private string filename;
		private ImageSource original;
		private ImageSource processed;
		private int scaleFactor = 1;
		private readonly int maxScaleFactor = 8;
		private System.Windows.Point startPos;
		private Thickness oldMargin;
		private RenderParams param;

		private bool IsFittingGrid
		{
			get
			{
				return image.ActualWidth <= ImageGrid.ActualWidth &&
					image.ActualHeight <= ImageGrid.ActualHeight;
			}
		}

		private bool IsOriginalSize
		{
			get
			{
				return double.IsNaN(image.Width) || OriginalWidth == image.Width;
			}
		}

		public MainWindow()
		{
			InitializeComponent();

			Height = SystemParameters.PrimaryScreenHeight * 0.9;
			Width = SystemParameters.PrimaryScreenWidth * 0.9;
			FillComboBox(Smoothing, typeof(SmoothingType));
			FillComboBox(LineType, typeof(LineType));
			FillComboBox(Method, typeof(MethodType));
			FillComboBox(Backend, typeof(BackendDrawerBase));
			LinesCount.Text = 50.ToString();
			Stroke.Text = 1.ToString();
			Factor.Text = 30.ToString();
			ChunkSize.Text = 5.ToString();
			BlackPoint.Text = 0.ToString();
			WhitePoint.Text = 255.ToString();
			Angle.Text = 0.ToString();
			DrawOnSides.IsChecked = true;
			Backend.SelectedItem = typeof(GDIPlus);
			Method.SelectedItem = ImageDrawer.MethodType.Ridge;
			LineType.SelectedItem = ImageDrawer.LineType.Curve;

			filename = @"..\soldier.png";
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
			param = new RenderParams
			{
				LinesCount = Convert.ToInt32(LinesCount.Text),
				Stroke = Convert.ToInt32(Stroke.Text),
				Factor = Convert.ToInt32(Factor.Text),
				ChunkSize = Convert.ToInt32(ChunkSize.Text),
				BlackPoint = Convert.ToInt32(BlackPoint.Text),
				WhitePoint = Convert.ToInt32(WhitePoint.Text),
				Angle = Convert.ToInt32(Angle.Text),
				Smoothing = (SmoothingType)Smoothing.Items[Smoothing.SelectedIndex],
				LineType = (LineType)LineType.Items[LineType.SelectedIndex],
				Method = (MethodType)Method.Items[Method.SelectedIndex],
				DrawOnSides = DrawOnSides.IsChecked ?? false,
				FillInside = FillInside.IsChecked ?? false,
				Invert = Invert.IsChecked ?? false,
				Debug = Debug.IsChecked ?? false,
				Backend = (Type)Backend.Items[Backend.SelectedIndex]
			};

			Cursor = Cursors.Wait;
			original = ConvertToNativeDpi(new Bitmap(filename));
			try
			{
				processed = ConvertToNativeDpi(Logic.ProcessByFilename(filename, param));
			}
			catch (NotImplementedException e)
			{
				NotImplementedLabel.Content = e.Message;
				NotImplementedLabel.Visibility = Visibility.Visible;
			}

			image.Source = processed;
			image.MaxWidth = OriginalWidth * maxScaleFactor;
			image.MaxHeight = OriginalHeight * maxScaleFactor;
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

		/// <summary>
		/// Calculates the next scale factor
		/// </summary>
		/// <param name="zoomIn">Is zoom in or zoom out happened</param>
		/// <param name="oldScaleFactor">Old scale factor value</param>
		/// <returns>New scale factor value</returns>
		private int GetNextScaleFactor(bool zoomIn, int oldScaleFactor)
		{
			if ((zoomIn && oldScaleFactor == maxScaleFactor) || (!zoomIn && oldScaleFactor == 1))
				return oldScaleFactor;

			return oldScaleFactor + (zoomIn ? 1 : -1);
		}

		private void SetOriginalSize()
		{
			scaleFactor = 1;
			image.Width = image.Height = double.NaN;
		}

		private double OriginalWidth => image.Source.Width;

		private double OriginalHeight => image.Source.Height;

		private void ChangeScale(bool zoomIn)
		{

			if (image.Source == null)
				return;

			if (zoomIn)
			{
				if (image.ActualWidth < OriginalWidth)
				{
					image.Width = OriginalWidth;
					image.Height = OriginalHeight;
				}
				else
				{
					scaleFactor = GetNextScaleFactor(zoomIn, scaleFactor);
					image.Width = OriginalWidth * scaleFactor;
					image.Height = OriginalHeight * scaleFactor;
				}
			}
			else
			{
				if (image.Stretch == Stretch.None)
					return;

				if (image.Width <= OriginalWidth)
					SetOriginalSize();
				else
				{
					scaleFactor = GetNextScaleFactor(zoomIn, scaleFactor);
					image.Width = OriginalWidth * scaleFactor;
					image.Height = OriginalHeight * scaleFactor;
					if (image.Width < OriginalWidth)
						SetOriginalSize();
				}

				if (!IsOriginalSize)
					image.Margin = CheckBoundaries(image.Margin);
			}
		}

		private void ChangeZoom(bool zoomIn)
		{
			ChangeScale(zoomIn);
			ChangeUIProps();
		}

		private void ChangeUIProps()
		{
			if (image.Source == null)
				return;

			if (IsOriginalSize)
			{
				if (OriginalWidth < ImageGrid.ActualWidth &&
					OriginalHeight < ImageGrid.ActualHeight)
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
					image.Stretch = Stretch.None;
				}
				else
				{
					ImageGrid.Cursor = Cursors.SizeAll;
					image.Stretch = Stretch.Uniform;
				}

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

			image.Width = OriginalWidth;
			image.Height = OriginalHeight;
			ChangeUIProps();
		}
	}
}
