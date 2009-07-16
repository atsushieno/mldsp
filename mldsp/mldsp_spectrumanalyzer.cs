using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Threading;
using Commons.Music.Midi.Player;

namespace mldsp
{
	public class SpectrumAnalyzerPanel : Canvas, IPlayerStatusView
	{
		TextBlock label;
		Rectangle box;
		Line topline, baseline;

		public SpectrumAnalyzerPanel ()
		{
			label = new TextBlock () { Text = "SPECTRUM ANALYZER" };
			Canvas.SetLeft (label, 260);
			Canvas.SetTop (label, 0);
			Children.Add (label);
			box = new Rectangle () { Width = 340, Height = 100 };
			Canvas.SetLeft (box, 10);
			Canvas.SetTop (box, 10);
			Children.Add (box);
			topline = new Line () { X1 = 10, Y1 = 0, X2 = 350, Y2 = 0 };
			Canvas.SetLeft (topline, 0);
			Canvas.SetTop (topline, 0);
			Children.Add (topline);
			baseline = new Line () { X1 = 40, Y1 = 100, X2 = 340, Y2 = 100 };
			Canvas.SetLeft (baseline, 0);
			Canvas.SetTop (baseline, 0);
			Children.Add (baseline);
		}
		
		public double FontSize {
			get { return label.FontSize; }
			set { label.FontSize = value; }
		}

		public Brush Foreground {
			get { return baseline.Stroke; }
			set {
				label.Foreground = value;
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
				box.Fill = value;
			}
		}
		
		#region IPlayerStatusView implementation
		public void ProcessBeginPlay (MidiPlayer player, int totalMilliseconds)
		{
			throw new NotImplementedException ();
		}
		
		public void ProcessSkip (int seekMilliseconds)
		{
			throw new NotImplementedException ();
		}
		
		public void ProcessPause ()
		{
			throw new NotImplementedException ();
		}
		
		public void ProcessStop ()
		{
			throw new NotImplementedException ();
		}
		
		public void ProcessResume ()
		{
			throw new NotImplementedException ();
		}
		
		public void ProcessChangeTempo (int bpm)
		{
			throw new NotImplementedException ();
		}
		
		public void ProcessChangeTempoRatio (double ratio)
		{
			throw new NotImplementedException ();
		}
		#endregion
	}
}
