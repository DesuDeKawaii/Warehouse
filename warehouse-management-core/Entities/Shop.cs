namespace warehouse_management_core.Entities;

public class Shop : IEntity
{
    public Id Id { get; set; }
    public string Name { get; set; }
    public int Capacity { get; set; }

    public virtual ICollection <ItemShop> ItemsShop { get; set; }
    public virtual ICollection<Employee> Employees { get; set; }
    public virtual ICollection<Storage> Storages { get; set; }
}
