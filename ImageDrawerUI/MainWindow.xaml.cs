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
			ImageSource temp = model.Original;
			model.Original = Image.Source;
			Image.Source = temp;
		}

		private void Save_Click(object sender, RoutedEventArgs e)
		{
			if (model.Processed == null)
				return;

			Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog
			{
				Filter = "Image files (*.png) | *.png"
			};

			bool? result = dlg.ShowDialog();
			if (result == true)
				Logic.SaveAsPng(model.Processed as BitmapSource, dlg.FileName);
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
			if (point.X < 0 || point.X > OriginalWidth - 1 ||
				point.Y < 0 || point.Y > OriginalHeight - 1)
			{
				CursorPosition.Text = string.Empty;
				ColorValue.Text = string.Empty;
			}
			else
			{
				CursorPosition.Text = $"{point.X},{point.Y}";
				System.Drawing.Color color = GetPixelOfOriginal((int)point.X, (int)point.Y);
				ColorValue.Text = $"{color.R,3},{color.G,3},{color.B,3}";
			}

			if (model.Debug)
			{
				point = Mouse.GetPosition(ImageGrid);
				DebugPousePositionX.Margin = new Thickness(point.X, 0, 0, 0);
				DebugPousePositionY.Margin = new Thickness(0, point.Y, 0, 0);
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
			if (e.Key == Key.D0 || e.Key == Key.NumPad0 || e.Key == Key.F)
				SetOriginalSize();
			if (e.Key == Key.Add || e.Key == Key.OemPlus)
				ChangeZoom(true);
			if (e.Key == Key.Subtract || e.Key == Key.OemMinus)
				ChangeZoom(false);
			if (e.Key == Key.D)
				model.Debug = !model.Debug;
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

			TextBox tBox = (TextBox)sender;
			DependencyProperty prop = TextBox.TextProperty;
			BindingExpression binding = BindingOperations.GetBindingExpression(tBox, prop);
			if (binding != null)
				binding.UpdateSource();
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
				model.Filename = dlg.FileName;
				Render();
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
			Image.MouseDown += new MouseButtonEventHandler(Image_MouseDown);
			gridBorder.BorderThickness = new Thickness(5);
		}

		private void Image_MouseDown(object sender, MouseButtonEventArgs e)
		{
			Image.MouseDown -= new MouseButtonEventHandler(Image_MouseDown);

			System.Windows.Point point = GetCursorOverImagePosition();
			model.PullPointX = (int)point.X;
			model.PullPointY = (int)point.Y;
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
			Clipboard.SetText(Arguments.Text);
		}

		private void Window_MouseDown(object sender, MouseButtonEventArgs e)
		{
			gridBorder.BorderThickness = new Thickness(0);
			if (e.OriginalSource != Image)
			{
				Image.MouseDown -= new MouseButtonEventHandler(Image_MouseDown);
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
			DebugPousePositionX.Visibility = model.Debug ? Visibility.Visible : Visibility.Collapsed;
			DebugPousePositionY.Visibility = model.Debug ? Visibility.Visible : Visibility.Collapsed;
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
