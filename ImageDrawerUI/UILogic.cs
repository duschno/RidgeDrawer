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
		private RenderParams param;
		private ImageSource original;
		private ImageSource processed;
		private int scaleFactor = 1;
		private readonly int maxScaleFactor = 8;
		private System.Windows.Point startPos;
		private Thickness oldMargin;

		private double OriginalWidth
		{
			get
			{
				return Image.Source.Width;
			}
		}

		private double OriginalHeight
		{
			get
			{
				return Image.Source.Height;
			}
		}

		private bool IsFittingGrid
		{
			get
			{
				return Image.ActualWidth <= ImageGrid.ActualWidth &&
					Image.ActualHeight <= ImageGrid.ActualHeight;
			}
		}

		private bool IsOriginalSize
		{
			get
			{
				return double.IsNaN(Image.Width) || Image.Width == OriginalWidth;
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
			PullPointX.Text = 960.ToString();
			PullPointY.Text = 540.ToString();

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
				IgnoreNonAffectedPoints = IgnoreNonAffectedPoints.IsChecked ?? false,
				FillInside = FillInside.IsChecked ?? false,
				Invert = Invert.IsChecked ?? false,
				Debug = Debug.IsChecked ?? false,
				Backend = (Type)Backend.Items[Backend.SelectedIndex],
				PullPointX = Convert.ToInt32(PullPointX.Text),
				PullPointY = Convert.ToInt32(PullPointY.Text)
			};

			Cursor = Cursors.Wait;
			Arguments.Text = Logic.CopyArgs(filename, param);
			original = ConvertToNativeDpi(new Bitmap(filename));
			try
			{
				processed = ConvertToNativeDpi(Logic.ProcessByFilename(filename, param));
			}
			catch (NotImplementedException e)
			{
				NotImplementedLabel.Content = $"{e.Message}:\n{e.StackTrace}";
				NotImplementedLabel.Visibility = Visibility.Visible;
			}

			Image.Source = processed;
			Image.MaxWidth = OriginalWidth * maxScaleFactor;
			Image.MaxHeight = OriginalHeight * maxScaleFactor;
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

		private int GetNextScaleFactor(bool zoomIn, int oldScaleFactor)
		{
			if ((zoomIn && oldScaleFactor == maxScaleFactor) || (!zoomIn && oldScaleFactor == 1))
				return oldScaleFactor;

			return oldScaleFactor + (zoomIn ? 1 : -1);
		}

		private void SetToFitGrid()
		{
			scaleFactor = 1;
			Image.Width = Image.Height = double.NaN;
		}

		private void ChangeScale(bool zoomIn)
		{

			if (Image.Source == null)
				return;

			if (zoomIn)
			{
				if (Image.ActualWidth < OriginalWidth)
				{
					Image.Width = OriginalWidth;
					Image.Height = OriginalHeight;
				}
				else
				{
					scaleFactor = GetNextScaleFactor(zoomIn, scaleFactor);
					Image.Width = OriginalWidth * scaleFactor;
					Image.Height = OriginalHeight * scaleFactor;
				}
			}
			else
			{
				if (Image.Stretch == Stretch.None)
					return;

				if (Image.Width <= OriginalWidth)
					SetToFitGrid();
				else
				{
					scaleFactor = GetNextScaleFactor(zoomIn, scaleFactor);
					Image.Width = OriginalWidth * scaleFactor;
					Image.Height = OriginalHeight * scaleFactor;
					if (Image.Width < OriginalWidth)
						SetToFitGrid();
				}

				if (!IsOriginalSize)
					Image.Margin = CheckBoundaries(Image.Margin);
			}
		}

		private void ChangeZoom(bool zoomIn)
		{
			ChangeScale(zoomIn);
			ChangeUIProps();
		}

		private void ChangeUIProps()
		{
			if (Image.Source == null)
				return;

			if (IsOriginalSize)
			{
				if (OriginalWidth < ImageGrid.ActualWidth &&
					OriginalHeight < ImageGrid.ActualHeight)
					Image.Stretch = Stretch.None;
				else
					Image.Stretch = Stretch.Uniform;

				ImageGrid.Cursor = Cursors.Arrow;
				RenderOptions.SetBitmapScalingMode(Image, BitmapScalingMode.Linear);
				Image.Margin = new Thickness();
				oldMargin = new Thickness();
			}
			else
			{
				if (Image.Width <= ImageGrid.ActualWidth &&
					Image.Height <= ImageGrid.ActualHeight)
				{
					Image.Margin = new Thickness();
					oldMargin = new Thickness();
					ImageGrid.Cursor = Cursors.Arrow;
					Image.Stretch = Stretch.Uniform;
				}
				else
				{
					ImageGrid.Cursor = Cursors.SizeAll;
					Image.Stretch = Stretch.Uniform;
				}

				RenderOptions.SetBitmapScalingMode(Image, BitmapScalingMode.NearestNeighbor);
			}
		}

		private Thickness CheckBoundaries(Thickness margin)
		{
			double widthOver = ImageGrid.ActualWidth - Image.Width;
			double heightOver = ImageGrid.ActualHeight - Image.Height;

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

		private void SetOriginalSize()
		{
			if (Image.Source == null)
				return;

			scaleFactor = 1;
			Image.Width = OriginalWidth;
			Image.Height = OriginalHeight;
			ChangeUIProps();
		}
	}
}
