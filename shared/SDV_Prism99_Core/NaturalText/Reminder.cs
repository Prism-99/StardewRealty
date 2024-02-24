using System;


namespace Prism99_Core.NaturalText
{
    internal class Reminder
    {

        public struct DatePart
        {
            public string Date;
            public bool Repeats;
            public DateOnly StructureDate;
        }
        public struct TimePart
        {
            public string Time;
            public TimeOnly StructuredTime;
            public string TimeSuffix;
        }
        public struct RepeatPart
        {
            public string Every;
            public string Units;
            public bool IsValid;
        }
        public struct DelayPart
        {
            public string For;
            public string Units;
            public bool Days;
            public bool IsValid;
        }

        public bool IsValid;
        public string ReminderText;
        public DatePart Date;
        public TimePart Time;
        public RepeatPart Repeats;
        public DelayPart Delay;

        public Reminder()
        {
            Date = new DatePart();
            Time = new TimePart();
            Repeats = new RepeatPart();
            Delay = new DelayPart();
        }
        public string GetText()
        {
            string text = string.Empty;

            if (IsValid)
            {

                if (Date.StructureDate.IsValid)
                {
                    if (Date.StructureDate.IsSameDate(DateTime.Now))
                    {
                        text += "Today ";
                    }
                    else if (Date.StructureDate.IsSameDate(DateTime.Now.AddDays(1)))
                    {
                        text += "Tomorrow ";
                    }
                    else
                    {
                        text += $"On {Date.StructureDate} ";
                    }
                }
                if(Time.StructuredTime.IsValid)
                {
                    if (text == string.Empty)
                    {
                        text = $"At {Time.StructuredTime.ShortTime()} ";
                    }
                    else
                    {
                        text += $"at {Time.StructuredTime.ShortTime()} ";
                    }
                }
                text += ReminderText;
                if (Repeats.IsValid)
                {
                    text += $" (Repeat every {Repeats.Every} {Repeats.Units})";
                }
            }
            else
            {
                text = "I did not understand what you said.";
            }

            return text;
        }
        public override string ToString()
        {
            string time = "--";
            string date = Date.StructureDate.IsValid ? Date.StructureDate.ToString() : Date.Date ?? "--";

            if (Time.StructuredTime.IsValid)
            {
                time = Time.StructuredTime.ToString();
            }
            else if (!string.IsNullOrEmpty(Time.Time))
            {
                time = $"{Time.Time}{(string.IsNullOrEmpty(Time.TimeSuffix) ? "" : $" {Time.TimeSuffix}")}";
            }
            else
            {
                if (!string.IsNullOrEmpty(Delay.For))
                {
                    if (Delay.Days)
                    {
                        date = $"{Delay.For} {Delay.Units}";
                    }
                    else
                    {
                        time = $"{Delay.For} {Delay.Units}";
                    }
                }
            }

            return $"{IsValid}:[text: '{ReminderText}', date: {date}, at: {time},{(string.IsNullOrEmpty(Repeats.Every) ? "" : $"every {Repeats.Every}")} {Repeats.Units ?? ""}]";
        }
    }
}
