using ImageDrawer;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.IO;

namespace ImageDrawerUI
{
	public partial class MainWindow : Window
	{
		private readonly string appName = "Ridge Drawer";
		private RenderParamsModel Model { get; set; }
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

			Window.Title = appName;
			Height = SystemParameters.PrimaryScreenHeight * 0.9;
			Width = SystemParameters.PrimaryScreenWidth * 0.9;
			FillComboBox(Smoothing, typeof(SmoothingType));
			FillComboBox(LineType, typeof(LineType));
			FillComboBox(Method, typeof(MethodType));
			FillComboBox(Backend, typeof(BackendDrawerBase));
			Model = new RenderParamsModel(
				new RenderParams
				{
					LinesCount = 50,
					Stroke = 1,
					Factor = 30,
					ChunkSize = 5,
					GreyPoint = 127,
					BlackPoint = 0,
					WhitePoint = 255,
					Angle = 0,
					Smoothing = SmoothingType.None,
					LineType = ImageDrawer.LineType.Line,
					Method = ImageDrawer.MethodType.Squiggle,
					Backend = typeof(GDIPlus),
					DrawOnSides = false,
					PointsAroundPeak = -1,
					FillInside = false,
					Invert = false,
					Debug = false,
					PullPointX = 960,
					PullPointY = 540
				},
				@"..\soldier.png", Render);
			DataContext = Model;
			Render();
		}

		public System.Drawing.Color GetPixelOfOriginal(int x, int y)
		{
			return Model.OriginalBitmap.GetPixel(x, y);
		}

		private void FillComboBox(ComboBox comboBox, Type type)
		{
			if (type.IsEnum)
				comboBox.ItemsSource = Enum.GetValues(type);
			if (type.IsAbstract)
				comboBox.ItemsSource = Logic.GetImplementations(type);
		}
		private void LockUnusedParams()
		{
			switch (Model.Param.Method)
			{
				case MethodType.Ridge:
					WhitePoint.IsEnabled = true;
					BlackPoint.IsEnabled = true;
					break;
				case MethodType.Squiggle:
					WhitePoint.IsEnabled = true;
					BlackPoint.IsEnabled = true;
					break;
				default:
					break;
			}
		}

		private void Render()
		{
			if (!File.Exists(Model.Filename))
				return;

			LockUnusedParams();
			NotImplementedLabel.Visibility = Visibility.Collapsed;
			Cursor = Cursors.Wait;
			Arguments.Text = Logic.CopyArgs(Model.Filename, Model.Param);
			Window.Title = $"{Path.GetFileName(Model.Filename)} - {appName}";
			Model.OriginalBitmap = new Bitmap(Model.Filename);
			Model.Original = ConvertToNativeDpi(Model.OriginalBitmap);
			try
			{
				Model.Processed = ConvertToNativeDpi(Logic.ProcessByFilename(Model.Filename, Model.Param));
			}
			catch (NotImplementedException e)
			{
				NotImplementedLabel.Content = $"{e.Message}:\n{e.StackTrace}";
				NotImplementedLabel.Visibility = Visibility.Visible;
				Arguments.Text = string.Empty;
			}

			Image.Source = Model.Processed;
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
