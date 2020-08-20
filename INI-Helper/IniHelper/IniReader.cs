using System;
using System.Collections.Generic;
using System.Text;

namespace IniHelper
{
    // https://www.php.net/manual/ja/function.parse-ini-file.php
    // https://www.php.net/manual/ja/function.parse-ini-string.php

    public sealed class IniReader
    {
        private readonly string text;
        private int offset;
        private int lineCount;

        private bool isRequireNewLine = false;

        public IniReader(string text)
        {
            this.text = text;
        }

        public bool IsReadEnd
        {
            get { return !(offset < text.Length); }
        }

        public IniToken PeekToken()
        {
            // Skip white space.
            var offset = this.offset;
            while (offset < text.Length && char.IsWhiteSpace(text[offset]))
            {
                offset++;
            }

            if (!(offset < text.Length))
            {
                return IniToken.None;
            }
            switch (text[offset])
            {
                case '[':
                    return IniToken.Section;
                case ';':
                case '/' when ((offset + 1) < text.Length) && (text[offset + 1] == '/'):
                    return IniToken.Comment;
                default:
                    return IniToken.Parameter;
            }
        }

        private int? PeekNextLineAppearIndex()
        {
            if (IsReadEnd)
            {
                return null;
            }

            for (var i = offset; i < text.Length; i++)
            {
                switch (text[i])
                {
                    case '\r':
                    case '\n':
                        return i;
                }
            }
            return null;
        }

        private int? PeekCommentAppearIndex()
        {
            if (IsReadEnd)
            {
                return null;
            }

            for (var i = offset; i < text.Length; i++)
            {
                switch (text[i])
                {
                    case '\r':
                    case '\n':
                        return null;
                    case ';':
                    case '/' when ((i + 1) < text.Length) && (text[i + 1] == '/'):
                        return i;
                }
            }
            return null;
        }

        private int? PeekParamSeparatorAppearIndex()
        {
            if (IsReadEnd)
            {
                return null;
            }

            for (var i = offset; i < text.Length; i++)
            {
                switch (text[i])
                {
                    case '\r':
                    case '\n':
                        return null;
                    case '=':
                        return i;
                }
            }
            return null;
        }

        public string? ReadSection()
        {
            SkipWhiteSpace();
            if (IsReadEnd)
            {
                return null;
            }

            if (isRequireNewLine)
            {
                throw CreateIniParsingException("同じ行に複数のセクション、またはパラメータが含まれています。");
            }

            int end;
            var startIndex = (PeekCommentAppearIndex() ?? PeekNextLineAppearIndex() ?? text.Length) - 1;
            if ((end = text.LastIndexOf(']', startIndex, startIndex - offset)) == -1)
            {
                throw CreateIniParsingException("セクション定義行に ']' が存在しません。");
            }

            isRequireNewLine = true;
            var start = offset + 1; // Skip '['
            offset = end + 1;       // Skip ']'
            return text[start..end].Trim();
        }

        public KeyValuePair<string, string>? ReadParameter()
        {
            SkipWhiteSpace();
            if (IsReadEnd)
            {
                return null;
            }

            var separatorIndex = PeekParamSeparatorAppearIndex()
                ?? throw CreateIniParsingException("パラメータ定義行に '=' が存在しません。");
            isRequireNewLine = true;

            string key = text[offset..separatorIndex].Trim();
            offset = separatorIndex + 1; // Skip '='

            SkipWhiteSpace();
            if (IsReadEnd || !isRequireNewLine) // Valueが空
            {
                return new KeyValuePair<string, string>(key, string.Empty);
            }

            var isInSq = false;
            var isInDq = false;
            var builder = new StringBuilder();
            while (offset < text.Length)
            {
                switch (text[offset])
                {
                    // 改行の出現
                    case '\r' when !isInSq && !isInDq:
                    case '\n' when !isInSq && !isInDq:
                        return new KeyValuePair<string, string>(key, builder.ToString().Trim());

                    // エスケープされた改行
                    case '\r' when isInDq && ((offset + 1) < text.Length) && (text[offset + 1] == '\n'):
                        offset += 2;
                        builder.AppendLine();
                        continue;
                    case '\r' when isInDq:
                    case '\n' when isInDq:
                        offset += 1;
                        builder.AppendLine();
                        continue;

                    // コメントの出現
                    case ';' when !isInSq && !isInDq:
                    case '/' when !isInSq && !isInDq && ((offset + 1) < text.Length) && (text[offset + 1] == '/'):
                        return new KeyValuePair<string, string>(key, builder.ToString().Trim());

                    // シングルクォート及びダブルクォートのエスケープ処理
                    case '\\' when isInSq && ((offset + 1) < text.Length) && (text[offset + 1] == '\''):
                        builder.Append('\'');
                        offset += 2;
                        continue;
                    case '\\' when isInDq && ((offset + 1) < text.Length) && (text[offset + 1] == '"'):
                        builder.Append('"');
                        offset += 2;
                        continue;

                    // ダブルクォート
                    case '"' when isInSq:
                        builder.Append(text[offset++]);
                        continue;
                    case '"' when !isInSq && isInDq:
                        isInDq = false;
                        offset++;
                        continue;
                    case '"' when !isInSq && !isInDq:
                        isInDq = true;
                        offset++;
                        continue;

                    // シングルクォート
                    case '\'' when isInDq:
                        builder.Append(text[offset++]);
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
                        builder.Append(text[offset++]);
                        continue;
                }
            }

            if (isInSq)
            {
                throw CreateIniParsingException("シングルクォーテーションが閉じられていません。");
            }
            if (isInDq)
            {
                throw CreateIniParsingException("ダブルクォーテーションが閉じられていません。");
            }

            return new KeyValuePair<string, string>(key, builder.ToString().Trim());
        }

        public string? ReadComment()
        {
            SkipWhiteSpace();
            if (IsReadEnd)
            {
                return null;
            }

            switch (text[offset])
            {
                case ';':
                case '/' when ((offset + 1) < text.Length) && (text[offset + 1] == '/'):
                    return ReadLine()?.Trim();
                default:
                    return null;
            }
        }

        public void SkipWhiteSpace()
        {
            while (offset < text.Length && char.IsWhiteSpace(text[offset]))
            {
                if (text[offset] == '\r' || text[offset] == '\n')
                {
                    isRequireNewLine = false;
                }
                offset++;
            }
        }

        private string? ReadLine()
        {
            if (IsReadEnd)
            {
                return null;
            }

            int start = offset, end = offset;
            while (end < text.Length)
            {
                switch (text[end])
                {
                    case '\r' when ((end + 1) < text.Length) && (text[end + 1] == '\n'):
                        isRequireNewLine = false;
                        offset = end + 2;
                        return text[start..end];
                    case '\r':
                    case '\n':
                        isRequireNewLine = false;
                        offset = end + 1;
                        return text[start..end];
                }
                end++;
            }

            isRequireNewLine = false;
            offset = end;
            return text[start..end];
        }

        private IniParsingException CreateIniParsingException(in string? message, Exception? innerException = null)
        {
            var lineCount = 1;
            for (var i = 0; i < offset; i++)
            {
                switch (text[i])
                {
                    case '\r' when ((i + 1) < text.Length) && (text[i + 1] == '\n'):
                        i++;
                        lineCount++;
                        continue;
                    case '\r':
                    case '\n':
                        lineCount++;
                        continue;
                }
            }
            return new IniParsingException($"{message} ({lineCount}行目)", lineCount, innerException);
        }
    }
}
