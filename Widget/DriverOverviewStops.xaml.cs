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
using TMTVO.Data.Modules;
using TMTVO.Data;
using System.Threading;
using F1TVOverlay.Interfaces;

namespace F1TVOverlay.Widget
{
	/// <summary>
	/// Interaktionslogik für DriverOverviewStops.xaml
	/// </summary>
	public partial class DriverOverviewStops : UserControl, ISideBarElement
	{
        private static readonly SolidColorBrush improvedBrush = new SolidColorBrush(Color.FromRgb(0x13, 0x7A, 0x15));       // #137A15
        private static readonly SolidColorBrush neutralBrush = new SolidColorBrush(Color.FromRgb(0x62, 0x61, 0x67));        // #626167
        private static readonly SolidColorBrush lostBrush = new SolidColorBrush(Color.FromRgb(0xDF, 0xAA, 0x1B));           // #DFAA1B

        private static readonly SolidColorBrush whiteBrush = new SolidColorBrush(Colors.White);
        private static readonly SolidColorBrush blackBrush = new SolidColorBrush(Colors.Black);

        private static readonly float improvedAngle = 0;
        private static readonly float neutralAngle = -90;
        private static readonly float lostAngle = 180;

        public bool Active { get; private set; }

        private LiveStandingsItem driver;

		public DriverOverviewStops()
		{
			this.InitializeComponent();
		}

        public void FadeIn(LiveStandingsItem driver, int delay)
        {
            if (Active || driver == null)
                return;

            this.driver = driver;
            Active = true;
            Tick();

            Thread t = new Thread(fadeInLater);
            t.Start(delay);
        }

        private void fadeInLater(object obj)
        {
            Thread.Sleep((int)obj);
            Application.Current.Dispatcher.BeginInvoke(new Action(() => 
            {
                (FindResource("FadeIn") as Storyboard).Begin();
            }));
        }

        public void FadeOut()
        {
            if (!Active)
                return;

            Reset();
            (FindResource("FadeOut") as Storyboard).Begin();
        }

        public void Tick()
        {
            int improvement = GridModule.FindDriverStatic(driver.Driver.CarIndex).Position - driver.PositionLive;
            if (improvement < 0)
            {
                Improved.Text = (-improvement).ToString("0");
                ImprovedBg.Fill = lostBrush;
                ImpAngle.Angle = lostAngle;
                ImpTriangle.Fill = blackBrush;
                Improved.Foreground = blackBrush;
            }
            else if (improvement == 0)
            {
                Improved.Text = "0";
                ImprovedBg.Fill = neutralBrush;
                ImpAngle.Angle = neutralAngle;
                ImpTriangle.Fill = whiteBrush;
                Improved.Foreground = whiteBrush;
            }
            else
            {
                Improved.Text = improvement.ToString("0");
                ImprovedBg.Fill = improvedBrush;
                ImpAngle.Angle = improvedAngle;
                ImpTriangle.Fill = whiteBrush;
                Improved.Foreground = whiteBrush;
            }

            if (driver.GapLaps > 0)
            {
                if (driver.GapLaps == 1)
                    Gap.Text = "+1 Lap";
                else
                    Gap.Text = "+" + driver.GapLaps.ToString("0") + " Laps";
            }
            else
            {
                if (driver.PositionLive == 1)
                    Gap.Text = "Leader";
                else
                    Gap.Text = "+" + driver.GapLiveLeader.ConvertToTimeString();
            }
        }

        public void Reset()
        {
            driver = null;
            Active = false;
        }
    }
}