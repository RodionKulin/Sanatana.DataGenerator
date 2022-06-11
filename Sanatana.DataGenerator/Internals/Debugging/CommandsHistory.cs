using Sanatana.DataGenerator.Commands;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sanatana.DataGenerator.Internals.Debugging
{
    public class CommandsHistory
    {
        public Stack<string> History { get; protected set; }
        protected int _clearHistoryCount;


        public CommandsHistory()
        {
            History = new Stack<string>();
        }


        public virtual void TrackCommand(ICommand command)
        {
            History.Push(command.GetLogEntry());

            if (History.Count > 1100)
            {
                History = new Stack<string>(History.Take(100).Reverse());
                _clearHistoryCount++;
            }
        }

        public virtual string Combine()
        {
            return string.Join(Environment.NewLine, History);
        }

        public virtual void Clear()
        {
            History.Clear();
        }
    }
}
