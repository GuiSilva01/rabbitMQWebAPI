using APIOrder.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;
using System.Text;

namespace APIOrder.Controllers
{
    [Route("api/order")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private ILogger<OrderController> _logger;

        public OrderController(ILogger<OrderController> logger)
        {
            _logger = logger;
        }


        public IActionResult IsertOrder(Order order)
        {
            try
            {
                #region insert na fila
                var factory = new ConnectionFactory() { HostName = "localhost" };
                factory.UserName = "admin";
                factory.Password = "123456";
                using (var connection = factory.CreateConnection())
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare(queue: "orderQueue",
                                         durable: false,
                                         exclusive: false,
                                         autoDelete: false,
                                         arguments: null);

                    string message = System.Text.Json.JsonSerializer.Serialize(order);
                    var body = Encoding.UTF8.GetBytes(message);

                    channel.BasicPublish(exchange: "",
                                         routingKey: "orderQueue",
                                         basicProperties: null,
                                         body: body);
                }

                #endregion

                return Accepted(order);
            }
            catch (Exception ex)
            {
                _logger.LogError("Erro ao tentrar criar um novo pedido", ex);
                return new StatusCodeResult(500);
            }
        }
    }
}
