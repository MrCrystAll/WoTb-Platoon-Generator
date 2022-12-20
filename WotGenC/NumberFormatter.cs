using System;
using System.Collections.Generic;
using System.Linq;

namespace WotGenC
{

    public static class NumberFormatter
    {
        /// <summary>
        /// Format the number to the following form :
        /// 123456789 => 123_456_789
        /// </summary>
        /// <param name="number">Number to format</param>
        /// <returns>Formatted number</returns>
        public static string Format_To_Underscore(float number)
        {
            bool negative = number < 0;
            string numbString = Math.Abs(number).ToString("F0");
            List<string> separatedByComma = numbString.Split(",").ToList();
            List<string> result = separatedByComma[0].Select(digit => digit.ToString()).ToList();

            int cpt = 0;
            for (int i = result.Count - 1; i > 0; i--)
            {
                cpt++;
                if (cpt % 3 == 0)
                {
                    result.Insert(i,"_");
                }

            
            }

            if(negative)
            {
                result.Insert(0,"-"); 
            }

            //result.Add(",");
            if(separatedByComma.Count > 1)
                result.Add(separatedByComma[1]);
            
            return string.Concat(result);
        }
    }
}