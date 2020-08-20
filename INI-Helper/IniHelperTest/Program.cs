using System;
using System.Collections.Generic;
using System.IO;
using IniHelper;

namespace IniHelperTest
{
    class Program
    {
        private static readonly List<KeyValuePair<string?, KeyValuePair<string, string>>> Answer
            = new List<KeyValuePair<string?, KeyValuePair<string, string>>>()
        {
			// Global Parameter
			new KeyValuePair<string?, KeyValuePair<string, string>>(null, new KeyValuePair<string, string>("g_key1" , "g_value1")),
            new KeyValuePair<string?, KeyValuePair<string, string>>(null, new KeyValuePair<string, string>("g_key2" , "g_value2")),
            new KeyValuePair<string?, KeyValuePair<string, string>>(null, new KeyValuePair<string, string>("g_key3" , "g_value3")),
            new KeyValuePair<string?, KeyValuePair<string, string>>(null, new KeyValuePair<string, string>("g_key4" , "g_value4")),

			// CommentTest1
			new KeyValuePair<string?, KeyValuePair<string, string>>("CommentTest1", new KeyValuePair<string, string>("key1" , "value1")),
            new KeyValuePair<string?, KeyValuePair<string, string>>("CommentTest1", new KeyValuePair<string, string>("key2" , "value2")),
            new KeyValuePair<string?, KeyValuePair<string, string>>("CommentTest1", new KeyValuePair<string, string>("key3" , "value3")),
            new KeyValuePair<string?, KeyValuePair<string, string>>("CommentTest1", new KeyValuePair<string, string>("key4" , "value4")),

			// CommentTest2
			new KeyValuePair<string?, KeyValuePair<string, string>>("CommentTest2", new KeyValuePair<string, string>("key1" , "value1")),
            new KeyValuePair<string?, KeyValuePair<string, string>>("CommentTest2", new KeyValuePair<string, string>("key2" , "value2")),
            new KeyValuePair<string?, KeyValuePair<string, string>>("CommentTest2", new KeyValuePair<string, string>("key3" , "value3")),
            new KeyValuePair<string?, KeyValuePair<string, string>>("CommentTest2", new KeyValuePair<string, string>("key4" , "value4")),

			// Section1
			new KeyValuePair<string?, KeyValuePair<string, string>>("Section1", new KeyValuePair<string, string>("key1" , "aaa")),
            new KeyValuePair<string?, KeyValuePair<string, string>>("Section1", new KeyValuePair<string, string>("key2" , "aaa")),
            new KeyValuePair<string?, KeyValuePair<string, string>>("Section1", new KeyValuePair<string, string>("key3" , "a\"a")),
            new KeyValuePair<string?, KeyValuePair<string, string>>("Section1", new KeyValuePair<string, string>("key4" , "a'a")),

			// Section2
			new KeyValuePair<string?, KeyValuePair<string, string>>("Section2", new KeyValuePair<string, string>("key1" , "aaabbb")),
            new KeyValuePair<string?, KeyValuePair<string, string>>("Section2", new KeyValuePair<string, string>("key2" , "aaabbb")),
            new KeyValuePair<string?, KeyValuePair<string, string>>("Section2", new KeyValuePair<string, string>("key3" , "aaabbb")),
            new KeyValuePair<string?, KeyValuePair<string, string>>("Section2", new KeyValuePair<string, string>("key4" , "aaabbb")),
            new KeyValuePair<string?, KeyValuePair<string, string>>("Section2", new KeyValuePair<string, string>("key5" , "aaabbb")),
            new KeyValuePair<string?, KeyValuePair<string, string>>("Section2", new KeyValuePair<string, string>("key6" , "aaabbb")),
            new KeyValuePair<string?, KeyValuePair<string, string>>("Section2", new KeyValuePair<string, string>("key7" , "aaabbb")),
            new KeyValuePair<string?, KeyValuePair<string, string>>("Section2", new KeyValuePair<string, string>("key8" , "aaabbb")),

			// Section3
			new KeyValuePair<string?, KeyValuePair<string, string>>("Section3", new KeyValuePair<string, string>("key1" , "aaacccbbb")),
            new KeyValuePair<string?, KeyValuePair<string, string>>("Section3", new KeyValuePair<string, string>("key2" , "aaacccbbb")),
            new KeyValuePair<string?, KeyValuePair<string, string>>("Section3", new KeyValuePair<string, string>("key3" , "aaacccbbb")),
            new KeyValuePair<string?, KeyValuePair<string, string>>("Section3", new KeyValuePair<string, string>("key4" , "aaacccbbb")),

			// Section4
			new KeyValuePair<string?, KeyValuePair<string, string>>("Section4", new KeyValuePair<string, string>("key1" , "aaa\r\nbbb")),
            new KeyValuePair<string?, KeyValuePair<string, string>>("Section4", new KeyValuePair<string, string>("key2" , "aaaccc\r\nbbb")),
            new KeyValuePair<string?, KeyValuePair<string, string>>("Section4", new KeyValuePair<string, string>("key3" , "aaa\r\ncccbbb")),
            new KeyValuePair<string?, KeyValuePair<string, string>>("Section4", new KeyValuePair<string, string>("key4" , "aaa\r\nccc\r\nbbb")),
        };

        static void Main(string[] args)
        {
            Console.WriteLine(Check(IniSample.Text));
        }

        static bool Check(string ini)
        {
            var result = IniParser.Parse(ini);
            if (Answer.Count != result.Length)
            {
                return false;
            }

            for (var i = 0; i < result.Length; i++)
            {
                if ((Answer[i].Key != result[i].Key)
                || (Answer[i].Value.Key != result[i].Value.Key)
                || (Answer[i].Value.Value != result[i].Value.Value))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
