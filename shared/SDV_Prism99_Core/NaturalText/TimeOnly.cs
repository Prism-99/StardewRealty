using System;


namespace Prism99_Core.NaturalText
{
    internal class TimeOnly
    {
        public enum TimeSeg
        {
            AM,
            PM
        }

        public int Second = 0;
        public int Minute = 0;
        public int Hour = 0;
        public TimeSeg AMPM;
        public bool IsValid;
        public bool useGameTime;
        private int gameStartTime;
        private DateTime startDate;
        public TimeOnly() { }
        public TimeOnly(int gameTime)
        {
            string[] arParts = new string[2];
            arParts[0] = gameTime.ToString("D4").Substring(0, 2);
            arParts[1] = gameTime.ToString("D4").Substring(2, 2);
            int.TryParse(arParts[0], out Hour);
            int.TryParse(arParts[1], out Minute);

            AMPM = Hour >= 12 ? TimeSeg.PM : TimeSeg.AM;
            Hour = Hour > 12 ? Hour - 12 : Hour;
            Hour = Hour == 0 ? 12 : Hour;
            IsValid = true;
            useGameTime = true;
            gameStartTime = gameTime;
        }
        public TimeOnly(DateTime refDate)
        {
            Second = refDate.Second;
            Minute = refDate.Minute;
            Hour = refDate.Hour;
            AMPM = Hour >= 12 ? TimeSeg.PM : TimeSeg.AM;
            Hour = Hour > 12 ? Hour - 12 : Hour;
            Hour = Hour == 0 ? 12 : Hour;
            startDate = refDate;
            IsValid = true;
        }
        public TimeOnly(int hrs, int mins, int secs, TimeSeg tseg)
        {
            Second = secs;
            Minute = mins;
            Hour = hrs;
            AMPM = tseg;
            IsValid = true;
            startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, hrs, mins, secs);

        }
        public TimeOnly(int hrs, int mins, int secs)
        {
            Second = secs;
            Minute = mins;
            Hour = hrs;
            AMPM = hrs >= 12 ? TimeSeg.PM : TimeSeg.AM;
            Hour = Hour > 12 ? Hour - 12 : Hour;
            Hour = Hour == 0 ? 12 : Hour;
            IsValid = true;
            startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, hrs, mins, secs);
        }
        public TimeOnly(string timeString)
        {
            ParseTimeString(timeString);
            startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, Hour, Minute, Second);
        }
        public TimeOnly(string timeString, string timeSeg, bool gameTime)
        {
            useGameTime = gameTime;
            ParseTimeString(timeString);
            if (timeSeg != null)
            {
                if (timeSeg.Trim().ToLower() == "am")
                {
                    AMPM = TimeSeg.AM;
                }
                else if (timeSeg.Trim().ToLower() == "pm")
                {
                    AMPM = TimeSeg.PM;
                }

            }
        }
        public Tuple<bool, TimeOnly> AddMinutes(int mins)
        {
            if (useGameTime)
            {
                int newMins = Minute + mins % 60;
                int newHours = Hour + (int)Math.Floor(mins / (double)60);
                if (newMins > 59)
                {
                    newMins -= 60;
                    newHours++;
                }
                bool carryover = newHours > 23;
                if (carryover) { newHours -= 24; }
                return Tuple.Create(carryover, new TimeOnly(newHours, newMins, 0));
            }
            else
            {
                DateTime ret = startDate.AddMinutes(mins);
                bool rollover = ret.Day != startDate.Day;
                return Tuple.Create(rollover, new TimeOnly(ret));
            }
        }
        private void ParseTimeString(string timeString)
        {
            if (string.IsNullOrEmpty(timeString))
                return;

            string[] arTime = timeString.Split(':');

            switch (arTime.Length)
            {
                case 1:
                    int.TryParse(arTime[0], out Hour);
                    IsValid = true;
                    break;
                case 2:
                    int.TryParse(arTime[0], out Hour);
                    int.TryParse(arTime[1], out Minute);
                    IsValid = true;
                    break;
                case 3:
                    int.TryParse(arTime[0], out Hour);
                    int.TryParse(arTime[1], out Minute);
                    int.TryParse(arTime[2], out Second);
                    IsValid = true;
                    break;
            }
        }
        public string ShortTime()
        {
            return $"{Hour.ToString("D2")}:{Minute.ToString("D2")} {AMPM}";
        }
        public override string ToString()
        {
            return $"{Hour.ToString("D2")}:{Minute.ToString("D2")}:{Second.ToString("D2")}{AMPM}";
        }
    }
}
