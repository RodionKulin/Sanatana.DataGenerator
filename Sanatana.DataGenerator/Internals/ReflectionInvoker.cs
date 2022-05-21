using Sanatana.DataGenerator.Generators;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Linq;
using System.Collections;
using System.Threading.Tasks;
using Sanatana.DataGenerator.Storages;
using Sanatana.DataGenerator.Entities;
using Sanatana.DataGenerator.Modifiers;

namespace Sanatana.DataGenerator.Internals
{
    public class ReflectionInvoker
    {
        //fields
        protected static Dictionary<Type, MethodInfo> _modifyMethods;
        protected static Dictionary<Type, MethodInfo> _insertMethods;
        protected static Dictionary<Type, MethodInfo> _selectMethods;


        //init
        public ReflectionInvoker()
        {
            _modifyMethods = new Dictionary<Type, MethodInfo>();
            _insertMethods = new Dictionary<Type, MethodInfo>();
            _selectMethods = new Dictionary<Type, MethodInfo>();
        }


        //methods
        protected virtual MethodInfo GetCachedMethodInfo(Type targetType, string methodName,
            Type entityType, Dictionary<Type, MethodInfo> methods)
        {
            MethodInfo targetMethod;

            if (!methods.ContainsKey(entityType))
            {
                targetMethod = targetType.GetMethods().First(x => x.Name == methodName);
                targetMethod = targetMethod.MakeGenericMethod(entityType);
                methods.Add(entityType, targetMethod);
            }

            return methods[entityType];
        }

        public virtual IList InvokeModify(IModifier modifier, 
            GeneratorContext context, object generatedEntities)
        {
            Type entityType = context.Description.Type;
            Type targetType = typeof(IModifier);
            string methodName = nameof(IModifier.Modify);
            MethodInfo modifyMethod = GetCachedMethodInfo(targetType, methodName, entityType, _modifyMethods);

            object result = modifyMethod.Invoke(modifier, new object[] { context, generatedEntities });
            return (IList)result;
        }

        public virtual Task InvokeInsert(IPersistentStorage storage, 
            IEntityDescription description, IList itemsList)
        {
            Type targetType = typeof(IPersistentStorage);
            string methodName = nameof(IPersistentStorage.Insert);
            MethodInfo insertMethod = GetCachedMethodInfo(targetType, methodName, description.Type, _insertMethods);

            object result = insertMethod.Invoke(storage, new object[] { itemsList });
            return (Task)result;
        }

        public virtual IList CreateEntityList(Type entityType)
        {
            Type listType = typeof(List<>);
            Type constructedListType = listType.MakeGenericType(entityType);
            return (IList)Activator.CreateInstance(constructedListType);
        }

    }
}
