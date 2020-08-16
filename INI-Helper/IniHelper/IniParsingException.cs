using System;

namespace IniHelper
{
	public class IniParsingException : Exception
	{
		public string? Line { get; }
		public int LineCount { get; }
		public int Offset { get; }

		public IniParsingException(in string message, in string? line, in int lineCount, in int offset)
			: base(message)
		{
			this.Line = line;
			this.LineCount = lineCount;
			this.Offset = offset;
		}

		public IniParsingException(in string message, in string? line, in int lineCount, in int offset, in Exception innerException)
			: base(message, innerException)
		{
			this.Line = line;
			this.LineCount = lineCount;
			this.Offset = offset;
		}
	}
}
