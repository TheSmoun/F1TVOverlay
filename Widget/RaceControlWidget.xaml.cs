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
	/// Interaktionslogik für RaceControlWidget.xaml
	/// </summary>
	public partial class RaceControlWidget : UserControl
	{
		public RaceControlWidget()
		{
			this.InitializeComponent();
		}

        public void Show(string text)
        {
            string[] textArray = text.Split(new char[] { ' ' });
            List<string> lines = new List<string>();
            int count = 0;
            int maxSize = 200;
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < textArray.Length; i++)
            {
                string s = textArray[i];
                if (s.Length * 2 + 1 + count <= maxSize)
                {
                    sb.Append(s).Append(' ');
                    count += s.Length * 2 + 1;
                }
                else
                {
                    lines.Add(sb.ToString());
                    count = 0;
                }
            }

            switch (lines.Count)
            {
                case 1:
                    ShowOneLine(lines[0]);
                    break;
                case 2:
                    ShowTwoLines(lines[0], lines[1]);
                    break;
                default:
                    break;
            }
        }

        private void ShowOneLine(string line)
        {
            // TODO Set Line.
            Storyboard sb = FindResource("ShowOne") as Storyboard;
            sb.Begin();
        }

        private void ShowTwoLines(string line1, string line2)
        {
            // TODO Set Lines.
            Storyboard sb = FindResource("ShowTwo") as Storyboard;
            sb.Begin();
        }
	}
}