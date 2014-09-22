using F1TVOverlay.Widget;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
using System.Windows.Threading;
using TMTVO.Data;
using TMTVO.Data.Modules;
using TMTVO.ThemeApi;

namespace F1TVOverlay
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class Overlay : Window
    {
        private Dictionary<string, Brush> RacingLicenceToBrush = new Dictionary<string, Brush>()
        {
            {"R", new SolidColorBrush(Colors.Red)},
            {"D", new SolidColorBrush(Colors.Orange)},
            {"C", new SolidColorBrush(Colors.Yellow)},
            {"B", new SolidColorBrush(Colors.Green)},
            {"A", new SolidColorBrush(Colors.Blue)},
            {"P", new SolidColorBrush(Colors.DarkGray)},
            {"W", new SolidColorBrush(Colors.LightGray)}
        };

        private Dictionary<string, Color> RacingLicenceToColor = new Dictionary<string, Color>()
        {
            {"R", Colors.Red},
            {"D", Colors.Orange},
            {"C", Colors.Yellow},
            {"B", Colors.Green},
            {"A", Colors.Blue},
            {"P", Colors.DarkGray},
            {"W", Colors.LightGray}
        };

        public int CurrentFps { get; private set; }
        private int fps;

        public DriverInfo DriverInfo { get; private set; }
        public LapsRemainingWidget LapsRemaining { get; private set; }
        public LapTimerLeft LapTimerLeft { get; private set; }
        public LiveTimingWidget LiveTimingWidget { get; private set; }
        public RaceBar RaceBar { get; private set; }
        public ResultsWidget ResultsWidget { get; private set; }
        public RevMeter RevMeter { get; private set; }
        public SessionTimer SessionTimer { get; private set; }
        public SideBarWidget SideBar { get; private set; }
        public SpeedCompareWidget SpeedCompareWidget { get; private set; }
        public TeamRadio TeamRadio { get; private set; }
        public WeatherWidget WeatherWidget { get; private set; }

        private DispatcherTimer timer;
        private DispatcherTimer fpsTimer;
        private List<IWidget> widgets;

        public Overlay()
        {
            InitializeComponent();

            CompositionTarget.Rendering += CompositionTarget_Rendering;
            fpsTimer = new DispatcherTimer();
            fpsTimer.Interval = TimeSpan.FromSeconds(1);
            fpsTimer.Tick += fpsTimer_Tick;
            fpsTimer.Start();

            widgets = new List<IWidget>();

            this.DriverInfo = new DriverInfo();
            this.DriverInfo.Width = 640;
            this.DriverInfo.Height = 72;
            this.DriverInfo.HorizontalAlignment = HorizontalAlignment.Left;
            this.DriverInfo.VerticalAlignment = VerticalAlignment.Top;
            this.DriverInfo.Margin = new Thickness(370, 899, 0, 0);

            this.LapsRemaining = new LapsRemainingWidget();
            this.LapsRemaining.Width = 320;
            this.LapsRemaining.Height = 36;
            this.LapsRemaining.HorizontalAlignment = HorizontalAlignment.Left;
            this.LapsRemaining.VerticalAlignment = VerticalAlignment.Top;
            this.LapsRemaining.Margin = new Thickness(798, 106, 0, 0);

            this.LapTimerLeft = new LapTimerLeft();
            this.LapTimerLeft.Width = 430;
            this.LapTimerLeft.Height = 108;
            this.LapTimerLeft.HorizontalAlignment = HorizontalAlignment.Left;
            this.LapTimerLeft.VerticalAlignment = VerticalAlignment.Top;
            this.LapTimerLeft.Margin = new Thickness(370, 863, 0, 0);

            this.LiveTimingWidget = new LiveTimingWidget();
            this.LiveTimingWidget.Width = 310;
            this.LiveTimingWidget.Height = 792;
            this.LiveTimingWidget.HorizontalAlignment = HorizontalAlignment.Left;
            this.LiveTimingWidget.VerticalAlignment = VerticalAlignment.Top;
            this.LiveTimingWidget.Margin = new Thickness(158, 70, 0, 0);

            this.RaceBar = new RaceBar();
            this.RaceBar.Width = 1920;
            this.RaceBar.Height = 50;
            this.RaceBar.HorizontalAlignment = HorizontalAlignment.Center;
            this.RaceBar.VerticalAlignment = VerticalAlignment.Top;
            this.RaceBar.Margin = new Thickness(0, 970, 0, 0);

            this.ResultsWidget = new ResultsWidget();
            this.ResultsWidget.Width = 1152;
            this.ResultsWidget.Height = 580;
            this.ResultsWidget.HorizontalAlignment = HorizontalAlignment.Left;
            this.ResultsWidget.VerticalAlignment = VerticalAlignment.Top;
            this.ResultsWidget.Margin = new Thickness(418, 143, 0, 0);

            this.RevMeter = new RevMeter();
            this.RevMeter.Width = 315;
            this.RevMeter.Height = 301;
            this.RevMeter.HorizontalAlignment = HorizontalAlignment.Left;
            this.RevMeter.VerticalAlignment = VerticalAlignment.Top;
            this.RevMeter.Margin = new Thickness(100, 400, 0, 0);

            this.SessionTimer = new SessionTimer();
            this.SessionTimer.Width = 410;
            this.SessionTimer.Height = 36;
            this.SessionTimer.HorizontalAlignment = HorizontalAlignment.Left;
            this.SessionTimer.VerticalAlignment = VerticalAlignment.Top;
            this.SessionTimer.Margin = new Thickness(860, 70, 0, 0);

            this.SideBar = new SideBarWidget();
            this.SideBar.Width = 280;
            this.SideBar.Height = 792;
            this.SideBar.HorizontalAlignment = HorizontalAlignment.Left;
            this.SideBar.VerticalAlignment = VerticalAlignment.Top;
            this.SideBar.Margin = new Thickness(158, 70, 0, 0);

            this.SpeedCompareWidget = new SpeedCompareWidget();
            this.SpeedCompareWidget.Width = 555;
            this.SpeedCompareWidget.Height = 203;
            this.SpeedCompareWidget.HorizontalAlignment = HorizontalAlignment.Left;
            this.SpeedCompareWidget.VerticalAlignment = VerticalAlignment.Top;
            this.SpeedCompareWidget.Margin = new Thickness(1215, 762, 0, 0);

            this.TeamRadio = new TeamRadio();
            this.TeamRadio.Width = 342;
            this.TeamRadio.Height = 76;
            this.TeamRadio.HorizontalAlignment = HorizontalAlignment.Left;
            this.TeamRadio.VerticalAlignment = VerticalAlignment.Top;
            this.TeamRadio.Margin = new Thickness(1458, 520, 0, 0);

            this.WeatherWidget = new WeatherWidget();
            this.WeatherWidget.Width = 675;
            this.WeatherWidget.Height = 235;
            this.WeatherWidget.HorizontalAlignment = HorizontalAlignment.Center;
            this.WeatherWidget.VerticalAlignment = VerticalAlignment.Top;
            this.WeatherWidget.Margin = new Thickness(1045, 755, 0, 0);

            widgets.Add(DriverInfo);
            widgets.Add(LapsRemaining);
            widgets.Add(LapTimerLeft);
            widgets.Add(LiveTimingWidget);
            widgets.Add(RaceBar);
            widgets.Add(ResultsWidget);
            widgets.Add(RevMeter);
            widgets.Add(SessionTimer);
            widgets.Add(SideBar);
            widgets.Add(SpeedCompareWidget);
            widgets.Add(TeamRadio);
            widgets.Add(WeatherWidget);

            timer = new DispatcherTimer();
            timer.Tick += timer_Tick;
            timer.Interval = new TimeSpan(0, 0, 0, 0, 20);
            timer.Start();
        }

        private void fpsTimer_Tick(object sender, EventArgs e)
        {
            CurrentFps = fps;
            fps = 0;
        }

        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            ++fps;
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            if (!TMTVO.Controller.TMTVO.Instance.Api.Run)
                return;

            foreach (var item in widgets)
            {
                if (item.Active)
                    item.Tick();
            }
        }

        private void Overlay_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Visibility = Visibility.Hidden;
        }

        public void DriverInfoFadeIn(DriverInfo.DriverInfoMode mode)
        {
            if (!MyCanvas.Children.Contains(DriverInfo))
                MyCanvas.Children.Add(DriverInfo);

            DriverInfo.FadeIn(mode);
        }

        public void DriverInfoFadeOut()
        {
            DriverInfo.FadeOut();
        }

        public void LapsRemainingFadeIn(int remaining)
        {
            if (!MyCanvas.Children.Contains(LapsRemaining))
                MyCanvas.Children.Add(LapsRemaining);

            LapsRemaining.FadeIn(remaining);
        }

        public void LapTimerLeftFadeIn(LiveStandingsItem driver)
        {
            if (!MyCanvas.Children.Contains(LapTimerLeft))
                MyCanvas.Children.Add(LapTimerLeft);

            LapTimerLeft.FadeIn(driver);
        }

        public void LapTimerLeftFadeOut()
        {
            LapTimerLeft.FadeOut();
        }

        public void LiveTimingWidgetFadeIn()
        {
            if (!MyCanvas.Children.Contains(LiveTimingWidget))
                MyCanvas.Children.Add(LiveTimingWidget);

            LiveTimingWidget.FadeIn();
        }

        public void LiveTimingWidgetFadeOut()
        {
            LiveTimingWidget.FadeOut();
        }

        public void RaceBarFadeIn()
        {
            if (!MyCanvas.Children.Contains(RaceBar))
                MyCanvas.Children.Add(RaceBar);

            RaceBar.FadeIn();
        }

        public void RaceBarFadeOut()
        {
            RaceBar.FadeOut();
        }

        public void ResultsFadeIn(int ms, ResultsWidget.ResultsMode mode)
        {
            if (!MyCanvas.Children.Contains(ResultsWidget))
                MyCanvas.Children.Add(ResultsWidget);

            ResultsWidget.Show(ms, mode);
        }

        public void RevMeterFadeIn()
        {
            if (!MyCanvas.Children.Contains(RevMeter))
                MyCanvas.Children.Add(RevMeter);

            RevMeter.FadeIn();
        }

        public void RevMeterFadeOut()
        {
            RevMeter.FadeOut();
        }

        public void SessionTimerFadeIn()
        {
            if (!MyCanvas.Children.Contains(SessionTimer))
                MyCanvas.Children.Add(SessionTimer);

            SessionTimer.FadeIn();
        }

        public void SessionTimerFadeOut()
        {
            SessionTimer.FadeOut();
        }

        public void SideBarFadeInDriverOverview(LiveStandingsItem driver1, LiveStandingsItem driver2)
        {
            if (!MyCanvas.Children.Contains(SideBar))
                MyCanvas.Children.Add(SideBar);

            SideBar.FadeInDriverOverview(driver1, driver2);
        }

        public void SideBarFadeInBattleForPos(int firstPos, int count)
        {
            if (!MyCanvas.Children.Contains(SideBar))
                MyCanvas.Children.Add(SideBar);

            SideBar.FadeInBattleForPos(firstPos, count);
        }

        public void SideBarFadeOut()
        {
            SideBar.FadeOut();
        }

        public void SpeedCompFadeIn(LiveStandingsItem driver1, LiveStandingsItem driver2)
        {
            if (!MyCanvas.Children.Contains(SpeedCompareWidget))
                MyCanvas.Children.Add(SpeedCompareWidget);

            SpeedCompareWidget.FadeIn(driver1, driver2);
        }

        public void SpeedCompFadeOut()
        {
            SpeedCompareWidget.FadeOut();
        }

        public void TeamRadioFadeIn(Driver driver)
        {
            if (!MyCanvas.Children.Contains(TeamRadio))
                MyCanvas.Children.Add(TeamRadio);

            TeamRadio.StartsSpeaking(driver.LastUpperName, driver.Car.CarNumber, driver.LicColor);
        }

        public void TeamRadioFadeOut()
        {
            TeamRadio.FadeOut();
        }

        public void WeatherFadeIn()
        {
            if (!MyCanvas.Children.Contains(WeatherWidget))
                MyCanvas.Children.Add(WeatherWidget);

            WeatherWidget.FadeIn();
        }

        public void WeatherFadeOut()
        {
            WeatherWidget.FadeOut();
        }

        private void Overlay_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double skale = ActualWidth / 1920D;

            GridTransformation.ScaleX = skale;
            GridTransformation.ScaleY = skale;

            Height = 1080D * skale;
        }
    }
}
