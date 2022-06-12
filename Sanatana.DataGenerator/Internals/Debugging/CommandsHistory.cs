using Sanatana.DataGenerator.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using Serilog;
using System.IO;

namespace Sanatana.DataGenerator.Internals.Debugging
{
    public class CommandsHistory
    {
        //Stack shows latest rows inserted as first, which is easier for debugging
        public Stack<string> Logs { get; protected set; }
        protected int _clearHistoryCount;
        protected ILogger _logger;
        protected string _logFilePath = "command_logs.log";


        //properties
        public bool IsFileLoggingEnabled { get; set; }


        //init
        public CommandsHistory()
        {
            Logs = new Stack<string>();
            _logger = new LoggerConfiguration()
               .MinimumLevel.Debug()
               .WriteTo.File(_logFilePath, rollingInterval: RollingInterval.Day)
               .CreateLogger();
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

            if (IsFileLoggingEnabled)
            {
                _logger.Information(logEntry);
            }
        }

        public virtual string Combine()
        {
            return string.Join(Environment.NewLine, Logs);
        }

        public virtual void Clear()
        {
            Logs.Clear();
            if (File.Exists(_logFilePath) && IsFileLoggingEnabled)
            {
                File.Delete(_logFilePath);
            }
        }
    }
}
