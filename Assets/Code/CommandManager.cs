using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class CommandManager : MonoBehaviour
{
    public class ConsoleCommand
    {   
        public ConsoleCommand()
        {
            func = DoNothing;
        }

        public ConsoleCommand(string inCommandStr, Func<string[], string> inFunc)
        {
            commandStr = inCommandStr;
            func = inFunc;
        }

        public string Execute(params string[] args)
        {
            return func.Invoke(args);
        }

        private string DoNothing(params string[] args) { return ""; }
        public string GetCommandString() { return commandStr; }

        private string commandStr;

        private Func<string[], string> func;
    };

    public CommandManager()
    {
        AddDefaultCommands();
    }

    private List<ConsoleCommand> commands = new List<ConsoleCommand>();

    public bool AddCommand(string commandStr, Func<string[], string> func)
    {
        commands.Add(new ConsoleCommand(commandStr, func));
        return true;
    }

    public bool ProcessCommand(ref string output, string command, params string[] list)
    {
        try
        {
            ConsoleCommand cmd = commands.Where(x => x.GetCommandString() == command).FirstOrDefault();

            if (cmd == null)
                return false;

            output = cmd.Execute(list);
            return true;
        }
        catch (Exception e)
        {
            string error = "Exception: " + e.ToString();
            Debug.Log(error);
            output = error;
        }

        return false;
    }

    void AddDefaultCommands()
    {
        AddCommand("debugw", BoxCar.Debug.GenericDebugCommands.AddDebugWidget);
    }
}

