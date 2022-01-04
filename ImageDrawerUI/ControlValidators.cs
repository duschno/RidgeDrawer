using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ImageDrawerUI
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private void PositiveNumberValidation(object sender, TextCompositionEventArgs e)
		{
			string text = GetTextForValidation(sender as TextBox, e);

			Regex regex = new Regex("^[0-9]+$");
			e.Handled = !regex.IsMatch(text);
		}

		private void AngleValidation(object sender, TextCompositionEventArgs e)
		{
			string text = GetTextForValidation(sender as TextBox, e);

			Regex regex = new Regex("^-?[0-9]*$");
			e.Handled = !regex.IsMatch(text);
		}

		private void ColorRangeValidation(object sender, TextCompositionEventArgs e)
		{
			string text = GetTextForValidation(sender as TextBox, e);

			e.Handled = !(int.TryParse(text, out int value) && value >= 0 && value < 256);
		}

		private string GetTextForValidation(TextBox textBox, TextCompositionEventArgs e)
		{
			string text;
			if (textBox.SelectionLength == 0)
				text = textBox.Text + e.Text;
			else
			{
				text = textBox.Text.Substring(0, textBox.SelectionStart) +
					   e.Text +
					   textBox.Text.Substring(textBox.SelectionStart + textBox.SelectionLength);
			}

			return text;
		}
	}

}
