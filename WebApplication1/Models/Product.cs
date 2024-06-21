namespace WebApplication1.Models;

public class Product
{
    public int Id { get; set; }
    public string ProductName { get; set; }
    public int SupplierId { get; set; }
    public int UnitPrice { get; set; }
    public string Package { get; set; }
    public Supplier Supplier { get; set; }
}