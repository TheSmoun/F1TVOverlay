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
using TMTVO.Controller;
using TMTVO.Data;
using TMTVO.Data.Modules;
using TMTVO.ThemeApi;

namespace F1TVOverlay.Widget
{
	/// <summary>
	/// Interaktionslogik für TeamRadio.xaml
	/// </summary>
	public partial class TeamRadio : UserControl, IWidget
	{
        public bool Active { get; private set; }
        public TeamRadioModule Module { get; set; }

		public TeamRadio()
		{
			this.InitializeComponent();
            Active = false;
		}

        public void StartsSpeaking(string LastNameDriver, string driverNumber, Color classColor)
        {
            DriversNumber.Text = driverNumber;
            DriversName.Text = LastNameDriver;
            NumberPlate.Fill = new SolidColorBrush(classColor);

            if (!Active)
                Active = true;

            Storyboard sb = FindResource("FadeIn") as Storyboard;
            sb.Begin();
        }

        public void FadeOut()
        {
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

        public void SetClubColor(Brush brush)
        {
            this.NumberPlate.Fill = brush;
        }

        public void Tick()
        {
            if (Module.SpeekingCarIndex == -1 && Active)
                FadeOut();
            else if (Module.SpeekingCarIndex != -1 && !Active)
            {
                Driver driver = ((DriverModule)TMTVO.Controller.TMTVO.Instance.Api.FindModule("DriverModule")).Drivers.Find(d => d.CarIndex == Module.SpeekingCarIndex);
                if (driver != null)
                    TMTVO.Controller.TMTVO.Instance.Window.TeamRadioFadeIn(driver);
            }
            else if (Module.SpeekingCarIndex != -1)
            {
                Driver driver = ((DriverModule)TMTVO.Controller.TMTVO.Instance.Api.FindModule("DriverModule")).Drivers.Find(d => d.CarIndex == Module.SpeekingCarIndex);
                if (driver != null)
                {
                    DriversNumber.Text = driver.Car.CarNumber;
                    DriversName.Text = driver.LastUpperName;
                    NumberPlate.Fill = new SolidColorBrush(driver.LicColor);
                }
            }
        }
    }
}