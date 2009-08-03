using System;
using System.IO;
using System.Linq;
using Gtk;
using Moonlight.Gtk;

using Path = System.IO.Path;

namespace mldspgtk
{
	
	public partial class MainWindow: Gtk.Window
	{	
		public MainWindow (): base (Gtk.WindowType.Toplevel)
		{
			Build ();
		}
	
		protected void OnDeleteEvent (object sender, DeleteEventArgs a)
		{
			Application.Quit ();
			a.RetVal = true;
		}
	}
}