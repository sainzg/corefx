// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Specialized;
using System.Globalization;

namespace System.Configuration
{
    /// <summary>
    ///     The AppSettingsReader class provides a wrapper for System.Configuration.ConfigurationManager.AppSettings
    ///     which provides a single method for reading values from the config file of a particular type.
    /// </summary>
   public class AppSettingsReader
    {
        private NameValueCollection _map;
        private static Type s_stringType = typeof(string);
        private static Type[] _paramsArray = new Type[] { s_stringType };

        public AppSettingsReader() => _map = ConfigurationManager.AppSettings;

        /// <summary>
        /// Gets the value for specified key from ConfigurationManager.AppSettings, and returns
        /// an object of the specified type containing the value from the config file.  If the key
        /// isn't in the config file, or if it is not a valid value for the given type, it will 
        /// throw an exception with a descriptive message so the user can make the appropriate
        /// change
        /// </summary>
        public object GetValue(string key, Type type)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (_map[key] == null) throw new InvalidOperationException(string.Format(SR.AppSettingsReaderNoKey, key));

            string val = _map[key];           
            if (type == s_stringType)
            {
                int NoneNesting = GetNoneNesting(val);
                return NoneNesting == 0 ? val : NoneNesting == 1 ? null : val.Substring(1, val.Length - 2);
            }

            try
            {
                return Convert.ChangeType(val, type, CultureInfo.InvariantCulture);
            }
            catch (Exception)
            {
                throw new InvalidOperationException(string.Format(SR.AppSettingsReaderCantParse, (val.Length == 0) ? SR.AppSettingsReaderEmptyString : val, key, type.ToString()));
            }
        }

        private int GetNoneNesting(string val)
        {
            if (string.IsNullOrWhiteSpace(val)) return 0;
            int len = val.Length;
            int count = 0;
            while (val[count] == '(' && val[len - count - 1] == ')') count++;
            return (count > 0 && string.Compare("None", 0, val, count, len - 2 * count, StringComparison.Ordinal) != 0) ? 0 : count;
        }
    }
}
