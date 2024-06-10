namespace warehouse_management_core.DTO_s;

public class ItemDTO
{
    public Id Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public DateTime ExpirationDate { get; set; }
    public int Temperature { get; set; }
    public float Price { get; set; }

    public static implicit operator ItemDTO(Item other) =>
    new()
    {
        Id = other.Id,
        Name = other.Name,
        Description = other.Description,
        ExpirationDate = other.ExpirationDate,
        Temperature = other.Temperature,
        Price = other.Price
    };
}
