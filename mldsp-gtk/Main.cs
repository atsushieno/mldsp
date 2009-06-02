using System;
using Gtk;

namespace mldspgtk
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			Application.Init ();
			//MainWindow win = new MainWindow ();
			//win.Show ();
			Window w = new Window ("mldsp");
			w.DefaultHeight = 600;
			w.DefaultWidth = 800;
			w.DeleteEvent += delegate { Application.Quit (); };
			var moon = new Moonlight.Gtk.MoonlightHost ();
			moon.LoadXamlFromFile ("App.xaml");
			w.Add (moon);
			w.ShowAll ();

			Application.Run ();
		}
	}
}