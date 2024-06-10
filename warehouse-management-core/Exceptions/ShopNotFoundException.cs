namespace warehouse_management_core.Exceptions;

public class ShopNotFoundException(Guid wrongId) : Exception($"Shop {wrongId} is not found.") { }
