using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
#if Moonlight
using MidiOutput = System.IntPtr;
#else
using PortMidiSharp;
#endif

namespace Commons.Music.Midi
{
	public class Driver
	{
		public static void Main (string [] args)
		{
#if Moonlight
			var output = IntPtr.Zero;
#else
			var output = MidiDeviceManager.OpenOutput (MidiDeviceManager.DefaultOutputDeviceID);
#endif
			foreach (var arg in args) {
				var parser = new SmfReader (File.OpenRead (arg));
				parser.Parse ();
#if true
/* // test reader/writer sanity
				using (var outfile = File.Create ("testtest.mid")) {
					var data = parser.Music;
					var gen = new SmfWriter (outfile);
					gen.WriteHeader (data.Format, (short)data.Tracks.Count, data.DeltaTimeSpec);
					foreach (var tr in data.Tracks)
						gen.WriteTrack (tr);
				}
*/
// test merger/splitter
/*
				var merged = SmfTrackMerger.Merge (parser.Music);
//				var result = merged;
				var result = SmfTrackSplitter.Split (merged.Tracks [0].Events, parser.Music.DeltaTimeSpec);
				using (var outfile = File.Create ("testtest.mid")) {
					var gen = new SmfWriter (outfile);
					gen.DisableRunningStatus = true;
					gen.WriteHeader (result.Format, (short)result.Tracks.Count, result.DeltaTimeSpec);
					foreach (var tr in result.Tracks)
						gen.WriteTrack (tr);
				}
*/

				var player = new PortMidiSyncPlayer (output, parser.Music);
				player.PlayerLoop ();
#else
				var player = new PortMidiPlayer (output, parser.MusicData);
				player.PlayAsync ();
				Console.WriteLine ("empty line to quit, P to pause");
				while (true) {
					string line = Console.ReadLine ();
					if (line == "p") {
						if (player.State == PlayerState.Playing)
							player.PauseAsync ();
						else
							player.PlayAsync ();
					}
					else if (line == "") {
						player.Dispose ();
						break;
					}
				}
#endif
			}
		}
	}

	public enum PlayerState
	{
		Stopped,
		Playing,
		Paused
	}

	// Player implementation. Plays a MIDI song synchronously.
	public class PortMidiSyncPlayer : IDisposable
	{
		public PortMidiSyncPlayer (MidiOutput output, SmfMusic music)
		{
			if (output == null)
				throw new ArgumentNullException ("output");
			if (music == null)
				throw new ArgumentNullException ("music");

			this.output = output;
			this.music = music;
			events = SmfTrackMerger.Merge (music).Tracks [0].Events;
		}

		MidiOutput output;
		SmfMusic music;
		IList<SmfEvent> events;
		ManualResetEvent pause_handle = new ManualResetEvent (true);
		bool pause, stop;

		public int PlayDeltaTime { get; set; }

		public void Dispose ()
		{
#if Moonlight
#else
			output.Close ();
#endif
		}

		public void Play ()
		{
			if (pause_handle != null) {
				pause_handle.Set ();
				pause_handle = null;
			}
		}

		public void Pause ()
		{
			pause = true;
		}

		int event_idx = 0;

		public void PlayerLoop ()
		{
			while (true) {
				pause_handle.WaitOne ();
				if (stop)
					break;
				if (pause) {
					pause_handle.Reset ();
					pause = false;
					continue;
				}
				if (event_idx == events.Count)
					break;
				HandleEvent (events [event_idx++]);
			}
		}

		int current_tempo = 500000; // dummy
		int tempo_ratio = 1;

		int GetDeltaTimeInMilliseconds (int deltaTime)
		{
			if (music.DeltaTimeSpec >= 0x80)
				throw new NotSupportedException ();
			return (int) (current_tempo / 1000 * deltaTime / music.DeltaTimeSpec / tempo_ratio);
		}

		string ToBinHexString (byte [] bytes)
		{
			string s = "";
			foreach (byte b in bytes)
				s += String.Format ("{0:X02} ", b);
			return s;
		}

		public virtual void HandleEvent (SmfEvent e)
		{
			if (e.DeltaTime != 0) {
				var ms = GetDeltaTimeInMilliseconds (e.DeltaTime);
				//Thread.Sleep (ms);
			}
			if (e.Message.StatusByte == 0xFF && e.Message.Msb == 0x51)
				current_tempo = (e.Message.Data [0] << 16) + (e.Message.Data [1] << 8) + e.Message.Data [2];

			OnMessage (e);
			PlayDeltaTime += e.DeltaTime;
		}

		protected virtual void OnMessage (SmfEvent e)
		{
			if ((e.Message.Value & 0xFF) == 0xF0)
				;//output.WriteSysEx (0, e.SysEx);
			else if ((e.Message.Value & 0xFF) == 0xF7)
				;//output.WriteSysEx (0, e.SysEx);
			else if ((e.Message.Value & 0xFF) == 0xFF)
				return; // meta. Nothing to send.
			else
#if Moonlight
				;
#else
				output.Write (0, new MidiMessage (e.Message.StatusByte, e.Message.Msb, e.Message.Lsb));
#endif
		}

		public void Stop ()
		{
			if (pause_handle != null)
				pause_handle.Set ();
			stop = true;
		}
	}

	// Provides asynchronous player control.
	public class PortMidiPlayer : IDisposable
	{
		PortMidiSyncPlayer player;
		Thread sync_player_thread;

		public PortMidiPlayer (MidiOutput output, SmfMusic music)
		{
			player = new PortMidiSyncPlayer (output, music);
			sync_player_thread = new Thread (new ThreadStart (delegate { player.Play (); }));
			State = PlayerState.Stopped;
		}

		public PlayerState State { get; set; }

		public void Dispose ()
		{
			player.Dispose ();
			if (sync_player_thread.ThreadState == ThreadState.Running)
				sync_player_thread.Abort ();
		}

		public void PlayAsync ()
		{
			switch (State) {
			case PlayerState.Playing:
				return; // do nothing
			case PlayerState.Paused:
				player.Play ();
				State = PlayerState.Playing;
				return;
			case PlayerState.Stopped:
				player.Play ();
				return;
			}
		}

		public void PauseAsync ()
		{
			switch (State) {
			case PlayerState.Playing:
				player.Pause ();
				State = PlayerState.Paused;
				return;
			default: // do nothing
				return;
			}
		}
	}
}
