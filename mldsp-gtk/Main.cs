using System;
using System.IO;
using System.Linq;
using GLib;
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
			w.DefaultWidth = 760;
			w.DeleteEvent += delegate { Application.Quit (); };

			var moon = new MoonlightHost ();
			var xappath = System.IO.Path.Combine (System.IO.Path.GetDirectoryName (new Uri (typeof (MainClass).Assembly.CodeBase).LocalPath), "mldsp.clr.xap");
			moon.LoadXap (xappath);
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

			var vbox = new VBox (false, 0);
			w.Add (vbox);
			var mainmenu = new MenuBar ();
			vbox.PackStart (mainmenu, false, true, 0);
			var optmi = new MenuItem ("_Options");
			mainmenu.Add (optmi);
			var devmi = new MenuItem ("Select Device");
			var optmenu = new Menu ();
			optmi.Submenu = optmenu;
			optmenu.Append (devmi);
			var devlist = new SList (IntPtr.Zero);
			var devmenu = new Menu ();
			devmi.Submenu = devmenu;
			foreach (var dev in PortMidiSharp.MidiDeviceManager.AllDevices) {
				if (dev.IsOutput) {
					var mi = new RadioMenuItem (devlist, dev.Name);
					mi.Data ["Device"] = dev.ID;
					devlist = mi.Group;
					int id = dev.ID;
					mi.Activated += delegate {
						((mldsp.App) moon.Application).ResetDevice ((int) mi.Data ["Device"]);
					};
					devmenu.Append (mi);
				}
			}
			
			vbox.PackEnd (moon);

			w.ShowAll ();
			Application.Run ();
		}
	}
}
