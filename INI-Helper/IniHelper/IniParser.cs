﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;

namespace IniHelper
{
    public static class IniParser
    {
        public static KeyValuePair<string?, KeyValuePair<string, string>>[] Parse(string text)
        {
            return Parse(new IniReader(text));
        }

        public static KeyValuePair<string?, KeyValuePair<string, string>>[] Parse(IniReader reader)
        {
            var result = new List<KeyValuePair<string?, KeyValuePair<string, string>>>();

            string? sectionName = null;
            while (!reader.IsReadEnd)
            {
                switch (reader.PeekToken())
                {
                    case IniToken.Section:
                        sectionName = reader.ReadSection();
#if DEBUG
                        Console.WriteLine($"SECTION:{sectionName}");
#endif
                        continue;

                    case IniToken.Comment:
                        var comment = reader.ReadComment(); // 未使用
#if DEBUG
                        Console.WriteLine($"COMMENT:{comment}");
#endif
                        continue;

                    case IniToken.Parameter:
                        var parameter = reader.ReadParameter();
                        result.Add(new KeyValuePair<string?, KeyValuePair<string, string>>(sectionName, new KeyValuePair<string, string>(parameter.Value.Key, parameter.Value.Value)));
#if DEBUG
                        Console.WriteLine($"PARAMETER:{{{parameter.Value.Key}, {parameter.Value.Value}}}");
#endif
                        continue;

                    case IniToken.None:
                    default:
                        reader.SkipWhiteSpace();
                        continue;
                }
            }

            return result.ToArray();
        }
    }
}
