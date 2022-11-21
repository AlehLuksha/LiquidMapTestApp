using DotLiquid.Util;
using System;
using System.Collections;
using System.Linq;

namespace LiquidMapTestApp
{
    /// <summary>Additional custom filters</summary>
    public static class CustomFilters
    {
        /// <summary>
        /// Formats the numbers.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="format">The format.</param>
        /// <returns>A string.</returns>
        public static string Format(object input, string format)
        {
            if (input == null)
                return null;
            else if (string.IsNullOrWhiteSpace(format))
                return input.ToString();

            var result = string.Format("{0:" + format + "}", input);
            return result;
        }

        /// <summary>
        /// Creates an array including only the objects with an empty property value.
        /// </summary>
        /// <param name="input">An array to be filtered</param>
        /// <param name="propertyName">The name of the property to filter by</param>
        public static IEnumerable WhereIsNull(IEnumerable input, string propertyName)
        {
            if (input == null)
                return null;

            if (string.IsNullOrWhiteSpace(propertyName))
                throw new ArgumentNullException(paramName: nameof(propertyName), message: $"'{nameof(propertyName)}' cannot be null or empty.");

            return input.Cast<object>().Where(source => source.HasNullValue(propertyName));
        }

        /// <summary>
        /// Have the null value.
        /// </summary>
        /// <param name="any">The any.</param>
        /// <param name="propertyName">The property name.</param>
        /// <returns>A bool.</returns>
        private static bool HasNullValue(this object any, string propertyName)
        {
            // Check if the 'any' object has a propertyName
            object propertyValue = null;
            if (any is IDictionary dictionary && dictionary.Contains(key: propertyName))
            {
                propertyValue = dictionary[propertyName];
            }
            else if (any != null && any.RespondTo(propertyName))
            {
                propertyValue = any.Send(propertyName);
            }

            return propertyValue == null;
        }
    }
}
