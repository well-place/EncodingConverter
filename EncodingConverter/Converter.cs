using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Hnx8.ReadJEnc;

namespace EncodingConverter
{
    public class Converter
    {
        private readonly DirectoryInfo _destinationRoot;
        private readonly Encoding _encoding;
        private readonly List<string> _fileExtensions;
        
        public Converter(string baseDir, Encoding encoding, IReadOnlyList<string> fileExtensions)
        {
            _destinationRoot = Directory.Exists(baseDir) ? new DirectoryInfo(baseDir) : Directory.CreateDirectory(baseDir);
            _encoding = encoding;
            _fileExtensions = new List<string>();

            // ex) "txt" -> ".txt"
            for (int i = 0; i < fileExtensions.Count; i++)
            {
                _fileExtensions.Add(fileExtensions[i].Substring(0, 1) == "." ? fileExtensions[i] : $".{fileExtensions[i]}");
            }
        }

        /// <summary>
        /// 与えられたディレクトリ、もしくはファイルをコンバートする
        /// </summary>
        /// <param name="path"></param>
        public void Convert(string path)
        {
            if (File.GetAttributes(path).HasFlag(FileAttributes.Directory))
            {
                Convert(new DirectoryInfo(path), _destinationRoot, 0);
            }
            else
            {
                Convert(new FileInfo(path), _destinationRoot, 0);
            }
        }

        /// <summary>
        /// 与えられたディレクトリ以下のファイルをコンバートするよう振り分ける
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        /// <param name="depth"></param>
        public void Convert(DirectoryInfo source, DirectoryInfo destination, int depth = 0)
        {
            var depthStr = new string(' ', depth * 2);

            // ドットから始まるディレクトリは特殊なので無視
            if (source.Name.Substring(0, 1) == ".")
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine($"Ignore     : {depthStr}{source.FullName}/");
                Console.ResetColor();
                return;
            }

            foreach (var dir in source.GetDirectories())
            {
                if (dir.Name.Substring(0, 1) != ".")
                {
                    // ディレクトリがドットから始まらないなら作成し、変換を継続
                    var nextDestination = destination.CreateSubdirectory(dir.Name);
                    Console.WriteLine($"Dir Create : {depthStr}{nextDestination.Name}/");

                    Convert(dir, nextDestination, depth + 1);
                }
                else
                {
                    // ドットから始まるディレクトリは特殊なので無視
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.WriteLine($"Ignore     : {depthStr}{dir.Name}/");
                    Console.ResetColor();
                }
            }

            foreach (var file in source.GetFiles())
            {
                Convert(file, destination, depth);
            }
        }

        /// <summary>
        /// 与えられたファイルをコンバートする
        /// </summary>
        /// <param name="file"></param>
        /// <param name="target"></param>
        /// <param name="depth"></param>
        public void Convert(FileInfo file, DirectoryInfo target, int depth = 0)
        {
            var depthStr = new string(' ', depth * 2);
            var destinationFilePath = $@"{target.FullName}\{file.Name}";

            if (_fileExtensions.Contains(file.Extension))
            {
                var (code, text) = DetectEncoding(file.FullName);
                string codeText;

                if (code != null)
                {
                    codeText = code.ToString();
                    File.WriteAllText(destinationFilePath, text, _encoding);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"File Create: {depthStr}{file.Name} - Code: {codeText}");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"File Copy  : {depthStr}{file.Name} - Code: ??? (Detection Failure)");
                    file.CopyTo(destinationFilePath, true);
                }

                Console.ResetColor();
            }
            else
            {
                // ファイルを変換先のディレクトリにそのままコピー（常に上書き）
                file.CopyTo(destinationFilePath, true);

                Console.WriteLine($"File Copy  : {depthStr}{file.Name}");
            }
        }

        /// <summary>
        /// 与えられたディレクトリ以下のファイルをすべてそのままコピーする
        /// </summary>
        /// <param name="sourceDirectory"></param>
        /// <param name="destinationDirectory"></param>
        private static void DirectoryCopy(DirectoryInfo sourceDirectory, DirectoryInfo destinationDirectory)
        {
            //コピー先のディレクトリがなければ作成する
            if (destinationDirectory.Exists == false)
            {
                destinationDirectory.Create();
                destinationDirectory.Attributes = sourceDirectory.Attributes;
            }

            //ファイルのコピー
            foreach (FileInfo fileInfo in sourceDirectory.GetFiles())
            {
                //同じファイルが存在していたら、常に上書きする
                fileInfo.CopyTo(destinationDirectory.FullName + @"\" + fileInfo.Name, true);
            }

            //ディレクトリのコピー（再帰を使用）
            foreach (DirectoryInfo directoryInfo in sourceDirectory.GetDirectories())
            {
                DirectoryCopy(directoryInfo, new DirectoryInfo(destinationDirectory.FullName + @"\" + directoryInfo.Name));
            }
        }

        /// <summary>
        /// 文字エンコードを取得する
        /// 参考：http://namco.hatenablog.jp/entry/2017/02/05/160119
        /// </summary>
        /// <param name="fname"></param>
        /// <returns></returns>
        private static (CharCode, string) DetectEncoding(string fname)
        {
            // ファイルをbyte形で全て読み込み
            FileStream fs = new FileStream(fname, FileMode.Open);
            byte[] data = new byte[fs.Length];
            fs.Read(data, 0, data.Length);
            fs.Close();

            // 文字エンコード推定（hnx8氏公開のDLL）
            var charCode = ReadJEnc.JP.GetEncoding(data, data.Length, out var str);

            return (charCode, str);
        }
    }
}
