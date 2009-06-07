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
			this.output = output;
			MessageReceived += delegate (SmfMessage m) { SendMidiMessage (m); };
		}

		// it should not be disposed here. The module that
		// created this object should dispose it instead.
		MidiOutput output;

		void SendMidiMessage (SmfMessage m)
		{
			if ((m.Value & 0xFF) == 0xF0)
				WriteSysEx (0xF0, m.Data);
			else if ((m.Value & 0xFF) == 0xF7)
				WriteSysEx (0xF7, m.Data);
			else if ((m.Value & 0xFF) == 0xFF)
				return; // meta. Nothing to send.
			else
				output.Write (0, new MidiMessage (m.StatusByte, m.Msb, m.Lsb));
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

