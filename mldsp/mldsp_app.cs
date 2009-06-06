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
#if Moonlight
using System.Windows.Browser;
using MidiOutput = System.IntPtr;
#else
using PortMidiSharp;
#endif

namespace mldsp
{
	public partial class App : ProcessingApplication
	{
		static Panel white_key_panel, black_key_panel;
		static Rectangle [,] key_rectangles = new Rectangle [16,128 - 24];

		protected override void OnApplicationSetup ()
		{
			AddFileSelectionTextBoxHack ();
			AddParameterVisualizer ();
		}
		
		public ParameterVisualizerPanel [] ParameterVisualizers { get; private set; }

		void AddParameterVisualizer ()
		{
			ParameterVisualizers = new ParameterVisualizerPanel [16];
			for (int i = 0; i < ParameterVisualizers.Length; i++) {
				var p = new ParameterVisualizerPanel ();
				p.Location = new Point (80, i * 32);
				p.FontSize = 7;
				p.Foreground = new SolidColorBrush (Color.FromArgb (255, 192, 192, 255));
				ParameterVisualizers [i] = p;
				Host.Children.Add (p);
			}
		}

		void AddFileSelectionTextBoxHack ()
		{
			TextBlock tb = new TextBlock () { Width = 300, Height = 50 };
			tb.Text = "Click Here and select a MIDI file (it sometimes fails; retry in such case)";
			tb.Foreground = new SolidColorBrush (Color.FromArgb (255, 255, 255, 255));
			Canvas.SetLeft (tb, 400);
			Canvas.SetTop (tb, 400);
			tb.MouseLeftButtonUp += delegate { SelectFile (); };
			Host.Children.Add (tb);
		}

		Dispatcher disp;
		MidiOutput output;
		MidiPlayer player;
		MidiMachine registers;

		void SelectFile ()
		{
#if Moonlight
			disp = HtmlPage.Window.Dispatcher;
#else
			disp = Deployment.Current.Dispatcher;
#endif
			var dialog = new OpenFileDialog ();
			dialog.Multiselect = false;
			if ((bool) dialog.ShowDialog ()) {
				try {
					Play (dialog.File, dialog.File.OpenRead ());
				} catch (Exception ex) {
#if Moonlight
					System.Windows.Browser.HtmlPage.Window.Alert (ex.ToString ());
#else
					Console.WriteLine (ex);
#endif
				}
			}
		}

		public SmfMusic Music { get; set; }

		public int? OutputDeviceID { get; set; }

		public void Play (FileInfo filename, Stream stream)
		{
#if !Moonlight
			if (output == null)
				output = MidiDeviceManager.OpenOutput (OutputDeviceID ?? MidiDeviceManager.DefaultOutputDeviceID);
			Console.WriteLine ("Opened Device " + OutputDeviceID);
#endif
			var reader = new SmfReader (stream);
			reader.Parse ();
			if (player != null) {
				player.PauseAsync ();
				// FIXME: it should dispose the player, but it causes crash
			}
			Music = reader.Music;
#if Moonlight
			player = new MidiPlayer (Music);
#else
			player = new PortMidiPlayer (output, Music);
#endif
			registers = new MidiMachine ();
			player.MessageReceived += delegate (SmfMessage m) {
				registers.ProcessMessage (m);
			};
			registers.MessageReceived += delegate (SmfMessage m) {
				disp.BeginInvoke (() => HandleSmfMessage (m));
			};

			player.StartLoop ();
			player.PlayAsync ();
		}

		static readonly int [] key_to_keyboard_idx = {0, 0, 1, 1, 2, 3, 3, 4, 4, 5, 5, 6};
		static bool IsWhiteKey (int note)
		{
			switch (note % 12) {
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

		void HandleSmfMessage (SmfMessage m)
		{
			switch (m.MessageType) {
			case SmfMessage.NoteOn:
				if (m.Lsb == 0)
					goto case SmfMessage.NoteOff; // It is equivalent to note off
				int note = GetKeyIndexForNote (m.Msb);
				if (note < 0)
					break; // out of range
				key_rectangles [m.Channel, note].Fill = new SolidColorBrush (color_keyon);
				break;
			case SmfMessage.NoteOff:
				note = GetKeyIndexForNote (m.Msb);
				if (note < 0)
					break; // out of range
				Color c = registers.Channels [m.Channel].Controls [0x40] > 63 ? color_hold :
					IsWhiteKey (note) ? color_white_key : color_black_key;
				key_rectangles [m.Channel, note].Fill = new SolidColorBrush (c);
				break;
			case SmfMessage.CC:
				switch (m.Msb) {
				case SmfCC.Volume:
					ParameterVisualizers [m.Channel].Volume.SetValue (m.Lsb);
					break;
				case SmfCC.Expression:
					ParameterVisualizers [m.Channel].Expression.SetValue (m.Lsb);
					break;
				case SmfCC.Rsd:
					ParameterVisualizers [m.Channel].Rsd.SetValue (m.Lsb);
					break;
				case SmfCC.Csd:
					ParameterVisualizers [m.Channel].Csd.SetValue (m.Lsb);
					break;
				case SmfCC.Hold:
					ParameterVisualizers [m.Channel].Hold.Value = (m.Lsb > 63);
					if (m.Lsb < 64 && key_rectangles != null) { // reset held keys to nothing
						for (int i = 0; i < 128; i++) {
							note = GetKeyIndexForNote (i);
							if (note < 0)
								continue;
							var rect = key_rectangles [m.Channel, note];
							if (rect == null)
								continue;
							if (((SolidColorBrush) rect.Fill).Color == color_hold)
								key_rectangles [m.Channel, note].Fill = new SolidColorBrush (IsWhiteKey (i) ? color_white_key : color_black_key);
						}
					}
					break;
				case SmfCC.PortamentoSwitch:
					ParameterVisualizers [m.Channel].PortamentoSwitch.Value = (m.Lsb > 63);
					break;
				case SmfCC.Sostenuto:
					ParameterVisualizers [m.Channel].Sostenuto.Value = (m.Lsb > 63);
					break;
				case SmfCC.SoftPedal:
					ParameterVisualizers [m.Channel].SoftPedal.Value = (m.Lsb > 63);
					break;
				}
				break;
			}
		}
	}
}
