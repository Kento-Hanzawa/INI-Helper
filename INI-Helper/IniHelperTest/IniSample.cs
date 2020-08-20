using System;
using System.Collections.Generic;
using System.Text;

namespace IniHelperTest
{
    public static class IniSample
    {
        public static readonly string Text = @"
; Global Parameter
g_key1 = g_value1
g_key2 = g_value2
g_key3 = g_value3
g_key4 = g_value4

; CommentTest1
[CommentTest1]  ; Comment1
key1 = value1   ; Comment2
key2 = value2   ; Comment3
key3 = value3   ; Comment4
key4 = value4   ; Comment5

// CommentTest2
[CommentTest2]  // Comment1
key1 = value1   // Comment2
key2 = value2   // Comment3
key3 = value3   // Comment4
key4 = value4   // Comment5

; Section1
[Section1]
; aaa
key1 = 'aaa'
; aaa
key2 = ""aaa""
; a""a
key3 = 'a""a'
; a'a
key4 = ""a'a""

; Section2
[Section2]
; aaabbb
key1 = 'aaa'bbb
; aaabbb
key2 = aaa'bbb'
; aaabbb
key3 = ""aaa""bbb
; aaabbb
key4 = aaa""bbb""
; aaabbb
key5 = 'aaa'""bbb""
; aaabbb
key6 = ""aaa""'bbb'
; aaabbb
key7 = 'aaa''bbb'
; aaabbb
key8 = ""aaa""""bbb""

; Section3
[Section3]
; aaacccbbb
key1 = 'aaa'ccc""bbb""
; aaacccbbb
key2 = ""aaa""ccc'bbb'
; aaacccbbb
key3 = 'aaa'ccc'bbb'
; aaacccbbb
key4 = ""aaa""ccc""bbb""

; Section4
[Section4]
; aaa
; bbb
key1 = ""aaa
bbb""
; aaaccc
; bbb
key2 = ""aaa""ccc""
bbb""
; aaa
; cccbbb
key3 = ""aaa
""ccc""bbb""
; aaa
; ccc
; bbb
key4 = ""aaa
""ccc""
bbb""
";
    }
}
