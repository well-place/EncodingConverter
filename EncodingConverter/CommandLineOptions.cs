using System;
using System.Collections.Generic;
using System.Text;

namespace EncodingConverter
{
    class CommandLineOptions
    {
        [CommandLine.Option('i', "input", Required = true, HelpText = "変換元のファイル・フォルダ")]
        public string Input
        {
            get;
            set;
        }

        [CommandLine.Option('o', "output", Required = false, HelpText = "出力先のフォルダ")]
        public string OutputDir
        {
            get;
            set;
        }

        [CommandLine.Option('f', "fileExtension", Required = true, HelpText = "コンバートする拡張子　例:「-f php,txt,html」")]
        public string FileExtension
        {
            get;
            set;
        }

        [CommandLine.Option('e', "encoding", Required = true, HelpText = "変更先のエンコーディング (Shift_JIS / UTF-8 / EUC-JP / ASCII)")]
        public string Encoding
        {
            get;
            set;
        }
    }
}
