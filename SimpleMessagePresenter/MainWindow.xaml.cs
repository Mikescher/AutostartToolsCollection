using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
namespace SimpleMessagePresenter
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();

			var args = Environment.GetCommandLineArgs();

			if (args.Length > 1)
			{
				this.Title = args[1];
			}

			if (args.Length > 2)
			{
				try
				{
					MainBox.Text = File.ReadAllText(args[2]);
					File.Delete(args[2]);
				}
				catch (Exception e)
				{
					MessageBox.Show(e.ToString());
				}
			}

		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}
	}
}
