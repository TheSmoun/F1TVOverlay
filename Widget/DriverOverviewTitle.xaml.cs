using F1TVOverlay.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
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

namespace F1TVOverlay.Widget
{
	/// <summary>
	/// Interaktionslogik für DriverOverviewTitle.xaml
	/// </summary>
	public partial class DriverOverviewTitle : UserControl, ISideBarElement
	{
        public bool Active { get; private set; }

        private LiveStandingsItem driver;

        public DriverOverviewTitle()
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
            if (driver == null)
                return;

            int pos = driver.PositionLive;
            if (pos == 1)
                NumberLeader.Visibility = Visibility.Visible;
            else
                NumberLeader.Visibility = Visibility.Hidden;

            Position.Text = pos.ToString("0");

            NumberPlate.Fill = new SolidColorBrush(driver.Driver.LicColor);
            DriversNumber.Text = driver.Driver.Car.CarNumber;
            DriverName.Text = driver.Driver.LastUpperName;
        }

        public void Reset()
        {
            driver = null;
            Active = false;
        }
    }
}