/* ===============
ProcessingProjectSource {
Namespace: mldsp
Sources: /svn/commons-music-prog/mldsp/mldsp.pde
Source Readers: 
Data: 
}
================*/
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using ProcessingCli;
namespace mldsp
{
public partial class App
{
public App () : base (() => Run ())
{
} // end of App.ctor()

// placeholder for global variables
internal static int all_keys;
internal static int channels;
internal static int key_width;
internal static int key_height;
internal static double blackKeyWidth;
internal static double blackKeyHeight;
internal static int ch_height;
internal static int text_height;
internal static int play_info_section_width;
internal static System.Windows.Media.Color color_background;
internal static System.Windows.Media.Color color_white_key;
internal static System.Windows.Media.Color color_basic_stroke;
internal static System.Windows.Media.Color color_black_key;
internal static System.Windows.Media.Color color_black_key_edge;
internal static System.Windows.Media.Color color_keyon;
internal static System.Windows.Media.Color color_aftertouch;
internal static System.Windows.Media.Color color_bend;
internal static System.Windows.Media.Color color_hold;
internal static System.Windows.Media.Color color_bright;
internal static System.Windows.Media.Color color_usual;
internal static System.Windows.Media.Color color_dark;
internal static System.Windows.Media.Color color_hidden;
internal static System.Windows.Media.Color color_ch_base;
internal static System.Windows.Media.Color color_ch_colored;
internal static System.Windows.Media.Color color_ch_dark;
internal static System.Windows.Media.Color color_ch_hidden;
internal static System.Windows.Media.Color color_ch_text_colored;
internal static System.Windows.Media.Color color_ch_text_base;
internal static System.Windows.Media.Color color_ch_text_dark;
internal static System.Windows.Media.Color color_ch_text_hidden;
internal static PFont font_title;
internal static PFont font16;
internal static PFont font8;
internal static string [] ch_types;

// placeholder for global functions
public static void setup ()
{
font_title = ProcessingApplication.Current.@createFont (@"PalatinoLinotype-BoldItalic", 16);
font16 = ProcessingApplication.Current.@createFont (@"Tahoma", 16);
font8 = ProcessingApplication.Current.@createFont (@"Tahoma", 8);
ProcessingApplication.Current.@size (760, ch_height * channels + 10);
ProcessingApplication.Current.@background (color_background);
for (int i = 0; i < channels; i = i + 1)
{
setupChannelInfo (i);
}
for (int i = 0; i < channels; i = i + 1)
{
setupKeyboard (i);
}
ProcessingApplication.Current.@pushMatrix ();
ProcessingApplication.Current.@translate (400, 0);
setupPlayInfoSection ();
ProcessingApplication.Current.@popMatrix ();
ProcessingApplication.Current.@pushMatrix ();
ProcessingApplication.Current.@translate (400, 160);
ProcessingApplication.Current.@popMatrix ();
}
public static void setupPlayInfoSection ()
{
setupTitleArea ();
setupDriverInfo ();
}
public static void setupTitleArea ()
{
ProcessingApplication.Current.@textFont (font_title);
ProcessingApplication.Current.@fill (color_ch_text_colored);
ProcessingApplication.Current.@text (@"MLDSP", 0, 16);
ProcessingApplication.Current.@stroke (color_ch_colored);
ProcessingApplication.Current.@line (0, 18, 60, 18);
ProcessingApplication.Current.@textFont (font8);
ProcessingApplication.Current.@fill (color_ch_text_dark);
ProcessingApplication.Current.@text (@"music visualizer and file selector", 70, text_height);
ProcessingApplication.Current.@fill (color_ch_text_colored);
ProcessingApplication.Current.@text (@"ver 0.03 / for moonlight 2.0 (C)2009-2011 atsushieno", 70, text_height * 2);
ProcessingApplication.Current.@stroke (color_ch_dark);
ProcessingApplication.Current.@line (70, 18, 310, 18);
}
public static void setupDriverInfo ()
{
ProcessingApplication.Current.@textFont (font8);
ProcessingApplication.Current.@fill (color_ch_text_dark);
ProcessingApplication.Current.@text (@"DRIVER ଆ", 0, text_height * 4);
ProcessingApplication.Current.@fill (color_ch_text_colored);
ProcessingApplication.Current.@text (@"SMF support", 50, text_height * 4);
ProcessingApplication.Current.@text (@"0.01", 100, text_height * 4);
ProcessingApplication.Current.@text (@"(C)2009 atsushieno", 130, text_height * 4);
}
public static void setupChannelInfo (int channel)
{
double yText1 = getChannelYPos (channel) + text_height;
double yText2 = getChannelYPos (channel) + text_height * 2;
ProcessingApplication.Current.@fill (color_ch_text_colored);
ProcessingApplication.Current.@textFont (font16);
ProcessingApplication.Current.@textSize (16);
ProcessingApplication.Current.@text (ProcessingApplication.Current.@nf (channel + 1, 2), 35, yText2);
ProcessingApplication.Current.@textFont (font8);
ProcessingApplication.Current.@fill (color_ch_text_colored);
ProcessingApplication.Current.@text (ch_types [channel], 0, yText1);
ProcessingApplication.Current.@fill (color_ch_text_base);
ProcessingApplication.Current.@text (@"TRACK.", 0, yText2);
ProcessingApplication.Current.@stroke (color_ch_colored);
ProcessingApplication.Current.@line (340, getChannelYPos (channel) + 2, 360, getChannelYPos (channel) + text_height - 2);
ProcessingApplication.Current.@fill (color_ch_text_colored);
ProcessingApplication.Current.@text (ProcessingApplication.Current.@nf (1000, 5), 364, getChannelYPos (channel) + text_height);
ProcessingApplication.Current.@fill (color_ch_text_base);
ProcessingApplication.Current.@text (@"M:--------", 340, yText2);
}
public static void setupKeyboard (int channel)
{
			// user_code
			var h = ProcessingApplication.Current.Host;
			white_key_panel = new Canvas () { Width = h.Width, Height = h.Height };
			black_key_panel = new Canvas () { Width = h.Width, Height = h.Height };
int octaves = all_keys / 12;
for (int octave = 0; octave < octaves; octave = octave + 1)
{
drawOctave (channel, octave);
}
			// user code
			ProcessingApplication.Current.Host.Children.Add (white_key_panel);
			ProcessingApplication.Current.Host.Children.Add (black_key_panel);
}
public static double getChannelYPos (int channel)
{
return channel * ch_height;
}
		// modified to provide different layer for black keys
		public static void drawOctave (int channel, int octave)
		{
			double x = octave * key_width * 7;
			double y = getChannelYPos (channel) + ch_height - key_height;
			ProcessingApplication.Current.pushMatrix (); // user_code
			var h = ProcessingApplication.Current.Host; // user_code
			// <user_code>
			for (int n = 0; n < 12; n++)
			{
				if (!IsWhiteKey (n))
					continue;
				int k = key_to_keyboard_idx [n];
				// </user_code>
				ProcessingApplication.Current.@strokeJoin (ProcessingApplication.ROUND);
				ProcessingApplication.Current.@strokeWeight (1);
				ProcessingApplication.Current.@stroke (color_basic_stroke);
				ProcessingApplication.Current.@fill (color_white_key);
				ProcessingApplication.Current.@rect (x + k * key_width, y, key_width, key_height);
				// user_code
				key_rectangles [channel, octave * 12 + n] = (Rectangle) h.Children.Last ();
			}
			// <user_code>
			var wh = ProcessingApplication.Current.Host;
			ProcessingApplication.Current.popMatrix ();
			ProcessingApplication.Current.Host.Children.Remove (wh);
			white_key_panel.Children.Add (wh);

			ProcessingApplication.Current.pushMatrix ();
			h = ProcessingApplication.Current.Host; // user_code
			for (int n = 0; n < 12; n++)
			{
				if (IsWhiteKey (n))
					continue;
				int k = key_to_keyboard_idx [n];
				// </custom_code>
				if (k != 2 && k != 6) {
					ProcessingApplication.Current.@strokeJoin (ProcessingApplication.BEVEL);
					ProcessingApplication.Current.@strokeWeight (1);
					ProcessingApplication.Current.@stroke (color_basic_stroke);
					ProcessingApplication.Current.@fill (color_black_key);
					double blackKeyStartX = x + (k + 0.8) * key_width;
					ProcessingApplication.Current.@rect (blackKeyStartX, y + 1, blackKeyWidth, blackKeyHeight);
					// user_code
					key_rectangles [channel, octave * 12 + n] = (Rectangle) h.Children.Last ();
					double bottom = y + blackKeyHeight + 1;
					ProcessingApplication.Current.@stroke (color_black_key_edge);
					ProcessingApplication.Current.@line (blackKeyStartX + 1, bottom, blackKeyStartX + blackKeyWidth - 1, bottom);
				}
			}
			// <user_code>
			var bh = ProcessingApplication.Current.Host;
			ProcessingApplication.Current.popMatrix ();
			ProcessingApplication.Current.Host.Children.Remove (bh);
			black_key_panel.Children.Add (bh);
			// </user_code>
		}
public static void Run ()
{
all_keys = 128 - 24;
channels = 16;
key_width = 7;
key_height = 16;
blackKeyWidth = key_width * 0.4 + 1;
blackKeyHeight = key_height / 2;
ch_height = 32;
text_height = 8;
play_info_section_width = 200;
color_background = ProcessingApplication.Current.color ("#000008");
color_white_key = ProcessingApplication.Current.color ("#AaAaAa");
color_basic_stroke = ProcessingApplication.Current.color ("#000000");
color_black_key = ProcessingApplication.Current.color ("#000000");
color_black_key_edge = ProcessingApplication.Current.color ("#FfFfFf");
color_keyon = ProcessingApplication.Current.color ("#FfFf00");
color_aftertouch = ProcessingApplication.Current.color ("#Ff8000");
color_bend = ProcessingApplication.Current.color ("#0080Ff");
color_hold = ProcessingApplication.Current.color ("#0080C0");
color_bright = ProcessingApplication.Current.color ("#FfFfE0");
color_usual = ProcessingApplication.Current.color ("#3060C0");
color_dark = ProcessingApplication.Current.color ("#1830C0");
color_hidden = ProcessingApplication.Current.color ("#000030");
color_ch_base = App.color_bright;
color_ch_colored = App.color_usual;
color_ch_dark = App.color_dark;
color_ch_hidden = App.color_hidden;
color_ch_text_colored = App.color_ch_colored;
color_ch_text_base = App.color_ch_base;
color_ch_text_dark = App.color_ch_dark;
color_ch_text_hidden = App.color_ch_hidden;
ch_types = new string [] {@"MIDI", @"MIDI", @"MIDI", @"MIDI", @"MIDI", @"MIDI", @"MIDI", @"MIDI", @"MIDI", @"MIDI", @"MIDI", @"MIDI", @"MIDI", @"MIDI", @"MIDI", @"MIDI"};
setup ();
OnApplicationSetup (ProcessingApplication.Current);
}
}
}

