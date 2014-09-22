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
using TMTVO.Data;
using TMTVO.Data.Modules;

namespace F1TVOverlay.Widget
{
	/// <summary>
	/// Interaktionslogik für BattleElement.xaml
	/// </summary>
	public partial class BattleElement : UserControl, ISideBarElement
	{
        public bool Active { get; private set; }
        public LiveStandingsItem Driver { get; internal set; }

        private SideBarWidget widget;
        private LiveStandingsModule module;

        public BattleElement(SideBarWidget widget, LiveStandingsModule module)
        {
            this.InitializeComponent();

            this.widget = widget;
            this.module = module;
        }

        internal void FadeIn(LiveStandingsItem item, int delay)
        {
            if (Active || item == null)
                return;

            Driver = item;
            Active = true;

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
            if (Driver == null)
            {
                FadeOut();
                return;
            }

            if (Driver.PositionLive == 1)
                NumberLeader.Visibility = Visibility.Visible;
            else
                NumberLeader.Visibility = Visibility.Hidden;

            Position.Text = Driver.PositionLive.ToString("0");
            ThreeLetterCode.Text = Driver.Driver.ThreeLetterCode;
            ClassColorNormal.Color = ClassColorLeader.Color = Driver.Driver.LicColor;

            if (Driver.PositionLive > widget.FirstPos)
            {
                float gap = 0;
                gap =  Driver.GapLive;

                GapText.Text = gap.ConvertToTimeString();
            }
            else
                GapText.Text = string.Empty;
        }

        public void Reset()
        {
            Active = false;
            Driver = null;
        }
    }
}