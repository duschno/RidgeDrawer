using ImageDrawer;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.Text.RegularExpressions;
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
			ImageSource temp = original;
			original = Image.Source;
			Image.Source = temp;
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
			LockUnusedParams();

			if (filename != null)
				Render(filename);
		}

		private void LockUnusedParams()
		{
			if (Method.SelectedItem != null)
			{
				switch ((MethodType)Method.SelectedItem)
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

			System.Windows.Point point = GetCursorOverImagePosition();
			CursorPosition.Text = $"{point.X}, {point.Y}";

			if (param.Debug)
			{
				point = Mouse.GetPosition(ImageGrid);
				DebugPousePositionX.Margin = new Thickness(point.X, 0, 0, 0);
				DebugPousePositionY.Margin = new Thickness(0, point.Y, 0, 0);
			}
		}

		private void ImageGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			Keyboard.ClearFocus();
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
			if (e.Key == Key.D0 || e.Key == Key.NumPad0 || e.Key == Key.F)
				SetOriginalSize();
			if (e.Key == Key.Add || e.Key == Key.OemPlus)
				ChangeZoom(true);
			if (e.Key == Key.Subtract || e.Key == Key.OemMinus)
				ChangeZoom(false);
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

		private void ParametersChanged(object sender, RoutedEventArgs e)
		{
			ParametersChanged(null, null);
		}

		private void ParamChange_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Space)
				e.Handled = true;
		}

		#endregion

		private void PullPointButton_Click(object sender, RoutedEventArgs e)
		{
			Image.MouseDown += new MouseButtonEventHandler(image_MouseDown);
			gridBorder.BorderThickness = new Thickness(5);
		}

		private void image_MouseDown(object sender, MouseButtonEventArgs e)
		{
			
			Image.MouseDown -= new MouseButtonEventHandler(image_MouseDown);

			System.Windows.Point point = GetCursorOverImagePosition();
			PullPointX.Text = point.X.ToString();
			PullPointY.Text = point.Y.ToString();
			ParametersChanged(null, null);
		}

		private System.Windows.Point GetCursorOverImagePosition()
		{
			System.Windows.Point point = Mouse.GetPosition(Image);
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

		private void CopyArgs_Click(object sender, RoutedEventArgs e)
		{
			Clipboard.SetText(Logic.CopyArgs(filename, param));
		}

		private void Window_MouseDown(object sender, MouseButtonEventArgs e)
		{
			gridBorder.BorderThickness = new Thickness(0);
			if (e.OriginalSource != Image)
			{
				Image.MouseDown -= new MouseButtonEventHandler(image_MouseDown);
				e.Handled = false;
			}
		}

		private void ImageGrid_MouseLeave(object sender, MouseEventArgs e)
		{
			DebugPousePositionX.Visibility = Visibility.Collapsed;
			DebugPousePositionY.Visibility = Visibility.Collapsed;
		}

		private void ImageGrid_MouseEnter(object sender, MouseEventArgs e)
		{
			DebugPousePositionX.Visibility = param.Debug ? Visibility.Visible : Visibility.Collapsed;
			DebugPousePositionY.Visibility = param.Debug ? Visibility.Visible : Visibility.Collapsed;
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
			return DependencyProperty.UnsetValue;
		}
	}
}
