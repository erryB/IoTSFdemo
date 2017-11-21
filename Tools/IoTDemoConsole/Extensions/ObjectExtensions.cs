using System;
using System.Text;

namespace IoTDemoConsole.Extensions
{

    /// <summary>
    /// Class ObjectExtensions.
    /// </summary>
    public static class ObjectExtensions
    {

        /// <summary>
        /// Lesses the than.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns><c>true</c> if left is less than right, <c>false</c> otherwise.</returns>
        /// <exception cref="System.NullReferenceException">left</exception>
        /// <exception cref="System.ArgumentException">
        /// left
        /// or
        /// right
        /// or
        /// </exception>
        public static bool LessThan(this object left, object right)
        {
            if (left == null)
                throw new NullReferenceException(nameof(left));
            if (!left.GetType().IsValueType)
                throw new ArgumentException(nameof(left));
            if (!right.GetType().IsValueType)
                throw new ArgumentException(nameof(right));
            if (left.GetType() != right.GetType())
                throw new ArgumentException();

            switch (Type.GetTypeCode(left.GetType()))
            {
                case TypeCode.Byte:
                    return (Byte)left <= (Byte)right;
                case TypeCode.SByte:
                    return (SByte)left <= (SByte)right;
                case TypeCode.UInt16:
                    return (UInt16)left <= (Byte)right;
                case TypeCode.UInt32:
                    return (UInt32)left <= (UInt32)right;
                case TypeCode.UInt64:
                    return (UInt64)left <= (UInt64)right;
                case TypeCode.Int16:
                    return (Int16)left <= (Int16)right;
                case TypeCode.Int32:
                    return (Int32)left <= (Int32)right;
                case TypeCode.Int64:
                    return (Int64)left <= (Int64)right;
                case TypeCode.Decimal:
                    return (Decimal)left <= (Decimal)right;
                case TypeCode.Double:
                    return (Double)left <= (Double)right;
                case TypeCode.Single:
                    return (Single)left <= (Single)right; ;
                case TypeCode.DateTime:
                    return (DateTime)left <= (DateTime)right;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Greaters the than.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns><c>true</c> if left greater than right, <c>false</c> otherwise.</returns>
        /// <exception cref="System.NullReferenceException">left</exception>
        /// <exception cref="System.ArgumentException">
        /// left
        /// or
        /// right
        /// or
        /// </exception>
        public static bool GreaterThan(this object left, object right)
        {
            if (left == null)
                throw new NullReferenceException(nameof(left));
            if (!left.GetType().IsValueType)
                throw new ArgumentException(nameof(left));
            if (!right.GetType().IsValueType)
                throw new ArgumentException(nameof(right));
            if (left.GetType() != right.GetType())
                throw new ArgumentException();

            switch (Type.GetTypeCode(left.GetType()))
            {
                case TypeCode.Byte:
                    return (Byte)left >= (Byte)right;
                case TypeCode.SByte:
                    return (SByte)left >= (SByte)right;
                case TypeCode.UInt16:
                    return (UInt16)left >= (Byte)right;
                case TypeCode.UInt32:
                    return (UInt32)left >= (UInt32)right;
                case TypeCode.UInt64:
                    return (UInt64)left >= (UInt64)right;
                case TypeCode.Int16:
                    return (Int16)left >= (Int16)right;
                case TypeCode.Int32:
                    return (Int32)left >= (Int32)right;
                case TypeCode.Int64:
                    return (Int64)left >= (Int64)right;
                case TypeCode.Decimal:
                    return (Decimal)left >= (Decimal)right;
                case TypeCode.Double:
                    return (Double)left >= (Double)right;
                case TypeCode.Single:
                    return (Single)left >= (Single)right; ;
                case TypeCode.DateTime:
                    return (DateTime)left >= (DateTime)right;
                default:
                    return false;
            }
        }

        /// <summary>
        /// To the string multiline.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>System.String.</returns>
        /// <exception cref="System.NullReferenceException">obj</exception>
        public static string ToStringMultiline(this object obj)
        {
            if (obj == null)
                throw new NullReferenceException(nameof(obj));

            var strBuild = new StringBuilder();

            var propInfoList = obj.GetType().GetProperties();

            foreach (var propInfo in propInfoList)
            {
                var propValue = propInfo.GetValue(obj);
                strBuild.AppendLine($"{propInfo.Name} = {propValue}");
            }

            return strBuild.ToString();
        }
    }
}
