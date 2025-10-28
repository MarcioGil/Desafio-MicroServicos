using System.Text.Json.Serialization;

namespace SalesService.Models
{
    public class OrderCreatedEvent
    {
        public int OrderId { get; set; }
        public string CustomerName { get; set; }
        public List<OrderItemEvent> Items { get; set; }
    }

    public class OrderItemEvent
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
