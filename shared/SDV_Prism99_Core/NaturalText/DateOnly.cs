using System;
using System.Collections.Generic;
using StardewModdingAPI.Utilities;


namespace Prism99_Core.NaturalText
{
    internal class DateOnly:IEqualityComparer<DateOnly>,IComparable<DateOnly>,IEquatable<DateOnly>
    {
        public int Day;
        public int Month;
        public int Year;
        public bool IsValid = false;
        public bool GameTime;
        private DateTime refDTDate;

        public DateOnly() { }
        public DateOnly(DateOnly parent) 
        {
            Day = parent.Day;
            Month = parent.Month;
            Year = parent.Year;
            IsValid = parent.IsValid;
            GameTime = parent.GameTime;
        }

        public DateOnly(int day, int month, int year)
        {
            Day = day;
            Month = month;
            Year = year;
            IsValid = true;
            refDTDate = new DateTime(year, month, day);
        }
        public DateOnly(DateTime refDate)
        {
            Day = refDate.Day;
            Month = refDate.Month;
            Year = refDate.Year;
            IsValid = true;
            refDTDate = refDate;
        }
        public DateOnly(SDate refDate)
        {
            Day = refDate.Day;
            Month = refDate.SeasonIndex + 1;
            Year = refDate.Year;
            IsValid = true;
            GameTime = true;
        }
        public DateOnly(string testdate, SDate refDate)
        {
            GameTime = true;
            string[] arParts = testdate.Split('/');
            switch (arParts.Length)
            {
                case 1:
                    if (int.TryParse(arParts[0], out Day))
                    {
                        Month = refDate.SeasonIndex + 1;
                        Year = refDate.Year;
                        IsValid = true;
                    }
                    else
                    {
                        switch (arParts[0].ToLower())
                        {
                            case "today":
                                Day = refDate.Day;
                                Month = refDate.SeasonIndex + 1;
                                Year = refDate.Year;
                                IsValid = true;
                                break;
                            case "tomorrow":
                                Day = refDate.AddDays(1).Day;
                                Month = refDate.AddDays(1).SeasonIndex + 1;
                                Year = refDate.AddDays(1).Year;
                                IsValid = true;
                                break;
                        }
                    }
                    break;
                case 2:
                    IsValid = int.TryParse(arParts[0], out Day);
                    IsValid = IsValid && int.TryParse(arParts[1], out Month);
                    Year = refDate.Year;
                    break;
                case 3:
                    IsValid = int.TryParse(arParts[0], out Day);
                    IsValid = IsValid && int.TryParse(arParts[1], out Month);
                    IsValid = IsValid && int.TryParse(arParts[2], out Year);
                    break;
            }
        }
        public DateOnly(string testdate, DateTime refDate)
        {
            refDTDate = refDate;

            string[] arParts = testdate.Split('/');
            switch (arParts.Length)
            {
                case 1:
                    if (int.TryParse(arParts[0], out Day))
                    {
                        Month = refDate.Month;
                        Year = refDate.Year;
                        IsValid = true;
                    }
                    else
                    {
                        switch (arParts[0].ToLower())
                        {
                            case "today":
                                Day = refDate.Day;
                                Month = refDate.Month;
                                Year = refDate.Year;
                                IsValid = true;
                                break;
                            case "tomorrow":
                                Day = refDate.AddDays(1).Day;
                                Month = refDate.AddDays(1).Month;
                                Year = refDate.AddDays(1).Year;
                                IsValid = true;
                                break;
                        }
                    }
                    break;
                case 2:
                    IsValid = int.TryParse(arParts[0], out Day);
                    IsValid = IsValid && int.TryParse(arParts[1], out Month);
                    Year = refDate.Year;
                    break;
                case 3:
                    IsValid = int.TryParse(arParts[0], out Day);
                    IsValid = IsValid && int.TryParse(arParts[1], out Month);
                    IsValid = IsValid && int.TryParse(arParts[2], out Year);
                    break;
            }
        }
        public DateOnly AddMonths(int months)
        {
            if (GameTime)
            {
                DateOnly nDate = new DateOnly(this);
                nDate.Month += months;
                if (nDate.Month > 4)
                {
                    nDate.Month = nDate.Month - 4;
                    nDate.Year++;
                }
                return nDate;
            }
            else
            {
                return new DateOnly(refDTDate.AddMonths(months));
            }
        }
        public DateOnly AddDays(int days)
        {
            if (GameTime)
            {
                DateOnly nDate = new DateOnly(this);
                nDate.Day += days;
                if (nDate.Day > 28)
                {
                    nDate.Day = 1;
                    nDate.Month++;
                    if (nDate.Month > 4)
                    {
                        nDate.Month = 1;
                        nDate.Year++;
                    }
                }
                return nDate;
            }
            else
            {
                return new DateOnly(refDTDate.AddDays(days));
            }
        }
        public override string ToString()
        {
            return $"{Day:D2}/{Month:D2}/{Year:D4}";
        }
        public bool IsSameDate(DateTime testDate)
        {
            return testDate.Day == Day && testDate.Month == Month && testDate.Year == Year;
        }
        public bool IsSameDate(SDate testDate)
        {
            return testDate.Day == Day && testDate.SeasonIndex + 1 == Month && testDate.Year == Year;
        }

        public bool Equals(DateOnly x, DateOnly y)
        {
            return x.Year == y.Year &&
                x.Month == y.Month &&
                x.Day == y.Day;
        }

        public int GetHashCode([DisallowNull] DateOnly obj)
        {
            return (Year+Month+Day).GetHashCode();
        }

        public int CompareTo(DateOnly other)
        {
            //
            //  check for less than
            if(Year < other.Year) { return -1; }
            if(Year==other.Year && Month< other.Month) { return -1; }
            if(Year==other.Year && Month== other.Month && Day<other.Day) { return -1; }
            //
            //  check for greater than
            if (Year > other.Year) { return 1; }
            if (Year == other.Year && Month > other.Month) { return 1; }
            if (Year == other.Year && Month == other.Month && Day > other.Day) { return 1; }
            //
            //  there equal
            return 0;
        }

        public bool Equals(DateOnly other)
        {
            return Equals(this,other);
        }
    }
}
