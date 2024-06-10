namespace warehouse_management_core.Exceptions;

public class SimiliarItemException(Guid wrongId) : Exception($"Item {wrongId} is not found.") { }
