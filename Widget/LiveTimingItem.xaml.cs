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
    /// Interaktionslogik für LiveStandingsItem.xaml
    /// </summary>
    public partial class LiveTimingItem : UserControl
    {
        public LiveStandingsItem Item { get; private set; }
        public LiveStandingsModule Module { get; set; }

        public int OldPosition = -1;
        private float oldTime = int.MaxValue;
        private LiveTimingItemMode mode;
        private int OldCarIdx;

        public LiveTimingItem()
        {
            this.InitializeComponent();
        }

        private void LapTimeImproved()
        {
            Storyboard sb = FindResource("TimeImproved") as Storyboard;
            sb.Begin();
        }

        public void PositionImproved()
        {
            Storyboard sb = FindResource("PositionImproved") as Storyboard;
            sb.Begin();
        }

        private void UpdateWidget()
        {
            int position = Item.Position;
            float time = Item.FastestLapTime;

            if (position == 1)
            {
                switch (mode)
                {
                    case LiveTimingItemMode.LastName:
                        GapText.Visibility = Visibility.Hidden;
                        ThreeLetterCode.Text = Item.Driver.LastUpperName;
                        break;
                    default:
                        GapText.Visibility = Visibility.Visible;
                        ThreeLetterCode.Text = Item.Driver.ThreeLetterCode;
                        BackgroundLeader.Visibility = Visibility.Visible;
                        BackgroundLeader1.Visibility = Visibility.Visible;
                        NumberLeader.Visibility = Visibility.Visible;

                        int min = (int)(time / 60);
                        float sectime = time % 60;
                        StringBuilder sb = new StringBuilder();
                        if (min > 0)
                            sb.Append(min).Append(':');

                        sb.Append(sectime.ToString("00.000"));

                        GapText.Text = sb.ToString().Replace(',','.');

                        if (time != oldTime)
                            UpdateDiff();
                        break;
                }
            }
            else
            {
                BackgroundLeader.Visibility = Visibility.Hidden;
                BackgroundLeader1.Visibility = Visibility.Hidden;
                NumberLeader.Visibility = Visibility.Hidden;

                switch (mode)
                {
                    case LiveTimingItemMode.Gap:
                        GapText.Visibility = Visibility.Visible;
                        ThreeLetterCode.Text = Item.Driver.ThreeLetterCode;

                        if (time < 0)
                        {
                            GapText.Text = "No Time";
                            break;
                        }

                        float diff = time - Module.Leader.FastestLapTime;
                        int min = (int)(diff / 60);
                        float secDiff = diff % 60;
                        StringBuilder sb = new StringBuilder("+");
                        if (min > 0)
                            sb.Append(min).Append(':');

                        sb.Append(secDiff.ToString("0.000"));

                        GapText.Text = sb.ToString().Replace(',', '.');
                        break;
                    case LiveTimingItemMode.Time:
                        GapText.Visibility = Visibility.Visible;
                        ThreeLetterCode.Text = Item.Driver.ThreeLetterCode;

                        if (time < 0)
                        {
                            GapText.Text = "No Time";
                            break;
                        }

                        min = (int)(time / 60);
                        float sectime = time % 60;
                        sb = new StringBuilder();
                        if (min > 0)
                            sb.Append(min).Append(':');

                        sb.Append(sectime.ToString("00.000"));

                        GapText.Text = sb.ToString().Replace(',', '.');
                        break;
                    case LiveTimingItemMode.LastName:
                        GapText.Visibility = Visibility.Hidden;
                        ThreeLetterCode.Text = Item.Driver.LastUpperName;
                        break;
                }
            }

            ClassColorLeader.Color = Item.Driver.LicColor;
            ClassColorNormal.Color = Item.Driver.LicColor; // TODO ClassColor

            if (Item.InPits)
                JoinedPit();
            else
                LeftPit();

            Position.Text = position.ToString();
            oldTime = time;
            OldPosition = position;
        }

        public void JoinedPit()
        {
            InPit.Visibility = Visibility.Visible;
        }

        public void LeftPit()
        {
            InPit.Visibility = Visibility.Hidden;
        }

        public void ShowFlag()
        {
            Flag.Visibility = Visibility.Visible;
        }

        public void RemoveFlag()
        {
            Flag.Visibility = Visibility.Hidden;
        }

        public void Tick(LiveStandingsItem item, LiveTimingItemMode mode)
        {
            this.Item = item;
            this.mode = mode;
            this.Module = F1TVOverlay.Controller.TMTVO.Instance.Api.FindModule("LiveStandings") as LiveStandingsModule;

            if (Item == null)
            {
                LayoutRoot.Visibility = Visibility.Hidden;
                return;
            }

            LayoutRoot.Visibility = Visibility.Visible;

            UpdateWidget();
            if (Item.PositionImproved)
                PositionImproved();
            else if (Item.LapTimeImproved)
                LapTimeImproved();

            OldCarIdx = item.Driver.CarIndex;
            Item.PositionImproved = Item.LapTimeImproved = Item.PositionLost = false;
        }

        public void UpdateDiff()
        {
            float diff = oldTime - Module.Leader.FastestLapTime;
            int min = (int)(diff / 60);
            float secDiff = diff % 60;
            StringBuilder sb = new StringBuilder();
            if (min > 0)
                sb.Append(min).Append(':');

            sb.Append(secDiff.ToString("00.000"));
            GapText.Text = sb.ToString();
        }

        public void FadeIn()
        {
            Storyboard sb = FindResource("FadeIn") as Storyboard;
            sb.Begin();
        }

        internal void FadeOut()
        {
            Storyboard sb = FindResource("FadeOut") as Storyboard;
            sb.Begin();
        }

        internal void FadeInLater(int milliseconds)
        {
            Thread fadeInThread = new Thread(FadeInStart);
            fadeInThread.Start(milliseconds);
        }

        private void FadeInStart(object obj)
        {
            int time = (int)obj;
            Thread.Sleep(time);
            Application.Current.Dispatcher.BeginInvoke(new Action(FadeIn));
        }

        public void FadeOutElements()
        {
            Storyboard sb = FindResource("FadeOutElement") as Storyboard;
            sb.Begin();
        }

        public void FadeInElements()
        {
            Storyboard sb = FindResource("FadeInElement") as Storyboard;
            sb.Begin();
        }
    }

    public enum LiveTimingItemMode : int
    {
        Gap = 0,
        Time = 1,
        LastName = 2
    }
}