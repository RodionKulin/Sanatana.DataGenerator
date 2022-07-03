using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.DataGenerator.Internals.Extensions
{
    public static class TypeExtensions
	{
		public static bool IsGenericTypeOf(this Type genericType, Type someType)
		{
			if (someType.IsGenericType
				&& genericType == someType.GetGenericTypeDefinition())
			{
				return true;
			}

			if(someType.BaseType == null)
            {
				return false;
            }

			return IsGenericTypeOf(genericType, someType.BaseType);
		}
	}
}
