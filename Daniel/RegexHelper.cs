using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Daniel
{
    class RegexHelper
    {
        public static string ReplaceIn(string input, string substring, string search, string replace)
        {
            int shift = 0;
            string result = input;
            MatchCollection col = Regex.Matches(input, substring);
            for (int i = 0; i < col.Count; i++)
            {
                Match mt = col[i];
                string replacedStr = Regex.Replace(mt.Value, search, replace);//input.Remove(mt.Index, mt.Length);
                result = result
                    .Remove(mt.Index + shift, mt.Length)
                    .Insert(mt.Index + shift, replacedStr);
                shift += replacedStr.Length - mt.Length;
            }
            return result;
        }
        public static string[] ReplaceIn(string[] input, string search, string replace)
        {
            for (int i = 0; i < input.Length; i++)
                input[i] = Regex.Replace(input[i], search, replace);
            return input;
        }
    }
}
