using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using ProcessingCli;
using Commons.Music.Midi;
using Commons.Music.Midi.Player;

namespace mldsp
{
	public partial class App : ProcessingApplication
	{
		static Panel white_key_panel, black_key_panel;
		static Rectangle [,] key_rectangles = new Rectangle [16,128 - 24];

		protected override void OnApplicationSetup ()
		{
			Rectangle rect = new Rectangle () { Width = 100, Height = 50 };
			rect.Fill = new SolidColorBrush (Colors.Cyan);
			Canvas.SetLeft (rect, 600);
			Canvas.SetTop (rect, 400);
			rect.MouseLeftButtonUp += delegate { SelectFile (); };
			Host.Children.Add (rect);
		}

		void SelectFile ()
		{
			var dialog = new OpenFileDialog ();
			dialog.Multiselect = false;
			if ((bool) dialog.ShowDialog ()) {
				try {
					Play (dialog.File, dialog.File.OpenRead ());
				} catch (Exception ex) {
					System.Windows.Browser.HtmlPage.Window.Alert (ex.ToString ());
				}
			}
		}

		PortMidiPlayer player;
		Dispatcher disp = System.Windows.Browser.HtmlPage.Window.Dispatcher;
		public SmfMusic Music { get; set; }

		public void Play (FileInfo filename, Stream stream)
		{
			var reader = new SmfReader (stream);
			reader.Parse ();

			//player.Stop ();
			Music = reader.Music;
			player = new PortMidiPlayer (IntPtr.Zero, Music);
			player.MessageReceived += delegate(SmfEvent ev) {
				disp.BeginInvoke (() => HandleSmfEvent (ev));
			};

			TextBlock tb = new TextBlock () { Width = 200, Height = 50 };
			tb.Foreground = new SolidColorBrush (Color.FromArgb (30, 255, 128, 128));
			Canvas.SetLeft (tb, 200);
			Canvas.SetTop (tb, 100);
			tb.Text = "NOTE ON";
			note_text = tb;
			Host.Children.Add (tb);

			player.StartLoop ();
			player.PlayAsync ();
		}

		TextBlock note_text;

		static readonly int [] key_to_keyboard_idx = {0, 0, 1, 1, 2, 3, 3, 4, 4, 5, 5, 6};
		static bool IsWhiteKey (int key)
		{
			switch (key) {
			case 0: case 2: case 4: case 5: case 7: case 9: case 11:
					return true;
			default:
				return false;
			}
		}

		int GetKeyIndexForNote (int value)
		{
			int note = value - 24;
			if (note < 0 || 128 - 24 < note)
				return -1;
			return note;
		}
		public void HandleSmfEvent (SmfEvent e)
		{
			switch (e.Message.MessageType) {
			case SmfMessage.NoteOn:
				if (e.Message.Lsb == 0)
					goto case SmfMessage.NoteOff; // It is equivalent to note off
				int note = GetKeyIndexForNote (e.Message.Msb);
				if (note < 0)
					break; // out of range
				key_rectangles [e.Message.Channel, note].Fill = new SolidColorBrush (color_keyon);
				break;
			case SmfMessage.NoteOff:
				note = GetKeyIndexForNote (e.Message.Msb);
				if (note < 0)
					break; // out of range
				if (IsWhiteKey (note % 12))
					key_rectangles [e.Message.Channel, note].Fill = new SolidColorBrush (color_white_key);
				else
					key_rectangles [e.Message.Channel, note].Fill = new SolidColorBrush (color_black_key);
				break;
			}
		}
	}
}
