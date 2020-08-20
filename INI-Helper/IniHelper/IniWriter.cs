using IniHelper.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IniHelper
{
    // https://www.php.net/manual/ja/function.parse-ini-file.php
    // https://www.php.net/manual/ja/function.parse-ini-string.php

    public sealed class IniWriter
    {
        private static readonly char[] InvalidCharacters =
        {
            '?', '{', '}', '|', '&', '~', '!', '(', ')', '^', '"', '\r', '\n'
        };

        private static readonly string[] InvalidWords =
        {
            //"null", "yes", "no", "true", "false", "on", "off" , "none",
        };

        private readonly StringBuilder builder = new StringBuilder();

        public void WriteSection(string sectionName)
        {
            for (var i = 0; i < InvalidCharacters.Length; i++)
            {
                if (sectionName.Contains(InvalidCharacters[i]))
                {
                    throw new ArgumentException($"セクション名に '{InvalidCharacters[i]}' を含めることはできません。");
                }
            }
            for (var i = 0; i < InvalidWords.Length; i++)
            {
                if (sectionName.Contains(InvalidWords[i]))
                {
                    throw new ArgumentException($"セクション名に '{InvalidWords[i]}' を含めることはできません。");
                }
            }
            builder.AppendLine("[" + sectionName + "]");
        }

        public void WriteParameter(string key, string value)
        {
            for (var i = 0; i < InvalidCharacters.Length; i++)
            {
                if (key.Contains(InvalidCharacters[i]))
                {
                    throw new ArgumentException($"パラメーターのキーに '{InvalidCharacters[i]}' を含めることはできません。");
                }
            }
            for (var i = 0; i < InvalidWords.Length; i++)
            {
                if (key.Contains(InvalidWords[i]))
                {
                    throw new ArgumentException($"パラメーターのキーに '{InvalidWords[i]}' を含めることはできません。");
                }
            }
            builder.AppendLine(key + " = " + EscapeParameterValue(value));
        }

        public void WriteParameter(KeyValuePair<string, string> kvPair)
        {
            WriteParameter(kvPair.Key, kvPair.Value);
        }

        public void WriteComment(string? comment)
        {
            if (string.IsNullOrWhiteSpace(comment))
            {
                return;
            }

            foreach (var line in comment.ToLines())
            {
                builder.AppendLine("; " + line);
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

        private static string EscapeParameterValue(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return "''";
            }

            var result = new StringBuilder(str.Length + 64);
            var buffer = new StringBuilder(64);
            bool isNecessaryEscapeDq = false;
            bool isNecessaryEscapeSq = false;
            foreach (char ch in str)
            {
                switch (ch)
                {
                    case '"':
                        if (isNecessaryEscapeDq)
                        {
                            result.Append('"' + buffer.ToString() + '"');
                            buffer.Clear();
                            isNecessaryEscapeDq = false;
                        }
                        buffer.Append(ch);
                        isNecessaryEscapeSq = true;
                        break;

                    case '\'':
                    case '\r':
                    case '\n':
                        if (isNecessaryEscapeSq)
                        {
                            result.Append('\'' + buffer.ToString() + '\'');
                            buffer.Clear();
                            isNecessaryEscapeSq = false;
                        }
                        buffer.Append(ch);
                        isNecessaryEscapeDq = true;
                        break;

                    default:
                        buffer.Append(ch);
                        break;
                }
            }

            if (buffer.Length > 0)
            {
                string s = buffer.ToString();
                if (isNecessaryEscapeDq)
                {
                    result.Append('"' + s + '"');
                }
                else if (isNecessaryEscapeSq)
                {
                    result.Append('\'' + s + '\'');
                }
                else
                {
                    // 数値のみであれば " とか ' を付けない。
                    if (s.All(char.IsDigit))
                    {
                        result.Append(buffer.ToString());
                    }
                    else
                    {
                        result.Append('"' + s + '"');
                    }
                }
            }

            return result.ToString();
        }
    }
}
