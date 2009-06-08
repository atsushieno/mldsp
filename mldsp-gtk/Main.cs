using System;
using Gtk;
using Moonlight.Gtk;
using System.Windows;

using Application = Gtk.Application;

namespace mldspgtk
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			Application.Init ();
			//MainWindow win = new MainWindow ();
			//win.Show ();
			MoonlightRuntime.Init ();
			Window w = new Window ("mldsp");
			w.DefaultHeight = 520;
			w.DefaultWidth = 720;
			w.DeleteEvent += delegate { Application.Quit (); };
			var moon = new MoonlightHost ();
			moon.LoadXap ("mldsp.clr.xap");
			if (args.Length > 0) {
				int device;
				if (int.TryParse (args [0], out device))
					((mldsp.App) moon.Application).OutputDeviceID = device;
				else {
					Console.WriteLine ("WARNING: wrong device ID speficication. Specify an index.");
					foreach (var dev in PortMidiSharp.MidiDeviceManager.AllDevices)
						if (dev.IsOutput)
							Console.WriteLine ("{0} {1}", dev.ID, dev.Name);
				}
			}
			w.Add (moon);
			w.ShowAll ();

			Application.Run ();
		}
	}
}
