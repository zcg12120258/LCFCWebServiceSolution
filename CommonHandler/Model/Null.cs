using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace CommonHandler.Model
{
    public class Null
    {
        public static short NullShort
        {
            get { return -1; }
        }
        /// <summary>
        /// Return -1.
        /// </summary>
        public static int NullInteger
        {
            get { return -1; }
        }
        /// <summary>
        /// Return float MinValue.
        /// </summary>
        public static float NullSingle
        {
            get { return float.MinValue; }
        }
        /// <summary>
        /// Return double MinValue.
        /// </summary>
        public static double NullDouble
        {
            get { return double.MinValue; }
        }
        /// <summary>
        /// Return decimal MinValue.
        /// </summary>
        public static decimal NullDecimal
        {
            get { return decimal.MinValue; }
        }
        /// <summary>
        /// Return DateTime MinValue.
        /// </summary>
        public static System.DateTime NullDate
        {
            get { return System.DateTime.MinValue; }
        }
        /// <summary>
        /// Return empty string.
        /// </summary>
        public static string NullString
        {
            get { return ""; }
        }
        /// <summary>
        /// Return false.
        /// </summary>
        public static bool NullBoolean
        {
            get { return false; }
        }
        /// <summary>
        /// Return empty Guid.
        /// </summary>
        public static Guid NullGuid
        {
            get { return Guid.Empty; }

        }

        /// <summary>
        /// Sets a field to an application encoded null value (used in BLL layer).
        /// </summary>
        /// <param name="objPropertyInfo">Object of  class PropertyInfo.</param>
        /// <returns>object</returns>
        public static object SetNull(PropertyInfo objPropertyInfo)
        {
            object functionReturnValue = null;
            switch (objPropertyInfo.PropertyType.ToString())
            {
                case "System.Int16":
                    functionReturnValue = NullShort;
                    break;
                case "System.Int32":
                case "System.Int64":
                    functionReturnValue = NullInteger;
                    break;
                case "System.Single":
                    functionReturnValue = NullSingle;
                    break;
                case "System.Double":
                    functionReturnValue = NullDouble;
                    break;
                case "System.Decimal":
                    functionReturnValue = NullDecimal;
                    break;
                case "System.DateTime":
                    functionReturnValue = NullDate;
                    break;
                case "System.String":
                case "System.Char":
                    functionReturnValue = NullString;
                    break;
                case "System.Boolean":
                    functionReturnValue = NullBoolean;
                    break;
                case "System.Guid":
                    functionReturnValue = NullGuid;
                    break;
                default:
                    // Enumerations default to the first entry
                    Type pType = objPropertyInfo.PropertyType;
                    if (pType.BaseType.Equals(typeof(System.Enum)))
                    {
                        System.Array objEnumValues = System.Enum.GetValues(pType);
                        Array.Sort(objEnumValues);
                        functionReturnValue = System.Enum.ToObject(pType, objEnumValues.GetValue(0));
                    }
                    else
                    {
                        // complex object
                        functionReturnValue = null;
                    }

                    break;
            }
            return functionReturnValue;
        }
    }
}
