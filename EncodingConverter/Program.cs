/**
 * 2019/12/22 遠藤作成
 * 
 * 特定のディレクトリの指定の拡張子のファイルたちをエンコードしてコピーするプログラム
 * 
 * 以下のNuGetパッケージを使用
 * 
 * - CommandLineParser              コマンドライン引数をいい感じに取得できるライブラリ
 * - ReadJEnc                       特定のテキストファイルの文字コードを推定してくれるライブラリ
 * - System.Text.Encoding.CodePage  C#デフォルトではEUC-JPなどの文字コードをサポートしないため、それを追加するライブラリ
 */

using System;
using System.IO;
using System.Text;

namespace EncodingConverter
{
    class Program
    {
        static void Main(string[] args)
        {
            // アプリケーションへShift_JISの登録
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            // コマンドライン引数のチェックと取得
            var result = CommandLine.Parser.Default.ParseArguments<CommandLineOptions>(args) as CommandLine.Parsed<CommandLineOptions>;
            if (result == null)
            {
                Environment.Exit(-1);
            }

            Console.WriteLine($"Input: {result.Value.Input}");
            Console.WriteLine($"Output: {result.Value.OutputDir}");
            Console.WriteLine($"File Extension: {result.Value.FileExtension}");
            Console.WriteLine($"Encoding: {result.Value.Encoding}");

            // 変換元のパスから出力先のディレクトリを特定
            string outputDir = "";
            if (File.GetAttributes(result.Value.Input).HasFlag(FileAttributes.Directory))
            {
                var dir = result.Value.Input.Substring(result.Value.Input.Length-1, 1) == "\\" ? result.Value.Input[0..^1] : result.Value.Input;
                outputDir = $"{dir}_convert";
            }
            else
            {
                outputDir = Directory.GetParent(result.Value.Input).FullName;
            }

            // 出力先のディレクトリが指定されている場合はそちらを優先
            if (!string.IsNullOrEmpty(result.Value.OutputDir))
            {
                if (File.GetAttributes(result.Value.OutputDir).HasFlag(FileAttributes.Directory))
                {
                    outputDir = result.Value.OutputDir;
                }
                else
                {
                    Console.WriteLine($"Output:[{result.Value.OutputDir}] は不正なディレクトリ名です");
                    Environment.Exit(-1);
                }
            }

            // ここまで来て出力先のディレクトリが空だった場合はエラー
            if (string.IsNullOrEmpty(outputDir))
            {
                Console.WriteLine($"Input, もしくはOutputの形式が不正です。");
                Console.WriteLine($"Input: {result.Value.Input}");
                Console.WriteLine($"Output: {result.Value.OutputDir}");
                Environment.Exit(-1);
            }

            try
            {
                // UTF-8ならBOMなしにする
                var encoding = (result.Value.Encoding.ToLower() == "utf-8") ? new UTF8Encoding(false) : Encoding.GetEncoding(result.Value.Encoding);
                var fileExtensions = result.Value.FileExtension.Split(',');

                var converter = new Converter(outputDir, encoding, fileExtensions);

                converter.Convert(result.Value.Input);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Environment.Exit(-1);
            }
        }
    }
}
