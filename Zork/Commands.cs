using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zork
{
    public struct CommandContext
    {
        public string CommandString { get; }
        public Command Command { get; }
        public CommandContext(string commandString, Command command)
        {
            CommandString = commandString;
            Command = command;
        }
    }

    public class Command : IEquatable<Command>
    {
        public string Name { get; set; }
        public string[] Verbs { get; }
        public Action<Game, CommandContext> Action { get; }
        public Command(string name, string verb, Action<Game, CommandContext> action) :
            this(name, new string[] { verb }, action)
        {
        }
        public Command(string name, IEnumerable<string> verbs, Action<Game, CommandContext> action)
        {
            Name = name;
            Verbs = verbs.ToArray();
            Action = action;
        }

        public static bool operator ==(Command lhs, Command rhs)
        {
            if (ReferenceEquals(lhs, rhs)) return true;
            if (lhs is null || rhs is null) return false;

            return lhs.Name == rhs.Name;
        }

        public static bool operator !=(Command lhs, Command rhs) => !(lhs == rhs);
        public bool Equals(Command other) => this == other;
        public override bool Equals(object obj) => obj is Command ? this == (Command)obj : false;
        public override int GetHashCode() => Name.GetHashCode();
        public override string ToString() => Name;
    }

    public class CommandManager
    {
        public CommandManager() => mCommands = new HashSet<Command>();
        public CommandManager(IEnumerable<Command> commands) => mCommands = new HashSet<Command>(commands);
        public CommandContext Parse(string commandString)
        {
            var commandQuery = from command in mCommands
                               where command.Verbs.Contains(commandString, StringComparer.OrdinalIgnoreCase)
                               select new CommandContext(commandString, command);
            return commandQuery.FirstOrDefault();
        }
        public bool PerformCommand(Game game, string CommandString)
        {
            bool result;
            CommandContext commandContext = Parse(CommandString);
            if (commandContext.Command.Action != null)
            {
                commandContext.Command.Action(game, commandContext);
                result = true;
            }
            else
            {
                result = false;
            }
            return result;
        }
        public void AddCommand(Command command) => mCommands.Add(command);
        public void RemoveCommand(Command command) => mCommands.Remove(command);
        public void AddCommands(IEnumerable<Command> commands) => mCommands.UnionWith(commands);
        public void ClearCommands() => mCommands.Clear();

        private HashSet<Command> mCommands;
    }

    [CommandClass]
    public static class MovementCommands
    {
        [Command("NORTH", new string[] { "NORTH", "N" })]
        public static void North(Game game, CommandContext commandContext) => Move(game, Directions.North);
        [Command("SOUTH", new string[] { "SOUTH", "S" })]
        public static void South(Game game, CommandContext commandContext) => Move(game, Directions.South);
        [Command("EAST", new string[] { "EAST", "E" })]
        public static void East(Game game, CommandContext commandContext) => Move(game, Directions.East);
        [Command("WEST", new string[] { "WEST", "W" })]
        public static void West(Game game, CommandContext commandContext) => Move(game, Directions.West);

        private static void Move(Game game, Directions direction)
        {
            bool playerMoved = game.thePlayer.Move(direction);
            if (playerMoved)
            {
                Console.WriteLine("The way is shut!");
            }
        }
    }

    [CommandClass]
    public static class QuitCommand
    {
        [Command("QUIT", new string[] { "QUIT", "Q", "GOODBYE", "BYE" })]
        public static void Quit(Game game, CommandContext context)
        {
            if (game.ConfirmAction("Are you sure you want to quit?"))
            {
                game.Quit();
            }
        }
    }
    [CommandClass]
    public static class LookCommand
    {
        [Command("LOOK", new string[] { "LOOK", "L" })]
        public static void Load(Game game, CommandContext context) => Console.WriteLine(game.thePlayer.Location.Description);
    }
    [CommandClass]
    public static class RestartCommand
    {
        [Command("RESTART", "RESTART")]
        public static void Restart(Game game, CommandContext context)
        {
            if (game.ConfirmAction("Are you sure you want to quit?"))
            {
                game.Restart();
            }
        }
    }
}
