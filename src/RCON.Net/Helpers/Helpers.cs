using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RCON.Net
{
    public static class Helpers
    {
        /// <summary>
        /// Returns Enum Value given its Description Attribute
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="d"></param>
        /// <returns>Enum Value</returns>
        public static T GetValueFromDescription<T>(string d)
        {
            string description = d.ToLower();
            var type = typeof(T);
            if (!type.IsEnum) throw new InvalidOperationException();
            foreach (var field in type.GetFields())
            {
                DescriptionAttribute attribute = Attribute.GetCustomAttribute(field,
                    typeof(DescriptionAttribute)) as DescriptionAttribute;
                if (attribute != null)
                {
                    if (attribute.Description.ToLower() == description)
                        return (T)field.GetValue(null);
                }
                else
                {
                    if (field.Name.ToLower() == description)
                        return (T)field.GetValue(null);
                }
            }
            return default(T);
        }
        public static string GetEnumDescription(this Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());

            DescriptionAttribute[] attributes = fi.GetCustomAttributes(typeof(DescriptionAttribute), false) as DescriptionAttribute[];

            if (attributes != null && attributes.Any())
            {
                return attributes.First().Description;
            }

            return value.ToString();
        }
        /// <summary>
        /// Converts Hexidecimal String into ASCII String
        /// </summary>
        /// <param name="hexString"></param>
        /// <returns>ASCII String</returns>
        public static string Hex2Ascii(string hexString)
        {
            byte[] tmp;
            var j = 0;
            tmp = new byte[(hexString.Length) / 2];
            for (var i = 0; i <= hexString.Length - 2; i += 2)
            {
                tmp[j] = (byte)Convert.ToChar(Int32.Parse(hexString.Substring(i, 2), NumberStyles.HexNumber));

                j++;
            }
            return Bytes2String(tmp);
        }
        /// <summary>
        /// Get Byte[] Value of string
        /// </summary>
        /// <param name="s"></param>
        /// <returns>Byte[] of String</returns>
        public static byte[] String2Bytes(string s)
        {
            return Encoding.GetEncoding(1252).GetBytes(s);
        }
        /// <summary>
        /// Returns entire string value of byte[]
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns>string value</returns>
        public static string Bytes2String(IEnumerable<byte> bytes)
        {
            return Encoding.GetEncoding(1252).GetString(bytes.ToArray());
        }
        /// <summary>
        /// Returns string value of range in byte[]
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="index"></param>
        /// <param name="count"></param>
        /// <returns>string value</returns>
        public static string Bytes2String(IEnumerable<byte> bytes, int index, int count)
        {
            return Encoding.UTF8.GetString(bytes.ToArray(), index, count);
        }
        /// <summary>
        /// Returns Description Attribute of Enum
        /// </summary>
        /// <param name="value"></param>
        /// <returns>Description Attribute</returns>
        public static string StringValueOf(Enum value)
        {
            var fi = value.GetType().GetField(value.ToString());
            var attributes =
                (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (attributes.Length > 0)
            {
                return attributes[0].Description;
            }
            else
            {
                return value.ToString();
            }
        }

        /// <summary>
        /// Gets Enum Value given its Description Attribute
        /// </summary>
        /// <param name="value"></param>
        /// <param name="enumType"></param>
        /// <returns>Enum Value</returns>
        public static object EnumValueOf(string value, Type enumType)
        {
            var names = Enum.GetNames(enumType);
            foreach (var name in names)
            {
                if (StringValueOf((Enum)Enum.Parse(enumType, name)).Equals(value))
                {
                    return Enum.Parse(enumType, name);
                }
            }

            throw new ArgumentException("The string is not a description or value of the specified enum.");
        }

        public static IEnumerable<T> Range<T>(this IEnumerable<T> array,int from,int count)
        {
            return array.Skip(from).Take(count);
        }
    }
}
