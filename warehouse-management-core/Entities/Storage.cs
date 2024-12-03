namespace warehouse_management_core.Entities;

public class Storage : IEntity
{
    public Id Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int Capacity { get; set; }
    public double Longitude { get; set; }
    public double Latitude { get; set; }

    public int Temperature { get; set; }


    public virtual ICollection<ItemStorage> ItemsStorage { get; set; }
    public virtual ICollection<Shop> Shops { get; set; }
    public virtual ICollection<Employee> Employees { get; set; }
}
