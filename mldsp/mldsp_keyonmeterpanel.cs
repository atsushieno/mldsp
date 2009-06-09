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

		public KeyonMeterPanel ()
		{
			for (int i = 0; i < keyon_meter_frames.Length; i++) {
				var rf = new Rectangle () { Width = 16, Height = 66 };
				keyon_meter_frames [i] = rf;
				Canvas.SetLeft (rf, i * 20);
				Canvas.SetTop (rf, 0);
				Children.Add (rf);

				var r = new Rectangle () { Width = 16 - 2, Height = 64 };
				keyon_meters [i] = r;
				r.Fill = new SolidColorBrush (App.color_background);
				Canvas.SetLeft (r, i * 20 + 1);
				Canvas.SetTop (r, 0 + 1);
				Children.Add (r);
				
				var s = new Storyboard ();
				s.Duration = TimeSpan.FromSeconds (4);
				Storyboard.SetTarget (s, r);
				Storyboard.SetTargetProperty (s, new PropertyPath ("Height"));
				s.Children.Add (new DoubleAnimation () { By = 4 });
				keyon_storyboards [i] = s;
				
				var cf = new Ellipse () { Width = 16, Height = 16 };
				pan_frames [i] = cf;
				Canvas.SetLeft (cf, i * 20);
				Canvas.SetTop (cf, 68);
				Children.Add (cf);
				
				var ci = new Ellipse () { Width = 4, Height = 4 };
				ci.Fill = new SolidColorBrush (App.color_ch_colored);
				pan_indicators [i] = ci;
				SetPan (i, 64);
				Children.Add (ci);
			}
		}
		
		public void SetPan (int channel, byte value)
		{
			var p = pan_indicators [channel];
			Canvas.SetLeft (p, 6 + channel * 20 - 3 * Math.Cos (Math.PI * value / 128));
			Canvas.SetTop (p, 73 - 3 * Math.Sin (Math.PI * value / 128));
		}
		
		public Brush Stroke {
			get { return keyon_meter_frames [0].Stroke; }
			set {
				foreach (var r in keyon_meter_frames)
					r.Stroke = value;
				foreach (var c in pan_frames)
					c.Stroke = value;
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
