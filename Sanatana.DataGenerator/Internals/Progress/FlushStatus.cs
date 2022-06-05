using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.DataGenerator.Internals.Progress
{
    public enum FlushStatus : long
    {
        FlushNotRequired,
        FlushRequired,
        FlushInProgress,
        Flushed,
        FlushedAndReleased
    }
}
