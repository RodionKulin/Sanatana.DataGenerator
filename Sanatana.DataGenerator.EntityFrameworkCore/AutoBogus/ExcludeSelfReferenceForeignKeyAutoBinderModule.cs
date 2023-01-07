using Microsoft.EntityFrameworkCore;
using Sanatana.DataGenerator.EntityFrameworkCore.Internals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.DataGenerator.AutoBogus.Binders
{
    /// <summary>
    /// Exclude properties that are foreign keys to same table.
    /// </summary>
    public class ExcludeSelfReferenceForeignKeyAutoBinderModule : IExclusionAutoBinderModule
    {
        private readonly EfCoreModelService _efCoreModelService;


        //init
        public ExcludeSelfReferenceForeignKeyAutoBinderModule(Func<DbContext> dbContextFactory)
        {
            _efCoreModelService = new EfCoreModelService(dbContextFactory);
        }


        //methods
        public Dictionary<string, MemberInfo> FilterMembers(Type type, Dictionary<string, MemberInfo> members)
        {
            PropertyInfo[] selfReferenceForeignKeys = _efCoreModelService.GetSelfReferenceForeignKeys(type);

            string[] selfReferencePropNames = members.Values
                .Where(x => x.MemberType == MemberTypes.Property)
                .Select(x => (PropertyInfo)x)
                .Where(x => selfReferenceForeignKeys.Contains(x))
                .Select(x => x.Name)
                .ToArray();

            members = members.Where(x => !selfReferencePropNames.Contains(x.Key))
                .ToDictionary(x => x.Key, x => x.Value);

            return members;
        }
    }
}
