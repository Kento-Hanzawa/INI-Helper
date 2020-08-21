# INI-Helper
INIファイルを扱うための単純なユーティリティを提供します。

Install
---
このライブラリは.NET Standard 2.1で利用可能な標準ライブラリとしてNuGetで提供されております。
```
Install-Package IniHelper
```

Usage
---
```csharp
// すべてのAPIは 'IniHelper' 名前空間下に配置されています。
using IniHelper;

var iniText = @"
globalKey1 = 100
globalKey2 = value

[section1]
key1 = value
key2 = 100

[section2]
key1 = value
key2 = 100
";

// IniPaeserを使用することで、任意のINIテキストを解析することが可能です。
KeyValuePair<string?, KeyValuePair<string, string>>[] parsed = IniParser.Parse(iniText);

foreach(KeyValuePair<string?, KeyValuePair<string, string>> data in parsed)
{
    // 1階層目のKeyValuePairにはセクション名とパラメータが格納されます。
    // Keyがnullの場合はセクションに属していないグローバルパラメータであることを意味します。
    string? sectionName = data.Key;
    KeyValuePair<string, string> parameter = data.Value;
    
    // 2階層目のKeyValuePairはそのままパラメータのキーと値が格納されます。
    string paramKey = parameter.Key;
    string paramValue = parameter.Value;
}
```

Supported INI Format
---
1. セクションに属していないパラメータ
```ini
key1 = 100
key2 = 200

[FirstSection]
key1 = 100
︙
```

2. 行の途中から始まるコメント
```ini
[section] ; これはセクションです。
key = 100 ; これはパラメータです。
```

3. "//"を使用したコメント
```ini
// これもコメントとして扱う。
[section] // これはセクションです。
key = 100 // これはパラメータです。
```

4. ダブルクォーテーションを使用した、パラメータの改行を含む値
```ini
key = "このパラメータは
改行を含みます"
```

Parameter Escaping
---
IniPaeserを使用する上で、以下の決まりを守ってください。
いずれかの項目に違反している場合、予期しない挙動や例外が発生する可能性があります。

1. シングルクォーテーションまたは改行を含む値は、必ずダブルクォーテーションでエスケープされている必要がある
```ini
; Good!
key1 = "aaa' bbb' ccc"
key2 = "改行
している"

; Bad!
key1 = aaa' bbb' ccc
key2 = 改行
している
```

2. ダブルクォーテーションを含む値は、必ずシングルクォーテーションでエスケープされている必要がある
```ini
; Good!
key1 = 'aaa" bbb" ccc'

; Bad!
key1 = aaa" bbb" ccc
```

3. コメントトークン（; or //）を含む値は、必ずシングルクォーテーションあるいはダブルクォーテーションでエスケープされている必要がある
```ini
; Good!
key1 = 'aaa; bbb; ccc;'
key2 = "aaa; bbb; ccc;"
key3 = 'aaa// bbb// ccc//'
key4 = "aaa// bbb// ccc//"

; Bad!
key1 = aaa; bbb; ccc;
key2 = aaa; bbb; ccc;
key3 = aaa// bbb// ccc//
key4 = aaa// bbb// ccc//
```

5. シングルクォーテーションでエスケープされている途中でシングルクォーテーションを使用したい場合、バックスラッシュで文字エスケープをしている必要がある。
```ini
; Good!
; aaa' bbb' ccc
key1 = 'aaa\' bbb\' ccc'

; Bad!
key1 = 'aaa' bbb' ccc'
```

5. ダブルクォーテーションでエスケープされている途中でダブルクォーテーションを使用したい場合、バックスラッシュで文字エスケープをしている必要がある。
```ini
; Good!
; aaa" bbb" ccc
key1 = "aaa\" bbb\" ccc"

; Bad!
key1 = "aaa" bbb" ccc"
```

6. エスケープなしの値、シングルクォーテーションでエスケープされた値、およびダブルクォーテーションエスケープされた値は連結することができます
```ini
; aaabbbccc
key1 = aaa'bbb'"ccc"
; aaa"bbb'ccc
key2 = aaa'"bbb'"'ccc"
```

License
---
このライブラリは[MITライセンス](https://github.com/Kento-Hanzawa/INI-Helper/blob/master/LICENSE)です。
