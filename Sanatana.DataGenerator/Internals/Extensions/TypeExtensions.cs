using Sanatana.DataGenerator.Generators;
using Sanatana.DataGenerator.Modifiers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sanatana.DataGenerator.Internals.Extensions
{
    public static class TypeExtensions
	{
		public static bool IsGenericTypeOf(this Type genericType, Type typeToCheck)
		{
			if (typeToCheck.IsGenericType
				&& genericType == typeToCheck.GetGenericTypeDefinition())
			{
				return true;
			}

			if(typeToCheck.BaseType == null)
            {
				return false;
            }

			return IsGenericTypeOf(genericType, typeToCheck.BaseType);
		}

		public static bool IsSameTypeOrSubclass(this Type derived, Type baseType)
		{
			return derived == baseType || derived.IsSubclassOf(baseType);
		}

		public static bool IsNotParameterizedGenerator(this IGenerator generator)
		{
			return generator == null || !(generator is IDelegateParameterizedGenerator);
		}

		public static bool IsNotParameterizedModifiers(this List<IModifier> modifiers)
		{
			return modifiers == null || modifiers.All(m => !(m is IDelegateParameterizedModifier));
		}
	}
}
