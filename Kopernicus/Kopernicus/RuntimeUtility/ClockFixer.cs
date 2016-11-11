/**
 * Kopernicus Planetary System Modifier
 * ====================================
 * Created by: BryceSchroeder and Teknoman117 (aka. Nathaniel R. Lewis)
 * Maintained by: Thomas P., NathanKell and KillAshley
 * Additional Content by: Gravitasi, aftokino, KCreator, Padishar, Kragrathea, OvenProofMars, zengei, MrHappyFace
 * ------------------------------------------------------------- 
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with this library; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston,
 * MA 02110-1301  USA
 * 
 * This library is intended to be used as a plugin for Kerbal Space Program
 * which is copyright 2011-2015 Squad. Your usage of Kerbal Space Program
 * itself is governed by the terms of its EULA, not the license above.
 * 
 * https://kerbalspaceprogram.com
 */

using UnityEngine;
using System;
using System.Linq;
using System.Text;

namespace Kopernicus
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class ClockFixer : MonoBehaviour
    {
        public void Start()
        {
            // Find the home planet
            CelestialBody homePlanet = FlightGlobals.Bodies.First(b => b.transform.name == "Kerbin");

            // Get custom year and day duration
            ClockFormatter.year = homePlanet.orbitDriver.orbit.period;
            ClockFormatter.day = homePlanet.solarDayLength;

            // If tidally locked set day = year
            if (ClockFormatter.year == homePlanet.rotationPeriod)
                ClockFormatter.day = ClockFormatter.year;

            // Convert negative numbers to positive
            if (ClockFormatter.year < 0)
                ClockFormatter.year = -ClockFormatter.year;
            if (ClockFormatter.day < 0)
                ClockFormatter.day = -ClockFormatter.day;

            // If weird number revert to stock values
            if (double.IsInfinity(ClockFormatter.day) || double.IsNaN(ClockFormatter.day) || double.IsInfinity(ClockFormatter.year) || double.IsNaN(ClockFormatter.year))
            {
                ClockFormatter.year = 9201600;
                ClockFormatter.day = 21600;
            }

            // Replace the stock Formatter
            KSPUtil.dateTimeFormatter = new ClockFormatter();
        }
    }

    public class ClockFormatter : IDateTimeFormatter
    {
        public static KSPUtil.DefaultDateTimeFormatter DTF = new KSPUtil.DefaultDateTimeFormatter();

        public static double day = new double();
        public static double year = new double();
        public static int[] num = new int[6];

        public string PrintTimeLong(double time)
        {
            string text = CheckNum(time);
            if (text != null)
                return text;

            GetTime(time);
            StringBuilder sb = StringBuilderCache.Acquire(256);
            sb.Append(num[1]).Append(num[1] == 1 ? "Year" : "Years").Append(", ");
            sb.Append(num[5] < 0 ? 0 : num[5]).Append(num[5] == 1 ? "Day" : "Days").Append(", ");
            sb.Append(num[4]).Append(num[4] == 1 ? "Hour" : "Hours").Append(", ");
            sb.Append(num[3]).Append(num[3] == 1 ? "Min" : "Mins").Append(", ");
            sb.Append(num[2]).Append(num[2] == 1 ? "Sec" : "Secs");
            return sb.ToStringAndRelease();
        }

        public string PrintTimeStamp(double time, bool days = false, bool years = false)
        {
            string text = CheckNum(time);
            if (text != null)
                return text;

            GetTime(time);
            StringBuilder stringBuilder = StringBuilderCache.Acquire(256);

            if (years)
                stringBuilder.Append("Year ").Append(num[1]).Append(", ");
            if (days)
                stringBuilder.Append("Day ").Append(num[5] < 0 ? 0 : num[5]).Append(" - ");

            stringBuilder.AppendFormat("{0:00}:{1:00}", num[4], num[3]);

            if (num[1] < 10)
                stringBuilder.AppendFormat(":{0:00}", num[2]);

            return stringBuilder.ToStringAndRelease();
        }

        public string PrintTimeStampCompact(double time, bool days = false, bool years = false)
        {
            string text = CheckNum(time);
            if (text != null)
                return text;

            GetTime(time);
            StringBuilder stringBuilder = StringBuilderCache.Acquire(256);
            if (years)
                stringBuilder.Append(num[1]).Append("y, ");

            if (days)
                stringBuilder.Append(num[5] < 0 ? 0 : num[5]).Append("d, ");

            stringBuilder.AppendFormat("{0:00}:{1:00}", num[4], num[3]);
            if (num[1] < 10)
                stringBuilder.AppendFormat(":{0:00}", num[2]);

            return stringBuilder.ToStringAndRelease();
        }

        public string PrintTime(double time, int valuesOfInterest, bool explicitPositive)
        {
            string text = CheckNum(time);
            if (text != null)
                return text;

            bool flag = time < 0.0;
            GetTime(time);
            string[] array = new string[] { "s", "m", "h", "d", "y" };
            StringBuilder stringBuilder = StringBuilderCache.Acquire(256);
            if (flag)
                stringBuilder.Append("- ");
            else if (explicitPositive)
                stringBuilder.Append("+ ");

            int[] list = new int[] { num[2], num[3], num[4], num[5], num[1] };
            int num0 = list.Length;
            while (num0-- > 0)
            {
                if (list[num0] != 0)
                {
                    for (int i = num0; i > Mathf.Max(num0 - valuesOfInterest, -1); i--)
                    {
                        stringBuilder.Append(Math.Abs(list[i])).Append(array[i]);
                        if (i - 1 > Mathf.Max(num0 - valuesOfInterest, -1))
                            stringBuilder.Append(", ");
                    }
                    break;
                }
            }
            return stringBuilder.ToStringAndRelease();
        }

        public string PrintTimeCompact(double time, bool explicitPositive)
        {
            string text = CheckNum(time);
            if (text != null)
                return text;

            bool flag = time < 0.0;
            GetTime(time);
            StringBuilder stringBuilder = StringBuilderCache.Acquire(256);
            if (flag)
                stringBuilder.Append("T- ");
            else if (explicitPositive)
                stringBuilder.Append("T+ ");

            if (num[5] > 0)
                stringBuilder.Append(Math.Abs(num[5])).Append(":");

            stringBuilder.AppendFormat("{0:00}:{1:00}:{2:00}", num[4], num[3], num[2]);
            return stringBuilder.ToStringAndRelease();
        }
        public string PrintDateDelta(double time, bool includeTime, bool includeSeconds, bool useAbs)
        {
            string text = CheckNum(time);
            if (text != null)
                return text;

            if (useAbs && time < 0.0)
                time = -time;

            StringBuilder stringBuilder = StringBuilderCache.Acquire(256);
            GetTime(time);

            if (num[1] > 1)
                stringBuilder.Append(num[1]).Append(" years");
            else if (num[1] == 1)
                stringBuilder.Append(num[1]).Append(" year");

            if (num[5] > 1)
            {
                if (stringBuilder.Length != 0)
                    stringBuilder.Append(", ");
                stringBuilder.Append(num[5]).Append(" days");
            }
            else if (num[5] == 1)
            {
                if (stringBuilder.Length != 0)
                    stringBuilder.Append(", ");
                stringBuilder.Append(num[5]).Append(" day");
            }
            if (includeTime)
            {
                if (num[4] > 1)
                {
                    if (stringBuilder.Length != 0)
                        stringBuilder.Append(", ");
                    stringBuilder.Append(num[4]).Append(" hours");
                }
                else if (num[4] == 1)
                {
                    if (stringBuilder.Length != 0)
                        stringBuilder.Append(", ");
                    stringBuilder.Append(num[4]).Append(" hour");
                }
                if (num[3] > 1)
                {
                    if (stringBuilder.Length != 0)
                        stringBuilder.Append(", ");
                    stringBuilder.Append(num[3]).Append(" minutes");
                }
                else if (num[3] == 1)
                {
                    if (stringBuilder.Length != 0)
                        stringBuilder.Append(", ");
                    stringBuilder.Append(num[3]).Append(" minute");
                }
                if (includeSeconds)
                {
                    if (num[2] > 1)
                    {
                        if (stringBuilder.Length != 0)
                            stringBuilder.Append(", ");
                        stringBuilder.Append(num[2]).Append(" seconds");
                    }
                    else if (num[2] == 1)
                    {
                        if (stringBuilder.Length != 0)
                            stringBuilder.Append(", ");
                        stringBuilder.Append(num[2]).Append(" second");
                    }
                }
            }
            if (stringBuilder.Length == 0)
                stringBuilder.Append((!includeTime) ? "0 days" : ((!includeSeconds) ? "0 minutes" : "0 seconds"));

            return stringBuilder.ToStringAndRelease();
        }

        public string PrintDateDeltaCompact(double time, bool includeTime, bool includeSeconds, bool useAbs)
        {
            string text = CheckNum(time);
            if (text != null)
                return text;

            if (useAbs && time < 0.0)
                time = -time;

            StringBuilder stringBuilder = StringBuilderCache.Acquire(256);
            GetTime(time);
            if (num[1] > 0)
                stringBuilder.Append(num[1]).Append("y");

            if (num[5] > 0)
            {
                if (stringBuilder.Length != 0)
                    stringBuilder.Append(", ");
                stringBuilder.Append(num[5]).Append("d");
            }
            if (includeTime)
            {
                if (num[4] > 0)
                {
                    if (stringBuilder.Length != 0)
                        stringBuilder.Append(", ");
                    stringBuilder.Append(num[4]).Append("h");
                }
                if (num[3] > 0)
                {
                    if (stringBuilder.Length != 0)
                        stringBuilder.Append(", ");
                    stringBuilder.Append(num[3]).Append("m");
                }
                if (includeSeconds && num[2] > 0)
                {
                    if (stringBuilder.Length != 0)
                        stringBuilder.Append(", ");
                    stringBuilder.Append(num[2]).Append("s");
                }
            }
            if (stringBuilder.Length == 0)
                stringBuilder.Append((!includeTime) ? "0d" : ((!includeSeconds) ? "0m" : "0s"));
            return stringBuilder.ToStringAndRelease();
        }

        public string PrintDate(double time, bool includeTime, bool includeSeconds = false)
        {
            string text = CheckNum(time);

            if (text != null)
                return text;


            StringBuilder stringBuilder = StringBuilderCache.Acquire(256);
            GetDate(time);

            stringBuilder.Append("Year ").Append(num[1] + 1).Append(", Day ").Append(num[5] + 1);
            if (includeTime)
                stringBuilder.Append(" - ").Append(num[4]).Append("h, ").Append(num[3]).Append("m");
            if (includeSeconds)
                stringBuilder.Append(", ").Append(num[2]).Append("s");
            return stringBuilder.ToStringAndRelease();
        }
        public string PrintDateNew(double time, bool includeTime)
        {
            string text = CheckNum(time);
            if (text != null)
                return text;

            StringBuilder stringBuilder = StringBuilderCache.Acquire(256);
            GetDate(time);
            stringBuilder.Append("Year ").Append(num[1] + 1).Append(", Day ").Append(num[5] + 1);
            if (includeTime)
                stringBuilder.AppendFormat(" - {0:D2}:{1:D2}:{2:D2}", num[4], num[3], num[2]);
            return stringBuilder.ToStringAndRelease();
        }

        public string PrintDateCompact(double time, bool includeTime, bool includeSeconds = false)
        {
            string text = CheckNum(time);
            if (text != null)
                return text;

            StringBuilder stringBuilder = StringBuilderCache.Acquire(256);
            GetDate(time);
            stringBuilder.AppendFormat("Y{0}, D{1:00}", num[1] + 1, num[5] + 1);
            if (includeTime)
                stringBuilder.AppendFormat(", {0}:{1:00}", num[4], num[3]);
            if (includeSeconds)
                stringBuilder.AppendFormat(":{0:00}", num[2]);
            return stringBuilder.ToStringAndRelease();
        }

        private static string CheckNum(double time)
        {
            if (double.IsNaN(time))
                return "NaN";

            if (double.IsPositiveInfinity(time))
                return "+Inf";

            if (double.IsNegativeInfinity(time))
                return "-Inf";

            return null;
        }

        public void GetDate(double time)
        {
            // This will work also when a year cannot be divided in days without a remainder
            // If the year ends halfway through a day, the clock will go:
            // Year 1 Day 365   ==>   Year 2 Day 0    (Instead of starting directly with Day 1)
            // Day 0 will last untill Day 365 would have ended, then Day 1 will start.
            // This way the time shown by the clock will always be consistent with the position of the sun in the sky

            Debug.Log("SigmaLog: time = " + time);
            // Number of seconds in this day
            int num0 = (int)(time % day);
            Debug.Log("SigmaLog: num0 = " + num0);
            // Number of years in this time
            int num1 = (int)(time / year);
            Debug.Log("SigmaLog: num1 = " + num1);
            // Number of seconds in this minute
            int num2 = num0 % 60;
            Debug.Log("SigmaLog: num2 = " + num2);
            // Number of minutes in this hour
            int num3 = num0 / 60 % 60;
            Debug.Log("SigmaLog: num3 = " + num3);
            // Number of hours in this day
            int num4 = num0 / 3600;
            Debug.Log("SigmaLog: num4 = " + num4);
            // Number of days in this year
            int num5 = (int)(time / day) - (int)(Math.Round(year / day, 0, MidpointRounding.AwayFromZero) * num1);

            Debug.Log("SigmaLog: num5 = " + num5);
            num = new int[] { num0, num1, num2, num3, num4, num5 };
        }

        public void GetTime(double time)
        {
            // This will count the number of Years, Days, Hours, Minutes and Seconds
            // If a Year lasts 10.5 days, and time = 14 days, the result will be: 
            // 1 Year, 3 days, and whatever hours-minutes-seconds fit in 0.5 days.
            // ( 10.5 + 3 + 0.5 = 14 )

            int num1 = (int)(time / year);
            int num2 = (int)(time - (num1 * year));
            int num3 = num2 / 60 % 60;
            int num4 = num2 / 3600 % ((int)day / 3600);
            int num5 = (int)(num2 / year);
            num2 = num2 % 60;

            num = new int[] { 0, num1, num2, num3, num4, num5 };
        }

        public int Minute
        {
            get
            {
                return 60;
            }
        }
        public int Hour
        {
            get
            {
                return 3600;
            }
        }
        public int Day
        {
            get
            {
                return 21600;
            }
        }
        public int Year
        {
            get
            {
                return 9201600;
            }
        }

        public ClockFormatter()
        {
        }
    }
}
