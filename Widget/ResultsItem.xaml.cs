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
using TMTVO.ThemeApi;

namespace F1TVOverlay.Widget
{
	/// <summary>
	/// Interaktionslogik für ResultsItem.xaml
	/// </summary>
	public partial class ResultsItem : UserControl, IWidget
	{
        public bool Active { get; private set; }
        public bool Show { get; set; }

		public ResultsItem()
		{
			this.InitializeComponent();
		}

        public void FadeIn()
        {
            if (!Show || Active)
                return;

            Active = true;
            Storyboard sb = FindResource("FadeIn") as Storyboard;
            sb.Begin();
        }

        public void FadeInLater(int milliseconds)
        {
            Thread fadeInThread = new Thread(FadeInStart);
            fadeInThread.Start(milliseconds);
        }

        public void FadeOut()
        {
            if (!Active)
                return;

            Active = false;
            Storyboard sb = FindResource("FadeOut") as Storyboard;
            sb.Begin();
        }

        private void FadeInStart(object obj)
        {
            int time = (int)obj;
            Thread.Sleep(time);
            Application.Current.Dispatcher.BeginInvoke(new Action(FadeIn));
        }

        public void Tick()
        {
            throw new NotImplementedException();
        }
    }
}