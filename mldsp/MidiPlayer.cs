using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using Commons.Music.Midi;

namespace Commons.Music.Midi.Player
{
	public enum PlayerState
	{
		Stopped,
		Playing,
		Paused
	}

	public delegate void MidiMessageAction (SmfEvent ev);

	// Player implementation. Plays a MIDI song synchronously.
	public class MidiSyncPlayer : IDisposable
	{
		public MidiSyncPlayer (SmfMusic music)
		{
			if (music == null)
				throw new ArgumentNullException ("music");

			this.music = music;
			events = SmfTrackMerger.Merge (music).Tracks [0].Events;
		}

		SmfMusic music;
		IList<SmfEvent> events;
		ManualResetEvent pause_handle = new ManualResetEvent (true);
		bool pause, stop;

		public int PlayDeltaTime { get; set; }

		public virtual void Dispose ()
		{
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

		public MidiMessageAction MessageReceived;

		protected virtual void OnMessage (SmfEvent e)
		{
			if (MessageReceived != null)
				MessageReceived (e);
		}

		public void Stop ()
		{
			if (pause_handle != null)
				pause_handle.Set ();
			stop = true;
		}
	}

	// Provides asynchronous player control.
	public class MidiPlayer : IDisposable
	{
		MidiSyncPlayer player;
		Thread sync_player_thread;

		public MidiPlayer (SmfMusic music)
		{
			State = PlayerState.Stopped;
			player = new MidiSyncPlayer (music);
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

		public virtual void Dispose ()
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
}

