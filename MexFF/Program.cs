﻿using HSDRaw;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace MexFF
{
    class Program
    {
        static void Main(string[] args)
        {
            // Parse Args
            List<Tuple<int, string>> itemInputs = new List<Tuple<int, string>>();
            List<string> inputs = new List<string>();
            string output = "";
            string symbolName = "ftFunction";
            string datFile = null;
            string[] fightFuncTable = null;
            bool injectDat = false;
            bool quiet = false;
            bool yesOverwrite = false;
            bool disableWarnings = false;
            bool clean = false;

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-i")
                {
                    for (int j = i + 1; j < args.Length; j++)
                        if (File.Exists(args[j]))
                            inputs.Add(args[j]);
                        else
                            break;
                }
                if (args[i] == "-ii" && i + 2 < args.Length)
                {
                    itemInputs.Add(new Tuple<int, string>(int.Parse(args[i + 1]), args[i + 2]));
                }
                if (args[i] == "-o" && i + 1 < args.Length)
                    output = args[i + 1];
                if (args[i] == "-s" && i + 1 < args.Length)
                    symbolName = args[i + 1];
                if (args[i] == "-d" && i + 1 < args.Length)
                    datFile = args[i + 1];
                if (args[i] == "-ow")
                    yesOverwrite = true;
                if (args[i] == "-w")
                    disableWarnings = true;
                if (args[i] == "-c")
                    clean = true;
                if (args[i] == "-t" && i + 1 < args.Length)
                    fightFuncTable = File.ReadAllLines(args[i + 1]);
                if (args[i] == "-q")
                    quiet = true;
            }

            // don't allow both item tables and normal tables
            if(inputs.Count > 0 && itemInputs.Count > 0)
            {
                Console.WriteLine("Only -i or -ii can be used at once, not both");
                Console.ReadLine();
                return;
            }
            
            // print instruction and exit if no input
            if ((inputs.Count == 0
                 && itemInputs.Count == 0)
                || args.Length == 0)
            {
                PrintInstruction();
                Console.ReadLine();
                return;
            }

            // check if we should inject dat
            if (!string.IsNullOrEmpty(datFile))
            {
                output = datFile;
                injectDat = true;
            }

            // create output path if one isn't entered
            if (string.IsNullOrEmpty(output))
                output = Path.Combine(Path.GetDirectoryName(inputs[0]), Path.GetFileName(inputs[0]).Replace(Path.GetExtension(inputs[0]), ".dat"));
            
            // check if output already exists
            if (File.Exists(output) && !yesOverwrite)
            {
                Console.WriteLine(output + " already exists, overwrite? (y/n)");
                if (Console.ReadLine().Trim().ToLower() != "y")
                    return;
            }

            // compile functions
            HSDAccessor function = null;

            if (itemInputs.Count == 0)
                function = CompileInput(inputs.ToArray(), fightFuncTable, quiet, disableWarnings, clean);
            else
            {
                function = new HSDAccessor() { _s = new HSDStruct(4) };

                int count = 0;
                foreach(var f in itemInputs)
                {
                    count = Math.Max(count, f.Item1 + 1);

                    if (4 + 4 * (f.Item1 + 1) > function._s.Length)
                        function._s.Resize(4 + 4 * (f.Item1 + 1));

                    var relocFunc = CompileInput(new string[] { f.Item2 }, fightFuncTable, quiet, disableWarnings, clean);
                    function._s.SetReference(4 + 0x04 * f.Item1, relocFunc);
                }
                function._s.SetInt32(0x00, count);
            }
            
            // create DAT file
            if (function != null)
            {
                HSDRawFile f;

                if (injectDat)
                    f = new HSDRawFile(output);
                else
                    f = new HSDRawFile();

                // generate root
                var root = new HSDRootNode();
                root.Name = symbolName;
                root.Data = function;

                // if this symbol already exists in file, replace it
                foreach (var ro in f.Roots)
                {
                    if (ro.Name.Equals(root.Name))
                    {
                        ro.Data = root.Data;
                    }
                }
                // if symbol is not in file, then add it
                if (f.Roots.FindIndex(e => e.Name == root.Name) == -1)
                    f.Roots.Add(root);

                // save new file
                f.Save(output);

                // We did it boys
                Console.WriteLine();
                Console.WriteLine("Sucessfully Compiled and Converted to DAT!");
            }

            Console.WriteLine();
            Console.WriteLine("press enter to exit...");
            Console.ReadLine();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool CheckFileExists(string input)
        {
            if (!File.Exists(input) || (Path.GetExtension(input) != ".c" && Path.GetExtension(input) != ".o"))
            {
                Console.WriteLine($"Error:\n{input} does not exist or is invalid\nPress enter to exit");
                Console.ReadLine();
                return false;
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <param name="fightFuncTable"></param>
        /// <param name="quiet"></param>
        /// <returns></returns>
        private static HSDAccessor CompileInput(string[] inputs, string[] funcTable, bool quiet, bool disableWarnings, bool clean)
        {
            if (inputs.Length == 0)
                return null;

            if (Environment.OSVersion.Platform != PlatformID.Win32NT)
                throw new Exception("Invalid platform " + Environment.OSVersion.Platform.ToString());

            var devkitpath = Environment.GetEnvironmentVariable("DEVKITPPC").Replace("opt/", "C:/");

            if (string.IsNullOrEmpty(devkitpath))
                throw new FileNotFoundException("DEVKITPPC path not set");

            var gccPath = Path.Combine(devkitpath, "bin/powerpc-eabi-gcc.exe");

            if (!File.Exists(gccPath))
                devkitpath.Replace("C:/", "");

            if (!File.Exists(gccPath))
                throw new FileNotFoundException("powerpc-eabi-gcc.exe not found at " + gccPath);

            var ext = Path.GetExtension(inputs[0]).ToLower();
            List<RelocELF> elfs = new List<RelocELF>();

            foreach(var input in inputs)
            {
                if (ext.Equals(".o"))
                    elfs.Add(new RelocELF(File.ReadAllBytes(input)));

                if (ext.Equals(".c"))
                {
                    Process p = new Process();

                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.WorkingDirectory = Path.GetDirectoryName(input);
                    p.StartInfo.RedirectStandardOutput = true;
                    p.StartInfo.FileName = gccPath;
                    p.StartInfo.Arguments = $"-MMD -MP -MF -Wall -DGEKKO -mogc -mcpu=750 -meabi -mhard-float -c \"{input}\" {(disableWarnings ? "-w" : "")} -O2";
                    p.Start();

                    p.WaitForExit();

                    var outputPath = Path.Combine(Path.GetDirectoryName(input), Path.GetFileNameWithoutExtension(input) + ".o");

                    if (p.ExitCode != 0 || !File.Exists(outputPath))
                    {
                        Console.WriteLine();
                        throw new InvalidDataException($"{Path.GetFileName(input)} failed to compile, see output above for details");
                    }
                    
                    elfs.Add(new RelocELF(File.ReadAllBytes(outputPath)));

                    if (clean)
                        File.Delete(outputPath);
                }
            }

            return RelocELF.GenerateFunctionDAT(elfs.ToArray(), funcTable, quiet);
        }

        /// <summary>
        /// 
        /// </summary>
        private static void PrintInstruction()
        {
            Console.WriteLine(@"MexFF Compiler

Usage: mexff.exe (type) (options)
Options:
    -i (file.c)                          : Input File (.c and .o (elf)) files are supported can have multiple inputs for 'it' type
    -ii (item index) (file(.c, .o(ELF))) : specify index used for building ftItem tables
    -o (file.dat)                        : output dat filename
    -d (file.dat)                        : dat file to inject symbol into
    -ow                                  : overwrites files if it exists
    -s (symbol)                          : symbol name (default is ftFunction for ft and itFunction for it)
    -t (file.txt)                        : specify symbol table list
    -q                                   : Quiet Mode (Console doesn't print information)
    -w                                   : disable compilation warnings
    -c                                   : clean build (deletes .o files after compilation)

Ex: mexff.exe -i 'main.c' -o 'ftFunction.dat' -q
Compile 'main.c' and outputs 'ftFunction.dat' in quiet mode

Ex: mexff.exe -ii 0 'bomb.c' -ii 3 'arrow.c' -d 'PlMan.dat' -s 'itFunction'
Creates function table with bomb.c with index 0 and arrow.c with index 3
and injects it into PlMan.dat with the symbol name itFunction

Note: in order to compile .c files you must 
have DEVKITPPC installed and have the environment path setup");
        }
    }
}
