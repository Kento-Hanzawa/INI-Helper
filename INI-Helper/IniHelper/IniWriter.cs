using System;
using System.Collections.Generic;
using System.Text;

namespace IniHelper
{
    // https://www.php.net/manual/ja/function.parse-ini-file.php
    // https://www.php.net/manual/ja/function.parse-ini-string.php

    public sealed class IniWriter
    {
        private readonly StringBuilder builder = new StringBuilder();

        public void WriteSection(string sectionName)
        {
            builder.AppendLine("[" + sectionName + "]");
        }

        public void WriteParameter(string key, string value)
        {
            builder.AppendLine(key + " = " + value);
        }

        public void WriteParameter(KeyValuePair<string, string> kvPair)
        {
            WriteParameter(kvPair.Key, kvPair.Value);
        }

        public void WriteComment(string? comment)
        {
            if (!string.IsNullOrWhiteSpace(comment))
            {
                builder.AppendLine("; " + comment);
            }
        }

        public void WriteEmptyLine()
        {
            builder.AppendLine();
        }

        public override string ToString()
        {
            return builder.ToString();
        }
    }
}
