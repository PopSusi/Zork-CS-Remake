using System;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Reflection;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace Zork
{
    class Program
    {
        static void Main(string[] args)
        {
            const string defaultGameFilename = "RoomsData.json";
            string gameFileName = (args.Length > 0 ? args[(int)CommandLineArguments.GameFilename] : defaultGameFilename);
            
            Game.Start("RoomsData.json");
            Console.WriteLine("Thank you for playing!");
        }

        private enum CommandLineArguments
        {
            GameFilename = 0
        }
    }
}
