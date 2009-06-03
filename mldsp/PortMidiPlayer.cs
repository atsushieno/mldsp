using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using Commons.Music.Midi;
using PortMidiSharp;
using Timer = System.Timers.Timer;

namespace Commons.Music.Midi.Player
{
	public class PortMidiPlayer : MidiPlayer
	{
		public PortMidiPlayer (MidiOutput output, SmfMusic music)
			: base (music)
		{
			MessageReceived += delegate (SmfEvent ev) { SendMidiMessage (ev); };
		}

		MidiOutput output;

		public override void Dispose ()
		{
			if (output != null)
				output.Dispose ();
		}

		void SendMidiMessage (SmfEvent e)
		{
			if ((e.Message.Value & 0xFF) == 0xF0)
				WriteSysEx (0xF0, e.Message.Data);
			else if ((e.Message.Value & 0xFF) == 0xF7)
				WriteSysEx (0xF7, e.Message.Data);
			else if ((e.Message.Value & 0xFF) == 0xFF)
				return; // meta. Nothing to send.
			else
				output.Write (0, new MidiMessage (e.Message.StatusByte, e.Message.Msb, e.Message.Lsb));
		}

		void WriteSysEx (byte status, byte [] sysex)
		{
			var buf = new byte [sysex.Length + 1];
			buf [0] = status;
			Array.Copy (sysex, 0, buf, 1, buf.Length - 1);
			output.WriteSysEx (0, buf);
		}
	}
}

