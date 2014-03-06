using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DIPSViewer
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private bool loaded = false;

		public MainWindow()
		{
			InitializeComponent();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			loaded = true;
			updateLists();
			autoSelect();
		}

		private void btnLoad_Click(object sender, RoutedEventArgs e)
		{
			updateLists();
			autoSelect();
		}

		private void autoSelect()
		{
			if (lbRight.Items.Count >= 1) lbRight.SelectedIndex = lbRight.Items.Count - 1;
			if (lbLeft.Items.Count >= 2) lbLeft.SelectedIndex = lbLeft.Items.Count - 2;
		}

		private bool IsValidDateTimeFileName(string path)
		{
			string fn = System.IO.Path.GetFileName(path);
			string fnwe = System.IO.Path.GetFileNameWithoutExtension(path);

			DateTime t;

			if (Regex.IsMatch(fn, @"[0-9]{4}_[0-9]{2}_[0-9]{2}_[0-9]{2}_[0-9]{2}.json"))
				return DateTime.TryParseExact(fnwe, "yyyy_MM_dd_HH_mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out t);
			else
				return false;
		}

		private void updateLists()
		{
			lbLeft.Items.Clear();
			lbRight.Items.Clear();

			string path = Environment.ExpandEnvironmentVariables(edPath.Text);

			if (!Directory.Exists(path))
				return;

			List<string> versions = Directory.EnumerateFiles(path).
				Where(p => IsValidDateTimeFileName(p)).
				OrderBy(p => DateTime.ParseExact(System.IO.Path.GetFileNameWithoutExtension(p), "yyyy_MM_dd_HH_mm", CultureInfo.InvariantCulture)).
				ToList();

			foreach (String vpath in versions)
			{
				string fn = System.IO.Path.GetFileNameWithoutExtension(vpath);
				DateTime dt = DateTime.ParseExact(fn, "yyyy_MM_dd_HH_mm", CultureInfo.InvariantCulture);

				lbLeft.Items.Add(new LVElement(dt, vpath));
				lbRight.Items.Add(new LVElement(dt, vpath));
			}
		}

		private void listbox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			repaintCanvas();
		}

		private void cbFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			repaintCanvas();
		}

		private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			repaintCanvas();
		}

		private bool curr_lbChanges_Updating = false;
		private void lbChanges_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (curr_lbChanges_Updating) return;

			curr_lbChanges_Updating = true;
			repaintCanvas();
			curr_lbChanges_Updating = false;
		}

		private void repaintCanvas()
		{
			if (!loaded) return;

			int cur_sel = lbChanges.SelectedIndex;

			canvas.Children.Clear();
			lbChanges.Items.Clear();

			// ####################

			LVElement el_prev = lbLeft.SelectedItem as LVElement;
			LVElement el_curr = lbRight.SelectedItem as LVElement;

			if (el_curr == null || el_prev == null) return;

			if (el_curr.time < el_prev.time)
			{
				MathExt.Swap(ref el_curr, ref el_prev);
			}

			JObject json_prev = JObject.Parse(File.ReadAllText(el_prev.path));
			JObject json_curr = JObject.Parse(File.ReadAllText(el_curr.path));

			List<DesktopIcon> icons_prev = (json_prev["icons"] as JArray).Select(p => new DesktopIcon((string)p["title"], (int)p["x"], (int)p["y"])).ToList();
			List<DesktopIcon> icons_curr = (json_curr["icons"] as JArray).Select(p => new DesktopIcon((string)p["title"], (int)p["x"], (int)p["y"])).ToList();
		
			// ####################

			List<DesktopIcon> icons_all;
			List<DesktopIcon> icons_unchanged;
			List<Tuple<DesktopIcon, DesktopIcon>> icons_moved;
			List<Tuple<DesktopIcon, DesktopIcon>> icons_removed;
			List<Tuple<DesktopIcon, DesktopIcon>> icons_added;

			icons_all = icons_curr.Concat(icons_prev).ToList();

			icons_unchanged = icons_curr.Where(p => icons_prev.Contains(p)).ToList();

			icons_prev.RemoveAll(p => (icons_unchanged.Contains(p)));
			icons_curr.RemoveAll(p => (icons_unchanged.Contains(p)));

			icons_moved = new List<Tuple<DesktopIcon, DesktopIcon>>();

			foreach (DesktopIcon i_curr in icons_curr)
				foreach (DesktopIcon i_prev in icons_prev)
					if (i_curr.name == i_prev.name) 
					{
						icons_moved.Add(Tuple.Create(i_prev, i_curr));
						break;
					}

			icons_prev.RemoveAll(p => (icons_moved.Select(p2 => p2.Item1).Contains(p)));
			icons_curr.RemoveAll(p => (icons_moved.Select(p2 => p2.Item2).Contains(p)));

			icons_removed = icons_prev.Select(p => new Tuple<DesktopIcon, DesktopIcon>(p, null)).ToList();
			icons_added = icons_curr.Select(p => new Tuple<DesktopIcon, DesktopIcon>(null, p)).ToList();

			// ####################

			int sf = cbFilter.SelectedIndex;

			bool show_unchanged = (sf == 0 ||sf == 2);
			bool show_moved = (sf == 0 || sf == 1 || sf == 3);
			bool show_added = (sf == 0 || sf == 1 || sf == 4);
			bool show_removed = (sf == 0 || sf == 1 || sf == 5);

			if (show_unchanged)
				foreach (DesktopIcon fe_icon in icons_unchanged)
					lbChanges.Items.Add(new CLVElement(fe_icon, string.Format(@"[UNCHANGED] '{0}'", fe_icon.name)));

			if (show_moved)
				foreach (Tuple<DesktopIcon, DesktopIcon> fe_icon in icons_moved)
					lbChanges.Items.Add(new CLVElement(fe_icon, string.Format(@"[  MOVED  ] '{0}' FROM ({1}|{2}) TO ({3}|{4})", fe_icon.Item1.name, fe_icon.Item1.x, fe_icon.Item1.y, fe_icon.Item2.x, fe_icon.Item2.y)));

			if (show_added)
				foreach (Tuple<DesktopIcon, DesktopIcon> fe_icon in icons_added)
					lbChanges.Items.Add(new CLVElement(fe_icon, string.Format(@"[  ADDED  ] '{0}' AT ({1}|{2})", fe_icon.Item2.name, fe_icon.Item2.x, fe_icon.Item2.y)));

			if (show_removed)
				foreach (Tuple<DesktopIcon, DesktopIcon> fe_icon in icons_removed)
					lbChanges.Items.Add(new CLVElement(fe_icon, string.Format(@"[ REMOVED ] '{0}' AT ({1}|{2})", fe_icon.Item1.name, fe_icon.Item1.x, fe_icon.Item1.y)));

			lbChanges.SelectedIndex = cur_sel;

			// ####################

			int minX = 0;
			int maxX = int.MinValue;
			int minY = 0;
			int maxY = int.MinValue;

			object sel = lbChanges.SelectedItem == null ? null : ((CLVElement)lbChanges.SelectedItem).icn;

			foreach (DesktopIcon i in icons_all)
			{
				minX = Math.Min(minX, i.x);
				maxX = Math.Max(maxX, i.x);

				minY = Math.Min(minY, i.y);
				maxY = Math.Max(maxY, i.y);
			}

			minX -= 75;
			minY -= 75;
			maxX += 75;
			maxY += 75;

			int w = maxX - minX;
			int h = maxY - minY;

			double cv_w = canvas.ActualWidth;
			double cv_h = canvas.ActualHeight;

			double offset_X = -minX;
			double offset_Y = -minY;
			double scale;
			if ((cv_w / cv_h) > ((w * 1.0) / h))
			{ // Anpassen an Y
				scale = cv_h / h;
			}
			else
			{ // Anpassen an X
				scale = cv_w / w;
			}

			drawRectangle(offset_X, offset_Y, minX, minY, w, h, scale, Brushes.LightGray, null, false);

			if (show_unchanged)
				foreach (DesktopIcon icon in icons_unchanged)
				{
					drawMidRectangle(offset_X, offset_Y, icon.x, icon.y, 64, 64, scale, Brushes.Gray, Brushes.Black, icon == sel);
				}

			if (show_moved)
				foreach (Tuple<DesktopIcon, DesktopIcon> icon in icons_moved)
				{
					drawMidRectangle(offset_X, offset_Y, icon.Item1.x, icon.Item1.y, 64, 64, scale, Brushes.Gray, Brushes.Blue, icon == sel);
					drawMidRectangle(offset_X, offset_Y, icon.Item2.x, icon.Item2.y, 64, 64, scale, Brushes.Gray, Brushes.Blue, icon == sel);
					drawLine(offset_X, offset_Y, icon.Item1.x, icon.Item1.y, icon.Item2.x, icon.Item2.y, scale, Brushes.Blue);
				}

			if (show_added)
				foreach (Tuple<DesktopIcon, DesktopIcon> icon in icons_added)
				{
					drawMidRectangle(offset_X, offset_Y, icon.Item2.x, icon.Item2.y, 64, 64, scale, Brushes.Gray, Brushes.Green, icon == sel);
				}

			if (show_removed)
				foreach (Tuple<DesktopIcon, DesktopIcon> icon in icons_removed)
				{
					drawMidRectangle(offset_X, offset_Y, icon.Item1.x, icon.Item1.y, 64, 64, scale, Brushes.Gray, Brushes.Red, icon == sel);
				}
		}

		private void drawMidRectangle(double offx, double offy, double x, double y, double w, double h, double scale, Brush fill, Brush outer, bool emph)
		{
			drawRectangle(offx, offy, x - w / 2, y - h / 2, w, h, scale, fill, outer, emph);
		}

		private void drawRectangle(double offx, double offy, double x, double y, double w, double h, double scale, Brush fill, Brush outer, bool emph)
		{
			if (outer == null) outer = Brushes.Black;

			x += offx;
			y += offy;

			x *= scale;
			y *= scale;
			w *= scale;
			h *= scale;

			Rectangle r = new Rectangle();
			r.Width = w;
			r.Height = h;

			if (fill != null)
			{
				r.Fill = fill;
				r.Stretch = Stretch.Fill;
			}
			r.Stroke = outer;
			r.StrokeThickness = 1.5;

			if (emph)
			{
				r.Fill = Brushes.Black;
				r.Stretch = Stretch.Fill;
				r.StrokeThickness = 3;
			}

			Canvas.SetLeft(r, x);
			Canvas.SetTop(r, y);

			canvas.Children.Add(r);
		}

		private void drawLine(double offx, double offy, double x1, double y1, double x2, double y2, double scale, Brush outer = null)
		{
			if (outer == null) outer = Brushes.Black;

			x1 += offx;
			y1 += offy;
			x2 += offx;
			y2 += offy;

			x1 *= scale;
			y1 *= scale;
			y2 *= scale;
			x2 *= scale;

			Line r = new Line();
			r.X1 = x1;
			r.Y1 = y1;
			r.X2 = x2;
			r.Y2 = y2;

			r.Stroke = outer;
			r.StrokeThickness = 1;

			canvas.Children.Add(r);
		}
	}
}
