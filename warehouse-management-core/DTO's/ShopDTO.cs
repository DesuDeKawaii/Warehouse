namespace warehouse_management_core.DTO_s;

public class ShopDTO
{
    public Id Id { get; set; }
    public string Name { get; set; }
    public int Capacity { get; set; }

    public static implicit operator ShopDTO(Shop other) =>
    new()
    {
        Id = other.Id,
        Name = other.Name,
        Capacity = other.Capacity,

    };
}
