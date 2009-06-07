using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace mldsp
{
	public class PlayStatusPanel : Canvas
	{
		public PlayStatusPanel ()
		{
			for (int i = 0; i < 4; i++) {
				var r = new Rectangle () { Width = 5, Height = 18 };
				Canvas.SetTop (r, i * 22 + 6);
				Canvas.SetLeft (r, 1);
				Children.Add (r);
				rects.Add (r);
			}
			AddText ("Passed", 8, 2, true);
			AddText ("Time", 16, 12, true);
			AddText ("Tick", 8, 24, true);
			AddText ("Count", 12, 34, true);
			AddText ("Tempo", 8, 46, true);
			AddText ("Volume", 8, 68, true);
			AddText ("Ratio", 12, 78, true);

			AddText ("00:00:00", 60, 2, false);
			passed_time = last;
			AddText ("00000000", 60, 24, false);
			tick_count = last;
			AddText ("00000000", 60, 46, false);
			tempo = last;
			tempo.Tag = 0;
			AddText ("100%", 60, 68, false);
			volume_ratio = last;
			volume_ratio.Tag = 0;
			last = null;
		}

		TextBlock passed_time;
		TextBlock tick_count;
		TextBlock tempo;
		TextBlock volume_ratio;

		public int Tempo {
			get { return (int) tempo.Tag; }
			set {
				tempo.Tag = value;
				tempo.Text = value.ToString ("D08");
			}
		}

		public int VolumeRatio {
			get { return (int) volume_ratio.Tag; }
			set {
				volume_ratio.Tag = value;
				volume_ratio.Text = value.ToString ("D02%");
			}
		}

		public double LabelFontSize {
			get { return labels [0].FontSize; }
			set {
				foreach (var tb in labels)
					tb.FontSize = value;
			}
		}

		public double ValueFontSize {
			get { return values [0].FontSize; }
			set {
				foreach (var tb in values)
					tb.FontSize = value;
			}
		}

		public Brush Brush {
			get { return labels [0].Foreground; }
			set {
				foreach (var r in rects)
					r.Fill = value;
				foreach (var tb in labels)
					tb.Foreground = value;
				foreach (var tb in values)
					tb.Foreground = value;
			}
		}

		List<Rectangle> rects = new List<Rectangle> ();
		List<TextBlock> labels = new List<TextBlock> ();
		List<TextBlock> values = new List<TextBlock> ();
		TextBlock last;

		void AddText (string label, int x, int y, bool isLabel)
		{
			var tb = new TextBlock () { Text = label };
			Canvas.SetLeft (tb, x);
			Canvas.SetTop (tb, y);
			Children.Add (tb);
			if (isLabel)
				labels.Add (tb);
			else
				values.Add (tb);
			last = tb;
		}
	}
}
