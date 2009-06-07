using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Shapes;
using Commons.Music.Midi.Player;

namespace mldsp
{
	public class PlayerStatusPanel : Canvas
	{
		public PlayerStatusPanel ()
		{
			progress_slot = new Rectangle () { Width = 180, Height = 8 };
			Canvas.SetTop (progress_slot, 0);
			Canvas.SetLeft (progress_slot, 10);
			progress = new Rectangle () { Width = 0, Height = 8 };
			Canvas.SetTop (progress, 10);
			Canvas.SetLeft (progress, 10);
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

		Rectangle progress_slot, progress;
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
