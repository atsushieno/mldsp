using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Threading;
using Commons.Music.Midi.Player;

namespace mldsp
{
	public class PlayerStatusPanel : Canvas, IPlayerStatusView
	{
		Rectangle progress_slot, progress;
		Storyboard progress_story;
		Line [] circle_lines = new Line [20];
		DispatcherTimer circle_timer;

		public PlayerStatusPanel ()
		{
			progress = new Rectangle () { Width = 0, Height = 8 };
			Canvas.SetTop (progress, 8);
			Canvas.SetLeft (progress, 10);
			progress_slot = new Rectangle () { Width = 140, Height = 8 };
			Canvas.SetTop (progress_slot, 8);
			Canvas.SetLeft (progress_slot, 10);
			progress_slot.MouseLeftButtonUp += delegate (object o, MouseButtonEventArgs a) {
				if (ProgressClicked != null)
					ProgressClicked (progress_slot, a);
			};
			progress_story = new Storyboard ();
			Storyboard.SetTarget (progress_story, progress);
			Storyboard.SetTargetProperty (progress_story, new PropertyPath ("Width"));
			progress_story.Children.Add (new DoubleAnimation () { From = 0, To = 140 });

			Children.Add (progress_slot);
			Children.Add (progress);

			// play status/controller items
			AddText (PlayerState.Playing, "Play", 50, 20, null,
			         delegate (object o, MouseButtonEventArgs a) { if (PlayClicked != null) PlayClicked (o, a); });
			AddText (PlayerState.Paused, "Pause", 90, 20, null,
			         delegate (object o, MouseButtonEventArgs a) { if (PauseClicked != null) PauseClicked (o, a); });
			AddText (PlayerState.Stopped, "Stop", 130, 20, null,
			         delegate (object o, MouseButtonEventArgs a) { if (StopClicked != null) StopClicked (o, a); });
			AddText (PlayerState.FastForward, "FF", 50, 34,
			         delegate (object o, MouseButtonEventArgs a) { if (FastForwardMouseDown != null) FastForwardMouseDown (o, a); },
			         delegate (object o, MouseButtonEventArgs a) { if (FastForwardMouseUp != null) FastForwardMouseUp (o, a); });
			AddText (PlayerState.Rewind, "Rew", 90, 34,
			         delegate (object o, MouseButtonEventArgs a) { if (RewindMouseDown != null) RewindMouseDown (o, a); },
			         delegate (object o, MouseButtonEventArgs a) { if (RewindMouseUp != null) RewindMouseUp (o, a); });
			AddText (PlayerState.Loading, "Load", 130, 34, null,
			         delegate (object o, MouseButtonEventArgs a) { if (LoadClicked != null) LoadClicked (o, a); });

			// FIXME: add path figures for each items (PathFigureCollection.Parse() would be useful,
			// but it does not exist in Moonlight yet ...)
			foreach (var item in items)
				Children.Add (item.Label);

			// circle meter
			for (int i = 0; i < circle_lines.Length; i++) {
				var l = new Line () { X1 = 16, Y1 = 36,
					X2 = 16 + 16 * Math.Cos (Math.PI * i / circle_lines.Length * 2),
					Y2 = 36 + 16 * Math.Sin (Math.PI * i / circle_lines.Length * 2) };
				circle_lines [i] = l;
			}
			circle_timer = new DispatcherTimer ();
			circle_timer.Interval = TimeSpan.FromMinutes (1.0 / current_bpm / circle_lines.Length * 2);
			circle_timer.Tick += delegate { CircleTick (); };
			var el = new Ellipse () { Width = 16, Height = 16 };
			Canvas.SetTop (el, 36 - el.Width / 2);
			Canvas.SetLeft (el, 16 - el.Height / 2);
			el.Fill = new SolidColorBrush (App.color_background);

			foreach (var l in circle_lines)
				Children.Add (l);
			Children.Add (el);
		}

		public event MouseButtonEventHandler PlayClicked;
		public event MouseButtonEventHandler PauseClicked;
		public event MouseButtonEventHandler StopClicked;
		public event MouseButtonEventHandler FastForwardMouseDown;
		public event MouseButtonEventHandler FastForwardMouseUp;
		public event MouseButtonEventHandler RewindMouseDown;
		public event MouseButtonEventHandler RewindMouseUp;
		public event MouseButtonEventHandler LoadClicked;
		public event MouseButtonEventHandler ProgressClicked;

		List<PlayerStatusItem> items = new List<PlayerStatusItem> ();

		void AddText (PlayerState state, string text, int x, int y, MouseButtonEventHandler mouseDown, MouseButtonEventHandler mouseUp)
		{
			var l = new PlayerStatusItem (state);
			l.Label.Text = text;
			Canvas.SetLeft (l.Label, x);
			Canvas.SetTop (l.Label, y);
			items.Add (l);
			if (mouseDown != null)
				l.Label.MouseLeftButtonDown += mouseDown;
			l.Label.MouseLeftButtonUp += mouseUp;
			l.Label.MouseEnter += delegate { l.Label.Opacity = 0.8; };
			l.Label.MouseLeave += delegate { l.Label.Opacity = 1.0; };
		}

		public double FontSize {
			get { return items [0].Label.FontSize; }
			set {
				foreach (var item in items)
					item.Label.FontSize = value;
			}
		}
		
		public Brush InactiveBrush {
			get { return progress_slot.Stroke; }
			set {
				progress_slot.Stroke = value;
				progress.Stroke = value;
				ResetLabelColors ();
				foreach (var l in circle_lines)
					l.Stroke = value;
			}
		}

		public Brush ActiveBrush {
			get { return progress.Fill; }
			set {
				progress.Fill = value;
				ResetLabelColors ();
			}
		}

		PlayerState state = PlayerState.Stopped;
		public PlayerState State {
			get { return state; }
			set {
				state = value;
				ResetLabelColors ();
			}
		}

		void ResetLabelColors ()
		{
			foreach (var item in items) {
				if (item.State == state)
					item.Foreground = ActiveBrush;
				else
					item.Foreground = InactiveBrush;
			}
		}

		int circle_index = 0;

		void CircleTick ()
		{
			circle_lines [circle_index].Stroke = progress_slot.Stroke;
			circle_index++;
			circle_index %= circle_lines.Length;
			circle_lines [circle_index].Stroke = progress.Fill;
		}

		public void ProcessBeginPlay (MidiPlayer player, int totalMilliseconds)
		{
			progress_story.Stop ();
			var a = (DoubleAnimation) progress_story.Children [0];
			a.Duration = new Duration (TimeSpan.FromMilliseconds (totalMilliseconds));
			a.From = 0;
			progress_story.Begin ();
			circle_timer.Start ();
			State = PlayerState.Playing;
		}
		
		public void ProcessSkip (int seekMilliseconds)
		{
			progress_story.Stop ();
			progress_story.Begin ();
			progress_story.Seek (TimeSpan.FromMilliseconds (seekMilliseconds));
			circle_timer.Start ();
			State = PlayerState.Playing;
		}

		public void ProcessPause ()
		{
			progress_story.Pause ();
			circle_timer.Stop ();
			State = PlayerState.Paused;
		}

		public void ProcessStop ()
		{
			progress_story.Stop ();
			circle_timer.Stop ();
			circle_lines [circle_index].Stroke = progress_slot.Stroke;
			circle_index = 0;
			State = PlayerState.Stopped;
		}

		public void ProcessResume ()
		{
			progress_story.Resume ();
			circle_timer.Start ();
			State = PlayerState.Playing;
		}

		int current_bpm = 120;
		public void ProcessChangeTempo (int bpm)
		{
			circle_timer.Stop ();
			current_bpm = bpm;
			circle_timer.Interval = GetCircleTimerInterval ();
			circle_timer.Start ();
		}
		
		TimeSpan GetCircleTimerInterval ()
		{
			return TimeSpan.FromMinutes (1.0 / current_bpm / current_tempo_ratio / circle_lines.Length * 4);
		}

		TimeSpan adjustation = TimeSpan.Zero;
		double current_tempo_ratio = 1.0;

		public void ProcessChangeTempoRatio (double ratio)
		{
			current_tempo_ratio = ratio;
			var cur = progress_story.GetCurrentTime ();
			double start = progress.Width;
			progress_story.Stop ();
			var a = (DoubleAnimation) progress_story.Children [0];
			a.From = start;
			a.Duration = TimeSpan.FromMilliseconds ((a.Duration.TimeSpan - cur).TotalMilliseconds);
			a.SpeedRatio = ratio;
			adjustation = cur;
			progress_story.Begin ();
			circle_timer.Stop ();
			circle_timer.Interval = GetCircleTimerInterval ();
			circle_timer.Start ();
		}
	}
	
	public class PlayerStatusItem : Panel
	{
		public PlayerStatusItem (PlayerState state)
		{
			State = state;
			Label = new TextBlock ();
		}
		
		public Path Path { get; set; }
		public TextBlock Label { get; private set; }
		public PlayerState State { get; private set; }
		public Brush Foreground {
			get { return Label.Foreground; }
			set {
				if (Path != null)
					Path.Stroke = value;
				Label.Foreground = value;
			}
		}
	}
}
