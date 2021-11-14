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

        static string output_dir = "output";

        static string runlog = "";
            
        static void Main(string[] args)
        {
            log(@"        *      *");
            log(@"   (  (  `   (  `");
            log(@" ( )\ )\))(  )\))(");
            log(@" )((_|(_)()\((_)()\");
            log(@"((_)_(_()((_|_()((_)");
            log(@" | _ )  \/  |  \/  |");
            log(@" | _ \ |\/| | |\/| |");
            log(@" |___/_|  |_|_|  |_|");
            log("");
            log("Blade & Sorcery: Nomad Mod Merge");
            log("");
            log("substatica");
            log("");
            if (args.Length < 3)
            {
                log("Usage: ");
                log("");
                log("  bas-mod-merge.exe [unmodified bas.jsondb] [dominant mod] [subordinate mod]");
                log("");
                log("Example: ");
                log("");
                log("  bas-mod-merge.exe bas.jsondb ButterStabs.zip Gravity.7z");
                return;
            }

            string refFile = args[0];
            string domFile = args[1];
            string subFile = args[2];

            if(!File.Exists(refFile) || !File.Exists(domFile) || !File.Exists(subFile))
            {
                log("File not found");
                return;
            }

            log("Resetting temporary directories");
            ResetDirectories();

            log("Preparing reference");
            ExtractJsonDb(new FileInfo(refFile), ref_jsondb_dir);
            log("Preparing dominant mod");
            ExtractJsonDb(new FileInfo(domFile), dom_jsondb_dir);
            log("Preparing subordinate mod");
            ExtractJsonDb(new FileInfo(subFile), sub_jsondb_dir);

            log("Extracting reference");
            try
            {
                ZipFile.ExtractToDirectory(Directory.GetFiles(ref_jsondb_dir, "*.jsondb", SearchOption.AllDirectories)[0], ref_dir);
            }
            catch (Exception ex)
            {
                log("Failed to extract reference");
                throw (ex);
            }

            log("Extracting dominant mod");
            try
            {
                ZipFile.ExtractToDirectory(Directory.GetFiles(dom_jsondb_dir, "*.jsondb", SearchOption.AllDirectories)[0], dom_dir);
            }
            catch (Exception ex)
            {
                log("Failed to extract dominant mod");
                throw (ex);
            }

            log("Extracting subordinate mod");
            try
            {
                ZipFile.ExtractToDirectory(Directory.GetFiles(sub_jsondb_dir, "*.jsondb", SearchOption.AllDirectories)[0], sub_dir);
            }
            catch (Exception ex)
            {
                log("Failed to extract subordinate mod");
                throw (ex);
            }

            Int32 totalConflicts = 0;

            totalConflicts += CompareFiles(ref_dir);
            foreach(string directory in Directory.GetDirectories(ref_dir))
            {
                totalConflicts += CompareFiles(directory);
            }

            if (File.Exists(Path.Combine(output_dir, "bas.jsondb")))
            {
                log("Removing previous output");
                File.Delete(Path.Combine(output_dir, "bas.jsondb"));
            }

            try
            {
                log("Compressing merged JSON database");
                ZipFile.CreateFromDirectory(mer_dir, Path.Combine(output_dir, "bas.jsondb"));
            }
            catch (Exception ex)
            {
                log("Failed to compress merged JSON database");
                throw (ex);
            }

            try
            {
                log("Removing temporary files");
                Cleanup();
            }
            catch (Exception ex)
            {
                log("Failed to remove temporary files");
                log("Please delete temporary directory before running again");
                throw (ex);
            }

            log("-");
            log( totalConflicts + " Conflicts detected");
            log("Merge complete");

            try
            {
                Console.WriteLine("Writing log");
                File.WriteAllText(Path.Combine(output_dir, "output.log"), runlog);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to write log");
                throw (ex);
            }
            Console.WriteLine("Complete");
            Console.WriteLine("");
        }

        static void log(string line)
        {
            Console.WriteLine(line);
            runlog += line + Environment.NewLine;
        }

        static int CompareFiles(string directory) {
            DirectoryInfo dirInfo = new DirectoryInfo(directory);
            DirectoryInfo merInfo = directory == ref_dir ? new DirectoryInfo(mer_dir) : Directory.CreateDirectory(Path.Combine(mer_dir, dirInfo.Name));

            Int32 conflictCount = 0;
            
            foreach (string file in Directory.GetFiles(directory))
            {
                FileInfo fileInfo = new FileInfo(file);
                Boolean subChange = false;
                Boolean domChange = false;

                if (File.Exists(Path.Combine(sub_dir, dirInfo.Name, fileInfo.Name)))
                {
                    FileInfo subInfo = new FileInfo(Path.Combine(sub_dir, dirInfo.Name, fileInfo.Name));
                    if (!FilesAreEqual(fileInfo, subInfo))
                    {
                        subChange = true;
                        log("Subordinate mod - " + subInfo);
                        subInfo.CopyTo(Path.Combine(merInfo.FullName, subInfo.Name));
                    }
                }

                if (File.Exists(Path.Combine(dom_dir, dirInfo.Name, fileInfo.Name)))
                {
                    FileInfo domInfo = new FileInfo(Path.Combine(dom_dir, dirInfo.Name, fileInfo.Name));
                    if (!FilesAreEqual(fileInfo, domInfo))
                    {
                        domChange = true;
                        log("Dominant mod - " + domInfo);
                        domInfo.CopyTo(Path.Combine(merInfo.FullName, domInfo.Name), true);
                    }
                }

                if(!File.Exists(Path.Combine(merInfo.FullName, fileInfo.Name)))
                {
                    fileInfo.CopyTo(Path.Combine(merInfo.FullName, fileInfo.Name), true);
                }

                if (domChange && subChange)
                {
                    log("^");
                    log("Conflict detected");
                    log("-");
                    conflictCount++;
                }
            }
            return conflictCount;
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
                log("Could not reset temporary directories. Please close all directories.");
                log("If you continue to experience this issue manually delete the '"+temp+"' directory.");
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
                    log("Archive detected");
                    log("Extracting to " + dir);
                    ExtractFile(fi.FullName, dir);
                    break;
                case ".jsondb":
                    log("JSON database detected");
                    log("Copying to " + dir);
                    File.Copy(fi.FullName, Path.Combine(dir, fi.Name));
                    break;
                default:
                    log("Unsupported file type detected");
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
