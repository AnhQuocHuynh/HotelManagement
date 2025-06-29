using System;

namespace HotelManager.Exceptions
{
    public class EntityNotFoundException : BusinessException
    {
        public string EntityType { get; }
        public object EntityId { get; }

        public EntityNotFoundException(string entityType, object entityId)
            : base($"{entityType} with ID {entityId} not found", $"Không tìm thấy {entityType} với mã {entityId}", "ENTITY_NOT_FOUND")
        {
            EntityType = entityType;
            EntityId = entityId;
        }
    }
} 