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

		public void HandleSmfEvent (SmfEvent e)
		{
			switch (e.Message.MessageType) {
			case SmfMessage.NoteOn:
				//note_text.Text = e.ToString ();
				stroke (color (255,255,255));
				rect (500, 260, 200, 50);
				fill (color (128,128,128));
				stroke (color (128,128,128));
				textSize (16);
				text (e.Message.ToString (), 500, 230);
				break;
			case SmfMessage.NoteOff:
				//note_text.Text = "(note off)";
				break;
			}
		}
	}
}
