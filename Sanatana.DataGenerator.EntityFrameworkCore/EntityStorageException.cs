using System;

namespace Sanatana.DataGenerator.EntityFrameworkCore
{
    public class EntityEfStorageException : Exception
    {
        public Type EntityType { get; }
        public string MethodName { get; }

        public EntityEfStorageException(Type entityType, string methodName, Exception innerException)
            : base($"An error occurred in {methodName} while storing entity of type {entityType.Name}.", innerException)
        {
            MethodName = methodName;
            EntityType = entityType;
        }
    }
}
