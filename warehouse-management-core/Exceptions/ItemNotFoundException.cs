namespace warehouse_management_core.Exceptions;

public class ItemNotFoundException(Guid wrongId) : Exception($"Item {wrongId} is not found.") { }
