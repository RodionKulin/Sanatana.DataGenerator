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
        protected static Dictionary<Type, MethodInfo> _processMethods;
        protected static Dictionary<Type, MethodInfo> _insertMethods;


        //init
        public ReflectionInvoker()
        {
            _generateMethods = new Dictionary<Type, MethodInfo>();
            _processMethods = new Dictionary<Type, MethodInfo>();
            _insertMethods = new Dictionary<Type, MethodInfo>();
        }


        //methods
        public virtual object InvokeGenerate(IGenerator generator, GeneratorContext context)
        {
            Type targetType = context.Description.Type;
            MethodInfo generateMethod;

            if (!_generateMethods.ContainsKey(targetType))
            {
                Type generatorType = typeof(IGenerator);
                generateMethod = generatorType.GetMethods()
                    .First(x => x.Name == nameof(IGenerator.Generate));
                generateMethod = generateMethod.MakeGenericMethod(targetType);
                _generateMethods.Add(targetType, generateMethod);
            }

            generateMethod = _generateMethods[targetType];
            object result = generateMethod.Invoke(generator, new object[] { context });
            return result;
        }

        public virtual IList InvokePostProcess(IModifier modifier, 
            GeneratorContext context, object entities)
        {
            Type targetType = context.Description.Type;
            MethodInfo processMethod;

            if (!_processMethods.ContainsKey(targetType))
            {
                Type generatorType = typeof(IModifier);
                processMethod = generatorType.GetMethods()
                    .First(x => x.Name == nameof(IModifier.Modify));
                processMethod = processMethod.MakeGenericMethod(targetType);
                _processMethods.Add(targetType, processMethod);
            }

            processMethod = _processMethods[targetType];
            object result = processMethod.Invoke(modifier, new object[] { context, entities });
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
