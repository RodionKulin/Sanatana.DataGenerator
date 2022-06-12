using Sanatana.DataGenerator.Supervisors.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.DataGenerator.Internals.Progress
{
    public class ProgressEventTrigger : IDisposable
    {
        //fields
        protected GeneratorSetup _generatorSetup;
        protected decimal _lastPercents;
        protected long invokeOnEveryNCommand = 1000;


        //events
        /// <summary>
        /// Progress change event that will report overall completion percent in range from 0 to 100.
        /// </summary>
        public event Action<GeneratorSetup, decimal> Changed;


        //init
        public ProgressEventTrigger(GeneratorSetup generatorSetup)
        {
            _generatorSetup = generatorSetup;
        }


        //methods
        /// <summary>
        /// Internal method to update progress that is called by GeneratorSetup after each ICommand.
        /// </summary>
        /// <param name="forceUpdate"></param>
        protected virtual void UpdateProgress(bool forceUpdate)
        {
            long actionCalls = IdIterator.GetNextId<IProgressState>();

            //trigger event only every N generated instances
            bool invoke = actionCalls % invokeOnEveryNCommand == 0;
            if (!invoke && forceUpdate == false)
            {
                return;
            }

            //check if percent updated
            decimal percents = _generatorSetup.Supervisor.ProgressState.GetCompletionPercents();
            if (_lastPercents == percents)
            {
                return;
            }
            _lastPercents = percents;

            //invoke handler
            Action<GeneratorSetup, decimal> progressChanged = Changed;
            if (progressChanged != null)
            {
                progressChanged(_generatorSetup, percents);
            }
        }

        internal void UpdateProgressInt(bool forceUpdate)
        {
            UpdateProgress(forceUpdate);
        }

        internal void Clear()
        {
            _lastPercents = 0;
        }


        //IDisposalbe
        /// <summary>
        /// Will unsubscribe all ProgressChanged event handlers
        /// </summary>
        public virtual void Dispose()
        {
            if (Changed != null)
            {
                foreach (Delegate d in Changed.GetInvocationList())
                    Changed -= d as Action<GeneratorSetup, decimal>;
            }
        }
    }
}
