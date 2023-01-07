using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.DataGenerator.AutoBogus.Binders
{
    /// <summary>
    /// Exclude properties that duplicate enum properties as strings to save in database.
    /// </summary>
    public class ExcludeEnumAutoBinderModule : IExclusionAutoBinderModule
    {
        private string _dependentPropertiesPattern = "{0}Db";


        //init
        public ExcludeEnumAutoBinderModule(string dependentPropertiesPattern = "{0}Db")
        {
            _dependentPropertiesPattern = dependentPropertiesPattern
                ?? throw new ArgumentNullException(nameof(dependentPropertiesPattern));
        }


        //methods
        public Dictionary<string, MemberInfo> FilterMembers(Type type, Dictionary<string, MemberInfo> members)
        {
            string[] enumPropNames = members.Values
                .Where(x => x.MemberType == MemberTypes.Property)
                .Select(x => (PropertyInfo)x)
                .Where(x => x.PropertyType.IsEnum)
                .Select(x => x.Name)
                .ToArray();

            string[] dependentPropertiesToExclude = enumPropNames
                .Select(name => string.Format(_dependentPropertiesPattern, name))
                .ToArray();
            members = members.Where(x => !dependentPropertiesToExclude.Contains(x.Key))
                .ToDictionary(x => x.Key, x => x.Value);

            return members;
        }
    }
}
