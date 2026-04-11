using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace GeniView.Data
{
    public static class DataHelpers
    {
        public static string BatterySerialNumberCodeToSerialNumber(long serialNumberCode)
        {
            int yearCode = (int)(serialNumberCode >> 20) & 0xFF; // bits 20-27, 8 bit value.
            int monthCode = (int)(serialNumberCode >> 16) & 0x0F; // bits 16-19, 4 bit value.
            int sequentialNumber = (int)serialNumberCode & 0xFFFF; // bits 0-15, 16 bit value.

            // Year starts from 1 = 2016 = J. Convert to ASCII J and increment as necessary.
            // Month starts from 1 = January = A.
            string sn = Char.ConvertFromUtf32(yearCode + 0x49); // 0x49 = J - 1 in ASCII.
            sn += Char.ConvertFromUtf32(monthCode + 0x40); //0x40 = A - 1 in ASCII.
            sn += sequentialNumber.ToString("D6"); // ushort max digits + 1 by Bytec request.

            return sn;
        }

        /// <summary>
        /// Counts the bits that are set to 1.
        /// Source: https://www.dotnetperls.com/bitcount.
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public static int CountBits(int n)  
        {
            int count = 0;
            while (n != 0)
            {
                count++;
                n &= (n - 1);
            }
            return count;
        }

        public static string TimeSpanToFriendlyString(TimeSpan time)
        {
            if (time.TotalMinutes < 1)
                return "0 minute";
            else
            {
                int hours = (int)time.TotalMinutes / 60;
                int mins = (int)time.TotalMinutes % 60;

                return (hours == 0 ? "" : hours.ToString() + (hours == 1 ? " hour" : " hours"))
                    + (hours > 0 && mins > 0 ? " and " : "") + (mins == 0 ? "" : mins.ToString() + (mins == 1 ? " minute" : " minutes"));
            }
        }
    }

    public static class EnumHelper
    {
        public static string GetFriendlyText(Enum value, string divider = "\n")
        {
            if (value == null)
                return null;

            Type enumType = value.GetType();

            if (enumType.IsEnum == false)
                return null;

            Enum currentValue = (Enum)value;
            var zeroValue = Enum.ToObject(enumType, 0);

            // Get all possible values.
            var allValues = Enum.GetValues(enumType);
            List<Enum> currentValues = new List<Enum>();

            // Looks like if FlagsAttribute is not set in enum and values are consecutive,
            // HasFlag is returning true for multiple items. When FlagsAttritubute is not used,
            // Enum should have a single value.

            // If no FlagsAttribute, just process the only value.
            if (enumType.GetCustomAttributes<FlagsAttribute>().Any())
            {
                foreach (Enum item in allValues)
                {
                    if (currentValue.HasFlag(item))
                        currentValues.Add(item);
                }
            }
            else
            {
                currentValues.Add(currentValue);
            }

            StringBuilder returnValues = new StringBuilder();

            foreach (Enum item in currentValues)
            {
                // If has Flags attribute, remove the enum with 0 value (OK, Unknown, None, etc.),
                // unless it's the only value. Here we having multiple values automatically means
                // Flags attribute, so no need for reflection to check Flags attribute itself.
                if (zeroValue.Equals(item) && currentValues.Count > 1)
                    continue;

                DescriptionAttribute descriptionAttribute = enumType.GetField(item.ToString()).GetCustomAttribute<DescriptionAttribute>();

                // If no DescriptionAttribute, just return the value string.
                if (descriptionAttribute != null)
                {
                    returnValues.Append(descriptionAttribute.Description);
                    returnValues.Append(divider);
                }
                else
                {
                    returnValues.Append(item.ToString());
                    returnValues.Append(divider);
                }
            }

            return returnValues.ToString().TrimEnd(divider.ToCharArray());
        }

        /// <summary>
        /// Returns an indication whether Enum with Flags attritube defines the given values.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsFlagsDefined(Enum value)
        {
            Type underlyingenumtype = Enum.GetUnderlyingType(value.GetType());
            switch (Type.GetTypeCode(underlyingenumtype))
            {
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.SByte:
                case TypeCode.Single:
                    {
                        object obj = Activator.CreateInstance(underlyingenumtype);
                        long svalue = System.Convert.ToInt64(value);
                        if (svalue < 0)
                            throw new ArgumentException(
                                string.Format("Can't process negative {0} as {1} enum with flags", svalue, value.GetType().Name));
                    }
                    break;
                default:
                    break;
            }

            ulong flagsset = System.Convert.ToUInt64(value);
            Array values = Enum.GetValues(value.GetType());//.Cast<ulong />().ToArray<ulong />();
            int flagno = values.Length - 1;
            ulong initialflags = flagsset;
            ulong flag = 0;
            //start with the highest values
            while (flagno >= 0)
            {
                flag = System.Convert.ToUInt64(values.GetValue(flagno));
                if ((flagno == 0) && (flag == 0))
                {
                    break;
                }
                //if the flags set contain this flag
                if ((flagsset & flag) == flag)
                {
                    //unset this flag
                    flagsset -= flag;
                    if (flagsset == 0)
                        return true;
                }
                flagno--;
            }
            if (flagsset != 0)
            {
                return false;
            }
            if (initialflags != 0 || flag == 0)
            {
                return true;
            }
            return false;
        }
    }
}
