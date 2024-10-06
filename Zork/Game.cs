using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Newtonsoft.Json;
using System.IO;
using System.Reflection;
using System.Text;
using System;
using Zork;

namespace Zork
{
    public class Game
    {
        [JsonIgnore]
        public static Game Instance { get; private set; }
        public World theWorld { get; set; }

        [JsonIgnore]
        public Player thePlayer { get; private set; }

        [JsonIgnore]
        private bool IsRunning { get; set; }
        [JsonIgnore]
        public CommandManager CommandManager { get; }

        public Game() => CommandManager = new CommandManager();

        public Game(World world, Player player)
        {
            thePlayer = player;
            theWorld = world;
        }
        [JsonIgnore]
        private int five = 5;

        public static void Start(string gameFilename)
        {
            if (!File.Exists(gameFilename))
            {
                throw new FileNotFoundException("Expected file.", gameFilename);
            }
            while (Instance == null || Instance.mIsRestarting)
            {
                Instance = Load(gameFilename);
                Instance.LoadComments();
                Instance.LoadScripts();
                Instance.DisplayWelcomeMessage();
                Instance.Run();
            }
        }

        public void Run()
        {
            IsRunning = true;
            Room previousRoom = null;

            while (IsRunning)
            {
                Console.WriteLine(thePlayer.Location);
                if (previousRoom != thePlayer.Location)
                {
                    CommandManager.PerformCommand(this, "LOOK");
                    previousRoom = thePlayer.Location;
                }

                Console.Write("\n> ");
                if (CommandManager.PerformCommand(this, Console.ReadLine().Trim()))
                {
                    thePlayer.Moves++;
                }
                else
                {
                    Console.WriteLine("That's not a verb I recognize.");
                }
            }
        }

        public void Restart()
        {
            mIsRunning = false;
            mIsRestarting = true;
            Console.Clear();
        }
        public void Quit() => mIsRunning = false;

        public static Game Load(string filename)
        {
            Game game = JsonConvert.DeserializeObject<Game>(File.ReadAllText(filename));
            if (!(game == null))
            {
                game.thePlayer = game.theWorld.SpawnPlayer();
            }
            else
            {
                Console.WriteLine("Oh No");
            }

            return game;
        }
        private void LoadComments()
        {
            var commandMethods = (from type in Assembly.GetExecutingAssembly().GetTypes()
                                  from method in type.GetMethods()
                                  let attribute = method.GetCustomAttribute<CommandAttribute>()
                                  where type.IsClass && type.GetCustomAttribute<CommandClassAttribute>() != null
                                  where attribute != null
                                  select new Command(attribute.CommandName, attribute.Verbs,
                                  (Action<Game, CommandContext>)Delegate.CreateDelegate(typeof(Action<Game, CommandContext>), method)));
            CommandManager.AddCommands(commandMethods);
        }
        private void LoadScripts()
        {
            foreach (string file in Directory.EnumerateFiles(ScriptDirectory, ScriptFileExtension))
            {
                try
                {
                    var scriptOptions = ScriptOptions.Default.AddReferences(Assembly.GetExecutingAssembly());
#if DEBUG
                    scriptOptions = scriptOptions.WithEmitDebugInformation(true)
                        .WithFilePath(new FileInfo(file).FullName)
                        .WithFileEncoding(Encoding.UTF8);
#endif

                    string script = File.ReadAllText(file);
                    CSharpScript.RunAsync(script, scriptOptions).Wait();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error compiling script: {file} Error: {ex.Message}");
                }
            }
        }
        public bool ConfirmAction(string prompt)
        {
            Console.Write(prompt);

            while (true)
            {
                string response = Console.ReadLine().Trim().ToUpper();
                if (response == "YES" || response == "Y")
                {
                    return true;
                }
                else if (response == "NO" || response == "N")
                {
                    return false;
                }
                else
                {
                    Console.WriteLine("Please answer yes or no. > ");
                }
            }
        }
        private void DisplayWelcomeMessage() => Console.WriteLine(WelcomeMessage);
        public static readonly Random Random = new Random();
        private static readonly string ScriptDirectory = "Scripts";
        private static readonly string ScriptFileExtension = "*.csx";

        [JsonProperty]
        private string WelcomeMessage = null;
        private bool mIsRunning;
        private bool mIsRestarting;
    }
}