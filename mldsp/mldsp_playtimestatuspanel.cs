using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Threading;
using Commons.Music.Midi.Player;

namespace mldsp
{
	public class PlayTimeStatusPanel : Canvas, IPlayerStatusView
	{
		public PlayTimeStatusPanel ()
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
			AddText ("Total", 8, 24, true);
			AddText ("Time", 16, 34, true);
			AddText ("Tick", 8, 46, true);
			AddText ("Count", 12, 56, true);
			AddText ("Tempo", 8, 68, true);

			AddText ("00:00.00", 60, 6, false);
			passed_time = last;
			AddText ("00:00.00", 60, 28, false);
			total_time = last;
			AddText ("00000000", 60, 50, false);
			tick_count = last;
			tick_count.Tag = 0;
			AddText ("00000000", 60, 72, false);
			tempo = last;
			last = null;
		}

		TextBlock passed_time;
		TextBlock total_time;
		TextBlock tick_count;
		TextBlock tempo;

		public int TotalTime {
			get { return (int) total_time.Tag; }
			set {
				total_time.Tag = value;
				TimeSpan ts = TimeSpan.FromMilliseconds (value);
				total_time.Text = String.Format ("{0:D02}:{1:D02}.{2:D03}", (int) ts.TotalMinutes, (int) ts.Seconds, (int) ts.Milliseconds);
			}
		}
		
		public int Bpm {
			get { return current_bpm; }
			set {
				current_bpm = value;
				last_tempo_changed = DateTime.Now;
				tempo.Text = value.ToString ("D08");
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

		DispatcherTimer timer;
		DateTime timer_resumed;
		TimeSpan timer_offset;
		DateTime last_tempo_changed;
		int current_bpm, tick_offset;
		double tempo_ratio = 1.0;

		TimeSpan GetTimerOffsetWithTempoRatio ()
		{
			return TimeSpan.FromMilliseconds ((DateTime.Now - timer_resumed).TotalMilliseconds * tempo_ratio);
		}

		public void ProcessBeginPlay (MidiPlayer player, int totalMilliseconds)
		{
			timer = new DispatcherTimer ();
			timer.Interval = TimeSpan.FromMilliseconds (50);
			timer.Tick += delegate {
				tick_count.Text = player.PlayDeltaTime.ToString ("D08");
				TimeSpan now = GetTimerOffsetWithTempoRatio () + timer_offset;
				passed_time.Text = String.Format ("{0:D02}:{1:D02}.{2:D03}", (int) now.TotalMinutes, now.Seconds, now.Milliseconds);
			};
			timer_offset = TimeSpan.Zero;
			last_tempo_changed = timer_resumed = DateTime.Now;
			tick_offset = 0;
			timer.Start ();
		}
		
		public void ProcessSkip (int seekMilliseconds)
		{
		}

		public void ProcessPause ()
		{
			timer_offset += DateTime.Now - timer_resumed;
			timer.Stop ();
		}

		public void ProcessStop ()
		{
			timer.Stop ();
		}

		public void ProcessResume ()
		{
			timer_resumed = DateTime.Now;
			timer.Start ();
		}

		public void ProcessChangeTempoRatio (double ratio)
		{
			timer_offset += GetTimerOffsetWithTempoRatio ();
			timer_resumed = DateTime.Now;
			tempo_ratio = ratio;
		}
	}
}
