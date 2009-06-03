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
			w.DefaultHeight = 600;
			w.DefaultWidth = 800;
			w.DeleteEvent += delegate { Application.Quit (); };
			var moon = new MoonlightHost ();
			moon.LoadXap ("mldsp-clr.xap");
			w.Add (moon);
			w.ShowAll ();

			Application.Run ();
		}
	}
}
