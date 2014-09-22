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
using TMTVO.ThemeApi;

namespace F1TVOverlay.Widget
{
    /// <summary>
    /// Interaktionslogik für LapsRemainingWidget.xaml
    /// </summary>
    public partial class LapsRemainingWidget : UserControl, IWidget
    {
        private static readonly double CD_MS_INTERVAL = 10000;
        private static readonly string FINAL_LAP_STRING = "FINAL LAP";
        private static readonly string LAPS_REMAINING_STRING = " LAPS REMAINING";

        public bool Active { get; private set; }

        private Timer coolDown;

        private bool[] showed;

        public LapsRemainingWidget()
        {
            this.InitializeComponent();

            coolDown = new Timer(CD_MS_INTERVAL);
            showed = new bool[5] { false, false, false, false, false };
            coolDown.Elapsed += TimerElapsed;
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            coolDown.Stop();
            Application.Current.Dispatcher.BeginInvoke(new Action(FadeOut));
        }

        public void FadeIn(int remaining)
        {
            if (Active)
                return;

            if (remaining > 5 || remaining < 1 || showed[remaining - 1])
                return;

            if (remaining == 1)
                TeamCarName.Text = FINAL_LAP_STRING;
            else
                TeamCarName.Text = remaining + LAPS_REMAINING_STRING;

            Active = true;
            showed[remaining - 1] = true;
            Storyboard sb = FindResource("FadeIn") as Storyboard;
            sb.Begin();

            coolDown.Start();
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

        public void Tick()
        {
            
        }
    }
}