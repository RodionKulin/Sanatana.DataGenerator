using Sanatana.DataGenerator.Supervisors.Complete;
using Sanatana.DataGenerator.Supervisors.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.DataGeneratorSpecs.TestTools.DataProviders
{
    internal class PublicPropsCompleteSupervisor : CompleteSupervisor
    {
        public IFlushCandidatesRegistry FlushCandidatesRegistry { get { return _flushCandidatesRegistry; } }
        public IRequiredQueueBuilder RequiredQueueBuilder { get { return _requiredQueueBuilder; } }
        public INextNodeFinder NextNodeFinder { get { return _nextNodeFinder; } }
    }
}
