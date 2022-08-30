using RidgeDrawer;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.IO;

namespace RidgeDrawerUI
{
	public partial class MainWindow : Window
	{
		private readonly string appName = "Ridge Drawer";
		private RenderParamsModel Model { get; set; }
		private ViewportScaler scaler;
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
				return Image.ActualWidth <= Viewport.ActualWidth &&
					Image.ActualHeight <= Viewport.ActualHeight;
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
			scaler = new ViewportScaler(maxFactor: 8);
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
					LineType = RidgeDrawer.LineType.Line,
					Method = RidgeDrawer.MethodType.Squiggle,
					Backend = typeof(GDIPlus),
					DrawOnSides = false,
					PointsAroundPeak = -1,
					FillInside = false,
					Invert = false,
					Debug = false,
					PullPointX = 960,
					PullPointY = 540
				},
				@"..\soldier.png",
				() => {
					try
					{
						if (!File.Exists(Model.Filename))
							throw new ArgumentException($"'{Model.Filename}' could not be found");

						LockUnusedParams();
						NotImplementedLabel.Visibility = Visibility.Collapsed;
						Cursor = Cursors.Wait;
						Arguments.Text = Logic.CopyArgs(Model.Filename, Model.Param);
						Window.Title = $"{Path.GetFileName(Model.Filename)} - {appName}";
						Model.OriginalBitmap = new Bitmap(Model.Filename);
						Model.Original = ConvertToNativeDpi(Model.OriginalBitmap);
						Model.Processed = ConvertToNativeDpi(Logic.ProcessByFilename(Model.Filename, Model.Param));
						Image.Source = Model.Processed;
						Image.MaxWidth = OriginalWidth * scaler.MaxFactor;
						Image.MaxHeight = OriginalHeight * scaler.MaxFactor;
					}
					catch (Exception e)
					{
						NotImplementedLabel.Content = $"{e.Message}:\n{e.StackTrace}";
						NotImplementedLabel.Visibility = Visibility.Visible;
						Arguments.Text = string.Empty;
					}
					finally
					{
						Cursor = Cursors.Arrow;
					}
				});
			DataContext = Model;
			Model.UpdateView();
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

		private void SetToFitGrid()
		{
			scaler.CurrentFactor = 1;
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
					scaler.SetNextFactor(zoomIn);
					Image.Width = OriginalWidth * scaler.CurrentFactor;
					Image.Height = OriginalHeight * scaler.CurrentFactor;
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
					scaler.SetNextFactor(zoomIn);
					Image.Width = OriginalWidth * scaler.CurrentFactor;
					Image.Height = OriginalHeight * scaler.CurrentFactor;
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
				if (OriginalWidth < Viewport.ActualWidth &&
					OriginalHeight < Viewport.ActualHeight)
					Image.Stretch = Stretch.None;
				else
					Image.Stretch = Stretch.Uniform;

				Viewport.Cursor = Cursors.Arrow;
				RenderOptions.SetBitmapScalingMode(Image, BitmapScalingMode.Linear);
				Image.Margin = new Thickness();
				oldMargin = new Thickness();
			}
			else
			{
				if (Image.Width <= Viewport.ActualWidth &&
					Image.Height <= Viewport.ActualHeight)
				{
					Image.Margin = new Thickness();
					oldMargin = new Thickness();
					Viewport.Cursor = Cursors.Arrow;
					Image.Stretch = Stretch.Uniform;
				}
				else
				{
					Viewport.Cursor = Cursors.SizeAll;
					Image.Stretch = Stretch.Uniform;
				}

				RenderOptions.SetBitmapScalingMode(Image, BitmapScalingMode.NearestNeighbor);
			}
		}

		private Thickness CheckBoundaries(Thickness margin)
		{
			double widthOver = Viewport.ActualWidth - Image.Width;
			double heightOver = Viewport.ActualHeight - Image.Height;

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

			scaler.CurrentFactor = 1;
			Image.Width = OriginalWidth;
			Image.Height = OriginalHeight;
			ChangeUIProps();
		}
	}
}
