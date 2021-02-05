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

namespace ImageDrawerUI
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
		}

		private void button_Click(object sender, RoutedEventArgs e)
		{
			Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
			dlg.Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png";

			bool? result = dlg.ShowDialog();

			if (result == true)
				RenderOnUI(dlg.FileName);
		}

		private void RenderOnUI(string filename)
		{
			RenderParams param = new RenderParams
			{
				LinesCount = 120,
				Width = 1,
				Factor = 5,
				ChunkSize = 5,
				Smoothing = SmoothingMode.AntiAlias,
				LineType = RenderType.Line,
				Method = RenderMethod.Squiggle
			};

			image.Source = Program.DrawUI(filename, param);
		}
	}
}
