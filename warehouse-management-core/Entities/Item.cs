namespace warehouse_management_core.Entities;

public class Item : IEntity
{
    public Id Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public DateTime ExpirationDate { get; set; }
    public int Temperature { get; set; }
    public float Price { get; set; }


    public virtual ItemStorage ItemStorage { get; set; }
    public virtual ItemShop ItemShop { get; set; }
}
