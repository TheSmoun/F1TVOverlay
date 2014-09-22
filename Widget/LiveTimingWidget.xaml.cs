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
	/// Interaktionslogik für LiveStandingsWidget.xaml
	/// </summary>
	public partial class LiveTimingWidget : UserControl, IWidget
	{
        private static readonly double pageCd = 1100D;

        public bool Active { get; private set; }
        public LinkedList<LiveTimingItem> Items { get; private set; }
        public LinkedList<LiveTimingItem> Dummies { get; private set; }
        public LiveTimingItemMode Mode { get; private set; }
        public LiveStandingsModule Module { get; set; }

        private Button prevPageButton;
        private Button nextPageButton;
        private Button leaderPageButton;

        private Timer nextPageCd;
        private Timer prevPageCd;
        private Timer leaderPageCd;
        private Timer neutralizeTimer;
        private Timer changeModeTimer;

        private bool canUpdateButtons;
        private bool dummyActive;
        private int pageIndex;
        private int dummyPageIndex;

        private LiveTimingItemMode newMode;

		public LiveTimingWidget()
		{
			this.InitializeComponent();

            canUpdateButtons = true;
            dummyActive = false;
            PageSwitcherInnerDummy.Visibility = Visibility.Hidden;

            pageIndex = 0;
            nextPageCd = new Timer(pageCd);
            nextPageCd.Elapsed += LoadNextPage;

            prevPageCd = new Timer(pageCd);
            prevPageCd.Elapsed += LoadPrevPage;

            leaderPageCd = new Timer(pageCd);
            leaderPageCd.Elapsed += LoadLeaderPage;

            neutralizeTimer = new Timer(200);
            neutralizeTimer.Elapsed += NeutralizePage;

            changeModeTimer = new Timer(300);
            changeModeTimer.Elapsed += changeMode;

            Controls tvoC = TMTVO.Controller.TMTVO.Instance.Controls;
            prevPageButton = tvoC.TimingPrevPage;
            nextPageButton = tvoC.TimingNextPage;
            leaderPageButton = tvoC.TimingLeaderPage;

            Items = new LinkedList<LiveTimingItem>();
            Dummies = new LinkedList<LiveTimingItem>();
            int i = 1;

            LiveTimingItem item = Item1;
            item.Flag.Visibility = Visibility.Hidden;
            item.InPit.Visibility = Visibility.Hidden;

            if (item.Name != "Item1")
            {
                item.NumberLeader.Visibility = Visibility.Hidden;
                item.BackgroundLeader.Visibility = Visibility.Hidden;
                item.BackgroundLeader1.Visibility = Visibility.Hidden;
            }

            item.Position.Text = (i++).ToString();

            Items.AddLast(item);

            foreach (UIElement elem in PageSwitcherInner.Children)
            {
                item = (LiveTimingItem)elem;
                item.Flag.Visibility = Visibility.Hidden;
                item.InPit.Visibility = Visibility.Hidden;
                item.Position.Text = (i++).ToString();
                Items.AddLast(item);
            }

            foreach (UIElement elem in PageSwitcherInnerDummy.Children)
            {
                item = (LiveTimingItem)elem;
                item.Flag.Visibility = Visibility.Hidden;
                item.InPit.Visibility = Visibility.Hidden;
                item.Position.Text = (i++).ToString();
                Dummies.AddLast(item);
            }
		}

        public void ChangeMode(LiveTimingItemMode newMode)
        {
            this.newMode = newMode;
            if (Items == null)
            {
                Mode = newMode;
                return;
            }

            foreach (LiveTimingItem i in Items)
                i.FadeOutElements();

            changeModeTimer.Start();
        }

        private void changeMode(object sender, ElapsedEventArgs e)
        {
            changeModeTimer.Stop();
            Mode = newMode;
            foreach (LiveTimingItem i in Items)
                Application.Current.Dispatcher.BeginInvoke(new Action(i.FadeInElements));
        }

        public void FadeIn()
        {
            pageIndex = 0;
            Active = true;
            Storyboard sb = FindResource("FadeIn") as Storyboard;
            sb.Begin();
        }

        public void FadeOut()
        {
            Active = false;

            prevPageButton.IsEnabled = false;
            nextPageButton.IsEnabled = false;
            leaderPageButton.IsEnabled = false;

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
            LinkedListNode<LiveTimingItem> node = Items.First;
            for (int i = 1; i <= Module.Items.Count; i++)
            {
                int pos = 1;
                if (i > 1)
                    pos = i + (pageIndex * 21);

                LiveStandingsItem item = Module.Items.Find(it => it.PositionLive == pos);
                LiveTimingItem current = node.Value;
                current.Tick(item, Mode);

                node = node.Next;
                if (node == null)
                    break;
            }

            if (dummyActive)
                UpadeDummies();

            int j = (((pageIndex + 1) * 21) + 1 < Module.Items.Count) ? pageIndex + 1 : 0;
            if (pageIndex < j && canUpdateButtons)
                nextPageButton.IsEnabled = true;

            foreach (LiveTimingItem item in Items)
            {
                if (item.OldPosition == -1)
                    item.Visibility = Visibility.Hidden;
                else
                    item.Visibility = Visibility.Visible;
            }
        }

        public void FadeInPage()
        {
            UIElementCollection uiE = PageSwitcherInner.Children;
            for (int i = 0; i < uiE.Count; i++)
                ((LiveTimingItem)uiE[i]).FadeInLater(i * 38);
        }

        public void FadeOutPage()
        {
            UIElementCollection uiE = PageSwitcherInner.Children;
            for (int i = 0; i < uiE.Count; i++)
                ((LiveTimingItem)uiE[i]).FadeOut();
        }

        private void UpadeDummies()
        {
            LinkedListNode<LiveTimingItem> node = Dummies.First;
            for (int i = 2; i <= Module.Items.Count; i++)
            {
                int pos = i + (dummyPageIndex * 21);

                LiveStandingsItem item = Module.Items.Find(it => it.PositionLive == pos);
                LiveTimingItem current = node.Value;
                current.Tick(item, Mode);

                node = node.Next;
                if (node == null)
                    break;
            }
        }

        public void NextPage()
        {
            canUpdateButtons = false;
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                nextPageButton.IsEnabled = false;
                prevPageButton.IsEnabled = false;
                leaderPageButton.IsEnabled = false;
            }));

            int i = (((pageIndex + 1) * 21) + 1 < Module.Items.Count) ? pageIndex + 1 : 0;
            if (pageIndex >= i)
                return;

            LoadNextDummy();
            Storyboard sb = FindResource("NextPage") as Storyboard;
            sb.Begin();
            nextPageCd.Start();
        }

        private void LoadNextDummy()
        {
            dummyActive = true;
            dummyPageIndex = pageIndex + 1;
            PageSwitcherInnerDummy.Visibility = Visibility.Visible;

            UIElementCollection uiE = PageSwitcherInnerDummy.Children;
            for (int i = 0; i < uiE.Count; i++)
                ((LiveTimingItem)uiE[i]).FadeInLater(i * 38);
        }

        private void LoadPrevDummy(int npi = 0)
        {
            dummyActive = true;
            dummyPageIndex = npi;
            PageSwitcherInnerDummy.Visibility = Visibility.Visible;

            UIElementCollection uiE = PageSwitcherInnerDummy.Children;
            int j = 0;
            for (int i = uiE.Count - 1; i >= 0; i--)
                ((LiveTimingItem)uiE[i]).FadeInLater((j++) * 38);
        }

        private void LoadNextPage(object sender, ElapsedEventArgs e)
        {
            nextPageCd.Stop();
            LoadPage((((pageIndex + 1) * 21) + 1 < Module.Items.Count) ? pageIndex + 1 : 0);
        }

        public void PrevPage()
        {
            canUpdateButtons = false;
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                nextPageButton.IsEnabled = false;
                prevPageButton.IsEnabled = false;
                leaderPageButton.IsEnabled = false;
            }));

            if (pageIndex <= 0)
                return;

            LoadPrevDummy(pageIndex - 1);
            Storyboard sb = FindResource("PrevPage") as Storyboard;
            sb.Begin();
            prevPageCd.Start();
        }

        private void LoadPrevPage(object sender, ElapsedEventArgs e)
        {
            prevPageCd.Stop();
            LoadPage(pageIndex - 1);
        }

        internal void LeaderPage()
        {
            canUpdateButtons = false;
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                nextPageButton.IsEnabled = false;
                prevPageButton.IsEnabled = false;
                leaderPageButton.IsEnabled = false;
            }));

            if (pageIndex <= 0)
                return;

            LoadPrevDummy();
            Storyboard sb = FindResource("PrevPage") as Storyboard;
            sb.Begin();
            leaderPageCd.Start();
        }

        private void LoadLeaderPage(object sender, ElapsedEventArgs e)
        {
            leaderPageCd.Stop();
            LoadPage(0);
        }

        private void LoadPage(int npi)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                FadeInPositions(npi);
            }));
        }

        private void FadeInPositions(int npi)
        {
            pageIndex = npi;
            neutralizeTimer.Start();
        }

        private void NeutralizePage(object sender, ElapsedEventArgs e)
        {
            neutralizeTimer.Stop();
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                Storyboard sb = FindResource("NeutralizePage") as Storyboard;
                sb.Begin();

                PageSwitcherInnerDummy.Visibility = Visibility.Hidden;
                dummyActive = false;
                dummyPageIndex = -1;

                canUpdateButtons = true;
                if (pageIndex > 0)
                {
                    prevPageButton.IsEnabled = true;
                    leaderPageButton.IsEnabled = true;
                }
            }));
        }
    }
}