﻿using RidgeDrawer;
using System;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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

			string[] args = Environment.GetCommandLineArgs();
			string filename = args.Length > 1 ? args[1] : null;
			Window.Title = appName;
			Height = SystemParameters.PrimaryScreenHeight * 0.9;
			Width = SystemParameters.PrimaryScreenWidth * 0.9;
			FillComboBox(Smoothing, typeof(SmoothingType));
			FillComboBox(LineType, typeof(LineType));
			FillComboBox(Method, typeof(MethodType));
			FillComboBox(Effect, typeof(EffectBase));
			FillComboBox(Backend, typeof(BackendBase));
			Model = new RenderParamsModel(
				param: new RenderParams
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
					Effect = typeof(RidgeLines),
					Backend = typeof(GDIPlus),
					DrawOnSides = false,
					PointsAroundPeak = -1,
					FillInside = false,
					Invert = false,
					Debug = false,
					PullPointX = 960,
					PullPointY = 540
				},
				filename: filename,
				renderAction: () =>
				{
					try
					{
						LockUnusedParams();
						Error.Visibility = Visibility.Collapsed;

						if (string.IsNullOrEmpty(Model.Filename))
							return;
						if (!File.Exists(Model.Filename))
							throw new ArgumentException($"'{Model.Filename}' could not be found");

						Cursor = Cursors.Wait;
						Arguments.Text = ArgsHelper.ArgsFromParams(Model.Filename, Model.Param);
						Window.Title = $"{Path.GetFileName(Model.Filename)} - {appName}";
						Model.OriginalBitmap = new Bitmap(Model.Filename);
						Model.Original = ConvertToNativeDpi(Model.OriginalBitmap);
						Model.Processed = ConvertToNativeDpi(Logic.Render(Model.Filename, Model.Param)); // тут возможно изображение 2х памяти будет занимать
						Image.Source = Model.Processed;
						scaler.Initialize();
					}
					catch (Exception e)
					{
						ShowError(e);
					}
					finally
					{
						Cursor = Cursors.Arrow;
					}
				});
			DataContext = Model;
			scaler = new ViewportScaler(Image, Viewport, maxFactor: 8);
		}

		public void ShowError(Exception e, string customMsg = null)
		{
			if (!string.IsNullOrEmpty(customMsg))
				customMsg += new string('\n', 2);
			Error.Text = $"{customMsg}{e.Message}\n{e.StackTrace}";
			Error.Visibility = Visibility.Visible;
			Arguments.Text = string.Empty;

			Model.OriginalBitmap = null;
			Model.Original = null;
			Model.Processed = null;
			Image.Source = null;
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
			if (Model.Param.Effect == typeof(RidgeLines))
			{
				WhitePoint.IsEnabled = true;
				BlackPoint.IsEnabled = true;
			}
		}

		private BitmapSource ConvertToNativeDpi(Bitmap bitmap)
		{
			BitmapSource bitmapSource = BitmapHelper.BitmapToBitmapSource(bitmap);
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

		private void ChangeZoom(bool zoomIn)
		{
			if (Image.Source == null)
				return;

			scaler.ChangeScale(zoomIn);
			if (!zoomIn && (scaler.CurrentScaleType == ScaleType.EnlargedBiggerThanViewport ||
							scaler.CurrentScaleType == ScaleType.OriginalBiggerThanViewport))
				Image.Margin = CheckBoundaries(Image.Margin);

			UpdateUIProps();
		}

		private void UpdateUIProps()
		{
			if (scaler.CurrentScaleType == ScaleType.OriginalBiggerThanViewport ||
				scaler.CurrentScaleType == ScaleType.EnlargedBiggerThanViewport)
			{
				Viewport.Cursor = Cursors.SizeAll;
			}
			else
			{
				Image.Margin = new Thickness();
				oldMargin = new Thickness();
				Viewport.Cursor = Cursors.Arrow;
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

		private System.Windows.Point? GetCursorPositionOverImage()
		{
			System.Windows.Point? point = GetCursorPositionOver(Image);
			if (!point.HasValue)
				return null;
			else
			{
				System.Windows.Point resPoint = new System.Windows.Point
				{
					X = (int)(point.Value.X * Image.Source.Width / Image.ActualWidth),
					Y = (int)(point.Value.Y * Image.Source.Height / Image.ActualHeight)
				};

				return resPoint;
			}
		}

		private System.Windows.Point? GetCursorPositionOver(FrameworkElement element)
		{
			System.Windows.Point point = Mouse.GetPosition(element);
			if (point.X < 0 || point.X >= element.ActualWidth ||
				point.Y < 0 || point.Y >= element.ActualHeight)
			{
				return null;
			}

			return point;
		}

		private void UpdateDebugLines(bool isVisible)
		{
			if (isVisible)
			{
				System.Windows.Point? point = GetCursorPositionOver(Viewport);
				if (point.HasValue)
				{
					DebugPousePositionX.Margin = new Thickness(point.Value.X, 0, 0, 0);
					DebugPousePositionY.Margin = new Thickness(0, point.Value.Y, 0, 0);
				}
				else
				{
					isVisible = false;
				}
			}

			DebugPousePositionX.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
			DebugPousePositionY.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
		}

		private void UpdateView()
		{
			if (IsLoaded)
				Model.UpdateView();
		}
	}
}
