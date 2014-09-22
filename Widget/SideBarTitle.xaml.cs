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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TMTVO.ThemeApi;

namespace F1TVOverlay.Widget
{
	/// <summary>
	/// Interaktionslogik für SideBarTitle.xaml
	/// </summary>
	public partial class SideBarTitle : UserControl, ISideBarElement
	{
        public bool Active { get; private set; }

		public SideBarTitle()
		{
			this.InitializeComponent();
		}

        public void FadeIn(string title)
        {
            if (Active)
                return;

            TitleBox.Text = title;
            Active = true;
            (FindResource("FadeIn") as Storyboard).Begin();
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
            // Does Nothing.
        }

        public void Reset()
        {
            Active = false;
        }
	}
}