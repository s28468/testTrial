namespace WebApplication1.DTO;

public class CreateOrderDto
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public List<OrderItemDto> Items { get; set; }
}

public class OrderItemDto
{
    public string ProductName { get; set; }
    public string SupplierCompanyName { get; set; }
    public int UnitPrice { get; set; }
    public int Quantity { get; set; }
}

public class OrderDto
{
    public DateTime OrderDate { get; set; }
    public int TotalAmount { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public List<OrderItemDto> Items { get; set; }
}