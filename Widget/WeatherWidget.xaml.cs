using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TMTVO.Data;
using TMTVO.Data.Modules;
using TMTVO.ThemeApi;

namespace F1TVOverlay.Widget
{
	/// <summary>
	/// Interaktionslogik für WeatherWidget.xaml
	/// </summary>
	public partial class WeatherWidget : UserControl, IWidget
	{
        private static readonly string prefix = "CURRENT TRACK CONDITIONS - ";

        public bool Active { get; private set; }
        public SessionsModule Module { get; set; }

		public WeatherWidget()
		{
			this.InitializeComponent();
            Active = false;
		}

        public void FadeIn()
        {
            if (Active)
                return;

            Active = true;

            SkiesValue.Text = Module.Weather.Skies.GetStringValue();
            AirTempValue.Text = ((int)Module.Weather.AirTemp) + "°c";
            TrackTempValue.Text = ((int)Module.Weather.TrackTemp) + "°c";
            WindValue.Text = Module.Weather.WindSpeed.ToString("0.0").Replace(',', '.') + " m/s";
            HumidityValue.Text = Module.Weather.Humidity.ToString() + "%";
            TitleText.Text = prefix + Module.Track.City.ToUpper() + ", " + Module.Track.Country.ToUpper();

            Storyboard sb = FindResource("FadeIn") as Storyboard;
            sb.Begin();
        }

        public void FadeOut()
        {
            if (!Active)
                return;

            Active = false;
            Storyboard sb = FindResource("FadeOut") as Storyboard;
            sb.Completed += sb_Completed;
            sb.Begin();
        }

        private void sb_Completed(object sender, EventArgs e)
        {
            if (Parent != null)
                ((Grid)this.Parent).Children.Remove(this);
        }

        public void Tick() { }
    }
}