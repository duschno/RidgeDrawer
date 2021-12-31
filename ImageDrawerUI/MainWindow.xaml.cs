﻿using ImageDrawer;
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
			if ((e.LeftButton == MouseButtonState.Pressed || e.MiddleButton == MouseButtonState.Pressed) && !IsNonScaled)
			{
				System.Windows.Point newPos = Mouse.GetPosition(Window);
				Thickness margin = image.Margin;
				if (ImageGrid.ActualHeight < image.ActualHeight)
					margin.Top = oldMargin.Top - (startPos - newPos).Y;
				if (ImageGrid.ActualWidth < image.ActualWidth)
					margin.Left = oldMargin.Left - (startPos - newPos).X;

				image.Margin = CheckBoundaries(margin);

				//Debug.WriteLine("* margin.left = {0}, margin.top = {1}", image.Margin.Left, image.Margin.Top);
				//Debug.WriteLine("* width = {0}, height = {1}, ac. width = {2}, ac. height = {3}",
				//	image.Width, image.Height, image.ActualWidth, image.ActualHeight);
				//Debug.WriteLine("* grid.width = {0}, grid.height = {1}, grid.ac. width = {2}, grid.ac. height = {3}\n",
				//	ImageGrid.Width, ImageGrid.Height, ImageGrid.ActualWidth, ImageGrid.ActualHeight);
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
				SetFullsize();
			if (e.Key == Key.Add || e.Key == Key.OemPlus)
				ChangeZoom(true);
			if (e.Key == Key.Subtract || e.Key == Key.OemMinus)
				ChangeZoom(false);
		}

		private void ImageGrid_Loaded(object sender, RoutedEventArgs e)
		{
			if (!IsFitsGrid)
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
			SetFullsize();
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
	}
}
