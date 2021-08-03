using UnityEngine;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;


namespace ParallaxConfigurator
{
    public static class Parsers
    {
        private const string ColorStringPattern = @"(\d\.\d*)[^\d]*(\d\.\d*)[^\d]*(\d\.\d*)[^\d]*(\d\.\d*)";
        
        /// <summary>
        /// Forces to parse to float by cleaning string if necessary
        /// </summary>
        public static float ForceParseFloat(this string str)
        {
            // try parse
            if (float.TryParse(str, out float value))
                return value;
   
            // Clean string if it could not be parsed
            bool recordedDecimalPoint = false;
            var charList = new List<char>(str);
            for (int cnt = 0; cnt < charList.Count; cnt++)
            {
                UnicodeCategory type = CharUnicodeInfo.GetUnicodeCategory(str[cnt]);
                if (type != UnicodeCategory.DecimalDigitNumber)
                {
                    charList.RemoveRange(cnt, charList.Count-cnt);
                    break;
                }

                if (str[cnt] != '.') continue;
                
                if (recordedDecimalPoint)
                {
                    charList.RemoveRange(cnt, charList.Count-cnt);
                    break;
                }
                recordedDecimalPoint = true;
            }
   
            // Parse again
            if (charList.Count == 0)
                return 0;
            str = new string (charList.ToArray());
            if (!float.TryParse(str, out value))
                Debug.LogError("Could not parse " + str);
            
            return value;
        }

        public static Color ForceParseColor(this string str, Color defaultValue)
        {
            Color newValue;
            
            if(ColorUtility.TryParseHtmlString(str, out newValue))
                return newValue;

            Regex r = new Regex(ColorStringPattern);
            Match m = r.Match(str);
            
            if (!m.Success)
                return defaultValue;


            float[] col = {0, 0, 0, 0};
            for (int i = 0; i < 4; i++)
            {
                // groups[0] is the entire match, groups[1] is the first group 
                col[i] = Mathf.Clamp01(m.Groups[i+1].ToString().ForceParseFloat());
            }

            return new Color(col[0], col[1], col[2], col[3]);
        }
    }
}