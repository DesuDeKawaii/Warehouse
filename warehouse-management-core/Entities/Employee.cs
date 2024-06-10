namespace warehouse_management_core.Entities;

public class Employee : IEntity
{
    public Id Id { get; set; }
    public string Name { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public double Salary { get; set; }


    public virtual ICollection<Shop> Shops { get; set; }
    public virtual ICollection<Storage> Storages { get; set; }
}
