namespace warehouse_management_core.DTO_s;

public class ShopDTO
{
    public Id Id { get; set; }
    public string Name { get; set; }
    public int Capacity { get; set; }
    public double Longitude { get; set; }
    public double Latitude { get; set; }

    public static implicit operator ShopDTO(Shop other) =>
    new()
    {
        Id = other.Id,
        Name = other.Name,
        Capacity = other.Capacity,
        Longitude = other.Longitude,
        Latitude = other.Latitude

    };
}
