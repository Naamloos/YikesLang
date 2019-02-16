using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using YikesInterpreter.Entities;

namespace YikesInterpreter
{
    public class Interpreter
    {
        List<YikesAction> actions;

        public Interpreter()
        {
            actions = new List<YikesAction>();
        }

        public void CompileScript(StringStream str)
        {
            int i = 0;
            while (str.DataLeft)
            {
                actions.Add(YikesAction.ReadNext(str));
                Console.WriteLine($"line {i} {actions.Last().ActionType}");
                i++;
            }
        }

        string entrymethod = "";
        public void ExecuteScript()
        {
            if (actions.Count < 1)
                throw new Exception("Yikes! No code was loaded!");

            var metadata = actions.Where(x => x.ActionType == Entities.Action.SetMeta);
            if (metadata.Any(x => x.Metaname == "entry"))
                entrymethod = metadata.First(x => x.Metaname == "entry").Metavalue;

            Console.WriteLine($"Entry arg was {entrymethod}");

            var entries = actions.Where(x => x.ActionType == Entities.Action.SetMethod && x.Parameters.Contains(entrymethod));
            if(entries.Any())
            {
                Console.WriteLine($"Launched from entry method {entrymethod}.");
                ExecuteAction(entries.First());
            }
            else
            {
                Console.WriteLine($"Launched from script entry.");
                ExecuteActions(actions);
            }
        }

        private void ExecuteAction(YikesAction action)
        {
            if (action.ActionType == Entities.Action.SetMeta
                || action.ActionType == Entities.Action.SetMethod)
                return;

            switch (action.ActionType)
            {
                case Entities.Action.CallMethod:
                    // Call method
                    if (action.Parameters[0] == "print")
                    {
                        Console.WriteLine(action.Parameters[1]);
                    }
                    else
                    {
                        var methods = action.GetAvailableMethods().Where(x => x.Parameters[0] == action.Parameters[0]);
                        ExecuteAction(methods.First());
                    }
                    break;

                case Entities.Action.ElseStatement:
                    // Else statement
                    break;

                case Entities.Action.IfStatement:
                    // if statement
                    break;

                case Entities.Action.SetVariable:
                    // set variable;
                    break;
            }
        }

        private void ExecuteActions(List<YikesAction> actions)
        {
            foreach (var a in actions)
                ExecuteAction(a);
        }
    }
}
