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
			scaler = new ViewportScaler(Image, maxFactor: 8);
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

		private void ChangeScale(bool zoomIn)
		{
			if (Image.Source == null)
				return;

			scaler.ChangeScale(zoomIn);

			if (!zoomIn && !scaler.IsOriginalSize)
				Image.Margin = CheckBoundaries(Image.Margin);
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

			scaler.ChangeUIProps(Viewport);
			if (scaler.IsOriginalSize)
			{
				Viewport.Cursor = Cursors.Arrow;
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
				}
				else
				{
					Viewport.Cursor = Cursors.SizeAll;
				}
			}
		}

		private Thickness CheckBoundaries(Thickness margin)
		{
			double widthOver = Viewport.ActualWidth - Image.ActualWidth;
			double heightOver = Viewport.ActualHeight - Image.ActualHeight;

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

		private System.Windows.Point? GetCursorOverImagePosition()
		{
			System.Windows.Point point = Mouse.GetPosition(Image);
			if (point.X < 0 || point.X >= Image.ActualWidth ||
				point.Y < 0 || point.Y >= Image.ActualHeight)
			{
				return null;
			}

			System.Windows.Point resPoint = new System.Windows.Point();
			if (double.IsNaN(Image.Width))
			{
				resPoint.X = (int)(point.X * scaler.OriginalWidth / Image.ActualWidth);
				resPoint.Y = (int)(point.Y * scaler.OriginalHeight / Image.ActualHeight);
			}
			else
			{
				resPoint.X = (int)point.X / scaler.CurrentFactor;
				resPoint.Y = (int)point.Y / scaler.CurrentFactor;
			}

			return resPoint;
		}
	}
}
