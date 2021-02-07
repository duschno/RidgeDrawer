using ImageDrawer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Drawing.Drawing2D;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Reflection;
using System.Drawing;

namespace ImageDrawerUI
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		double scaleFactor = 2;
		System.Windows.Point startPos;
		System.Windows.Point endPos;
		Thickness oldMargin;
		string filename;
		ImageSource orig;

		public MainWindow()
		{
			InitializeComponent();
			RenderParams param = new RenderParams { };
			//FieldInfo[] amountField = param.GetType().GetFields();
			//foreach (var item in amountField)
			//{
			//	var someVar = Activator.CreateInstance(item.GetType(), item.GetValue(param));
			//}
			//yourComboBox.ItemsSource = Enum.GetValues(typeof(EffectStyle)).Cast<EffectStyle>();

		}

		private void button_Click(object sender, RoutedEventArgs e)
		{
			Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
			dlg.Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png";

			bool? result = dlg.ShowDialog();

			if (result == true)
			{
				filename = dlg.FileName;
				RenderOnUI(filename);
			}
		}

		private void RenderOnUI(string filename)
		{
			RenderParams param = new RenderParams
			{
				LinesCount = Convert.ToInt32(linescounttb.Text),
				Width = Convert.ToInt32(Widthtb.Text),
				Factor = Convert.ToInt32(Factortb.Text),
				ChunkSize = Convert.ToInt32(ChunkSizetb.Text),
				Smoothing = SmoothingMode.None,
				LineType = RenderType.Dot,
				Method = RenderMethod.Ridge
			};

			Cursor = Cursors.Wait;
			orig = Program.ImageSourceFromBitmap(new Bitmap(filename));
			image.Source = Program.DrawUI(filename, param);
			Cursor = Cursors.Arrow;
		}


		private void ChangeZoom(bool zoomIn)
		{
			if (zoomIn)
			{
				image.Width = image.ActualWidth * scaleFactor;
				image.Height = image.ActualHeight * scaleFactor;
			}
			else
			{
				if (NoScale)
					return;

				if (image.Width <= ImageGrid.ActualWidth)
					image.Width = image.Height = double.NaN;
				else
				{
					image.Width = image.ActualWidth / scaleFactor;
					image.Height = image.ActualHeight / scaleFactor;
					if (image.Width <= ImageGrid.ActualWidth) // why grid, not the image?
						image.Width = image.Height = double.NaN;
				}

			}

			if (NoScale)
			{
				ImageGrid.Cursor = Cursors.Arrow;
				RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.Linear);
				image.Margin = new Thickness();
				oldMargin = new Thickness();
			}
			else
			{
				ImageGrid.Cursor = Cursors.Hand;
				RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.NearestNeighbor);
			}
		}

		private void image_MouseWheel(object sender, MouseWheelEventArgs e)
		{
			ChangeZoom(e.Delta > 0);
		}

		private void image_MouseMove(object sender, MouseEventArgs e)
		{
			if (e.LeftButton == MouseButtonState.Pressed && !NoScale)
			{
				var c = Mouse.GetPosition(Window);

				Thickness margin = image.Margin;
				margin.Left = oldMargin.Left - (startPos - c).X;
				margin.Top = oldMargin.Top - (startPos - c).Y;

				if (margin.Left > 0) margin.Left = 0;
				if (margin.Top > 0) margin.Top = 0;
				if (margin.Top < -image.Height) margin.Top = -image.Height;
				if (margin.Left < -image.Width) margin.Left = -image.Width;
				image.Margin = margin;

				//Debug.WriteLine(image.Margin.Left + " " + image.Margin.Top);
			}
		}

		private void image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			startPos = Mouse.GetPosition(Window);
		}

		private void image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			oldMargin = image.Margin;
			endPos = Mouse.GetPosition(Window);
		}

		private bool NoScale
		{
			get
			{
				return double.IsNaN(image.Width);
			}
		}

		private void Grid_GiveFeedback(object sender, GiveFeedbackEventArgs e)
		{

		}

		private void comparebuton_Click(object sender, RoutedEventArgs e)
		{
			var t = orig;
			orig = image.Source;
			image.Source = t;
		}

		private void linescounttb_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (filename != null)
				RenderOnUI(filename);
		}

		private void Smoothingcb_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (filename != null)
				RenderOnUI(filename);
		}

		private void Window_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Add || e.Key == Key.OemPlus)
			{
				ChangeZoom(true);
			}
			if (e.Key == Key.Subtract || e.Key == Key.OemMinus)
			{
				ChangeZoom(false);
			}
		}
	}
}
