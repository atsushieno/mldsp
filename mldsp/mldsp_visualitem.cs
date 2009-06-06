
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace mldsp
{
	public class ParameterVisualizerPanel : Canvas
	{
		public ParameterVisualizerPanel ()
		{
			Volume = new NumericVisualItem ("VOL", 0);
			Expression = new NumericVisualItem ("EXP", 0);
			Rsd = new NumericVisualItem ("RSD", 0);
			Csd = new NumericVisualItem ("CSD", 0);
			Dsd = new NumericVisualItem ("DSD", 0);
			Hold = new SwitchVisualItem ("H", false);
			PortamentoSwitch = new SwitchVisualItem ("P", false);
			Sostenuto = new SwitchVisualItem ("So", false);
			SoftPedal = new SwitchVisualItem ("SP", false);
			Children.Add (Volume);
			Children.Add (Expression);
			Children.Add (Rsd);
			Children.Add (Csd);
			Children.Add (Dsd);
			Children.Add (Hold);
			Children.Add (PortamentoSwitch);
			Children.Add (Sostenuto);
			Children.Add (SoftPedal);
		}

		public double FontSize {
			get { return Volume.FontSize; }
			set {
				Volume.FontSize = value;
				Expression.FontSize = value;
				Rsd.FontSize = value;
				Csd.FontSize = value;
				Dsd.FontSize = value;
				Hold.FontSize = value;
				PortamentoSwitch.FontSize = value;
				Sostenuto.FontSize = value;
				SoftPedal.FontSize = value;
			}
		}

		public Brush Foreground {
			get { return Volume.Foreground; }
			set {
				Volume.Foreground = value;
				Expression.Foreground = value;
				Rsd.Foreground = value;
				Csd.Foreground = value;
				Dsd.Foreground = value;
				Hold.Foreground = value;
				PortamentoSwitch.Foreground = value;
				Sostenuto.Foreground = value;
				SoftPedal.Foreground = value;
			}
		}
		
		Point location;
		public Point Location {
			get { return location; }
			set {
				this.location = value;
				Volume.Location = new Point (value.X + 0, value.Y + 0);
				Expression.Location = new Point (value.X + 60, value.Y + 0);
				Rsd.Location = new Point (value.X + 0, value.Y + 8);
				Csd.Location = new Point (value.X + 60, value.Y + 8);
				Dsd.Location = new Point (value.X + 120, value.Y + 8);
				Hold.Location = new Point (value.X + 200, value.Y + 0);
				PortamentoSwitch.Location = new Point (value.X + 200, value.Y + 8);
				Sostenuto.Location = new Point (value.X + 220, value.Y + 0);
				SoftPedal.Location = new Point (value.X + 220, value.Y + 8);
			}
		}

		// volume, expression, RSD/CSD/DSD, Hold, BendMode
		public NumericVisualItem Volume { get; private set; }
		public NumericVisualItem Expression { get; private set; }
		public NumericVisualItem Rsd { get; private set; }
		public NumericVisualItem Csd { get; private set; }
		public NumericVisualItem Dsd { get; private set; }
		public SwitchVisualItem Hold { get; private set; }
		public SwitchVisualItem PortamentoSwitch { get; private set; }
		public SwitchVisualItem Sostenuto { get; private set; }
		public SwitchVisualItem SoftPedal { get; private set; }
	}

	public abstract class VisualItem : Canvas
	{
		public TextBlock Label { get; protected set; }
		public abstract Point Location { get; set; }
	}
	
	public class NumericVisualItem : VisualItem
	{
		public NumericVisualItem (string label, int initialValue)
		{
			Label = new TextBlock ();
			Label.Text = label + ":";
			ChangeDirection = new TextBlock ();
			ChangeDirection.Text = "*";
			Value = new TextBlock ();

			SetValue (initialValue);

			Children.Add (Label);
			Children.Add (ChangeDirection);
			Children.Add (Value);
		}
		
		public Brush Foreground {
			get { return Label.Foreground; }
			set {
				Label.Foreground = value;
				ChangeDirection.Foreground = value;
				Value.Foreground = value;
			}
		}
		
		public double FontSize {
			get { return Label.FontSize; }
			set {
				Label.FontSize = value;
				ChangeDirection.FontSize = value;
				Value.FontSize = value;
			}
		}

		Point location;
		public override Point Location {
			get { return location; }
			set {
				location = value;
				Canvas.SetLeft (Label, location.X);
				Canvas.SetTop (Label, location.Y);
				Canvas.SetLeft (ChangeDirection, location.X + 20);
				Canvas.SetTop (ChangeDirection, location.Y);
				Canvas.SetLeft (Value, location.X + 30);
				Canvas.SetTop (Value, location.Y);
			}
		}

		public TextBlock ChangeDirection { get; private set; }
		public TextBlock Value { get; private set; }
		int current_value;

		public void SetValue (int value)
		{
			if (current_value < value)
				ChangeDirection.Text = "\u25B2"; // FIXME: start animation
			else if (current_value > value)
				ChangeDirection.Text = "\u25BC"; // FIXME: start animation
			Value.Text = value.ToString ("D03");
			current_value = value;
		}
	}
	
	public class SwitchVisualItem : VisualItem
	{
		public SwitchVisualItem (string label, bool initialValue)
		{
			Label = new TextBlock ();
			Label.Text = label;
			flag_rect = new Rectangle () { Width = 8, Height = 8 };

			Value = initialValue;

			Children.Add (flag_rect);
			Children.Add (Label);
		}

		Brush fg_color;
		public Brush Foreground {
			get { return Label.Foreground; }
			set {
				if (fg_color == null || Label.Foreground == fg_color)
					Label.Foreground = value;
				fg_color = value;
			}
		}

		public double FontSize {
			get { return Label.FontSize; }
			set { Label.FontSize = value; }
		}

		Point location;
		public override Point Location {
			get { return location; }
			set {
				location = value;
				Canvas.SetLeft (Label, location.X);
				Canvas.SetTop (Label, location.Y);
				Canvas.SetLeft (flag_rect, location.X - 1);
				Canvas.SetTop (flag_rect, location.Y + 1);
			}
		}

		Rectangle flag_rect;
		SolidColorBrush on_color = new SolidColorBrush (Color.FromArgb (192, 128, 128, 255));
		SolidColorBrush off_color = new SolidColorBrush (Color.FromArgb (0, 0, 0, 0));
		SolidColorBrush white_color = new SolidColorBrush (Color.FromArgb (255, 255, 255, 255));

		bool current_value;
		public bool Value {
			get { return current_value; }
			set {
				if (current_value == value)
					return;
				flag_rect.Fill = value ? on_color : off_color;
				Label.Foreground = value ? white_color : fg_color;
			}
		}
	}
}