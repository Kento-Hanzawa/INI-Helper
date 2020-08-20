using System;

namespace IniHelper
{
    public class IniParsingException : Exception
    {
        public int LineCount { get; }

        public IniParsingException()
        {
        }

        public IniParsingException(string? message)
            : base(message)
        {
        }

        public IniParsingException(string? message, Exception? innerException)
            : base(message, innerException)
        {
        }

        public IniParsingException(in string? message, in int lineCount)
            : base(message)
        {
            this.LineCount = lineCount;
        }

        public IniParsingException(in string? message, in int lineCount, in Exception? innerException)
            : base(message, innerException)
        {
            this.LineCount = lineCount;
        }
    }
}
