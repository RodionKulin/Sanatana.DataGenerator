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
        protected static Dictionary<Type, MethodInfo> _generateMethods;
        protected static Dictionary<Type, MethodInfo> _modifyMethods;
        protected static Dictionary<Type, MethodInfo> _insertMethods;


        //init
        public ReflectionInvoker()
        {
            _generateMethods = new Dictionary<Type, MethodInfo>();
            _modifyMethods = new Dictionary<Type, MethodInfo>();
            _insertMethods = new Dictionary<Type, MethodInfo>();
        }


        //methods
        public virtual IList InvokeModify(IModifier modifier, 
            GeneratorContext context, object generatedEntities)
        {
            Type targetType = context.Description.Type;
            MethodInfo modifyMethod;

            if (!_modifyMethods.ContainsKey(targetType))
            {
                Type generatorType = typeof(IModifier);
                modifyMethod = generatorType.GetMethods()
                    .First(x => x.Name == nameof(IModifier.Modify));
                modifyMethod = modifyMethod.MakeGenericMethod(targetType);
                _modifyMethods.Add(targetType, modifyMethod);
            }

            modifyMethod = _modifyMethods[targetType];
            object result = modifyMethod.Invoke(modifier, new object[] { context, generatedEntities });
            return (IList)result;
        }

        public virtual Task InvokeInsert(IPersistentStorage storage, 
            IEntityDescription description, IList itemsList)
        {
            Type targetType = description.Type;
            MethodInfo insertMethod;

            if (!_insertMethods.ContainsKey(targetType))
            {
                Type generatorType = typeof(IPersistentStorage);
                insertMethod = generatorType.GetMethods()
                    .First(x => x.Name == nameof(IPersistentStorage.Insert));
                insertMethod = insertMethod.MakeGenericMethod(targetType);
                _insertMethods.Add(targetType, insertMethod);
            }

            insertMethod = _insertMethods[targetType];
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
