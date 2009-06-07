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

		PlayerStatusPanel player_status_panel;
		PlayTimeStatusPanel play_time_status_panel;
		ParameterVisualizerPanel [] parameter_visualizers;

		protected override void OnApplicationSetup ()
		{
#if !Moonlight
			if (output == null)
				output = MidiDeviceManager.OpenOutput (OutputDeviceID ?? MidiDeviceManager.DefaultOutputDeviceID);
			Console.WriteLine ("Opened Device " + OutputDeviceID);
#endif
			AddParameterVisualizer ();
			AddPlayerStatusPanel ();
			AddPlayTimeStatusPanel ();
		}

		void AddPlayerStatusPanel ()
		{
			var p = new PlayerStatusPanel ();
			p.FontSize = 10;
			p.InactiveBrush = new SolidColorBrush (color_dark);
			p.ActiveBrush = new SolidColorBrush (color_ch_colored);
			Canvas.SetLeft (p, 400);
			Canvas.SetTop (p, 50);
			p.PlayClicked += delegate { Play (); };
			p.PauseClicked += delegate { Pause (); };
			p.StopClicked += delegate { Stop (); };
			p.FastForwardMouseDown += delegate { StartFF (); };
			p.RewindMouseDown += delegate { StartRewind (); };
			p.LoadClicked += delegate { SelectFile (); };
			
			Host.Children.Add (p);
			player_status_panel = p;
		}

		void AddPlayTimeStatusPanel ()
		{
			var p = new PlayTimeStatusPanel ();
			p.LabelFontSize = 10;
			p.ValueFontSize = 16;
			p.Brush = new SolidColorBrush (color_usual);
			Canvas.SetLeft (p, 560);
			Canvas.SetTop (p, 50);
			Host.Children.Add (p);
			play_time_status_panel = p;
		}

		void AddParameterVisualizer ()
		{
			parameter_visualizers = new ParameterVisualizerPanel [16];
			for (int i = 0; i < parameter_visualizers.Length; i++) {
				var p = new ParameterVisualizerPanel ();
				p.Location = new Point (80, i * 32);
				p.FontSize = 7;
				p.Foreground = new SolidColorBrush (Color.FromArgb (255, 96, 160, 255));
				parameter_visualizers [i] = p;
				Host.Children.Add (p);
			}
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
					LoadFile (dialog.File, dialog.File.OpenRead ());
					Play ();
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

		public void LoadFile (FileInfo filename, Stream stream)
		{
			Stop ();
			var reader = new SmfReader (stream);
			reader.Parse ();
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

			play_time_status_panel.TotalTime = player.GetTotalPlayTimeMilliseconds ();
			player.StartLoop ();
			Console.WriteLine ("Player loop started");
		}
		
		void Pause ()
		{
			if (player != null) {
				player.PauseAsync ();
				player_status_panel.State = PlayerState.Paused;
			}
		}

		void Stop ()
		{
			if (player != null) {
				player.Dispose ();
				player_status_panel.State = PlayerState.Stopped;
			}
		}

		void Play ()
		{
			if (player != null) {
				player.PlayAsync ();
				player_status_panel.State = PlayerState.Playing;
			}
		}

		void StartFF ()
		{
			MessageBox.Show ("Oops, not supported");
		}

		void StartRewind ()
		{
			MessageBox.Show ("Oops, not supported");
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
					parameter_visualizers [m.Channel].Volume.SetValue (m.Lsb);
					break;
				case SmfCC.Expression:
					parameter_visualizers [m.Channel].Expression.SetValue (m.Lsb);
					break;
				case SmfCC.Rsd:
					parameter_visualizers [m.Channel].Rsd.SetValue (m.Lsb);
					break;
				case SmfCC.Csd:
					parameter_visualizers [m.Channel].Csd.SetValue (m.Lsb);
					break;
				case SmfCC.Hold:
					parameter_visualizers [m.Channel].Hold.Value = (m.Lsb > 63);
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
					parameter_visualizers [m.Channel].PortamentoSwitch.Value = (m.Lsb > 63);
					break;
				case SmfCC.Sostenuto:
					parameter_visualizers [m.Channel].Sostenuto.Value = (m.Lsb > 63);
					break;
				case SmfCC.SoftPedal:
					parameter_visualizers [m.Channel].SoftPedal.Value = (m.Lsb > 63);
					break;
				}
				break;
			case SmfMessage.Meta:
				switch (m.MetaType) {
				case SmfMetaType.Tempo:
					this.play_time_status_panel.Tempo = (int) ((60.0 / SmfMetaType.GetTempo (m.Data)) * 1000000.0);
					break;
				}
				break;
			}
		}
	}
}
