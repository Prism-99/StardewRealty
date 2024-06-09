using System;
using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI.Utilities;


namespace Prism99_Core.NaturalText
{
    internal class ReminderParser
    {

        public enum parseStage
        {
            Text,
            Date,
            Delay,
            Time,
            Repeat
        }

        public static Reminder ParseReminderText(string text, bool useGameTime)
        {
            Reminder reminder = new Reminder();

            List<string> weekdays = new List<string>
            {
                "sunday",
                "monday",
                "tuesday",
                "wednesday",
                "thursday",
                "friday",
                "saturday"
            };
            List<string> dateSeparators = new List<string>
            {
                "in",
                "at"
            };
            List<string> timedelayunits = new List<string>
            {
                "minute",
                "minutes",
                "hours",
                "hour"
             };
            List<string> datedelayunits = new List<string>
            {
                "day",
                "days",
                "week",
                "weeks",
                "month",
                "months",
                "year",
                "years"
            };
            List<string> repeatSeparators = new List<string>
            {
                "every"
            };
            List<string> daySeparators = new List<string>
            {
                "today",
                "tomorrow"
            };
            List<string> timeSuffixes = new List<string>
            {
                "am",
                "pm"
            };
            List<string> arTextPart = new List<string> { };
            string[] arSourceText = text.Split(' ');
            string dateArg = "";
            //string rptArg = "";
            List<string> rptParts = new List<string> { };
            List<string> dateParts = new List<string> { };
            string timeArg = "";
            string timeSuffix = "";
            List<string> timeParts = new List<string> { };
            string dlyArg = "";
            List<string> delayParts = new List<string> { };
            parseStage stage = parseStage.Text;
            int iWordPos = 0;

            DateOnly refDate;
            TimeOnly refTime;

            if (useGameTime)
            {
                refDate = new DateOnly(SDate.Now());
                refTime = new TimeOnly(Game1.timeOfDay);
            }
            else
            {
                refDate = new DateOnly(DateTime.Now);
                refTime = new TimeOnly(DateTime.Now);
            }


            for (int iWord = iWordPos; iWord < arSourceText.Length; iWord++)
            {
                if (arSourceText[iWord] == "at")
                {
                    if (iWord + 1 <= arSourceText.Length - 1)
                    {
                        if (IsTime(arSourceText[iWord + 1]))
                        {
                            timeArg = arSourceText[iWord];
                            if (arSourceText[iWord + 1].EndsWith("pm", StringComparison.OrdinalIgnoreCase))
                            {
                                timeSuffix = "pm";
                                timeParts.Add(arSourceText[iWord + 1].Substring(0, arSourceText[iWord + 1].Length - 2));
                            }
                            else if (arSourceText[iWord + 1].EndsWith("am", StringComparison.OrdinalIgnoreCase))
                            {
                                timeSuffix = "am";
                                timeParts.Add(arSourceText[iWord + 1].Substring(0, arSourceText[iWord + 1].Length - 2));
                            }
                            else
                            {
                                timeParts.Add(arSourceText[iWord + 1]);
                            }
                            stage = parseStage.Text;
                            iWord++;
                        }
                        else
                        {
                            arTextPart.Add(arSourceText[iWord]);
                        }
                    }
                    else
                    {
                        arTextPart.Add(arSourceText[iWord]);
                    }
                }
                else if (arSourceText[iWord] == "in")
                {
                    dlyArg = arSourceText[iWord];
                    if (iWord + 2 < arSourceText.Length - 1)
                    {
                        if (int.TryParse(arSourceText[iWord + 1], out _) && timedelayunits.Contains(arSourceText[iWord + 2].ToLower()))
                        {
                            reminder.Delay.For = arSourceText[iWord + 1];
                            reminder.Delay.Units = arSourceText[iWord + 2].ToLower();
                            reminder.Delay.Days = false;
                            reminder.Delay.IsValid = true;
                            iWord += 2;
                            stage = parseStage.Text;
                            dlyArg = "";
                        }
                        else if (int.TryParse(arSourceText[iWord + 1], out _) && datedelayunits.Contains(arSourceText[iWord + 2].ToLower()))
                        {
                            reminder.Delay.For = arSourceText[iWord + 1];
                            reminder.Delay.Units = arSourceText[iWord + 2].ToLower();
                            reminder.Delay.Days = true;
                            reminder.Delay.IsValid = true;
                            iWord += 2;
                            stage = parseStage.Text;
                            dlyArg = "";
                        }
                        else
                        {
                            stage = parseStage.Delay;
                        }
                    }
                    else
                    {
                        stage = parseStage.Delay;
                    }
                }
                else if (arSourceText[iWord] == "on")
                {
                    dateArg = arSourceText[iWord];
                    stage = parseStage.Date;
                }
                else if (arSourceText[iWord] == "every")
                {
                    if (iWord + 2 < arSourceText.Length - 1)
                    {
                        if (IsRepeater(arSourceText[iWord + 1]) && weekdays.Contains(arSourceText[iWord + 2].ToLower()))
                        {
                            reminder.Repeats.Every = arSourceText[iWord + 1];
                            reminder.Repeats.Units = arSourceText[iWord + 2];
                            reminder.Repeats.IsValid = true;
                            iWord += 2;
                        }
                        else if (int.TryParse(arSourceText[iWord + 1], out int eQty))
                        {
                            if (datedelayunits.Contains(arSourceText[iWord + 2]))
                            {
                                reminder.Repeats.Every = arSourceText[iWord + 1];
                                reminder.Repeats.Units = arSourceText[iWord + 2];
                                reminder.Repeats.IsValid = true;
                                iWord += 2;
                            }
                            else if (timedelayunits.Contains(arSourceText[iWord + 2]))
                            {
                                reminder.Repeats.Every = arSourceText[iWord + 1];
                                reminder.Repeats.Units = arSourceText[iWord + 2];
                                reminder.Repeats.IsValid = true;
                                iWord += 2;
                            }
                        }
                    }
                    stage = parseStage.Text;
                }
                else if (dateSeparators.Contains(arSourceText[iWord]))
                {
                    dlyArg = arSourceText[iWord];
                    stage = parseStage.Delay;
                }
                else if (daySeparators.Contains(arSourceText[iWord]))
                {
                    dateParts.Add(arSourceText[iWord]);
                    stage = parseStage.Text;
                }
                else if (timeSuffixes.Contains(arSourceText[iWord]))
                {
                    timeSuffix = arSourceText[iWord];
                    stage = parseStage.Text;
                }
                else if (IsTime(arSourceText[iWord]))
                {
                    if (arSourceText[iWord].EndsWith("pm", StringComparison.OrdinalIgnoreCase))
                    {
                        timeSuffix = "pm";
                        timeParts.Add(arSourceText[iWord].Substring(0, arSourceText[iWord].Length - 2));
                        stage = parseStage.Text;
                    }
                    else if (arSourceText[iWord].EndsWith("am", StringComparison.OrdinalIgnoreCase))
                    {
                        timeSuffix = "am";
                        timeParts.Add(arSourceText[iWord].Substring(0, arSourceText[iWord].Length - 2));
                        stage = parseStage.Text;
                    }
                    else
                    {
                        timeParts.Add(arSourceText[iWord]);
                        stage = parseStage.Text;
                    }
                }
                else if (IsDate(arSourceText[iWord]))
                {
                    dateParts.Add(arSourceText[iWord]);
                    stage = parseStage.Text;
                }
                else
                {
                    switch (stage)
                    {
                        case parseStage.Text:
                            arTextPart.Add(arSourceText[iWord]);
                            break;
                        case parseStage.Date:
                            dateParts.Add(arSourceText[iWord]);
                            if (IsDate(arSourceText[iWord]))
                            {
                                stage = parseStage.Text;
                            }
                            break;
                        case parseStage.Repeat:
                            rptParts.Add(arSourceText[iWord]);
                            break;
                        case parseStage.Delay:
                            delayParts.Add(arSourceText[iWord]);
                            break;
                        case parseStage.Time:
                            timeParts.Add(arSourceText[iWord]);
                            break;
                    }
                }
            }
            //
            //  text parsed, interpretation time
            //
            reminder.ReminderText = string.Join(" ", arTextPart);
            reminder.Time.TimeSuffix = timeSuffix;
            reminder.Time.Time = string.Join(" ", timeParts);
            reminder.Time.StructuredTime = new TimeOnly(reminder.Time.Time, timeSuffix, true);
            reminder.Date.Date = string.Join(" ", dateParts);

            if (useGameTime)
            {
                reminder.Date.StructureDate = new DateOnly(reminder.Date.Date, SDate.Now());
            }
            else
            {
                reminder.Date.StructureDate = new DateOnly(reminder.Date.Date, DateTime.Now);
            }

            //
            //  set delay
            //
            if (reminder.Delay.IsValid)
            {
                SetDelay(reminder, refDate, refTime, useGameTime);
            }
            else if (!string.IsNullOrEmpty(dlyArg))
            {
                if (dlyArg == "at")
                {
                    reminder.Time.Time = string.Join(" ", delayParts);
                }
                else if (dlyArg == "in")
                {
                    if (delayParts.Count() == 2)
                    {
                        reminder.Delay.For = delayParts[0];
                        reminder.Delay.Units = delayParts[1];
                        SetDelay(reminder, refDate, refTime, useGameTime);
                    }
                    else
                    {
                        reminder.Delay.For = string.Join(" ", delayParts);
                    }
                }
            }
            //
            //  set repeats
            //
            if (rptParts.Count() > 0 || reminder.Repeats.IsValid)
            {
                if (!reminder.Repeats.IsValid)
                {
                    reminder.Repeats.Every = rptParts[0];
                    if (rptParts.Count() > 1)
                    {
                        reminder.Repeats.Units = rptParts[1];
                    }
                }
                SetNextRepeat(reminder, refDate, refTime, useGameTime,false);

                //if (!reminder.Date.StructureDate.IsValid)
                //{
                //    reminder.Date.StructureDate = refDate;
                //}
                //if (!reminder.Time.StructuredTime.IsValid)
                //{
                //    reminder.Time.StructuredTime = refTime;
                //}
                reminder.Repeats.IsValid = true;
            }

            if (!reminder.Date.StructureDate.IsValid)
            {
                reminder.Date.StructureDate = refDate;

            }

            reminder.IsValid = reminder.Repeats.IsValid || reminder.Delay.IsValid || reminder.Time.StructuredTime.IsValid || reminder.Date.StructureDate.IsValid;

            return reminder;
        }
        public static Tuple<DateOnly, TimeOnly> GetAdjustedDateTime(string howMany, string ofWhat, DateOnly refDate, TimeOnly refTime, bool useGameTime)
        {
            DateOnly doReturn = new DateOnly();
            TimeOnly toReturn = new TimeOnly();

            if (int.TryParse(howMany, out int qty))
            {
                switch (ofWhat)
                {
                    case "week":
                    case "weeks":
                        doReturn = refDate.AddDays(7 * qty);
                        break;
                    case "day":
                    case "days":
                        doReturn = refDate.AddDays(qty);
                        break;
                    case "month":
                    case "months":
                        doReturn = refDate.AddMonths(qty);
                        break;
                    case "minute":
                    case "minutes":
                        if (useGameTime)
                        {
                            Tuple<bool, TimeOnly> mins = refTime.AddMinutes(qty);
                            //reminder.Date.StructureDate = new DateOnly(mins.Item2.Day, mins.Item2.Month, mins.Year);
                            toReturn = new TimeOnly(mins.Item2.Hour, mins.Item2.Minute, mins.Item2.Second);
                        }
                        else
                        {
                            DateTime tDate = new DateTime(refDate.Year, refDate.Month, refDate.Day, refTime.Hour, refTime.Minute, refTime.Second).AddMinutes(qty);
                            doReturn = new DateOnly(tDate.Day, tDate.Month, tDate.Year);
                            toReturn = new TimeOnly(tDate.Hour, tDate.Minute, tDate.Second);
                        }
                        break;
                    case "hours":
                    case "hour":
                        if (useGameTime)
                        {
                            Tuple<bool, TimeOnly> mins = refTime.AddMinutes(qty * 60);
                            //reminder.Date.StructureDate = new DateOnly(mins.Item2.Day, mins.Item2.Month, mins.Year);
                            toReturn = new TimeOnly(mins.Item2.Hour, mins.Item2.Minute, mins.Item2.Second);
                        }
                        else
                        {
                            DateTime hrs = DateTime.Now.AddHours(qty);
                            doReturn = new DateOnly(hrs.Day, hrs.Month, hrs.Year);
                            toReturn = new TimeOnly(hrs.Hour, hrs.Minute, hrs.Second);
                        }
                        break;

                }
            }
            return Tuple.Create(doReturn, toReturn);
        }
        private static void SetNextRepeat(Reminder reminder, DateOnly refDate, TimeOnly refTime, bool useGameTime, bool overWrite)
        {
            Tuple<DateOnly, TimeOnly> adjust = GetAdjustedDateTime(reminder.Repeats.Every, reminder.Repeats.Units, refDate, refTime, useGameTime);
            if ((overWrite || !reminder.Date.StructureDate.IsValid) && adjust.Item1.IsValid)
            {
                reminder.Date.StructureDate = adjust.Item1;
            }
            if ((overWrite || !reminder.Time.StructuredTime.IsValid) && adjust.Item2.IsValid)
            {
                reminder.Time.StructuredTime = adjust.Item2;
            }
        }
        public static void SetDelay(Reminder reminder, DateOnly refDate, TimeOnly refTime, bool useGameTime)
        {

            Tuple<DateOnly, TimeOnly> adjust = GetAdjustedDateTime(reminder.Delay.For, reminder.Delay.Units, refDate, refTime, useGameTime);

            if (adjust.Item1.IsValid)
            {
                reminder.Date.StructureDate = adjust.Item1;
            }
            if (adjust.Item2.IsValid)
            {
                reminder.Time.StructuredTime = adjust.Item2;
            }
            //if (int.TryParse(reminder.Delay.For, out int qty))
            //{
            //    switch (reminder.Delay.Units)
            //    {
            //        case "week":
            //        case "weeks":
            //            DateOnly wks = refDate.AddDays(7 * qty);
            //            reminder.Date.StructureDate = new DateOnly(wks.Day, wks.Month, wks.Year);
            //            break;
            //        case "day":
            //        case "days":
            //            DateOnly days = refDate.AddDays(qty);
            //            reminder.Date.StructureDate = new DateOnly(days.Day, days.Month, days.Year);
            //            break;
            //        case "month":
            //        case "months":
            //            DateOnly mnth = refDate.AddMonths(qty);
            //            reminder.Date.StructureDate = new DateOnly(mnth.Day, mnth.Month, mnth.Year);
            //            break;
            //        case "minute":
            //        case "minutes":
            //            if (useGameTime)
            //            {
            //                Tuple<bool, TimeOnly> mins = refTime.AddMinutes(qty);
            //                //reminder.Date.StructureDate = new DateOnly(mins.Item2.Day, mins.Item2.Month, mins.Year);
            //                reminder.Time.StructuredTime = new TimeOnly(mins.Item2.Hour, mins.Item2.Minute, mins.Item2.Second);
            //            }
            //            else
            //            {
            //                DateTime tDate = new DateTime(refDate.Year, refDate.Month, refDate.Day, refTime.Hour, refTime.Minute, refTime.Second).AddMinutes(qty);
            //                reminder.Date.StructureDate = new DateOnly(tDate.Day, tDate.Month, tDate.Year);
            //                reminder.Time.StructuredTime = new TimeOnly(tDate.Hour, tDate.Minute, tDate.Second);
            //            }
            //            break;
            //        case "hours":
            //        case "hour":
            //            if (useGameTime)
            //            {
            //                Tuple<bool, TimeOnly> mins = refTime.AddMinutes(qty * 60);
            //                //reminder.Date.StructureDate = new DateOnly(mins.Item2.Day, mins.Item2.Month, mins.Year);
            //                reminder.Time.StructuredTime = new TimeOnly(mins.Item2.Hour, mins.Item2.Minute, mins.Item2.Second);
            //            }
            //            else
            //            {
            //                DateTime hrs = DateTime.Now.AddHours(qty);
            //                reminder.Date.StructureDate = new DateOnly(hrs.Day, hrs.Month, hrs.Year);
            //                reminder.Time.StructuredTime = new TimeOnly(hrs.Hour, hrs.Minute, hrs.Second);
            //            }
            //            break;

            //    }
            //}

        }
        public static bool IsRepeater(string text)
        {
            List<string> repeaters = new List<string>
            {
                "st",
                "nd",
                "rd",
                "th"
            };

            foreach (string s in repeaters)
            {
                if (text.EndsWith(s, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
        private static string RemoveTimeSuffix(string time)
        {
            if (string.IsNullOrEmpty(time)) { return time; }

            return time.Replace("pm", "", StringComparison.OrdinalIgnoreCase).Replace("am", "", StringComparison.OrdinalIgnoreCase);
        }
        private static bool IsDate(string testText)
        {
            if (string.IsNullOrEmpty(testText)) return false;
            string[] arDate = testText.Split('/');
            if (arDate.Length == 2)
            {
                // 11/23
                return int.TryParse(arDate[0], out _) && int.TryParse(arDate[1], out _);
            }
            else if (arDate.Length == 3)
            {
                return int.TryParse(arDate[0], out _) && int.TryParse(arDate[1], out _) && int.TryParse(arDate[2], out _);
            }

            return false;
        }
        private static bool IsTime(string timeText)
        {
            if (string.IsNullOrEmpty(timeText)) return false;
            string[] arTime = timeText.Split(':');
            if (arTime.Length == 2)
            {
                // 00:00 00:00am
                return int.TryParse(arTime[0], out _) && int.TryParse(RemoveTimeSuffix(arTime[1]), out _);
            }
            else if (arTime.Length == 1)
            {
                // 4 4pm
                if (int.TryParse(RemoveTimeSuffix(arTime[0]), out _))
                {
                    return true;
                }
            }

            return false;
        }
        public static void Test()
        {
            List<string> tests = new List<string>
            {
                "every 2 hours get up",
                "every 2 days at 10:00 milk the cows",
                "go feed the fish at 10am",
                "feed the fish at 10:00am",
                "feed the fish at 10:00 am",
                "walk the dog 10:00am today",
                "feed the cat at 4 tomorrow",
                "get haircut at 14:24 pm",
                "credit card pay at 8am",
                "credit card pay at 8:00 every 20th",
                "cafe with Omar at Borrone 17:30",
                "cafe with Justin at Ginza at 6 on 08/23",
                "pick up books at library at 10am every Sunday",
                "get some weed at 10 every 2nd monday",
                "in 10 minutes call the dog",
                "in 2 weeks order a car"
             };

            Reminder reminder;

            foreach (string test in tests)
            {
                reminder = ParseReminderText(test, true);
                Console.WriteLine(test + ": " + reminder.ToString());
            }
        }
    }
}
