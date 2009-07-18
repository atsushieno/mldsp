using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Threading;
using Commons.Music.Midi;
using Commons.Music.Midi.Player;

namespace mldsp
{
	public class SpectrumAnalyzerPanel : Canvas, IPlayerStatusView
	{
		TextBlock label;
		Rectangle box;
		Line topline, baseline;
		byte [] prev_numbers = new byte [64]; // 5 dots per slot. 4 dots for meter.
		byte [] numbers = new byte [64]; // 5 dots per slot. 4 dots for meter.
		Rectangle [] volume_boxes = new Rectangle [64];
		Rectangle [] volume_levels = new Rectangle [64];
		List<Rectangle> mesh = new List<Rectangle> ();
		DispatcherTimer timer = new DispatcherTimer ();

		public SpectrumAnalyzerPanel ()
		{
			label = new TextBlock () { Text = "PSEUDO SPECTRUM ANALYZER" };
			Canvas.SetLeft (label, 230);
			Canvas.SetTop (label, 0);
			Children.Add (label);
			box = new Rectangle () { Width = 350, Height = 110 };
			Canvas.SetLeft (box, 5);
			Canvas.SetTop (box, 10);
			Children.Add (box);
			topline = new Line () { X1 = 5, Y1 = 0, X2 = 355, Y2 = 0 };
			Canvas.SetLeft (topline, 0);
			Canvas.SetTop (topline, 0);
			Children.Add (topline);
			baseline = new Line () { X1 = 30, Y1 = 115, X2 = 350, Y2 = 115 };
			Canvas.SetLeft (baseline, 0);
			Canvas.SetTop (baseline, 0);
			Children.Add (baseline);
			
			timer.Tick += delegate { UpdateSpectrumSnapshot (); };
			timer.Interval = TimeSpan.FromMilliseconds (200);

			for (int i = 0; i < 64; i++) {
				var r = new Rectangle () { Width = 4, Height = 100 };
				volume_boxes [i] = r;
				Canvas.SetLeft (r, i * 5 + 30);
				Canvas.SetTop (r, 15);
				Children.Add (r);

				r = new Rectangle () { Width = 4, Height = 100 };
				volume_levels [i] = r;
				Canvas.SetLeft (r, i * 5 + 30);
				Canvas.SetTop (r, 15);
				Children.Add (r);
				numbers [i] = 100;
			}

			for (int x = 0; x < 100; x += 3) {
				var xl = new Rectangle () { Width = 320, Height = 2};
				Canvas.SetLeft (xl, 30);
				Canvas.SetTop (xl, 15 + x);
				Children.Add (xl);
				mesh.Add (xl);
			}
			var brush = new SolidColorBrush (App.color_background);
			box.Fill = brush;
			foreach (var ml in mesh)
				ml.Fill = brush;
		}
		
		public double FontSize {
			get { return label.FontSize; }
			set { label.FontSize = value; }
		}

		public MidiMachine Registers { get; set; }
		
		public Brush Foreground {
			get { return baseline.Stroke; }
			set {
				label.Foreground = value;
				for (int i = 0; i < 64; i++)
					volume_boxes [i].Fill = value;
			}
		}
		
		public Brush DarkColor {
			get { return topline.Stroke; }
			set {
				topline.Stroke = value;
				baseline.Stroke = value;
			}
		}

		public Brush Background {
			get { return box.Fill; }
			set {
				for (int i = 0; i < 64; i++)
					volume_levels [i].Fill = value;
			}
		}
		
		void UpdateSpectrumSnapshot ()
		{
			for (int i = 0; i < numbers.Length; i++) {
				if (prev_numbers [i] != numbers [i]) {
					prev_numbers [i] = numbers [i];
					volume_levels [i].Height = numbers [i];
				}
				if (numbers [i] < 100) {
					numbers [i] += 3;
					volume_levels [i].Height += 3;
				}
			}
		}
		
		public void ProcessKeyOn (byte channel, byte note, byte velocity)
		{
			// FIXME: it is too simple.
			var chRegs = Registers.Channels [channel];
			double baseValue = (chRegs.Controls [SmfCC.Volume] / 128.0) * (chRegs.Controls [SmfCC.Expression] / 128.0) * velocity / 128.0 * 100;
			baseValue = 100 - baseValue;
			if (numbers [note / 2] > baseValue)
				numbers [note / 2] = (byte) baseValue;
			double d;
			int n;
			for (n = note / 2, d = baseValue; n < 100 && d < 100; n++, d += 8.0)
				if (numbers [n] > d)
					numbers [n] = (byte) d;
			for (n = note / 2, d = baseValue; n >= 0 && d < 100; n--, d += 8.0)
				if (numbers [n] > d)
					numbers [n] = (byte) d;
			
			UpdateSpectrumSnapshot (); // FIXME: call it by timer.
		}

		#region IPlayerStatusView implementation
		public void ProcessBeginPlay (MidiPlayer player, int totalMilliseconds)
		{
			timer.Start ();
		}
		
		public void ProcessSkip (int seekMilliseconds)
		{
		}
		
		public void ProcessPause ()
		{
		}
		
		public void ProcessStop ()
		{
			timer.Stop ();
			for (int i = 0; i < 64; i++) {
				numbers [i] = 100;
				volume_levels [i].Height = 100;
			}
		}
		
		public void ProcessResume ()
		{
		}
		
		public void ProcessChangeTempo (int bpm)
		{
		}
		
		public void ProcessChangeTempoRatio (double ratio)
		{
		}
		#endregion
	}
}
