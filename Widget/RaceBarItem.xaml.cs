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

namespace F1TVOverlay.Widget
{
	/// <summary>
	/// Interaktionslogik für RaceBarItem.xaml
	/// </summary>
	public partial class RaceBarItem : UserControl
	{
        public bool Show { get; set; }
        public bool Active { get; internal set; }

		public RaceBarItem()
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

        public void FadeOut()
        {
            if (!Active)
                return;

            Active = false;
            Storyboard sb = FindResource("FadeOut") as Storyboard;
            sb.Begin();
        }
    }
}