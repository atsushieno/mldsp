using System;
using System.Collections.Generic;

namespace Commons.Music.Midi
{
	public delegate void MidiMessageAction (SmfMessage m);

	public class MidiMachine
	{
		public MidiMachine ()
		{
			var arr = new MidiMachineChannel [16];
			for (int i = 0; i < arr.Length; i++)
				arr [i] = new MidiMachineChannel ();
			Channels = arr;
		}

		public event MidiMessageAction MessageReceived;

		public IList<MidiMachineChannel> Channels { get; private set; }

		public virtual void ProcessMessage (SmfMessage msg)
		{
			switch (msg.MessageType) {
			case SmfMessage.NoteOn:
				Channels [msg.Channel].NoteVelocity [msg.Msb] = msg.Lsb;
				break;
			case SmfMessage.NoteOff:
				Channels [msg.Channel].NoteVelocity [msg.Msb] = 0;
				break;
			case SmfMessage.PAf:
				Channels [msg.Channel].PAfVelocity [msg.Msb] = msg.Lsb;
				break;
			case SmfMessage.CC:
				// FIXME: handle RPNs and NRPNs by DTE
				Channels [msg.Channel].Controls [msg.Msb] = msg.Lsb;
				break;
			case SmfMessage.Program:
				Channels [msg.Channel].Program = msg.Msb;
				break;
			case SmfMessage.CAf:
				Channels [msg.Channel].CAf = msg.Msb;
				break;
			case SmfMessage.Pitch:
				Channels [msg.Channel].PitchBend = (short) ((msg.Msb << 7) + msg.Lsb);
				break;
			}
			if (MessageReceived != null)
				MessageReceived (msg);
		}
	}
	
	public class MidiMachineChannel
	{
		public byte [] NoteVelocity = new byte [128];
		public byte [] PAfVelocity = new byte [128];
		public byte [] Controls = new byte [128];
		public short [] RPNs = new short [128]; // only 5 should be used though
		public short [] NRPNs = new short [128];
		public byte Program { get; set; }
		public byte CAf { get; set; }
		public short PitchBend { get; set; }
	}
}
