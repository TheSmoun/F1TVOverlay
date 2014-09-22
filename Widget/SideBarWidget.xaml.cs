using F1TVOverlay.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TMTVO.Data.Modules;
using TMTVO.ThemeApi;

namespace F1TVOverlay.Widget
{
	/// <summary>
	/// Interaktionslogik für SideBarWidget.xaml
	/// </summary>
	public partial class SideBarWidget : UserControl, IWidget
	{
        public bool Active { get; private set; }
        public int FirstPos { get; private set; }
        public int Count { get; private set; }
        public SideBarMode Mode { get; private set; }

        private List<ISideBarElement> elements;
        private LiveStandingsModule module;


		public SideBarWidget()
		{
			this.InitializeComponent();
            elements = new List<ISideBarElement>();
		}

        public void FadeInDriverOverview(LiveStandingsItem driver1, LiveStandingsItem driver2)
        {
            if (driver1 == null && driver2 == null)
                return;

            this.Mode = SideBarMode.DriverOverView;
            Active = true;
            if (driver1 != null)
                addDriverOverviewElement(driver1, 0);

            if (driver2 != null)
                addDriverOverviewElement(driver2, 1);
        }

        private void addDriverOverviewElement(LiveStandingsItem driver, int i)
        {
            DriverOverviewTitle title = new DriverOverviewTitle();
            DriverOverviewStops stops = new DriverOverviewStops();
            LapTimeElement best = new LapTimeElement();
            LapTimeElement last = new LapTimeElement();

            title.VerticalAlignment = VerticalAlignment.Top;
            stops.VerticalAlignment = VerticalAlignment.Top;
            best.VerticalAlignment = VerticalAlignment.Top;
            last.VerticalAlignment = VerticalAlignment.Top;

            elements.Add(title);
            elements.Add(stops);
            elements.Add(best);
            elements.Add(last);

            title.Margin = new Thickness(0, 0 + i * 180, 0, 0);
            stops.Margin = new Thickness(0, 36 + i * 180, 0, 0);
            best.Margin = new Thickness(0, 72 + i * 180, 0, 0);
            last.Margin = new Thickness(0, 108 + i * 180, 0, 0);

            LayoutRoot.Children.Add(title);
            LayoutRoot.Children.Add(stops);
            LayoutRoot.Children.Add(best);
            LayoutRoot.Children.Add(last);

            title.FadeIn(driver, 0 + i * 125);
            stops.FadeIn(driver, 25 + i * 125);
            best.FadeIn("BEST", driver, 50 + i * 125);
            last.FadeIn("LAST", driver, 75 + i * 125);
        }

        public void FadeInBattleForPos(int pos, int count)
        {
            this.FirstPos = pos;
            this.Count = count;

            Active = true;
            Mode = SideBarMode.BattleForPosition;
            SideBarTitle title = new SideBarTitle();
            string t = "BATTLE FOR ";
            if (pos == 1)
                t += "1st";
            else if (pos == 2)
                t += " 2nd";
            else if (pos == 3)
                t += " 3rd";
            else
                t += pos.ToString("0") + "th";

            title.VerticalAlignment = VerticalAlignment.Top;
            LayoutRoot.Children.Add(title);
            elements.Add(title);
            title.FadeIn(t);

            if (module == null)
                module = (LiveStandingsModule)TMTVO.Controller.TMTVO.Instance.Api.FindModule("LiveStandings");

            int j = 1;
            for (int i = pos; i < pos + count; i++)
            {
                LiveStandingsItem item = module.FindDriverByPos(i);
                if (item == null)
                    break;

                BattleElement e = new BattleElement(this, module);
                e.VerticalAlignment = VerticalAlignment.Top;
                elements.Add(e);
                LayoutRoot.Children.Add(e);
                e.Margin = new Thickness(0, j * 36, 0, 0);
                e.FadeIn(item, j * 25);

                j++;
            }
        }

        public void FadeOut()
        {
            if (!Active)
                return;

            Reset();
            foreach (ISideBarElement e in elements)
                e.FadeOut();
        }

        public void Tick()
        {
            if (Mode == SideBarMode.BattleForPosition)
            {
                int j = 1;
                for (int i = FirstPos; i < FirstPos + Count; i++)
                {
                    BattleElement e = elements[j++] as BattleElement;
                    e.Driver = module.FindDriverByPos(i);
                }
            }

            foreach (ISideBarElement e in elements)
                e.Tick();
        }

        public void Reset()
        {
            Active = false;
            elements.Clear();
            LayoutRoot.Children.Clear();
        }

        public enum SideBarMode
        {
            DriverOverView,
            BattleForPosition,
            Improvements
        }
    }
}