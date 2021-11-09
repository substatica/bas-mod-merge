using System;
using System.IO;
using System.IO.Compression;
using SevenZipExtractor;

namespace bas_mod_merge
{
    class Program
    {
        static string temp = "bas_mod_merge_temp";

        static string ref_dir = temp + @"/ref_extracted";
        static string dom_dir = temp + @"/dom_extracted";
        static string sub_dir = temp + @"/sub_extracted";
        static string mer_dir = temp + @"/merged";

        static string ref_jsondb_dir = temp + @"/ref_bas_jsondb";
        static string dom_jsondb_dir = temp + @"/dom_bas_jsondb";
        static string sub_jsondb_dir = temp + @"/sub_bas_jsondb";

        static string output_dir = "merge_output";
            
        static void Main(string[] args)
        {
            if (args.Length < 3)
            {
                Console.WriteLine(@"        *      *");
                Console.WriteLine(@"   (  (  `   (  `");
                Console.WriteLine(@" ( )\ )\))(  )\))(");
                Console.WriteLine(@" )((_|(_)()\((_)()\");  
                Console.WriteLine(@"((_)_(_()((_|_()((_)");
                Console.WriteLine(@" | _ )  \/  |  \/  |");
                Console.WriteLine(@" | _ \ |\/| | |\/| |");
                Console.WriteLine(@" |___/_|  |_|_|  |_|");
                Console.WriteLine();
                Console.WriteLine("Blade & Sorcery: Nomad Mod Merge");
                Console.WriteLine();
                Console.WriteLine("Usage: ");
                Console.WriteLine();
                Console.WriteLine("  bas-mod-merge.exe [unmodified bas.jsondb] [dominant mod] [subordinate mod]");
                Console.WriteLine();
                Console.WriteLine("Example: ");
                Console.WriteLine();
                Console.WriteLine("  bas-mod-merge.exe bas.jsondb ButterStabs.zip Gravity.7z");
                return;
            }

            string refFile = args[0];
            string domFile = args[1];
            string subFile = args[2];

            if(!File.Exists(refFile) || !File.Exists(domFile) || !File.Exists(subFile))
            {
                Console.WriteLine("File not found");
                return;
            }

            ResetDirectories();

            ExtractJsonDb(new FileInfo(domFile), dom_jsondb_dir);
            ExtractJsonDb(new FileInfo(subFile), sub_jsondb_dir);
            ExtractJsonDb(new FileInfo(refFile), ref_jsondb_dir);

            ZipFile.ExtractToDirectory(Directory.GetFiles(ref_jsondb_dir, "*.jsondb", SearchOption.AllDirectories)[0], ref_dir);
            ZipFile.ExtractToDirectory(Directory.GetFiles(dom_jsondb_dir, "*.jsondb", SearchOption.AllDirectories)[0], dom_dir);
            ZipFile.ExtractToDirectory(Directory.GetFiles(sub_jsondb_dir, "*.jsondb", SearchOption.AllDirectories)[0], sub_dir);

            CompareFiles(ref_dir);
            foreach(string directory in Directory.GetDirectories(ref_dir))
            {
                CompareFiles(directory);
            }

            if(File.Exists(Path.Combine(output_dir, "bas.jsondb")))
            {
                File.Delete(Path.Combine(output_dir, "bas.jsondb"));
            }
            ZipFile.CreateFromDirectory(mer_dir, Path.Combine(output_dir, "bas.jsondb"));

            Cleanup();

            Console.WriteLine("Merge complete.");
        }

        static void CompareFiles(string directory) {
            DirectoryInfo dirInfo = new DirectoryInfo(directory);
            DirectoryInfo merInfo = directory == ref_dir ? new DirectoryInfo(mer_dir) : Directory.CreateDirectory(Path.Combine(mer_dir, dirInfo.Name));

            foreach (string file in Directory.GetFiles(directory))
            {
                FileInfo fileInfo = new FileInfo(file);

                if (File.Exists(Path.Combine(sub_dir, dirInfo.Name, fileInfo.Name)))
                {
                    FileInfo subInfo = new FileInfo(Path.Combine(sub_dir, dirInfo.Name, fileInfo.Name));
                    if (!FilesAreEqual(fileInfo, subInfo))
                    {
                        Console.WriteLine("Subordinate Mod: " + subInfo);
                        subInfo.CopyTo(Path.Combine(merInfo.FullName, subInfo.Name));
                    }
                }

                if (File.Exists(Path.Combine(dom_dir, dirInfo.Name, fileInfo.Name)))
                {
                    FileInfo domInfo = new FileInfo(Path.Combine(dom_dir, dirInfo.Name, fileInfo.Name));
                    if (!FilesAreEqual(fileInfo, domInfo))
                    {
                        Console.WriteLine("Dominant Mod: " + domInfo);
                        domInfo.CopyTo(Path.Combine(merInfo.FullName, domInfo.Name), true);
                    }
                }

                if(!File.Exists(Path.Combine(merInfo.FullName, fileInfo.Name)))
                {
                    fileInfo.CopyTo(Path.Combine(merInfo.FullName, fileInfo.Name), true);
                }
            }
        }

        static void Cleanup()
        {
            DelTree(temp);
        }

        static void ResetDirectories()
        {
            try
            {
                if (!Directory.Exists(output_dir))
                {
                    Directory.CreateDirectory(output_dir);
                }
                DelTree(temp);
                Directory.CreateDirectory(temp);
                Directory.CreateDirectory(ref_dir);
                Directory.CreateDirectory(dom_dir);
                Directory.CreateDirectory(sub_dir);
                Directory.CreateDirectory(mer_dir);
                Directory.CreateDirectory(ref_jsondb_dir);
                Directory.CreateDirectory(dom_jsondb_dir);
                Directory.CreateDirectory(sub_jsondb_dir);
            }
            catch(Exception ex)
            {
                Console.WriteLine("Could not reset temporary directories. Please close all directories.");
                Console.WriteLine("If you continue to experience this issue manually delete the '"+temp+"' directory.");
                throw (ex);
            }
    }

        const int BYTES_TO_READ = sizeof(Int64);
        static bool FilesAreEqual(FileInfo first, FileInfo second)
        {
            if (first.Length != second.Length)
                return false;

            if (string.Equals(first.FullName, second.FullName, StringComparison.OrdinalIgnoreCase))
                return true;

            int iterations = (int)Math.Ceiling((double)first.Length / BYTES_TO_READ);

            using (FileStream fs1 = first.OpenRead())
            using (FileStream fs2 = second.OpenRead())
            {
                byte[] one = new byte[BYTES_TO_READ];
                byte[] two = new byte[BYTES_TO_READ];

                for (int i = 0; i < iterations; i++)
                {
                    fs1.Read(one, 0, BYTES_TO_READ);
                    fs2.Read(two, 0, BYTES_TO_READ);

                    if (BitConverter.ToInt64(one, 0) != BitConverter.ToInt64(two, 0))
                        return false;
                }
            }

            return true;
        }

        static void ExtractJsonDb(FileInfo fi, string dir)
        {
            switch (fi.Extension.ToLower()) 
            {
                case ".7z":
                case ".zip":
                case ".rar":
                    ExtractFile(fi.FullName, dir);
                    break;
                case ".jsondb":
                    File.Copy(fi.FullName, Path.Combine(dir, fi.Name));
                    break;
                default:
                    Console.WriteLine("Unsupported file type detected");
                    throw (new Exception("Unsupported file type detected"));
            }
        }

        static void ExtractFile(string source, string destination)
        {
            ArchiveFile zipFile = new ArchiveFile(source);
            zipFile.Extract(destination);
        }

        static void DelTree(string dir)
        {
            DirectoryInfo di = new DirectoryInfo(dir);
            if (di.Exists)
            {
                Directory.Delete(di.FullName, true);
            }
        }
    }
}
