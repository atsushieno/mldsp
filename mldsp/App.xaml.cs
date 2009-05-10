using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using ProcessingCli;

using color = System.UInt32;

namespace mldsp
{
	public partial class App : ProcessingApplication
	{
		public App ()
		{
			SetupColors ();
			InitializeComponent ();
			var c = new Canvas ();
			Current = this;
			Current.SetHost (c);
			RootVisual = c;
			c.Loaded += delegate { setup (); };
		}

		// constants
		int all_keys = 128 - 24; // strip octave 0 and octave 10
		int channels = 16;
		int key_width = 7;
		int key_height = 20;
		int ch_height = 36;
		int text_height = 8;
		int play_info_section_width = 200;

		static uint C (uint value)
		{
			return value;
		}

		color color_background = C (0x000008);
		color color_white_key = C (0xAaAaAa);
		color color_basic_stroke = C (0x000000);
		color color_black_key = C (0x000000);
		color color_black_key_edge = C (0xFfFfFf);
		color color_bright = C (0xFfFfE0);
		color color_usual = C (0x3060C0);
		color color_dark = C (0x1830C0);
		color color_hidden = C (0x000030);
		
		color color_ch_base;
		color color_ch_colored;
		color color_ch_dark;
		color color_ch_hidden;
		color color_ch_text_colored;
		color color_ch_text_base;
		color color_ch_text_dark;
		color color_ch_text_hidden;
		
		void SetupColors ()
		{
			color_ch_base = color_bright;
			color_ch_colored = color_usual;
			color_ch_dark = color_dark;
			color_ch_hidden = color_hidden;
			color_ch_text_colored = color_ch_colored;
			color_ch_text_base = color_ch_base;
			color_ch_text_dark = color_ch_dark;
			color_ch_text_hidden = color_ch_hidden;
		}

		PFont font_title;
		PFont font16;
		PFont font8;
		
		String [] ch_types = {
			"MIDI", "MIDI", "MIDI", "MIDI", "MIDI", "MIDI", "MIDI", "MIDI",
			"MIDI", "MIDI", "MIDI", "MIDI", "MIDI", "MIDI", "MIDI", "MIDI" };

		void setup ()
		{
			font_title = createFont ("PalatinoLinotype-BoldItalic", 16);
			font16 = createFont ("Tahoma", 16);
			font8= createFont ("Tahoma", 8);
			size (800, ch_height * channels);
			background (color_background);
			
			for (int i = 0; i < channels; i++) {
				setupChannelInfo (i);
			}
			for (int i = 0; i < channels; i++) {
				setupKeyboard (i);
			}
			
			pushMatrix ();
			translate (400, 0);
			setupPlayInfoSection ();
			popMatrix ();
			
			pushMatrix ();
			translate (400, 160);
			setupSpectrumAnalyzer ();
		}
	
		void setupSpectrumAnalyzer ()
		{
			stroke (color_usual);
			line (0, 0, 380, 0);
			textFont (font8);
			fill (color_dark);
			text ("SPECTRUM ANALYZER", 300, text_height * 2 - 2);
			stroke (color_hidden);
			fill (color_hidden);
			rect (50, text_height * 2, 320, 100);
		}
	
		void setupPlayInfoSection ()
		{
			setupTitleArea ();
			setupDriverInfo ();
			setupPlayStatus ();
			setupSongStatus ();
		}
	
		void setupTitleArea ()
		{
			// title area
			textFont (font_title);
			fill (color_ch_text_colored);
			text ("MLDSP", 0, 16);
			stroke (color_ch_colored);
			line (0, 18, 60, 18);
			textFont (font8);
			fill (color_ch_text_dark);
			text ("music visualizer and file selector", 70, text_height);
			fill (color_ch_text_colored);
			text ("ver 0.01 / for moonlight 2.0 (C)2009 atsushieno", 70, text_height * 2);
			stroke (color_ch_dark);
			line (70, 18, 310, 18);
		}
	
		void setupDriverInfo ()
		{
			textFont (font8);
			fill (color_ch_text_dark);
			text ("DRIVER \u25B6", 0, text_height * 4);
			fill (color_ch_text_colored);
			text ("SMF plugin", 50, text_height * 4);
			text ("0.01", 100, text_height * 4);
			text ("(C)2009 atsushieno", 130, text_height * 4);
		}
	
		void setupPlayStatus ()
		{
			stroke (color_ch_hidden);
			fill (color_background);
			rect (40, text_height * 9, 150, text_height);
			rect (190, text_height * 9, 20, text_height);
			textFont (font8);
			fill (color_ch_text_colored);
			text ("[>PLAY", 40, text_height * 11);
			fill (color_ch_text_hidden);
			text ("||PAUSE", 80, text_height * 11);
			text ("[]STOP", 120, text_height * 11);
			text ("\\\\FADE", 160, text_height * 11);
			text (">>FF", 40, text_height * 12);
			text ("<<REW", 80, text_height * 12);
			text ("<]LOAD", 120, text_height * 12);
		}
	
		void setupSongStatus ()
		{
			// labels
			fill (color_ch_colored);
			rect (230, text_height * 5, 4, text_height * 2);
			text ("PASSED", 240, text_height * 6);
			text ("    TIME", 240, text_height * 7);
			rect (230, text_height * 8, 4, text_height * 2);
			text ("TICK", 240, text_height * 9);
			text ("   COUNT", 240, text_height * 10);
			rect (230, text_height * 11, 4, text_height * 2);
			text ("TIMER", 240, text_height * 12);
			text ("   CYCLE", 240, text_height * 13);
			rect (230, text_height * 14, 4, text_height * 2);
			text ("LOOP", 240, text_height * 15);
			text ("   COUNT", 240, text_height * 16);
			rect (230, text_height * 17, 4, text_height * 2);
			text ("VOLUME", 240, text_height * 18);
			text ("   RATIO", 240, text_height * 19);
			// values
			textFont (font16);
			text ("00:00:00", 300, text_height * 7);
			text ("00000000", 300, text_height * 10);
			text ("00000200", 300, text_height * 13);
			text ("00000000", 300, text_height * 16);
			text ("100%", 300, text_height * 19);
		}
	
		void setupChannelInfo (int channel)
		{
			double yText1 = getChannelYPos (channel) + text_height;
			double yText2 = getChannelYPos (channel) + text_height * 2;
			// channel type and number
			fill (color_ch_text_colored);
			textFont (font16);
			textSize (16.0);
			text (nf (channel + 1, 2), 35, yText2);
			textFont (font8);
			fill (color_ch_text_colored);
			text (ch_types [channel], 0.0, yText1);
			fill (color_ch_text_base);
			text ("TRACK.", 0.0, yText2);
			// key-on meter
			fill (color_ch_colored);
			rect (80, getChannelYPos (channel), 20, text_height);
			fill (color_ch_colored);
			rect (100, getChannelYPos (channel), 6 * (16 - channel), text_height);
			// portament, LFO
			stroke (color_ch_colored);
			line (340, getChannelYPos (channel) + 2, 360, getChannelYPos (channel) + text_height - 2);
			fill (color_ch_text_colored);
			text (nf (1000, 5), 364, getChannelYPos (channel) + text_height);
			// channel parameters
			fill (color_ch_text_base);
			text ("KN:o" + nf (5, 1) + "c", 80, yText2);
			text ("TN:" + nf (1, 3), 130, yText2);
			text ("VEL:" + nf (110, 3), 180, yText2);
			text ("GT:" + nf (8, 3), 230, yText2);
			text ("DT:" + nf (-8, 3), 280, yText2);
			text ("M:--------", 340, yText2);
		}
	
		void setupKeyboard (int channel)
		{
			int octaves = all_keys / 12;
			for (int octave = 0; octave < octaves; octave++) {
				drawOctave (channel, octave);
			}
		}
	
		double getChannelYPos (int channel)
		{
			return channel * ch_height;
		}
	
		void drawOctave (int channel, int octave)
		{
			double x = octave * key_width * 7;
			double y = getChannelYPos (channel) + ch_height - key_height;
			for (int k = 0; k < 7; k++) {
				// white keys
				strokeJoin (ROUND);
				strokeWeight (1.0);
				stroke (color_basic_stroke);
				fill (color_white_key);
				rect (x + k * key_width, y, key_width, key_height);
			}
			for (int k = 0; k < 7; k++) {
				// black keys
				if (k != 2 && k != 6) {
					strokeJoin (BEVEL);
					strokeWeight (1.0);
					stroke (color_basic_stroke);
					fill (color_black_key);
					double blackKeyStartX = x + (k + 0.8) * key_width;
					double blackKeyWidth = key_width * 0.4;
					double blackKeyHeight = key_height / 2.0;
					rect (blackKeyStartX, y + 1, blackKeyWidth, blackKeyHeight);
					double bottom = y + blackKeyHeight + 1;
					stroke (color_black_key_edge);
					line (blackKeyStartX + 1, bottom, blackKeyStartX + blackKeyWidth - 1, bottom);
				}
			}
		}
	}
}
