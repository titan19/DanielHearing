using Jint;
using Jint.Native;
using Jint.Parser;
using Jint.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Daniel
{
    class Program
    {
        static void Main(string[] args)
        {
            string script = args != null && args.Length > 0 ? args[0] : @"examples/test.dan";
            Runtime rt = new Runtime();
            rt.LoadingScripts(Directory.GetFiles(
                            Directory.GetCurrentDirectory(),
                            "*.js",
                            SearchOption.AllDirectories));
            string[] code = File.ReadAllLines(script);
            rt.Init();
            rt.Execute(new CodeBase(code));
            Console.WriteLine("Goodbye!");
        }
    }
}
