using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace YikesInterpreter.Entities
{
    public class YikesAction
    {
        public Action ActionType;
        public List<string> Parameters = new List<string>();
        public List<YikesAction> Children = new List<YikesAction>();
        public YikesAction Parent;

        public string Metaname = "";
        public string Metavalue = "";

        public string FullRead = "";

        public List<YikesAction> GetAvailableMethods()
        {
            var result = new List<YikesAction>();
            result.AddRange(Parent.Children.Where(x => x.ActionType == Action.SetMethod));
            result.AddRange(Children.Where(x => x.ActionType == Action.SetMethod));
            return result;
        }

        public static YikesAction ReadNext(StringStream reader)
        {
            var action = new YikesAction();
            var state = ReadState.StartCall;
            var readdata = "";
            bool instring = false;
            bool incomment = false;

            while(reader.ReadNextChar(out var c))
            {
                action.FullRead += c;

                if (c == '$')
                {
                    incomment = !incomment;
                    continue;
                }

                if (incomment)
                    continue;

                if (c == '"')
                {
                    instring = !instring;
                    continue;
                }

                if (!instring)
                {
                    if(c == ' ' || c == '\n' || c == '\r')
                    {
                        continue;
                    }
                }

                switch (state)
                {
                    case ReadState.StartCall:
                        if (c == '@')
                        {
                            state = ReadState.CallName;
                            continue;
                        }
                        else if (c == '^')
                        {
                            action.ActionType = Action.SetMeta;
                            state = ReadState.ReadMeta;
                            continue;
                        }
                        Console.WriteLine($"wtf, found a(n) {c}");
                        throw new Exception("Yikes, calls start with @!");

                    case ReadState.CallName:
                        if(c == ':')
                        {
                            switch (readdata)
                            {
                                default:
                                    action.Parameters.Add(readdata);
                                    action.ActionType = Action.CallMethod;
                                    break;

                                case "if":
                                    action.ActionType = Action.IfStatement;
                                    break;

                                case "else":
                                    action.ActionType = Action.ElseStatement;
                                    break;

                                case "setvar":
                                    action.ActionType = Action.SetVariable;
                                    break;

                                case "method":
                                    action.ActionType = Action.SetMethod;
                                    break;
                            }
                            readdata = "";
                            state = ReadState.Parameters;
                            continue;
                        }else if(c == '.')
                        {
                            readdata = "";
                            state = ReadState.Done;
                            continue;
                        }
                        readdata += c;
                        continue;

                    case ReadState.Parameters:
                        if(c == '-')
                        {
                            action.Parameters.Add(readdata);
                            readdata = "";
                            continue;
                        }else if(c == '[')
                        {
                            action.Parameters.Add(readdata);
                            readdata = "";
                            state = ReadState.Body;
                            continue;
                        }else if (c == '.')
                        {
                            action.Parameters.Add(readdata);
                            state = ReadState.Done;
                            return action;
                        }
                        readdata += c;
                        continue;

                    case ReadState.Body:
                        if(c == ']')
                        {
                            var str = new StringStream(readdata);
                            while (str.DataLeft)
                            {
                                action.Children.Add(ReadNext(str));
                            }
                            readdata = "";
                            state = ReadState.End;
                            continue;
                        }
                        readdata += c;
                        continue;

                    case ReadState.End:
                        if(c != '.')
                            throw new Exception("Yikes, calls end with .!");
                        state = ReadState.Done;
                        return action;

                    case ReadState.ReadMeta:
                        // Read metadata here
                        if(action.Metaname == "")
                        {
                            if(c == ':')
                            {
                                action.Metaname = readdata;
                                readdata = "";
                                continue;
                            }
                        }
                        if (c == '.')
                        {
                            action.Metavalue = readdata;
                            readdata = "";
                            return action;
                        }
                        readdata += c;
                        continue;

                    case ReadState.Done:
                        return action;
                }
            }
            return action;
        }
    }

    public enum Action
    {
        CallMethod,
        IfStatement,
        ElseStatement,
        SetVariable,
        SetMethod,
        SetMeta
    }

    public enum ReadState
    {
        StartCall,
        CallName,
        Parameters,
        Body,
        End,
        Done,
        ReadMeta
    }
}
