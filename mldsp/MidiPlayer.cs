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

	public interface IMidiPlayerStatus
	{
		PlayerState State { get; }
		int Tempo { get; }
		int PlayDeltaTime { get; }
		int GetTotalPlayTimeMilliseconds ();
	}

	// Player implementation. Plays a MIDI song synchronously.
	public class MidiSyncPlayer : IDisposable, IMidiPlayerStatus
	{
		public MidiSyncPlayer (SmfMusic music)
		{
			if (music == null)
				throw new ArgumentNullException ("music");

			this.music = music;
			events = SmfTrackMerger.Merge (music).Tracks [0].Events;
			stop = true;
		}

		SmfMusic music;
		IList<SmfEvent> events;
		ManualResetEvent pause_handle = new ManualResetEvent (true);
		bool pause, stop;

		public PlayerState State {
			get { return stop ? PlayerState.Stopped : pause ? PlayerState.Paused : PlayerState.Playing; }
		}
		public int PlayDeltaTime { get; set; }
		public int Tempo {
			get { return current_tempo; }
		}
		public int GetTotalPlayTimeMilliseconds ()
		{
			return SmfMusic.GetTotalPlayTimeMilliseconds (events, music.DeltaTimeSpec);
		}

		public virtual void Dispose ()
		{
			if (!stop)
				Stop ();
			Mute ();
		}

		public void Play ()
		{
			if (pause_handle != null)
				pause_handle.Set ();
		}

		void AllControlReset ()
		{
			for (int i = 0; i < 16; i++)
				OnMessage (new SmfMessage ((byte) (i + 0xB0), 0x79, 0, null));
		}

		void Mute ()
		{
			for (int i = 0; i < 16; i++)
				OnMessage (new SmfMessage ((byte) (i + 0xB0), 0x78, 0, null));
		}

		public void Pause ()
		{
			pause = true;
			Mute ();
		}

		int event_idx = 0;

		public void PlayerLoop ()
		{
			stop = false;
			Mute ();
			AllControlReset ();
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

		int current_tempo = SmfMetaType.DefaultTempo; // dummy
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
			if (e.Message.StatusByte == 0xFF && e.Message.Msb == SmfMetaType.Tempo)
				current_tempo = SmfMetaType.GetTempo (e.Message.Data);

			OnMessage (e.Message);
			PlayDeltaTime += e.DeltaTime;
		}

		public MidiMessageAction MessageReceived;

		protected virtual void OnMessage (SmfMessage m)
		{
			if (MessageReceived != null)
				MessageReceived (m);
		}

		public void Stop ()
		{
			stop = true;
			Mute ();
			if (pause_handle != null)
				pause_handle.Set ();
		}
	}

	// Provides asynchronous player control.
	public class MidiPlayer : IDisposable, IMidiPlayerStatus
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

		public PlayerState State { get; private set; }
		public int Tempo {
			get { return player.Tempo; }
		}
		public int PlayDeltaTime {
			get { return player.PlayDeltaTime; }
		}
		public int GetTotalPlayTimeMilliseconds ()
		{
			return player.GetTotalPlayTimeMilliseconds ();
		}

		public event MidiMessageAction MessageReceived {
			add { player.MessageReceived += value; }
			remove { player.MessageReceived -= value; }
		}

		public virtual void Dispose ()
		{
			player.Stop ();
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

