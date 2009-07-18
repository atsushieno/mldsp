using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Threading;
using Commons.Music.Midi.Player;

namespace mldsp
{
	public class KeyonMeterPanel : Canvas
	{
		Rectangle [] keyon_meter_frames = new Rectangle [16];
		Rectangle [] keyon_meters = new Rectangle [16];
		Storyboard [] keyon_storyboards = new Storyboard [16];
		Ellipse [] pan_frames = new Ellipse [16];
		Ellipse [] pan_indicators = new Ellipse [16];
		TextBlock [] prog_values = new TextBlock [16];
		TextBlock [] bank_values = new TextBlock [16];
		TextBlock prog_label, bank_label;

		public KeyonMeterPanel ()
		{
			for (int i = 0; i < keyon_meter_frames.Length; i++) {
				var rf = new Rectangle () { Width = 16, Height = 66 };
				keyon_meter_frames [i] = rf;
				Canvas.SetLeft (rf, i * 22 + 8);
				Canvas.SetTop (rf, 0);
				Children.Add (rf);

				var r = new Rectangle () { Width = 16 - 2, Height = 64 };
				keyon_meters [i] = r;
				r.Fill = new SolidColorBrush (App.color_background);
				Canvas.SetLeft (r, i * 22 + 9);
				Canvas.SetTop (r, 0 + 1);
				Children.Add (r);
				
				for (int x = 0; x < 66 - 3; x += 3) {
					var xl = new Rectangle () { Width = 14, Height = 2};
					xl.Stroke = new SolidColorBrush (App.color_background);
					Canvas.SetLeft (xl, i * 22 + 9);
					Canvas.SetTop (xl, 0 + 1 + x);
					Children.Add (xl);
				}
				
				var s = new Storyboard ();
				s.Duration = TimeSpan.FromSeconds (4);
				Storyboard.SetTarget (s, r);
				Storyboard.SetTargetProperty (s, new PropertyPath ("Height"));
				s.Children.Add (new DoubleAnimation () { By = 8 });
				keyon_storyboards [i] = s;
				
				var cf = new Ellipse () { Width = 16, Height = 16 };
				pan_frames [i] = cf;
				Canvas.SetLeft (cf, i * 22 + 8);
				Canvas.SetTop (cf, 68);
				Children.Add (cf);
				
				var ci = new Ellipse () { Width = 4, Height = 4 };
				ci.Fill = new SolidColorBrush (App.color_ch_colored);
				pan_indicators [i] = ci;
				SetPan (i, 64);
				Children.Add (ci);

				var pv = new TextBlock () { Text = "000", FontSize = 8 };
				Canvas.SetLeft (pv, i * 22 + 8);
				Canvas.SetTop (pv, 84);
				prog_values [i] = pv;
				Children.Add (pv);

				var bv = new TextBlock () { Text = "000", FontSize = 8 };
				bv.Tag = 0;
				Canvas.SetLeft (bv, i * 22 + 8);
				Canvas.SetTop (bv, 94);
				bank_values [i] = bv;
				Children.Add (bv);
			}

			var pl = new TextBlock () { Text = "P", FontSize = 8 };
			Canvas.SetLeft (pl, 0);
			Canvas.SetTop (pl, 84);
			prog_label = pl;
			Children.Add (pl);
				
			var bl = new TextBlock () { Text = "B", FontSize = 8 };
			Canvas.SetLeft (bl, 0);
			Canvas.SetTop (bl, 94);
			bank_label = bl;
			Children.Add (bl);
		}
		
		public void SetProgram (int channel, byte value)
		{
			prog_values [channel].Text = value.ToString ("D03");
		}
		
		public void SetBank (int channel, byte value, bool msb)
		{
			var b = bank_values [channel];
			int current = (int) b.Tag;
			if (msb)
			//	current = (current & 0x7F) + (value << 7);
			//else
				current = (current & 0x3F80) + value;
			b.Tag = current;
			b.Text = current.ToString ("D03");
		}
		
		public void SetPan (int channel, byte value)
		{
			var p = pan_indicators [channel];
			Canvas.SetLeft (p, 6 + channel * 22 + 8 - 3 * Math.Cos (Math.PI * value / 128));
			Canvas.SetTop (p, 73 - 3 * Math.Sin (Math.PI * value / 128));
		}
		
		public Brush Stroke {
			get { return keyon_meter_frames [0].Stroke; }
			set {
				foreach (var r in keyon_meter_frames)
					r.Stroke = value;
				foreach (var c in pan_frames)
					c.Stroke = value;
				foreach (var p in prog_values)
					p.Foreground = value;
				foreach (var b in bank_values)
					b.Foreground = value;
				prog_label.Foreground = value;
				bank_label.Foreground = value;
			}
		}
		
		public Brush Fill {
			get { return keyon_meter_frames [0].Fill; }
			set {
				foreach (var r in keyon_meter_frames)
					r.Fill = value;
			}
		}
		
		public void ProcessKeyOn (byte channel, byte velocity)
		{
			var s = keyon_storyboards [channel];
			s.Stop ();
			var m = keyon_meters [channel];
			var d = (DoubleAnimation) s.Children [0];
			d.From = 1 + 64 - velocity / 2.0; // 2.0 = (64dots / 128.0 valrange)
			d.To = 64;//velocity / 2.0;
			Canvas.SetTop (m, 1);
			s.Begin ();
		}
	}
}
