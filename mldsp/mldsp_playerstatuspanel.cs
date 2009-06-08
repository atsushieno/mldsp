using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Controls;
using System.Windows.Shapes;
using Commons.Music.Midi.Player;

namespace mldsp
{
	public class PlayerStatusPanel : Canvas, IPlayerStatusView
	{
		Rectangle progress_slot, progress;
		Storyboard progress_story;

		public PlayerStatusPanel ()
		{
			progress_slot = new Rectangle () { Width = 140, Height = 8 };
			Canvas.SetTop (progress_slot, 8);
			Canvas.SetLeft (progress_slot, 10);
			progress = new Rectangle () { Width = 0, Height = 8 };
			Canvas.SetTop (progress, 8);
			Canvas.SetLeft (progress, 10);
			progress_story = new Storyboard ();
			Storyboard.SetTarget (progress_story, progress);
			Storyboard.SetTargetProperty (progress_story, new PropertyPath ("Width"));
			progress_story.Children.Add (new DoubleAnimation () { From = 0, To = 140 });
			AddText (PlayerState.Playing, "Play", 40, 20, null,
			         delegate (object o, MouseButtonEventArgs a) { if (PlayClicked != null) PlayClicked (o, a); });
			AddText (PlayerState.Paused, "Pause", 80, 20, null,
			         delegate (object o, MouseButtonEventArgs a) { if (PauseClicked != null) PauseClicked (o, a); });
			AddText (PlayerState.Stopped, "Stop", 120, 20, null,
			         delegate (object o, MouseButtonEventArgs a) { if (StopClicked != null) StopClicked (o, a); });
			AddText (PlayerState.FastForward, "FF", 40, 34,
			         delegate (object o, MouseButtonEventArgs a) { if (FastForwardMouseDown != null) FastForwardMouseDown (o, a); },
			         delegate (object o, MouseButtonEventArgs a) { if (FastForwardMouseUp != null) FastForwardMouseUp (o, a); });
			AddText (PlayerState.Rewind, "Rew", 80, 34,
			         delegate (object o, MouseButtonEventArgs a) { if (RewindMouseDown != null) RewindMouseDown (o, a); },
			         delegate (object o, MouseButtonEventArgs a) { if (RewindMouseUp != null) RewindMouseUp (o, a); });
			AddText (PlayerState.Loading, "Load", 120, 34, null,
			         delegate (object o, MouseButtonEventArgs a) { if (LoadClicked != null) LoadClicked (o, a); });

			Children.Add (progress_slot);
			Children.Add (progress);
			foreach (var item in items)
				// FIXME: add item icon too
				Children.Add (item.Label);
		}

		public event MouseButtonEventHandler PlayClicked;
		public event MouseButtonEventHandler PauseClicked;
		public event MouseButtonEventHandler StopClicked;
		public event MouseButtonEventHandler FastForwardMouseDown;
		public event MouseButtonEventHandler FastForwardMouseUp;
		public event MouseButtonEventHandler RewindMouseDown;
		public event MouseButtonEventHandler RewindMouseUp;
		public event MouseButtonEventHandler LoadClicked;

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

		public void ProcessBeginPlay (MidiPlayer player, int totalMilliseconds)
		{
			progress_story.Children [0].Duration = new Duration (TimeSpan.FromMilliseconds (totalMilliseconds));
			progress_story.Begin ();
			State = PlayerState.Playing;
		}
		
		public void ProcessSkip (int seekMilliseconds)
		{
			progress_story.Seek (TimeSpan.FromMilliseconds (seekMilliseconds));
		}

		public void ProcessPause ()
		{
			progress_story.Pause ();
			State = PlayerState.Paused;
		}

		public void ProcessStop ()
		{
			progress_story.Stop ();
			State = PlayerState.Stopped;
		}

		public void ProcessResume ()
		{
			progress_story.Resume ();
			State = PlayerState.Playing;
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
