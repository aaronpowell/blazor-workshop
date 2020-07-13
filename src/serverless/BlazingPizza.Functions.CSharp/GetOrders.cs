using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

namespace BlazingPizza.Functions.CSharp
{
    public static class GetOrders
    {
        [FunctionName("get-orders")]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "orders")] HttpRequest req,
            [CosmosDB("blazingPizza",
                      "pizza",
                      ConnectionStringSetting = "CosmosConnectionString",
                      SqlQuery = "SELECT * FROM c WHERE IS_DEFINED(c.OrderId)")] IEnumerable<Order> orders,
            ILogger log)
        {
            var ordersForUser = orders.Where(o => o.UserId == "Mr Awesome").Select(OrderWithStatus.FromOrder);

            return new OkObjectResult(ordersForUser);
        }
    }
}
