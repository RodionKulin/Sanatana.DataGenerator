using Sanatana.DataGenerator.Internals.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace Sanatana.DataGenerator.Internals.Debugging
{
    public class CommandsHistory
    {
        //Stack shows latest rows inserted as first, which is easier for debugging
        public Stack<string> Logs { get; protected set; }
        protected int _clearHistoryCount;


        //init
        public CommandsHistory()
        {
            Logs = new Stack<string>();
        }


        public virtual void LogCommand(ICommand command)
        {
            string logEntry = command.GetLogEntry();

            Logs.Push(logEntry);
            if (Logs.Count > 1100)
            {
                Logs = new Stack<string>(Logs.Take(100).Reverse());
                _clearHistoryCount++;
            }

        }

        public virtual string Combine()
        {
            return string.Join(Environment.NewLine, Logs);
        }

        public virtual void Clear()
        {
            Logs.Clear();
        }
    }
}
