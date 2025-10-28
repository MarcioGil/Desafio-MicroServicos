using Microsoft.AspNetCore.Mvc;
using SalesService.Models;
using SalesService.Services;
using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;

namespace SalesService.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    [Produces(MediaTypeNames.Application.Json)]
    public class OrdersController : ControllerBase
    {
        private readonly SalesManagementService _salesService;
        private readonly MessageProducerService _messageProducer;

        public OrdersController(SalesManagementService salesService, MessageProducerService messageProducer)
        {
            _salesService = salesService;
            _messageProducer = messageProducer;
        }

        /// <summary>
        /// Cria um novo pedido de venda.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Order>> PostOrder([FromBody] OrderCreationDto orderDto)
        {
            // 1. Create the order in the database with 'Pending' status.
            var order = await _salesService.CreateOrderAsync(orderDto);
            
            // 2. Publish OrderCreated message to RabbitMQ for StockService to process and update stock.
            _messageProducer.PublishOrderCreated(order);

            return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
        }

        /// <summary>
        /// Obtém um pedido específico pelo ID.
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Order>> GetOrder(int id)
        {
            var order = await _salesService.GetOrderByIdAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            return Ok(order);
        }

        /// <summary>
        /// Obtém a lista de todos os pedidos.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrders()
        {
            return Ok(await _salesService.GetAllOrdersAsync());
        }
    }
}
