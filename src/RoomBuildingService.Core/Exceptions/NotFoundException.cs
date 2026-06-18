namespace RoomBuildingService.Core.Exceptions;
public class NotFoundException : Exception
    {
        public NotFoundException(string entity, Guid id)
        : base($"{entity} với Id '{id}' không tồn tại.") { }
    }