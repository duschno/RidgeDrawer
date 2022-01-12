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

		private void Image_MouseWheel(object sender, MouseWheelEventArgs e)
		{
			ChangeZoom(e.Delta > 0);
		}

		private void Image_MouseMove(object sender, MouseEventArgs e)
		{
			if ((e.LeftButton == MouseButtonState.Pressed || e.MiddleButton == MouseButtonState.Pressed) && image.Stretch != Stretch.None)
			{
				System.Windows.Point newPos = Mouse.GetPosition(Window);
				Thickness margin = image.Margin;
				if (ImageGrid.ActualHeight < image.ActualHeight)
					margin.Top = oldMargin.Top - (startPos - newPos).Y;
				if (ImageGrid.ActualWidth < image.ActualWidth)
					margin.Left = oldMargin.Left - (startPos - newPos).X;

				image.Margin = CheckBoundaries(margin);
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

		private void CopyArgs_Click(object sender, RoutedEventArgs e)
		{
			Clipboard.SetText(Logic.CopyArgs(filename, param));
		}

		#endregion

		private void PullPointButton_Click(object sender, RoutedEventArgs e)
		{
			if (gridBorder.BorderThickness.Left == 5)
			{
				image.MouseDown -= new MouseButtonEventHandler(image_MouseDown);
				gridBorder.BorderThickness = new Thickness(0);
			}
			else
			{
				image.MouseDown += new MouseButtonEventHandler(image_MouseDown);
				gridBorder.BorderThickness = new Thickness(5);
			}
		}

		private void image_MouseDown(object sender, MouseButtonEventArgs e)
		{
			image.MouseDown -= new MouseButtonEventHandler(image_MouseDown);
			gridBorder.BorderThickness = new Thickness(0);

			System.Windows.Point point = Mouse.GetPosition(image);
			PullPointX.Text = ((int)point.X / scaleFactor).ToString();
			PullPointY.Text = ((int)point.Y / scaleFactor).ToString();
			ParametersChanged(null, null);
		}
	}
}
