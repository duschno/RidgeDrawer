using ImageDrawer;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.Windows.Data;
using System.Globalization;

namespace ImageDrawerUI
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
#region Event handlers

		private void Compare_Click(object sender, RoutedEventArgs e)
		{
			ImageSource temp = Model.Original;
			Model.Original = Image.Source;
			Image.Source = temp;
		}

		private void Save_Click(object sender, RoutedEventArgs e)
		{
			if (Model.Processed == null)
				return;

			Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog
			{
				Filter = "Image files (*.png) | *.png"
			};

			bool? result = dlg.ShowDialog();
			if (result == true)
				Logic.SaveAsPng(Model.Processed as BitmapSource, dlg.FileName);
		}

		private void Reset_Click(object sender, RoutedEventArgs e)
		{
			Model.Param = Model.DefaultParam.Clone();
			Model.UpdateView();
		}

		private void ImageGrid_MouseWheel(object sender, MouseWheelEventArgs e)
		{
			ChangeZoom(e.Delta > 0);
		}

		private void ImageGrid_MouseMove(object sender, MouseEventArgs e)
		{
			if ((e.LeftButton == MouseButtonState.Pressed || e.MiddleButton == MouseButtonState.Pressed) && 
				Image.Stretch != Stretch.None)
			{
				System.Windows.Point newPos = Mouse.GetPosition(Window);
				Thickness margin = Image.Margin;
				if (ImageGrid.ActualHeight < Image.ActualHeight)
					margin.Top = oldMargin.Top - (startPos - newPos).Y;
				if (ImageGrid.ActualWidth < Image.ActualWidth)
					margin.Left = oldMargin.Left - (startPos - newPos).X;

				Image.Margin = CheckBoundaries(margin);
			}

			System.Windows.Point? point = GetCursorOverImagePosition();
			if (!point.HasValue)
			{
				CursorPosition.Text = string.Empty;
				ColorValue.Text = string.Empty;
			}
			else
			{
				CursorPosition.Text = $"{point.Value.X},{point.Value.Y}";
				System.Drawing.Color color = GetPixelOfOriginal((int)point.Value.X, (int)point.Value.Y);
				ColorValue.Text = $"{color.R,3},{color.G,3},{color.B,3}";
			}

			if (Model.Param.Debug)
			{
				point = Mouse.GetPosition(ImageGrid);
				DebugPousePositionX.Margin = new Thickness(point.Value.X, 0, 0, 0);
				DebugPousePositionY.Margin = new Thickness(0, point.Value.Y, 0, 0);
			}
		}

		private void ImageGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			ImageGrid.Focus();
			startPos = Mouse.GetPosition(Window);
			Mouse.Capture(Image);
		}

		private void ImageGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			oldMargin = Image.Margin;
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
			if (e.Key == Key.R)
				Reset_Click(null, null);
			if (e.Key == Key.D0 || e.Key == Key.NumPad0 || e.Key == Key.F)
				SetOriginalSize();
			if (e.Key == Key.Add || e.Key == Key.OemPlus)
				ChangeZoom(true);
			if (e.Key == Key.Subtract || e.Key == Key.OemMinus)
				ChangeZoom(false);
			if (e.Key == Key.D)
			{
				Model.Param.Debug = !Model.Param.Debug;
				Model.UpdateView();
			}
		}

		private void ImageGrid_Loaded(object sender, RoutedEventArgs e)
		{
			if (!IsFittingGrid)
			{
				Image.Stretch = Stretch.Uniform;
				RenderOptions.SetBitmapScalingMode(Image, BitmapScalingMode.Linear);
			}
		}

		private void ImageGrid_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			if (ImageGrid.IsLoaded)
				ChangeUIProps();
		}

		private void FullsizeButton_Click(object sender, RoutedEventArgs e)
		{
			SetOriginalSize();
		}

		private void ParamChange_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key != Key.Enter)
				return;
			if (string.IsNullOrWhiteSpace(((TextBox)e.Source).Text) ||
				!int.TryParse(((TextBox)e.Source).Text, out _))
				return;

			BindingExpression binding = BindingOperations.GetBindingExpression((TextBox)sender,
				TextBox.TextProperty);
			if (binding != null)
				binding.UpdateSource(); // manually update value in source
			Model.UpdateView();
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
				Model.Filename = dlg.FileName;
				Model.UpdateView();
			}
		}

		private void ParamChange_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Space)
				e.Handled = true;
		}

		#endregion

		private void PullPointButton_Click(object sender, RoutedEventArgs e)
		{
			ImageGrid.PreviewMouseDown += new MouseButtonEventHandler(ImageGrid_PreviewMouseDown);
			gridBorder.BorderThickness = new Thickness(3);
		}

		private void ImageGrid_PreviewMouseDown(object sender, MouseButtonEventArgs e)
		{
			ImageGrid.PreviewMouseDown -= new MouseButtonEventHandler(ImageGrid_PreviewMouseDown);

			System.Windows.Point? point = GetCursorOverImagePosition();
			if (point.HasValue)
			{
				Model.Param.PullPointX = (int)point.Value.X;
				Model.Param.PullPointY = (int)point.Value.Y;
				Model.UpdateView();
			}
		}
		private System.Windows.Point? GetCursorOverImagePosition()
		{
			System.Windows.Point point = Mouse.GetPosition(Image);
			if (point.X < 0 || point.X >= Image.ActualWidth ||
				point.Y < 0 || point.Y >= Image.ActualHeight)
				return null;

			System.Windows.Point resPoint = new System.Windows.Point();
			if (double.IsNaN(Image.Width))
			{
				resPoint.X = (int)(point.X * OriginalWidth / Image.ActualWidth);
				resPoint.Y = (int)(point.Y * OriginalHeight / Image.ActualHeight);
			}
			else
			{
				resPoint.X = (int)point.X / scaleFactor;
				resPoint.Y = (int)point.Y / scaleFactor;
			}

			return resPoint;
		}

		private void CopyImage_Click(object sender, RoutedEventArgs e)
		{
			Clipboard.SetImage(Image.Source as BitmapSource);
		}

		private void CopyArgs_Click(object sender, RoutedEventArgs e)
		{
			Clipboard.SetText(Arguments.Text);
		}

		private void PasteArgs_Click(object sender, RoutedEventArgs e)
		{
			LogicParams logicParams = new LogicParams()
			{
				InputFilename = Model.Filename,
				RenderParams = Model.Param
			};
			logicParams = Logic.ParseArgs(logicParams, Clipboard.GetText().Split());
			Model.Filename = logicParams.InputFilename;
			Model.Param = logicParams.RenderParams;
			Arguments.Text = Logic.CopyArgs(Model.Filename, Model.Param);
			Model.UpdateView();
		}

		private void Window_MouseDown(object sender, MouseButtonEventArgs e)
		{
			gridBorder.BorderThickness = new Thickness(0);
			if (e.OriginalSource != Image)
			{
				ImageGrid.PreviewMouseDown -= new MouseButtonEventHandler(ImageGrid_PreviewMouseDown);
				e.Handled = false;
			}
		}

		private void ImageGrid_MouseLeave(object sender, MouseEventArgs e)
		{
			CursorPosition.Text = string.Empty;
			ColorValue.Text = string.Empty;
			DebugPousePositionX.Visibility = Visibility.Collapsed;
			DebugPousePositionY.Visibility = Visibility.Collapsed;
		}

		private void ImageGrid_MouseEnter(object sender, MouseEventArgs e)
		{
			DebugPousePositionX.Visibility = Model.Param.Debug ? Visibility.Visible : Visibility.Collapsed;
			DebugPousePositionY.Visibility = Model.Param.Debug ? Visibility.Visible : Visibility.Collapsed;
		}

		private void Control_ValueChanged(object sender, RoutedEventArgs e)
		{
			Model.UpdateView();
		}
	}

	public class BooleanToVisibilityConverter : IValueConverter
	{

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			bool visible = System.Convert.ToBoolean(value, culture);
			return visible ? Visibility.Visible : Visibility.Collapsed;
		}


		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
	public class EnumToStringConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value.ToString();
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
