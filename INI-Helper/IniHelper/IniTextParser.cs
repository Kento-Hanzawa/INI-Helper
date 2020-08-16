using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace IniHelper
{
	public sealed class IniTextParser : IDisposable
	{
		private TextReader reader;
		private string? line = null;
		private int lineCount = 0;
		private int offset = 0;
		private ILogger? logger;

		public IniTextParser(string iniText)
			: this(new StringReader(iniText), null)
		{
		}
		public IniTextParser(string iniText, ILogger? logger)
			: this(new StringReader(iniText), logger)
		{
		}
		public IniTextParser(TextReader reader)
			: this(reader, null)
		{
		}
		public IniTextParser(TextReader reader, ILogger? logger)
		{
			this.reader = reader;
			this.logger = logger;
		}

		public List<KeyValuePair<string?, KeyValuePair<string, string>>> Parse()
		{
			var result = new List<KeyValuePair<string?, KeyValuePair<string, string>>>();

			string? sectionName = null;
			while (NextLine())
			{
				var lineBreakRequired = false;
				while (offset < line.Length)
				{
					switch (PeekToken())
					{
						case IniToken.Section:
							if (lineBreakRequired)
							{
								throw CreateINIParsingException("セクション定義、パラメータ定義、またはコメントの後には改行を入れる必要があります。");
							}

							sectionName = ReadSection();
							lineBreakRequired = true;

#if DEBUG
							Console.WriteLine($"SECTION  :{sectionName}");
#endif

							continue;

						case IniToken.Comment:
							var comment = ReadComment(); // 未使用
							lineBreakRequired = true;

#if DEBUG
							Console.WriteLine($"COMMENT  :{comment}");
#endif

							continue;

						case IniToken.Parameter:
							if (lineBreakRequired)
							{
								throw CreateINIParsingException("セクション定義、パラメータ定義、またはコメントの後には改行を入れる必要があります。");
							}

							var parameter = ReadParameter();
							if (parameter == null)
							{
								continue;
							}

							result.Add(new KeyValuePair<string?, KeyValuePair<string, string>>(sectionName, new KeyValuePair<string, string>(parameter.Value.Key, parameter.Value.Value)));
							lineBreakRequired = true;

#if DEBUG
							Console.WriteLine($"PARAMETER:{{{parameter.Value.Key}, {parameter.Value.Value}}}");
#endif

							continue;

						case IniToken.None:
						default:
							SkipWhiteSpace();
							continue;
					}
				}
			}
			return result;
		}

		private bool NextLine()
		{
			line = reader.ReadLine();
			var success = line != null;
			lineCount = success ? lineCount + 1 : lineCount;
			offset = 0;
			return success;
		}

		private IniToken PeekToken()
		{
			var offset = this.offset;
			while (offset < line.Length && char.IsWhiteSpace(line[offset]))
			{
				offset++;
			}

			if (!(offset < line.Length))
			{
				return IniToken.None;
			}

			switch (line[offset])
			{
				case '[':
					return IniToken.Section;
				case ';':
				case '/' when ((offset + 1) < line.Length) && (line[offset + 1] == '/'):
					return IniToken.Comment;
				default:
					return IniToken.Parameter;
			}
		}

		private string? ReadSection()
		{
			try
			{
				SkipWhiteSpace();
				if (!(offset < line.Length))
				{
					return null;
				}

				int begin, end;
				var commentIndex = PeekCommentAppearIndex(offset);
				if ((end = line.LastIndexOf(']', commentIndex < line.Length ? commentIndex : line.Length - 1)) == -1)
				{
					throw CreateINIParsingException("セクション定義行に ']' が存在しません。");
				}

				begin = offset + 1; // Skip '['
				offset = end + 1;   // Skip ']'
				return line[begin..end].Trim();
			}
			catch (Exception ex)
			{
				throw CreateINIParsingException("セクション定義行の読み取り中にエラーが発生しました。", ex);
			}
		}

		private KeyValuePair<string, string>? ReadParameter()
		{
			try
			{
				SkipWhiteSpace();
				if (!(offset < line.Length))
				{
					return null;
				}

				var separatorIndex = line.IndexOf('=');
				if (separatorIndex == -1)
				{
					logger?.LogWarning(CreateLogMessage("パラメータ定義行から '=' を見つけることが出来ませんでした。操作を次のコメントか次の行までスキップします。"));
					var commentIndex = PeekCommentAppearIndex(offset);
					offset = (commentIndex > -1) ? commentIndex : line.Length;
					return null;
				}

				string key = line[offset..separatorIndex].Trim();
				offset = separatorIndex + 1;
				SkipWhiteSpace();
				if (!(offset < line.Length)) // Valueが空
				{
					return new KeyValuePair<string, string>(key, string.Empty);
				}

				var isInSq = false;
				var isInDq = false;
				var valueBuilder = new StringBuilder();
				do
				{
					while (offset < line.Length)
					{
						switch (line[offset])
						{
							// コメントの出現
							case ';' when !isInSq && !isInDq:
							case '/' when !isInSq && !isInDq && ((offset + 1) < line.Length) && (line[offset + 1] == '/'):
								return new KeyValuePair<string, string>(key, valueBuilder.ToString().Trim());

							// エスケープ
							case '\\' when isInSq && ((offset + 1) < line.Length) && (line[offset + 1] == '\''):
								valueBuilder.Append('\'');
								offset += 2;
								continue;
							case '\\' when isInDq && ((offset + 1) < line.Length) && (line[offset + 1] == '"'):
								valueBuilder.Append('"');
								offset += 2;
								continue;

							case '"' when isInSq:
								valueBuilder.Append(line[offset++]);
								continue;
							case '"' when !isInSq && isInDq:
								isInDq = false;
								offset++;
								continue;
							case '"' when !isInSq && !isInDq:
								isInDq = true;
								offset++;
								continue;

							case '\'' when isInDq:
								valueBuilder.Append(line[offset++]);
								continue;
							case '\'' when isInSq && !isInDq:
								isInSq = false;
								offset++;
								continue;
							case '\'' when !isInSq && !isInDq:
								isInSq = true;
								offset++;
								continue;

							default:
								valueBuilder.Append(line[offset++]);
								continue;
						}
					}

					if (!isInSq && !isInDq)
					{
						return new KeyValuePair<string, string>(key, valueBuilder.ToString().Trim());
					}

					if (isInSq)
					{
						throw CreateINIParsingException("シングルクォーテーションが閉じられていません。");
					}

					if (isInDq)
					{
						if (NextLine())
						{
							valueBuilder.AppendLine();
						}
						else
						{
							throw CreateINIParsingException("ダブルクォーテーションが閉じられていません。");
						}
					}
				} while (true);
			}
			catch (Exception ex)
			{
				throw CreateINIParsingException("パラメータ定義行の読み取り中にエラーが発生しました。", ex);
			}
		}

		private string ReadComment()
		{
			try
			{
				SkipWhiteSpace();
				if (!(offset < line.Length))
				{
					return string.Empty;
				}

				switch (line[offset])
				{
					case ';':
					case '/' when ((offset + 1) < line.Length) && (line[offset + 1] == '/'):
						var comment = line[offset..].Trim();
						offset = line.Length;
						return comment;
					default:
						return string.Empty;
				}
			}
			catch (Exception ex)
			{
				throw CreateINIParsingException("コメントテキストの読み取り中にエラーが発生しました。", ex);
			}
		}

		private int PeekCommentAppearIndex()
		{
			return PeekCommentAppearIndex(0);
		}
		private int PeekCommentAppearIndex(int startIndex)
		{
			try
			{
				var semicolon = line.IndexOf(';', startIndex);
				var slash = line.IndexOf("//", startIndex);
				return Math.Min((semicolon > -1) ? semicolon : line.Length, (slash > -1) ? slash : line.Length);
			}
			catch (Exception ex)
			{
				throw CreateINIParsingException("空白文字のスキップ操作中にエラーが発生しました。", ex);
			}
		}

		private void SkipWhiteSpace()
		{
			try
			{
				while ((offset < line.Length) && char.IsWhiteSpace(line[offset]))
				{
					offset++;
				}
			}
			catch (Exception ex)
			{
				throw CreateINIParsingException("空白文字のスキップ操作中にエラーが発生しました。", ex);
			}
		}

		private string CreateLogMessage(in string? message)
		{
			const string format = "{0}\n    読み取り行: {1} ({2}行目 {3})";
			return string.Format(format, message, line, lineCount, offset);
		}

		private IniParsingException CreateINIParsingException(in string? message)
		{
			return new IniParsingException(CreateLogMessage(message), line, lineCount, offset);
		}

		private IniParsingException CreateINIParsingException(in string? message, Exception innerException)
		{
			return new IniParsingException(CreateLogMessage(message), line, lineCount, offset, innerException);
		}

		public void Dispose()
		{
			reader.Dispose();
			line = null;
			lineCount = 0;
			offset = 0;
			logger = null;
		}
	}
}
