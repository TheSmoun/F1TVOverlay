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
using TMTVO.Data;
using TMTVO.Data.Modules;
using TMTVO.ThemeApi;

namespace F1TVOverlay.Widget
{
	/// <summary>
	/// Interaktionslogik für UserControl1.xaml
	/// </summary>
	public partial class SessionTimer : UserControl, IWidget
	{
        private Dictionary<SessionType, string> SessionTypeToString = new Dictionary<SessionType, string>() {
            {SessionType.OfflineTesting, "T"},
            {SessionType.Practice, "P"},
            {SessionType.Qualifying, "Q"},
            {SessionType.WarmUp, "W"},
            {SessionType.LapRace, "R"},
            {SessionType.TimeRace, "R"},
            {SessionType.TimeTrial, "T"}
        };

        public SessionTimerState State { get; private set; }
        public SessionMode Mode { get; set; }
        public bool Active { get; private set; }

        private int lastDrivenLaps = -1;
        private int lastTotalLaps = -1;
        private int lastSeconds = -1;

        public SessionTimerModule Module { get; set; }

		public SessionTimer()
		{
			this.InitializeComponent();

            this.Active = false;
            this.Mode = SessionMode.LapMode;
            this.State = SessionTimerState.Normal;
		}

        public void FadeIn()
        {
            if (Active)
                return;

            SessionMode mode = SessionMode.TimeMode;
            if (Module.SessionState == SessionState.Racing && Module.SessionType == SessionType.LapRace)
                mode = SessionMode.LapMode;

            if (mode == SessionMode.LapMode)
            {
                SwitchToLap();
                UpdateLaps(0, Module.LapsTotal);
            }
            else
            {
                SwitchToTime();
                UpdateTime(Module.TimeTotal);
            }

            Active = true;
            Storyboard sb;
            if (Mode == SessionMode.LapMode)
                sb = FindResource("FadeInLap") as Storyboard;
            else
                sb = FindResource("FadeInTime") as Storyboard;
            sb.Begin();

            if (State == SessionTimerState.SafetyCar)
            {
                sb = FindResource("SafetyCarFadeIn") as Storyboard;
                sb.Begin();
            }
        }

        public void FadeOut()
        {
            if (!Active)
                return;

            Active = false;
            Storyboard sb;
            if (Mode == SessionMode.LapMode)
                sb = FindResource("FadeOutLap") as Storyboard;
            else
                sb = FindResource("FadeOutTime") as Storyboard;

            sb.Completed += sb_Completed;
            sb.Begin();

            if (State == SessionTimerState.SafetyCar)
            {
                sb = FindResource("SafetyCarFadeOut") as Storyboard;
                sb.Begin();
            }
        }

        private void sb_Completed(object sender, EventArgs e)
        {
            if (Parent != null)
                ((Grid)this.Parent).Children.Remove(this);
        }

        public void YellowFlag()
        {
            if (State == SessionTimerState.Yellow || State == SessionTimerState.SafetyCar)
                return;

            Storyboard sb = null;
            if (Mode == SessionMode.TimeMode && State == SessionTimerState.Red)
                sb = FindResource("RedFadeOutTime") as Storyboard;
            else if (Mode == SessionMode.LapMode && State == SessionTimerState.Red)
                sb = FindResource("RedFadeOutLap") as Storyboard;

            if (sb != null)
                sb.Begin();

            if (Mode == SessionMode.LapMode)
                sb = FindResource("Yellow") as Storyboard;
            else
                sb = FindResource("YellowFadeInTime") as Storyboard;
            sb.Begin();

            State = SessionTimerState.Yellow;
        }

        public void RedFlag()
        {
            if (State == SessionTimerState.Red)
                return;

            Storyboard sb = null;
            if (Mode == SessionMode.LapMode)
                sb = FindResource("Red") as Storyboard;
            else
                sb = FindResource("RedFadeInTime") as Storyboard;
            sb.Begin();

            sb = null;
            if (State == SessionTimerState.Yellow && Mode == SessionMode.LapMode)
                sb = FindResource("YellowFadeOutLap") as Storyboard;
            else if (State == SessionTimerState.SafetyCar)
                Normal();
            else if (State == SessionTimerState.Yellow && Mode == SessionMode.TimeMode)
                sb = FindResource("YellowFadeOutTime") as Storyboard;

            if (sb != null)
                sb.Begin();

            State = SessionTimerState.Red;
        }

        public void SafetyCarDeveloped()
        {
            if (State == SessionTimerState.SafetyCar)
                return;

            if (State != SessionTimerState.Yellow)
                YellowFlag();

            State = SessionTimerState.SafetyCar;
            Storyboard sb = FindResource("SafetyCarFadeIn") as Storyboard;
            sb.Begin();
        }

        public void SafetyCarIn()
        {
            if (State != SessionTimerState.SafetyCar)
                return;

            Normal();
        }

        public void Normal()
        {
            if (State == SessionTimerState.Normal)
                return;

            if (Mode == SessionMode.LapMode)
            {
                Storyboard sb = null;
                if (State == SessionTimerState.Yellow)
                    sb = FindResource("YellowFadeOutLap") as Storyboard;
                else if (State == SessionTimerState.SafetyCar)
                {
                    sb = FindResource("SafetyCarFadeOut") as Storyboard;
                    sb.Begin();
                    sb = FindResource("YellowFadeOutLap") as Storyboard;
                }
                else if (State == SessionTimerState.Red)
                    sb = FindResource("RedFadeOutLap") as Storyboard;

                if (sb != null)
                    sb.Begin();
            }
            else
            {
                Storyboard sb = null;
                if (State == SessionTimerState.Yellow)
                    sb = FindResource("YellowFadeOutTime") as Storyboard;
                else if (State == SessionTimerState.SafetyCar)
                {
                    sb = FindResource("SafetyCarFadeOut") as Storyboard;
                    sb.Begin();
                    sb = FindResource("YellowFadeOutTime") as Storyboard;
                }
                else if (State == SessionTimerState.Red)
                    sb = FindResource("RedFadeOutTime") as Storyboard;

                if (sb != null)
                    sb.Begin();
            }

            LapBackgroundCh.Opacity = 0;
            TimeBackgroundCh.Opacity = 0;

            State = SessionTimerState.Normal;
        }

        public void SwitchToTime()
        {
            if (Mode == SessionMode.TimeMode)
                return;

            Mode = SessionMode.TimeMode;
            SessionTimerLap.Opacity = 0F;
            SessionTimerTime.Opacity = 1F;
        }

        public void SwitchToLap()
        {
            if (Mode == SessionMode.LapMode)
                return;

            Mode = SessionMode.LapMode;
            SessionTimerLap.Opacity = 1F;
            SessionTimerTime.Opacity = 0F;
        }

        public void ChequeredFlag()
        {
            if (State == SessionTimerState.Chequered)
                return;

            if (State != SessionTimerState.Normal)
                Normal();

            if (Mode == SessionMode.LapMode)
                LapBackgroundCh.Opacity = 1;
            else
                TimeBackgroundCh.Opacity = 1;

            State = SessionTimerState.Chequered;
        }

        public void UpdateLaps(int drivenLaps, int maxLaps)
        {
            if (!LapsText.Text.StartsWith("-- / --") && SessionMode.LapMode == Mode && (drivenLaps < 0 || maxLaps <= 0 || drivenLaps > maxLaps))
            {
                ChequeredFlag();
                return;
            }

            if (drivenLaps < 0 || maxLaps <= 0 || drivenLaps > maxLaps || Mode != SessionMode.LapMode)
            {
                LapsText.Text = "-- / --";
                return;
            }

            if (lastDrivenLaps > 0 && lastDrivenLaps == lastTotalLaps)
                ChequeredFlag();
            else
                LapsText.Text = drivenLaps + " / " + maxLaps;
        }

        public void UpdateTime(int seconds)
        {
            if (seconds < 0 || Mode != SessionMode.TimeMode)
                return;

            if (lastSeconds == 0)
                ChequeredFlag();
            else
            {
                int s = seconds % 60;
                int m = (seconds / 60) % 60;
                int h = seconds / 3600;

                StringBuilder sb = new StringBuilder();
                if (h != 0)
                    sb.Append(h).Append(":");

                if (h != 0 && m < 10)
                    sb.Append("0");

                sb.Append(m).Append(":");

                if (s < 10)
                    sb.Append("0");

                sb.Append(s);

                LapsText2.Text = sb.ToString();
            }
        }

        public enum SessionTimerState
        {
            Yellow,
            SafetyCar,
            Normal,
            Red,
            Chequered
        }

        public enum SessionMode
        {
            LapMode,
            TimeMode
        }

        public void Tick()
        {
            string sType = "";
            if ((Module.SessionType == SessionType.LapRace || Module.SessionType == SessionType.TimeRace) && Module.SessionState == SessionState.Warmup)
                LapText2.Text = "W";
            else if (SessionTypeToString.TryGetValue(Module.SessionType, out sType))
                LapText2.Text = sType;
            else
                LapText2.Text = "-";

            if (Module.SessionState == SessionState.Racing && Module.SessionType == SessionType.LapRace && Mode == SessionMode.TimeMode)
                SwitchToLap();
            else if (Module.SessionType != SessionType.LapRace && Mode == SessionMode.LapMode)
                SwitchToTime();

            this.UpdateTime(Module.TimeRemaining);
            this.UpdateLaps(Module.LapsDriven, Module.LapsTotal);

            SessionFlag f = Module.SessionFlags;
            if (IsCheckeredFlag(f) || Module.SessionState == SessionState.Checkered || Module.SessionState == SessionState.Cooldown)
                ChequeredFlag();
            else if (IsYellowFlag(f))
                YellowFlag();
            else if (IsSafetyCar(f))
                SafetyCarDeveloped();
            else if (IsRedFlag(f))
                RedFlag();
            else if (IsWhiteFlag(f))
                return;
            else
                Normal();
        }


        private bool IsYellowFlag(SessionFlag f)
        {
            return f.FlagSet(SessionFlag.Yellow) || f.FlagSet(SessionFlag.YellowWaving);
        }

        private bool IsSafetyCar(SessionFlag f)
        {
            return f.FlagSet(SessionFlag.Caution) || f.FlagSet(SessionFlag.CautionWaving) || f.FlagSet(SessionFlag.OneLapToGreen);
        }

        private bool IsRedFlag(SessionFlag f)
        {
            return f.FlagSet(SessionFlag.Red);
        }

        private bool IsCheckeredFlag(SessionFlag f)
        {
            return f.FlagSet(SessionFlag.Checkered);
        }

        private bool IsWhiteFlag(SessionFlag f)
        {
            return f.FlagSet(SessionFlag.White);
        }
    }
}