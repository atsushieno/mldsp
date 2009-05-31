using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using Commons.Music.Midi;
#if Moonlight
using MidiOutput = System.IntPtr;
using System.Windows.Threading;
#else
using PortMidiSharp;
using Timer = System.Timers.Timer;
#endif

namespace Commons.Music.Midi.Player
{
#if !Moonlight
	public class Driver
	{
		public static void Main (string [] args)
		{
			var output = MidiDeviceManager.OpenOutput (MidiDeviceManager.DefaultOutputDeviceID);

			bool syncMode = false;

			foreach (var arg in args) {
				if (arg == "--sync") {
					syncMode = true;
					continue;
				}
				var parser = new SmfReader (File.OpenRead (arg));
				parser.Parse ();
#if false
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
#else

				// To sync player, just use it.
				if (syncMode) {
					var syncPlayer = new PortMidiSyncPlayer (output, parser.Music);
					syncPlayer.PlayerLoop ();
					return;
				}

				var player = new PortMidiPlayer (output, parser.Music);
				player.StartLoop ();
				player.PlayAsync ();
				Console.WriteLine ("empty line to quit, P to pause and resume");
				while (true) {
					string line = Console.ReadLine ();
					if (line == "P") {
						if (player.State == PlayerState.Playing)
							player.PauseAsync ();
						else
							player.PlayAsync ();
					}
					else if (line == "") {
						player.Dispose ();
						break;
					}
					else
						Console.WriteLine ("what do you mean by '{0}' ?", line);
				}
#endif
			}
		}
	}
#endif

	public enum PlayerState
	{
		Stopped,
		Playing,
		Paused
	}

	public delegate void MidiMessageAction (SmfEvent ev);

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
			if (pause_handle != null)
				pause_handle.Set ();
		}

		public void Pause ()
		{
			pause = true;
		}

		int event_idx = 0;

		public void PlayerLoop ()
		{
			try {
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
			} catch (ThreadAbortException ex) {
				// FIXME: call pause (to shut down all notes)
				// FIXME: call ResetAbort()
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
				Thread.Sleep (ms);
			}
			if (e.Message.StatusByte == 0xFF && e.Message.Msb == 0x51)
				current_tempo = (e.Message.Data [0] << 16) + (e.Message.Data [1] << 8) + e.Message.Data [2];

			OnMessage (e);
			PlayDeltaTime += e.DeltaTime;
		}

		void WriteSysEx (byte status, byte [] sysex)
		{
			var buf = new byte [sysex.Length + 1];
			buf [0] = status;
			Array.Copy (sysex, 0, buf, 1, buf.Length - 1);
#if Moonlight
#else
			output.WriteSysEx (0, buf);
#endif
		}

		public MidiMessageAction MessageReceived;

		protected virtual void OnMessage (SmfEvent e)
		{
			if (MessageReceived != null)
				MessageReceived (e);
			SendMidiMessage (e);
		}

		void SendMidiMessage (SmfEvent e)
		{
#if Moonlight
#else
			if ((e.Message.Value & 0xFF) == 0xF0)
				WriteSysEx (0xF0, e.Message.Data);
			else if ((e.Message.Value & 0xFF) == 0xF7)
				WriteSysEx (0xF7, e.Message.Data);
			else if ((e.Message.Value & 0xFF) == 0xFF)
				return; // meta. Nothing to send.
			else
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
			State = PlayerState.Stopped;
			player = new PortMidiSyncPlayer (output, music);
			ThreadStart ts = delegate {
				player.Pause ();
				player.PlayerLoop ();
				};
			sync_player_thread = new Thread (ts);
		}

		public PlayerState State { get; set; }

		public event MidiMessageAction MessageReceived {
			add { player.MessageReceived += value; }
			remove { player.MessageReceived -= value; }
		}

		public void Dispose ()
		{
			switch (sync_player_thread.ThreadState) {
			case ThreadState.Stopped:
			case ThreadState.AbortRequested:
			case ThreadState.Aborted:
				break;
			default:
				sync_player_thread.Abort ();
				break;
			}
		}

		public void StartLoop ()
		{
			sync_player_thread.Start ();
		}

		public void PlayAsync ()
		{
TextWriter.Null.WriteLine ("STATE: " + State); // FIXME: mono somehow fails to initialize this auto property.
			switch (State) {
			case PlayerState.Playing:
				return; // do nothing
			case PlayerState.Paused:
				player.Play ();
				State = PlayerState.Playing;
				return;
			case PlayerState.Stopped:
				player.Play ();
				State = PlayerState.Playing;
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

	#region Timer wrapper
	// I'm not sure if I will use this timer mode, but adding it so far.
	public abstract class TimerWrapper
	{
		public abstract void SetNextWait (int milliseconds);

		public event EventHandler Tick;

		protected void OnTick ()
		{
			if (Tick != null)
				Tick (null, null);
		}
	}

#if Moonlight
	public class MoonlightTimerWrapper : TimerWrapper
	{
		public MoonlightTimerWrapper ()
		{
			timer = new DispatcherTimer ();
			timer.Tick += delegate {
				timer.Stop ();
				OnTick ();
			};
		}

		DispatcherTimer timer;

		public override void SetNextWait (int milliseconds)
		{
			timer.Interval = TimeSpan.FromMilliseconds (milliseconds);
			timer.Start ();
		}
	}
#else
	public class MonoTimerWrapper : TimerWrapper
	{
		public MonoTimerWrapper ()
		{
			timer = new Timer () { AutoReset = false };
			timer.Elapsed += delegate {
				OnTick ();
			};
		}

		Timer timer;

		public override void SetNextWait (int milliseconds)
		{
			timer.Interval = (double) milliseconds;
			timer.Start ();
		}
	}
#endif

	#endregion
}

