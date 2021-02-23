using System;
using System.Collections.Generic;
using System.Linq;

namespace Common.Tools
{
    public class Tools
    {
        public static string AnalysisCode(Dictionary<string, string> dic, string pattern)
        {
            var res = string.Empty;
            if (string.IsNullOrEmpty(pattern)) return string.Empty;
            var rules = pattern.Split(new[] {'}', '{'}, StringSplitOptions.RemoveEmptyEntries).ToList();
            foreach (var rule in rules)
            {
                if (dic.ContainsKey(rule))
                {
                    res += dic[rule];
                }
                else if (rule.Contains(":"))
                {
                    var kv = rule.Split(':');
                    if (kv.Length == 2)
                    {
                        switch (kv[0])
                        {
                            case "date":
                                res += DateTime.Now.ToString(kv[1]);
                                break;
                        }
                    }else if (kv.Length == 3)
                    {
                        if (dic.ContainsKey(kv[0]))
                        {
                            if (int.TryParse(kv[1], out var length))
                            {
                                res += dic[kv[0]].PadLeft(length, kv[2][0]);
                            }
                        }
                    }
                }
                else
                {
                    res += rule;
                }
            }

            return res;
        }

        public static void DictionaryAdd(Dictionary<string, string> dic, Dictionary<string, string> add)
        {
            foreach (var kv in add)
            {
                if (dic.ContainsKey(kv.Key)) dic[kv.Key] = kv.Value;
                else dic.Add(kv.Key,kv.Value);
            }
        }
    }
}