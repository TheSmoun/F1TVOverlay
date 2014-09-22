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
    /// Interaktionslogik für DriverInfo.xaml
    /// </summary>
    public partial class DriverInfo : UserControl, IWidget
    {
        private static readonly SolidColorBrush improvedBrush = new SolidColorBrush(Color.FromRgb(0x24, 0x9B, 0x21));       // #249B21
        private static readonly SolidColorBrush neutralBrush = new SolidColorBrush(Color.FromRgb(0xFF, 0xFF, 0xFF));        // #FFFFFF
        private static readonly SolidColorBrush lostBrush = new SolidColorBrush(Color.FromRgb(0xFF, 0x0, 0x0));             // #FF0000

        private static readonly float improvedAngle = 0;
        private static readonly float neutralAngle = -90;
        private static readonly float lostAngle = 180;

        public bool Active { get; private set; }

        private CameraModule cameraModule;
        private LiveStandingsModule standingsModule;
        private SessionTimerModule sessionTimer;
        private DriverInfoMode mode;

        private LiveStandingsItem driver;

        private bool pActive;
        private bool fActive;
        private bool gActive;
        private bool bActive;

        public DriverInfo()
        {
            this.InitializeComponent();
            mode = DriverInfoMode.NameOnly;
        }

        public void FadeIn(DriverInfoMode mode)
        {
            if (cameraModule == null)
                cameraModule = TMTVO.Controller.TMTVO.Instance.Api.FindModule("CameraModule") as CameraModule;

            if (standingsModule == null)
                standingsModule = TMTVO.Controller.TMTVO.Instance.Api.FindModule("LiveStandings") as LiveStandingsModule;

            if (sessionTimer == null)
                sessionTimer = TMTVO.Controller.TMTVO.Instance.Api.FindModule("SessionTimer") as SessionTimerModule;

            if (Active)
                return;

            int camIndex = cameraModule.FollowedDriver;
            driver = standingsModule.FindDriver(camIndex);
            if (driver == null)
                return;

            Active = true;
            this.mode = mode;

            DriversName.Text = driver.Driver.LastUpperName;
            TeamCarName.Text = Controller.TMTVO.Instance.Cars.GetValue(driver.Driver.Car.CarName);
            DriversNumber.Text = driver.Driver.Car.CarNumber;
            NumberPlate.Fill = new SolidColorBrush(driver.Driver.LicColor);

            Storyboard sb = FindResource("FadeInName") as Storyboard;
            sb.Begin();

            if (mode != DriverInfoMode.NameOnly)
            {
                int position = 0;
                if (mode == DriverInfoMode.QualiTimeOnly || mode == DriverInfoMode.QualiTimeWithGap)
                    position = GridModule.FindDriverStatic(driver).Position;
                else
                    position = driver.PositionLive;

                if (position == 1)
                    BackgroundLeader.Visibility = Visibility.Visible;
                else
                    BackgroundLeader.Visibility = Visibility.Hidden;

                Position.Text = position.ToString("0");
                (FindResource("FadeInPosition") as Storyboard).Begin();
                pActive = true;
            }

            if (mode == DriverInfoMode.Improvements)
            {
                BestTime.Text = driver.FastestLapTime.ConvertToTimeString();
                LastTime.Text = driver.LastLapTime.ConvertToTimeString();

                int improvement = GridModule.FindDriverStatic(camIndex).Position - driver.PositionLive;
                if (improvement < 0)
                {
                    Improvement.Text = (-improvement).ToString("0");
                    ImpAngle.Angle = lostAngle;
                    ImpTriangle.Fill = lostBrush;
                }
                else if (improvement == 0)
                {
                    Improvement.Text = "0";
                    ImpAngle.Angle = neutralAngle;
                    ImpTriangle.Fill = neutralBrush;
                }
                else
                {
                    Improvement.Text = improvement.ToString("0");
                    ImpAngle.Angle = improvedAngle;
                    ImpTriangle.Fill = improvedBrush;
                }

                (FindResource("FadeInImprovements") as Storyboard).Begin();
                bActive = true;
            }

            if (mode == DriverInfoMode.FastestLapTimeOnly || mode == DriverInfoMode.FastestLapTimeWithGap || mode == DriverInfoMode.QualiTimeOnly || mode == DriverInfoMode.QualiTimeWithGap)
            {
                FastestTime.Text = ((mode == DriverInfoMode.QualiTimeOnly || mode == DriverInfoMode.QualiTimeWithGap) ? GridModule.FindDriverStatic(camIndex).QualiTime : driver.FastestLapTime).ConvertToTimeString();
                (FindResource("FadeInFastestLap") as Storyboard).Begin();
                fActive = true;
            }

            if (mode == DriverInfoMode.FastestLapTimeWithGap || mode == DriverInfoMode.QualiTimeWithGap)
            {
                if (mode == DriverInfoMode.FastestLapTimeWithGap)
                    Gap.Text = "+" + (driver.FastestLapTime - standingsModule.Leader.FastestLapTime).ConvertToTimeString();
                else
                    Gap.Text = "+" + (GridModule.FindDriverStatic(camIndex).QualiTime - GridModule.GetLeader().QualiTime).ConvertToTimeString();

                (FindResource("FadeInGap") as Storyboard).Begin();
                gActive = true;
            }
        }

        public void FadeOut()
        {
            if (!Active)
                return;

            Active = false;
            Storyboard sb = FindResource("FadeOutName") as Storyboard;
            sb.Completed += sb_Completed;
            sb.Begin();

            if (gActive)
                (FindResource("FadeOutGap") as Storyboard).Begin();

            if (fActive)
                (FindResource("FadeOutFastestLap") as Storyboard).Begin();

            if (bActive)
                (FindResource("FadeOutImprovements") as Storyboard).Begin();

            if (pActive)
                (FindResource("FadeOutPosition") as Storyboard).Begin();

            gActive = false;
            fActive = false;
            bActive = false;
            pActive = false;
        }

        private void sb_Completed(object sender, EventArgs e)
        {
            if (Parent != null)
                ((Grid)this.Parent).Children.Remove(this);
        }

        public void SwitchMode(DriverInfoMode newMode)
        {
            if (mode == DriverInfoMode.NameOnly && (newMode == DriverInfoMode.PositionOnly || newMode == DriverInfoMode.QualiTimeOnly || newMode == DriverInfoMode.QualiTimeWithGap || newMode == DriverInfoMode.Improvements || newMode == DriverInfoMode.FastestLapTimeOnly || newMode == DriverInfoMode.FastestLapTimeWithGap))
            {
                (FindResource("FadeInPosition") as Storyboard).Begin();
                pActive = true;
            }

            if (newMode == DriverInfoMode.NameOnly && (mode == DriverInfoMode.PositionOnly || mode == DriverInfoMode.QualiTimeOnly || mode == DriverInfoMode.QualiTimeWithGap || mode == DriverInfoMode.Improvements || mode == DriverInfoMode.FastestLapTimeOnly || mode == DriverInfoMode.FastestLapTimeWithGap))
            {
                (FindResource("FadeOutPosition") as Storyboard).Begin();
                pActive = false;
            }

            if ((mode == DriverInfoMode.NameOnly || mode == DriverInfoMode.PositionOnly || mode == DriverInfoMode.Improvements)
                && (newMode == DriverInfoMode.FastestLapTimeOnly || newMode == DriverInfoMode.FastestLapTimeWithGap || newMode == DriverInfoMode.QualiTimeOnly || newMode == DriverInfoMode.QualiTimeWithGap))
            {
                (FindResource("FadeInFastestLap") as Storyboard).Begin();
                fActive = true;
            }

            if (((newMode == DriverInfoMode.NameOnly || newMode == DriverInfoMode.PositionOnly || newMode == DriverInfoMode.Improvements)
                && (mode == DriverInfoMode.FastestLapTimeOnly || mode == DriverInfoMode.FastestLapTimeWithGap || mode == DriverInfoMode.QualiTimeOnly || mode == DriverInfoMode.QualiTimeWithGap)))
            {
                (FindResource("FadeOutFastestLap") as Storyboard).Begin();
                fActive = false;
            }

            if ((mode == DriverInfoMode.NameOnly || mode == DriverInfoMode.PositionOnly || mode == DriverInfoMode.QualiTimeOnly || mode == DriverInfoMode.FastestLapTimeOnly || mode == DriverInfoMode.Improvements)
                && (newMode == DriverInfoMode.FastestLapTimeWithGap || newMode == DriverInfoMode.QualiTimeWithGap))
            {
                (FindResource("FadeInGap") as Storyboard).Begin();
                gActive = true;
            }

            if ((newMode == DriverInfoMode.NameOnly || newMode == DriverInfoMode.PositionOnly || newMode == DriverInfoMode.QualiTimeOnly || newMode == DriverInfoMode.FastestLapTimeOnly || newMode == DriverInfoMode.Improvements)
                && (mode == DriverInfoMode.FastestLapTimeWithGap || mode == DriverInfoMode.QualiTimeWithGap))
            {
                (FindResource("FadeOutGap") as Storyboard).Begin();
                gActive = false;
            }

            if (newMode == DriverInfoMode.Improvements && mode != DriverInfoMode.Improvements)
            {
                (FindResource("FadeInImprovements") as Storyboard).Begin();
                bActive = true;
            }

            if (newMode != DriverInfoMode.Improvements && mode == DriverInfoMode.Improvements)
            {
                (FindResource("FadeOutImprovements") as Storyboard).Begin();
                bActive = false;
            }

            mode = newMode;
        }

        public void Tick()
        {
            int index = cameraModule.FollowedDriver;
            LiveStandingsItem driver = standingsModule.FindDriver(index);
            if (driver == null)
            {
                FadeOut();
                return;
            }

            DriversName.Text = driver.Driver.LastUpperName;
            TeamCarName.Text = Controller.TMTVO.Instance.Cars.GetValue(driver.Driver.Car.CarName);
            DriversNumber.Text = driver.Driver.Car.CarNumber;
            NumberPlate.Fill = new SolidColorBrush(driver.Driver.LicColor);

            if (pActive)
            {
                int position = 0;
                if (mode == DriverInfoMode.QualiTimeOnly || mode == DriverInfoMode.QualiTimeWithGap)
                    position = GridModule.FindDriverStatic(driver).Position;
                else
                    position = driver.PositionLive;

                if (position == 1)
                    BackgroundLeader.Visibility = Visibility.Visible;
                else
                    BackgroundLeader.Visibility = Visibility.Hidden;

                Position.Text = position.ToString("0");
            }

            if (fActive)
                if (mode == DriverInfoMode.FastestLapTimeOnly || mode == DriverInfoMode.FastestLapTimeWithGap)
                    FastestTime.Text = driver.FastestLapTime.ConvertToTimeString();
                else if (mode == DriverInfoMode.QualiTimeOnly || mode == DriverInfoMode.QualiTimeWithGap)
                    FastestTime.Text = GridModule.FindDriverStatic(driver).QualiTime.ConvertToTimeString();

            if (gActive)
            {
                if (mode == DriverInfoMode.FastestLapTimeWithGap)
                {
                    if ((sessionTimer.SessionType == SessionType.LapRace || sessionTimer.SessionType == SessionType.TimeRace) && (sessionTimer.SessionState == SessionState.Racing || sessionTimer.SessionState == SessionState.Checkered || sessionTimer.SessionState == SessionState.Cooldown))
                        if (driver.GapLaps == 0)
                            Gap.Text = "+" + driver.GapTime.ConvertToTimeString();
                        else if (driver.GapLive == 1)
                            Gap.Text = "+1 Lap";
                        else Gap.Text = "+" + driver.GapLaps.ToString("0") + " Laps";
                    else
                        Gap.Text = "+" + (driver.FastestLapTime - standingsModule.Leader.FastestLapTime).ConvertToTimeString();

                }
                else
                    Gap.Text = "+" + (GridModule.FindDriverStatic(driver).QualiTime - GridModule.GetLeader().QualiTime).ConvertToTimeString();
            }

            if (bActive)
            {
                BestTime.Text = driver.FastestLapTime.ConvertToTimeString();
                LastTime.Text = driver.LastLapTime.ConvertToTimeString();

                int improvement = GridModule.FindDriverStatic(driver).Position - driver.PositionLive;
                if (improvement < 0)
                {
                    Improvement.Text = (-improvement).ToString("0");
                    ImpAngle.Angle = lostAngle;
                    ImpTriangle.Fill = lostBrush;
                }
                else if (improvement == 0)
                {
                    Improvement.Text = "0";
                    ImpAngle.Angle = neutralAngle;
                    ImpTriangle.Fill = neutralBrush;
                }
                else
                {
                    Improvement.Text = improvement.ToString("0");
                    ImpAngle.Angle = improvedAngle;
                    ImpTriangle.Fill = improvedBrush;
                }
            }
        }

        public enum DriverInfoMode : int
        {
            NameOnly = 0,
            PositionOnly = 1,
            FastestLapTimeOnly = 2,
            FastestLapTimeWithGap = 3,
            Improvements = 4,
            QualiTimeOnly = 5,
            QualiTimeWithGap = 6
        }
    }
}