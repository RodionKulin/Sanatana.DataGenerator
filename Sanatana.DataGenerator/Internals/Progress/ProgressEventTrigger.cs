using Sanatana.DataGenerator.Supervisors.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.DataGenerator.Internals.Progress
{
    public class ProgressEventTrigger
    {
        //fields
        protected ISupervisor _supervisor;
        protected decimal _lastPercents;
        protected long invokeOnEveryNCommand = 1000;
        /// <summary>
        /// Progress change event handlers that will receive completion percent change events. Percents are supplied in form in range from 0 to 100.
        /// </summary>
        protected List<Action<decimal>> _eventHandlers;



        //init
        public ProgressEventTrigger()
        {}

        public ProgressEventTrigger(List<Action<decimal>> eventHandlers)
        {
            _eventHandlers = eventHandlers;
        }

        public virtual ProgressEventTrigger Clone()
        {
            _eventHandlers = _eventHandlers == null ? null : new List<Action<decimal>>(_eventHandlers);
            return new ProgressEventTrigger(_eventHandlers);
        }

        public virtual void Setup(ISupervisor supervisor)
        {
            _supervisor = supervisor;
            _lastPercents = 0;
        }



        //methods
        public virtual void Subscribe(Action<decimal> eventHandler)
        {
            _eventHandlers = _eventHandlers ?? new List<Action<decimal>>();
            _eventHandlers.Add(eventHandler);
        }

        public virtual void Unsubscribe(Action<decimal> eventHandler)
        {
            if(_eventHandlers == null)
            {
                return;
            }
            _eventHandlers.Remove(eventHandler);
        }

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
            decimal percents = _supervisor.ProgressState.GetCompletionPercents();
            if (_lastPercents == percents)
            {
                return;
            }
            _lastPercents = percents;

            //invoke handlers
            if (_eventHandlers != null)
            {
                foreach (Action<decimal> eventHandler in _eventHandlers)
                {
                    eventHandler(percents);
                }
            }
        }

        internal void UpdateProgressInt(bool forceUpdate)
        {
            UpdateProgress(forceUpdate);
        }


    }
}
