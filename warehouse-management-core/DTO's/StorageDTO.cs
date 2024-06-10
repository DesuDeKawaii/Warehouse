namespace warehouse_management_core.DTO_s;

public class StorageDTO
{
    public Id Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int Capacity { get; set; }

    public int Temperature { get; set; }

    public static implicit operator StorageDTO(Storage other) =>
    new()
    {
        Id = other.Id,
        Name = other.Name,
        Description = other.Description,
        Capacity = other.Capacity,
        Temperature = other.Temperature
    };
}
