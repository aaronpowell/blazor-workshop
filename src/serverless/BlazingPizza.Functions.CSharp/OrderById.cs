using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace BlazingPizza.Functions.CSharp
{
    public static class OrderById
    {
        [FunctionName("order-by-id")]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "orders/{orderId:int}")] HttpRequest req,
            [CosmosDB("blazingPizza",
                      "pizza",
                      ConnectionStringSetting = "CosmosConnectionString",
                      SqlQuery = "SELECT * FROM c WHERE c.OrderId = {orderId}")] IEnumerable<Order> requestedOrder,
            int orderId,
            ILogger log)
        {
            var principal = StaticWebAppsAuth.Parse(req);
            var userId = principal.Claims.First(c => c.Type == ClaimTypes.NameIdentifier);

            var order = requestedOrder.FirstOrDefault(order => order.UserId == userId.Value);

            if (order == null)
            {
                return new NotFoundResult();
            }

            return new OkObjectResult(OrderWithStatus.FromOrder(order));
        }
    }
}
