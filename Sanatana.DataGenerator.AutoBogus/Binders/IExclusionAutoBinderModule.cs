using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.DataGenerator.AutoBogus.Binders
{
    public interface IExclusionAutoBinderModule
    {
        Dictionary<string, MemberInfo> FilterMembers(Type type, Dictionary<string, MemberInfo> members);
    }
}
