using CommandLine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

namespace SearchAndDestroy {
    class Program {

        public class Options {


            public Dictionary<string, string> FindReplacePairs { get; set; }

            public Dictionary<string, int> FindReplaceStatsContents { get; set; }
            public Dictionary<string, int> FindReplaceStatsFiles { get; set; }
            public Dictionary<string, int> FindReplaceStatsDirs { get; set; }
            public Dictionary<string, int> FileTypeStats { get; set; }
            public Dictionary<string, int> FileContentReplaceStats { get; set; }



            [Option(
                'v', "verbose", 
                Required = false, 
                HelpText = "Set output to verbose messages."
            )]
            public bool Verbose { get; set; }

            [Option(
                'r', "replace", 
                Required = true, 
                HelpText = "Set the find/replace pairs. You must specify this parameter always in pairs, ie the first of the pair is the find pattern, the second of the pair is the replace patter. For example, to add two replaces, you would add the argument --replace \"findA\" \"withA\" \"findB\" \"withB\", where all strings matching 'findA' are replaced with 'withA' and all strings matching 'findB' are replaced with 'withA'. "
            )]
            public IEnumerable<string> Replaces { get; set; }


            [Option(
                'd', "dir", 
                Required = true, 
                HelpText = "The directory (or directories) to perform the find/replace on."
            )]
            public IEnumerable<string> Directories { get; set; }

            [Option(
                'i', "include",
                Required = false,
                Default = null,
                HelpText = "The inclusion pattern (or patterns) that all directories and files must match to. To include everything, use '*'."
            )]
            public IEnumerable<string> Includes { get; set; }

            [Option(
                'e', "exclude",
                Required = false,
                Default = null,
                HelpText = "The exclusion pattern (or patterns) that will skip any included directories and files. This argument has precedance over the inclusion argument."
            )]
            public IEnumerable<string> Excludes { get; set; }

            [Option(
                'R', "recursively",
                Required = false,
                Default = true,
                HelpText = "If set, the directories are traversed recursevily through all sub-directories."
            )]
            public bool Recursively { get; set; }

            [Option(
                'D', "dry-run",
                Required = false,
                Default = false,
                HelpText = "If set, no actual changes are performed."
            )]
            public bool DryRun { get; set; }

            [Option(
                'n', "no-confirm",
                Required = false,
                Default = false,
                HelpText = "If set, the confirmation is skipped."
            )]
            public bool NoConfirm { get; set; }

            [Option(
                's', "slow",
                Required = false,
                Default = false,
                HelpText = "If set, a key-press is requested at each traversal through the directories and files. This is useful together with dry-run if you want to walk the actions together with the program before doing a real run."
            )]
            public bool Slow { get; set; }

            [Option(
                'g', "use-git-move",
                Required = false,
                Default = false,
                HelpText = "If set, a \"git mv\" command is attempted if any directories or files are to be renamed."
            )]
            public bool UseGitMove { get; set; }

            [Option(
                'b', "binary",
                Required = false,
                Default = false,
                HelpText = "If true, contents inside binary files will also be replaced (not recommended)."
            )]
            public bool Binary { get; set; }
        }

        static void Main(string[] args) {
            Parser.Default.ParseArguments<Options>(args)
                   .WithParsed<Options>(opts => {

                       if (opts.Includes == null) opts.Includes = new List<string>() { "*" };
                       if (opts.Excludes == null) opts.Excludes = new List<string>();

                       opts.FindReplacePairs = new Dictionary<string, string>();
                       opts.FindReplaceStatsContents = new Dictionary<string, int>();
                       opts.FindReplaceStatsFiles = new Dictionary<string, int>();
                       opts.FindReplaceStatsDirs = new Dictionary<string, int>();
                       opts.FileTypeStats = new Dictionary<string, int>();
                       opts.FileContentReplaceStats = new Dictionary<string, int>();


                       var replaces = new List<string>(opts.Replaces);
                       if (replaces.Count % 2 != 0) throw new Exception("The replace argument must always provide a pair of find and replace values.");
                       for(var i = 0; i < replaces.Count; i+=2) {
                           opts.FindReplacePairs.Add(replaces[i], replaces[i + 1]);
                       }

                       // Show config
                       Console.WriteLine($"Verbose:\t{opts.Verbose}");
                       Console.WriteLine($"No Confirm:\t{opts.NoConfirm}");
                       Console.WriteLine($"DryRun:\t\t{opts.DryRun}");
                       Console.WriteLine($"Recursively:\t{opts.Recursively}");
                       Console.WriteLine($"UseGitMove:\t{opts.UseGitMove}");
                       foreach (var o in opts.Includes) {
                           Console.WriteLine($"Include:\t'{o}'");
                       }
                       foreach (var o in opts.Excludes) {
                           Console.WriteLine($"Exclude:\t'{o}'");
                       }
                       foreach (var find in opts.FindReplacePairs.Keys) {
                           var replace = opts.FindReplacePairs[find];
                           Console.WriteLine($"Replace:\t'{find}' with '{replace}'");
                       }
                       foreach (var o in opts.Directories) {
                           Console.WriteLine($"Directory:\t'{o}'");
                       }

                       // Confirm?
                       if(!opts.NoConfirm) {
                           if(!opts.DryRun) Console.WriteLine("***WARNING: this may rename files and replace file contents!");
                           Console.WriteLine("\nPress enter key to continue...");
                           Console.ReadLine();
                       }

                       // Proces each root dir
                       foreach (var dir in opts.Directories) {
                           if (!opts.Verbose) Console.WriteLine($"Dir {dir}...");
                           ProcessDirectory(dir, 0, opts);
                       }

                       // Stats
                       Console.WriteLine("");

                       Console.WriteLine($"Found files:");
                       foreach (var ext in opts.FileTypeStats.Keys) {
                           var count = opts.FileTypeStats[ext];
                           Console.WriteLine($"  {ext}: {count}x");
                       }
                       foreach (var find in opts.FindReplaceStatsContents.Keys) {
                           var count = opts.FindReplaceStatsContents[find];
                           Console.WriteLine($"Replaced '{find}' in file contents {count}x");
                       }
                       foreach (var find in opts.FindReplaceStatsDirs.Keys) {
                           var count = opts.FindReplaceStatsDirs[find];
                           Console.WriteLine($"Replaced '{find}' in directory name {count}x");
                       }
                       foreach (var find in opts.FindReplaceStatsFiles.Keys) {
                           var count = opts.FindReplaceStatsFiles[find];
                           Console.WriteLine($"Replaced '{find}' in file name {count}x");
                       }
                       Console.WriteLine($"Replaced inside files:");
                       foreach (var ext in opts.FileContentReplaceStats.Keys) {
                           var count = opts.FileContentReplaceStats[ext];
                           Console.WriteLine($"  {ext}: {count}x");
                       }
                   });
        }

        static void ProcessDirectory(string dir, int depth, Options opts) {
            if(opts.Verbose) Console.WriteLine($"Dir {dir}...");
            if(opts.Slow) Console.ReadLine();

            // Change dir name?
            {
                var dirInfo = new System.IO.DirectoryInfo(dir);
                var newDirName = dirInfo.Name;
                // Loop each find/replace pair
                foreach (var find in opts.FindReplacePairs.Keys) {
                    var replace = opts.FindReplacePairs[find];
                    var matches = Regex.Matches(newDirName, find);
                    if (matches.Count > 0) {
                        if (opts.Verbose) {
                            Console.WriteLine($"  Found {find} in dir name");
                        }
                        // Register in stats
                        if (opts.FindReplaceStatsDirs.ContainsKey(find)) {
                            opts.FindReplaceStatsDirs[find] += matches.Count;
                        } else {
                            opts.FindReplaceStatsDirs[find] = matches.Count;
                        }
                        // Update new name
                        newDirName = newDirName.Replace(find, replace);
                    }
                }
                if(newDirName != dirInfo.Name) {
                    Console.WriteLine($"  Renaming directory from '{dirInfo.Name}' to '{newDirName}'");
                    if(opts.DryRun == false) {
                        var newPath = dirInfo.Parent.FullName + System.IO.Path.DirectorySeparatorChar.ToString() + newDirName;
                        RenameDir(dirInfo.FullName, newPath, opts);
                        dir = newPath;
                    }
                }
            }

            // Find files
            foreach (var include in opts.Includes) {
                foreach (var includedFile in System.IO.Directory.GetFiles(dir, include)) {
                    // Excluded?
                    var excluded = false;
                    foreach (var exclude in opts.Excludes) {
                        foreach (var excludedFile in System.IO.Directory.GetFiles(dir, exclude)) {
                            if(excludedFile == includedFile) {
                                if(opts.Verbose) Console.WriteLine($"  Excluded file {includedFile} due to '{exclude}'...");
                                excluded = true;
                                break;
                            }
                        }
                    }
                    if (!excluded) {
                        ProcessFile(dir, depth, opts, includedFile);
                    }
                }
            }

            if (opts.Recursively) {
                var subdirs = System.IO.Directory.GetDirectories(dir);
                foreach (var subdir in subdirs) {

                    // Excluded?
                    var excluded = false;
                    foreach (var exclude in opts.Excludes) {
                        foreach (var excludedDir in System.IO.Directory.GetDirectories(dir, exclude)) {
                            if (excludedDir == subdir) {
                                if (opts.Verbose) Console.WriteLine($"Excluded dir {subdir} due to '{exclude}'...");
                                excluded = true;
                                break;
                            }
                        }
                    }
                    if (!excluded) {
                        ProcessDirectory(subdir, depth + 1, opts);
                    }

                    
                }
            }
        }

        static void RenameDir(string source, string dest, Options opts) {
            if (opts.DryRun) return; // should never happen
            if (opts.UseGitMove) {
                // Git move
                if (opts.Verbose) Console.WriteLine($"  Executing 'git mv {source} {dest}'");
                var proc = Process.Start("git", $"mv {source} {dest}");
                proc.WaitForExit();
                if (proc.ExitCode != 0) {
                    // Fallback to system move
                    if (opts.Verbose) Console.WriteLine($"  Git command failed with exit code {proc.ExitCode}");
                    System.IO.Directory.Move(source, dest);
                }
            } else {
                // System move
                System.IO.Directory.Move(source, dest);
            }
        }

        static void RenameFile(string source, string dest, Options opts) {
            if (opts.DryRun) return; // should never happen
            if (opts.UseGitMove) {
                // Git move
                if (opts.Verbose) Console.WriteLine($"    Executing 'git mv {source} {dest}'");
                var proc = Process.Start("git", $"mv {source} {dest}");
                proc.WaitForExit();
                if (proc.ExitCode != 0) {
                    // Fallback to system move
                    if (opts.Verbose) Console.WriteLine($"  Git command failed with exit code {proc.ExitCode}");
                    System.IO.File.Move(source, dest);
                }
            } else {
                // System move
                System.IO.File.Move(source, dest);
            }
        }

        static void ProcessFile(string dir, int depth, Options opts, string file) {
            if(opts.Verbose) Console.WriteLine($"  Found file {file}...");
            if (opts.Slow) Console.ReadLine();

            // Init
            var fileInfo = new System.IO.FileInfo(file);

            // Change file name?
            {
                var newFileName = fileInfo.Name;
                // Register in stats
                if (opts.FileTypeStats.ContainsKey(fileInfo.Extension)) {
                    opts.FileTypeStats[fileInfo.Extension] += 1;
                } else {
                    opts.FileTypeStats[fileInfo.Extension] = 1;
                }
                // Loop each find/replace pair
                foreach (var find in opts.FindReplacePairs.Keys) {
                    var replace = opts.FindReplacePairs[find];
                    var matches = Regex.Matches(newFileName, find);
                    if (matches.Count > 0) {
                        if (opts.Verbose) {
                            Console.WriteLine($"  Found {find} in file name");
                        }
                        // Register in stats
                        if (opts.FindReplaceStatsFiles.ContainsKey(find)) {
                            opts.FindReplaceStatsFiles[find] += matches.Count;
                        } else {
                            opts.FindReplaceStatsFiles[find] = matches.Count;
                        }
                        // Update new name
                        newFileName = newFileName.Replace(find, replace);
                    }
                }
                if (newFileName != fileInfo.Name) {
                    Console.WriteLine($"  Renaming file from '{fileInfo.Name}' to '{newFileName}'");
                    if (opts.DryRun == false) {
                        var newPath = fileInfo.Directory.FullName + System.IO.Path.DirectorySeparatorChar.ToString() + newFileName;
                        RenameFile(fileInfo.FullName, newPath, opts);
                        file = newPath;
                    }
                }
            }

            // Open file
            var fileContents = System.IO.File.ReadAllText(file);
            var totalMatches = 0;

            // Skip file contents if it is binary...
            var isBinary = fileContents.Any(ch => char.IsControl(ch) && ch != '\r' && ch != '\n' && ch != '\t');
            if (opts.Binary == false && isBinary == true) return;

            // Loop each find/replace pair
            foreach (var find in opts.FindReplacePairs.Keys) {
                var replace = opts.FindReplacePairs[find];
                if (opts.Verbose) Console.WriteLine($"    Replacing '{find}' with '{replace}'...");


                // Search for matches
                var matches = Regex.Matches(fileContents, find);
                if(matches.Count > 0) {

                    if (opts.Verbose) {
                        Console.WriteLine($"    Found {matches.Count} matche(s)");
                    }

                    // Register in stats
                    totalMatches += matches.Count;
                    if (opts.FindReplaceStatsContents.ContainsKey(find)) {
                        opts.FindReplaceStatsContents[find] += matches.Count;
                    } else {
                        opts.FindReplaceStatsContents[find] = matches.Count;
                    }

                    // Do replace
                    fileContents = fileContents.Replace(find, replace);
                    
                }

            }

            // Write out again?
            if (totalMatches > 0) {

                // Register in stats
                if (opts.FileContentReplaceStats.ContainsKey(fileInfo.Extension)) {
                    opts.FileContentReplaceStats[fileInfo.Extension] += 1;
                } else {
                    opts.FileContentReplaceStats[fileInfo.Extension] = 1;
                }

                if (opts.DryRun == false) {
                    if (opts.Verbose) Console.WriteLine($"    Writing file...");
                    System.IO.File.WriteAllText(file, fileContents);
                }

                Console.WriteLine($"  {file}: {totalMatches}x");
            }

        }


    }
}
