using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Helpers;
using WebApplication1.Models;
using WebApplication1.DTO;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(ApplicationDbContext context, ILogger<OrdersController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrders(CancellationToken cancellationToken)
        {
            var orders = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product.Supplier)
                .ToListAsync(cancellationToken);

            var orderDtos = orders.Select(o => new OrderDto
            {
                OrderDate = o.OrderDate,
                TotalAmount = o.TotalAmount,
                FirstName = o.Customer.FirstName,
                LastName = o.Customer.LastName,
                Items = o.OrderItems.Select(oi => new OrderItemDto
                {
                    ProductName = oi.Product.ProductName,
                    SupplierCompanyName = oi.Product.Supplier.CompanyName,
                    UnitPrice = oi.UnitPrice,
                    Quantity = oi.Quantity
                }).ToList()
            }).ToList();

            return Ok(orderDtos);
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<OrderDto>> GetOrder(int id, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Fetching order with ID {id}");

            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product.Supplier)
                .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

            if (order == null)
            {
                _logger.LogWarning($"Order with ID {id} not found");
                return NotFound();
            }

            var orderDto = new OrderDto
            {
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                FirstName = order.Customer.FirstName,
                LastName = order.Customer.LastName,
                Items = order.OrderItems.Select(oi => new OrderItemDto
                {
                    ProductName = oi.Product.ProductName,
                    SupplierCompanyName = oi.Product.Supplier.CompanyName,
                    UnitPrice = oi.UnitPrice,
                    Quantity = oi.Quantity
                }).ToList()
            };

            return Ok(orderDto);
        }



[HttpPost]
public async Task<ActionResult<Order>> PostOrder(CreateOrderDto createOrderDto, CancellationToken cancellationToken)
{
    try
    {
        var customer = await _context.Customers.FirstOrDefaultAsync(
            c => c.FirstName == createOrderDto.FirstName && c.LastName == createOrderDto.LastName,
            cancellationToken);

        if (customer == null)
        {
            customer = new Customer
            {
                FirstName = createOrderDto.FirstName,
                LastName = createOrderDto.LastName,
                City = "Unknown",
                Country = "Unknown",
                Phone = "0000000000"  
            };
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync(cancellationToken);
        }

        var order = new Order
        {
            OrderDate = DateTime.UtcNow,
            CustomerId = customer.Id,
            TotalAmount = 0
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync(cancellationToken);

        foreach (var itemDto in createOrderDto.Items)
        {
            var product = await _context.Products
                                         .FirstOrDefaultAsync(p => p.ProductName == itemDto.ProductName, cancellationToken);

            if (product == null)
            {
                return BadRequest($"Product with name {itemDto.ProductName} does not exist.");
            }

            var supplier = await _context.Suppliers
                                         .FirstOrDefaultAsync(s => s.Id == product.SupplierId && s.CompanyName == itemDto.SupplierCompanyName, cancellationToken);

            if (supplier == null)
            {
                return BadRequest($"Supplier with company name {itemDto.SupplierCompanyName} does not exist for product {itemDto.ProductName}.");
            }

            var orderItem = new OrderItem
            {
                OrderId = order.Id,
                ProductId = product.Id,
                Quantity = itemDto.Quantity,
                UnitPrice = product.UnitPrice
            };

            _context.OrderItems.Add(orderItem);
            order.TotalAmount += product.UnitPrice * itemDto.Quantity;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "An error occurred while creating the order");
        return StatusCode(500, new { title = "Internal server error. Please retry later.", status = 500 });
    }
}




        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id, CancellationToken cancellationToken)
        {
            var order = await _context.Orders.FindAsync(new object[] { id }, cancellationToken);
            if (order == null)
            {
                return NotFound();
            }

            var orderItems = await _context.OrderItems.Where(oi => oi.OrderId == id).ToListAsync(cancellationToken);
            _context.OrderItems.RemoveRange(orderItems);

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync(cancellationToken);

            return NoContent();
        }
    }
}
