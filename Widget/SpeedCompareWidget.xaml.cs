using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;
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
using TMTVO.ThemeApi;

namespace F1TVOverlay.Widget
{
	/// <summary>
	/// Interaktionslogik für SpeedCompareWidget.xaml
	/// </summary>
	public partial class SpeedCompareWidget : UserControl, IWidget
	{
        private static readonly Dictionary<int, string> gears = new Dictionary<int, string>()
        {
            {-1, "R"},
            {0, "N"},
            {1, "1"},
            {2, "2"},
            {3, "3"},
            {4, "4"},
            {5, "5"},
            {6, "6"},
            {7, "7"}
        };

        public bool Active { get; private set; }

        private LiveStandingsItem driver1;
        private LiveStandingsItem driver2;

        private Timer neutralCooldown1;
        private Timer neutralCooldown2;
        private int currentGear1;
        private int currentGear2;
        private int prevGear1;
        private int prevGear2;
        private bool canUpdateGear1;
        private bool canUpdateGear2;
        private bool pushToPass1;
        private bool pushToPass2;
        private bool prevPushToPass1;
        private bool prevPushToPass2;

		public SpeedCompareWidget()
		{
			this.InitializeComponent();
		}

        public void FadeIn(LiveStandingsItem driver1, LiveStandingsItem driver2)
        {
            if (Active || driver1 == null || driver2 == null)
                return;

            Active = true;
            this.driver1 = driver1;
            this.driver2 = driver2;

            DriversNumber1.Text = driver1.Driver.Car.CarNumber;
            DriversNumber2.Text = driver2.Driver.Car.CarNumber;

            DriversName1.Text = driver1.Driver.LastUpperName;
            DriversName2.Text = driver2.Driver.LastUpperName;

            NumberPlate1.Fill = new SolidColorBrush(driver1.Driver.LicColor);
            NumberPlate2.Fill = new SolidColorBrush(driver2.Driver.LicColor);

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

        private void setSpeeds(int speed1, int speed2)
        {
            if (speed1 <= 0)
                LeftSpeedRotation.Angle = -180;
            else if (speed1 > 360)
                LeftSpeedRotation.Angle = 0;
            else
                LeftSpeedRotation.Angle = -180 + 180F * (speed1 / 360F);

            if (speed1 <= 100)
            {
                Speed.Opacity = 0F;
                SpeedLow.Opacity = 1f;
            }
            else if (speed1 >= 200)
            {
                Speed.Opacity = 1F;
                SpeedLow.Opacity = 0f;
            }
            else
            {
                Speed.Opacity = 0F + ((speed1 - 100F) / 100F);
                SpeedLow.Opacity = 1f;
            }

            Speed.Text = speed1.ToString("0");
            SpeedLow.Text = speed1.ToString("0");

            if (speed2 <= 0)
                RightSpeedRotation.Angle = -180;
            else if (speed2 > 360)
                RightSpeedRotation.Angle = 0;
            else
                RightSpeedRotation.Angle = -180 + 180F * (speed2 / 360F);

            if (speed2 <= 100)
            {
                Speed_Copy.Opacity = 0F;
                SpeedLow_Copy.Opacity = 1f;
            }
            else if (speed2 >= 200)
            {
                Speed_Copy.Opacity = 1F;
                SpeedLow_Copy.Opacity = 0f;
            }
            else
            {
                Speed_Copy.Opacity = 0F + ((speed2 - 100F) / 100F);
                SpeedLow_Copy.Opacity = 1f;
            }

            Speed_Copy.Text = speed2.ToString("0");
            SpeedLow_Copy.Text = speed2.ToString("0");
        }

        private void setRevs(int rev1, int rev2)
        {
            if (rev1 <= 0)
                RevsFillLeft.Height = 0;
            else if (rev1 > 18000)
                RevsFillLeft.Height = 125;
            else
                RevsFillLeft.Height = 125F * (rev1 / 18000F);

            if (rev2 <= 0)
                RevsFillRight.Height = 0;
            else if (rev2 > 18000)
                RevsFillRight.Height = 125;
            else
                RevsFillRight.Height = 125F * (rev2 / 18000F);
        }

        private void updateGear1()
        {
            // TODO implement
        }

        private void updateGear2()
        {
            // TODO implement
        }

        private void setPushToPass(bool p2p1, bool p2p2)
        {
            // TODO implement
        }

        public void Tick()
        {
            float[] rpms = (float[])Controller.TMTVO.Instance.Api.GetData("CarIdxRPM");
            float rpm1 = rpms[driver1.Driver.CarIndex];
            float rpm2 = rpms[driver2.Driver.CarIndex];
            if (rpm1 < 0 || rpm2 < 0)
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(FadeOut));
                return;
            }

            setSpeeds((int)(driver1.SpeedKmh), (int)(driver2.SpeedKmh));
            setRevs((int)rpm1, (int)rpm2);

            int[] gears = (int[])Controller.TMTVO.Instance.Api.GetData("CarIdxGear");
            prevGear1 = gears[driver1.Driver.CarIndex];
            prevGear2 = gears[driver2.Driver.CarIndex];

            /*prevPushToPass1 = false;                                                                                             // TODO get Push to pass value
            prevPushToPass2 = false;                                                                                             // TODO get Push to pass value
            if (prevPushToPass1 && !pushToPass1)
                Application.Current.Dispatcher.Invoke(new Action(fadeInP2P));
            else if (!prevPushToPass && pushToPass)
                Application.Current.Dispatcher.Invoke(new Action(fadeOutP2P));*/

            if (prevGear1 == 0 && canUpdateGear1)
            {
                canUpdateGear1 = false;
                neutralCooldown1.Start();
            }

            if (prevGear2 == 0 && canUpdateGear2)
            {
                canUpdateGear2 = false;
                neutralCooldown2.Start();
            }

            if (canUpdateGear1)
                updateGear1();

            if (canUpdateGear2)
                updateGear2();
        }
    }
}