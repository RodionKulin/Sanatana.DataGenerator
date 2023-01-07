using AutoBogus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.DataGenerator.AutoBogus.Binders
{
    /// <summary>
    /// Exclude properties from auto generation by running IExclusionAutoBinderModule filtering modules.
    /// </summary>
    public class ExclusionAutoBinder : AutoBinder
    {
        //fields
        private List<IExclusionAutoBinderModule> _exclusionModules;


        //init 
        public ExclusionAutoBinder()
        {
            _exclusionModules = new List<IExclusionAutoBinderModule>();
        }

        public ExclusionAutoBinder(IExclusionAutoBinderModule exclusionModule)
        {
            _exclusionModules = new List<IExclusionAutoBinderModule>()
            {
                exclusionModule
            };
        }

        public ExclusionAutoBinder(params IExclusionAutoBinderModule[] exclusionModules)
        {
            _exclusionModules = exclusionModules.ToList();
        }


        //methods
        public override Dictionary<string, MemberInfo> GetMembers(Type type)
        {
            Dictionary<string, MemberInfo> members = base.GetMembers(type);

            foreach (IExclusionAutoBinderModule exclusionModule in _exclusionModules)
            {
                members = exclusionModule.FilterMembers(type, members);
            }

            return members;
        }
    }

}
