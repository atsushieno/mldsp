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
		static Rect GetKeyboardRect (int channel, int note)
		{
			int octave = note / 12;
			int key = note % 12;
			double x = octave * key_width * 7;
			double y = getChannelYPos (channel) + ch_height - key_height;
			int k = key_to_keyboard_idx [key];
			if (IsWhiteKey (key))
				return new Rect (x + k * key_width, y, key_width, key_height);
			else {
				double blackKeyStartX = x + (k + 0.8) * key_width;
				return new Rect (blackKeyStartX, y + 1, blackKeyWidth, blackKeyHeight);
			}
		}

		public void HandleSmfEvent (SmfEvent e)
		{
			switch (e.Message.MessageType) {
			case SmfMessage.NoteOn:
				Rect r = GetKeyboardRect (e.Message.Channel, e.Message.Msb);
				var rect = new Rectangle () { Width = r.Width, Height = r.Height };
				Canvas.SetLeft (rect, r.X);
				Canvas.SetTop (rect, r.Y);
				rect.Fill = new SolidColorBrush (color_keyon);
				if (IsWhiteKey (e.Message.Msb % 12))
					white_key_panel.Children.Add (rect);
				else
					black_key_panel.Children.Add (rect);
				break;
			case SmfMessage.NoteOff:
				//note_text.Text = "(note off)";
				break;
			}
		}
	}
}
