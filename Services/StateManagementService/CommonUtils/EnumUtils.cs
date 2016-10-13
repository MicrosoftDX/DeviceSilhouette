// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonUtils
{
    public static class EnumUtils
    {
        /// <summary>
        /// Parse a enum and constrain the result to be a defined enum value
        /// </summary>
        /// <typeparam name="TEnum"></typeparam>
        /// <param name="value"></param>
        /// <param name="ignoreCase"></param>
        /// <returns></returns>
        public static TEnum ConstrainedParse<TEnum>(string value, bool ignoreCase = false)
            where TEnum : struct
        {
            var enumType = typeof(TEnum);
            var enumValue = (TEnum)Enum.Parse(enumType, value, ignoreCase);
            if (!Enum.IsDefined(enumType, enumValue))
            {
                throw new ArgumentException($"Requested value '{value}' does not exist in enum '{enumType.Name}'");
            }
            return enumValue;
        }

        /// <summary>
        /// Parse a enum and constrain the result to be a defined enum value
        /// </summary>
        /// <typeparam name="TEnum"></typeparam>
        /// <param name="value"></param>
        /// <param name="ignoreCase"></param>
        /// <returns></returns>
        public static bool TryConstrainedParse<TEnum>(string value, out TEnum result, bool ignoreCase = false)
            where TEnum : struct
        {
            var enumType = typeof(TEnum);
            if(!Enum.TryParse<TEnum>(value, ignoreCase, out result))
            {
                return false;
            }
            if (!Enum.IsDefined(enumType, result))
            {
                result = default(TEnum);
                return false;
            }
            return true;
        }
    }
}

