// FIXMEs:
// - some bad mappings:
//	- C int -> C# int
//	- C long -> C# int
// not sure what they should be.
// The sources are wrong. Those C code should not use int and long for each.
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using PmDeviceID = System.Int32;
using PmTimestamp = System.Int32;
using PortMidiStream = System.IntPtr;
using PmMessage = System.Int32;
using PmError = PortMidiSharp.MidiErrorType;

namespace PortMidiSharp
{
	public class MidiDeviceManager
	{
		static MidiDeviceManager ()
		{
			PortMidiMarshal.Pm_Initialize ();
			AppDomain.CurrentDomain.DomainUnload += delegate (object o, EventArgs e) {
				PortMidiMarshal.Pm_Terminate ();
			};
		}

		public static int DeviceCount {
			get { return PortMidiMarshal.Pm_CountDevices (); }
		}

		public static int DefaultInputDeviceID {
			get { return PortMidiMarshal.Pm_GetDefaultInputDeviceID (); }
		}

		public static int DefaultOutputDeviceID {
			get { return PortMidiMarshal.Pm_GetDefaultOutputDeviceID (); }
		}

		public static IEnumerable<MidiDeviceInfo> AllDevices {
			get {
				for (int i = 0; i < DeviceCount; i++)
					yield return GetDeviceInfo (i);
			}
		}

		public static MidiDeviceInfo GetDeviceInfo (PmDeviceID id)
		{
			return new MidiDeviceInfo (id, PortMidiMarshal.Pm_GetDeviceInfo (id));
		}

		public static MidiOutput OpenOutput (PmDeviceID outputDevice)
		{
			PortMidiStream stream;
			var e = PortMidiMarshal.Pm_OpenOutput (out stream, outputDevice, IntPtr.Zero, 0, null, IntPtr.Zero, 0);
			if (e != PmError.NoError)
				throw new MidiException (e, String.Format ("Failed to open output device {0}", e));
			return new MidiOutput (stream, outputDevice, 0);
		}
	}

	public enum MidiErrorType
	{
		NoError = 0,
		NoData = 0,
		GotData = 1,
		HostError = -10000,
		InvalidDeviceId,
		InsufficientMemory,
		BufferTooSmall,
		BufferOverflow,
		BadPointer,
		BadData,
		InternalError,
		BufferMaxSize,
	}

	public class MidiException : Exception
	{
		PmError error_type;

		public MidiException (PmError errorType, string message)
			: this (errorType, message, null)
		{
		}

		public MidiException (PmError errorType, string message, Exception innerException)
			: base (message, innerException)
		{
			error_type = errorType;
		}

		public PmError ErrorType {
			get { return error_type; }
		}
	}

	public struct MidiDeviceInfo
	{
		PmDeviceInfo info;

		internal MidiDeviceInfo (int id, IntPtr ptr)
		{
			ID = id;
			this.info = (PmDeviceInfo) Marshal.PtrToStructure (ptr, typeof (PmDeviceInfo));
		}

		public int ID { get; set; }

		public string Interface {
			get { return Marshal.PtrToStringAnsi (info.Interface); }
		}

		public string Name {
			get { return Marshal.PtrToStringAnsi (info.Name); }
		}

		public bool IsInput { get { return info.Input != 0; } }
		public bool IsOutput { get { return info.Output != 0; } }
		public bool IsOpened { get { return info.Opened != 0; } }

		public override string ToString ()
		{
			return String.Format ("{0} - {1} ({2} {3})", Interface, Name, IsInput ? (IsOutput ? "I/O" : "Input") : (IsOutput ? "Output" : "N/A"), IsOpened ? "open" : String.Empty);
		}
	}

	public abstract class MidiStream : IDisposable
	{
		internal PortMidiStream stream;
		internal PmDeviceID device;

		protected MidiStream (PortMidiStream stream, PmDeviceID deviceID)
		{
			this.stream = stream;
			device = deviceID;
		}

		public void Abort ()
		{
			PortMidiMarshal.Pm_Abort (stream);
		}

		public void Close ()
		{
			Dispose ();
		}

		public void Dispose ()
		{
			PortMidiMarshal.Pm_Close (stream);
		}

		public void SetFilter (MidiFilter filters)
		{
			PortMidiMarshal.Pm_SetFilter (stream, filters);
		}

		public void SetChannelMask (int mask)
		{
			PortMidiMarshal.Pm_SetChannelMask (stream, mask);
		}
	}

	public class MidiInput : MidiStream
	{
		public MidiInput (PortMidiStream stream, PmDeviceID inputDevice)
			: base (stream, inputDevice)
		{
		}

		public int Read (MidiEvent [] buffer, int length)
		{
			return PortMidiMarshal.Pm_Read (stream, buffer, length);
		}
	}

	public class MidiOutput : MidiStream
	{
		public MidiOutput (PortMidiStream stream, PmDeviceID outputDevice, int latency)
			: base (stream, outputDevice)
		{
		}

		public void Write (MidiEvent mevent)
		{
			if (mevent.SysEx != null)
				WriteSysEx (mevent.Timestamp, mevent.SysEx);
			else
				Write (mevent.Timestamp, mevent.Message);
		}

		public void Write (PmTimestamp when, MidiMessage msg)
		{
			var ret = PortMidiMarshal.Pm_WriteShort (stream, when, msg);
			if (ret != PmError.NoError)
				throw new MidiException (ret, String.Format ("Failed to write message {1} (error code {0})", ret, msg.Value));
		}

		public void WriteSysEx (PmTimestamp when, byte [] sysex)
		{
			var ret = PortMidiMarshal.Pm_WriteSysEx (stream, when, sysex);
			if (ret != PmError.NoError)
				throw new MidiException (ret, String.Format ("Failed to write message (error code {0})", ret));
		}

		public void Write (MidiEvent [] buffer)
		{
			Write (buffer, 0, buffer.Length);
		}

		public void Write (MidiEvent [] buffer, int index, int length)
		{
			var gch = GCHandle.Alloc (buffer);
			try {
				var ptr = Marshal.UnsafeAddrOfPinnedArrayElement (buffer, index);
				var ret = PortMidiMarshal.Pm_Write (stream, ptr, length);
				if (ret != PmError.NoError)
					throw new MidiException (ret, String.Format ("Failed to write message (error code {0})", ret));
			} finally {
				gch.Free ();
			}
		}
	}

	[StructLayout (LayoutKind.Sequential)]
	public struct MidiEvent
	{
		MidiMessage msg;
		PmTimestamp ts;
		[NonSerialized]
		byte [] sysex;

		public MidiMessage Message {
			get { return msg; }
			set { msg = value; }
		}

		public PmTimestamp Timestamp {
			get { return ts; }
			set { ts = value; }
		}

		public byte [] SysEx {
			get { return sysex; }
			set { sysex = value; }
		}
	}

	public struct MidiMessage
	{
		PmMessage v;

		public MidiMessage (PmMessage value)
		{
			v = value;
		}

		public MidiMessage (int status, int data1, int data2)
		{
			v = ((((data2) << 16) & 0xFF0000) | (((data1) << 8) & 0xFF00) | ((status) & 0xFF)); 
		}

		public PmMessage Value {
			get { return v; }
		}
	}

	public delegate PmTimestamp MidiTimeProcDelegate (IntPtr timeInfo);

	[Flags]
	public enum MidiFilter : int
	{
		Active = 1 << 0x0E,
		SysEx = 1 << 0x00,
		Clock = 1 << 0x08,
		Play = ((1 << 0x0A) | (1 << 0x0C) | (1 << 0x0B)),
		Tick = (1 << 0x09),
		FD = (1 << 0x0D),
		Undefined = FD,
		Reset = (1 << 0x0F),
		RealTime = (Active | SysEx | Clock | Play | Undefined | Reset | Tick),
		Note = ((1 << 0x19) | (1 << 0x18)),
		CAF = (1 << 0x1D),
		PAF = (1 << 0x1A),
		AF = (CAF | PAF),
		Program = (1 << 0x1C),
		Control = (1 << 0x1B),
		PitchBend = (1 << 0x1E),
		MTC = (1 << 0x01),
		SongPosition = (1 << 0x02),
		SongSelect = (1 << 0x03),
		Tune = (1 << 0x06),
		SystemCommon = (MTC | SongPosition | SongSelect | Tune)
	}

	// Marshal types

	class PortMidiMarshal
	{
		[DllImport ("portmidi")]
		public static extern PmError Pm_Initialize ();

		[DllImport ("portmidi")]
		public static extern PmError Pm_Terminate ();

		// TODO
		[DllImport ("portmidi")]
		static extern int Pm_HasHostError (PortMidiStream stream);

		// TODO
		[DllImport ("portmidi")]
		static extern string Pm_GetErrorText (PmError errnum);

		// TODO
		[DllImport ("portmidi")]
		static extern void Pm_GetHostErrorText (IntPtr msg, uint len);

		const int HDRLENGTH = 50;
		const uint PM_HOST_ERROR_MSG_LEN = 256;

		// Device enumeration

		const PmDeviceID PmNoDevice = -1;

		[DllImport ("portmidi")]
		public static extern int Pm_CountDevices ();

		[DllImport ("portmidi")]
		public static extern PmDeviceID Pm_GetDefaultInputDeviceID ();

		[DllImport ("portmidi")]
		public static extern PmDeviceID Pm_GetDefaultOutputDeviceID ();

		[DllImport ("portmidi")]
		public static extern IntPtr Pm_GetDeviceInfo (PmDeviceID id);

		[DllImport ("portmidi")]
		public static extern PmError Pm_OpenInput (
			out PortMidiStream stream,
			PmDeviceID inputDevice,
			IntPtr inputDriverInfo,
			int bufferSize,
			MidiTimeProcDelegate timeProc,
			IntPtr timeInfo);

		[DllImport ("portmidi")]
		public static extern PmError Pm_OpenOutput (
			out PortMidiStream stream,
			PmDeviceID outputDevice,
			IntPtr outputDriverInfo,
			int bufferSize,
			MidiTimeProcDelegate time_proc,
			IntPtr time_info,
			int latency);

		[DllImport ("portmidi")]
		public static extern PmError Pm_SetFilter (PortMidiStream stream, MidiFilter filters);

		// TODO
		public static int Pm_Channel (int channel) { return 1 << channel; }

		[DllImport ("portmidi")]
		public static extern PmError Pm_SetChannelMask (PortMidiStream stream, int mask);

		[DllImport ("portmidi")]
		public static extern PmError Pm_Abort (PortMidiStream stream);

		[DllImport ("portmidi")]
		public static extern PmError Pm_Close (PortMidiStream stream);

		// TODO
		public static int Pm_MessageStatus (int msg) { return ((msg) & 0xFF); }
		// TODO
		public static int Pm_MessageData1 (int msg) { return (((msg) >> 8) & 0xFF); }
		// TODO
		public static int Pm_MessageData2 (int msg) { return (((msg) >> 16) & 0xFF); }

		[DllImport ("portmidi")]
		public static extern int Pm_Read (PortMidiStream stream, MidiEvent [] buffer, int length);

		[DllImport ("portmidi")]
		public static extern PmError Pm_Poll (PortMidiStream stream);

		[DllImport ("portmidi")]
		public static extern PmError Pm_Write (PortMidiStream stream, IntPtr buffer, int length);

		[DllImport ("portmidi")]
		public static extern PmError Pm_WriteShort (PortMidiStream stream, PmTimestamp when, MidiMessage msg);

		[DllImport ("portmidi")]
		public static extern PmError Pm_WriteSysEx (PortMidiStream stream, PmTimestamp when, byte [] msg);
	}

	[StructLayout (LayoutKind.Sequential)]
	struct PmDeviceInfo
	{
		public int StructVersion; // it is not actually used.
		public IntPtr Interface; // char*
		public IntPtr Name; // char*
		public int Input; // 1 or 0
		public int Output; // 1 or 0
		public int Opened;

		public override string ToString ()
		{
			return String.Format ("{0},{1:X},{2:X},{3},{4},{5}", StructVersion, Interface, Name, Input, Output, Opened);
		}
	}
}

