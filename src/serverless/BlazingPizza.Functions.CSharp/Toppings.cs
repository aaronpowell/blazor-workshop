using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace BlazingPizza.Functions.CSharp
{
    public static class Toppings
    {
        [FunctionName("Toppings")]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            [CosmosDB("blazingPizza",
                      "pizza",
                      ConnectionStringSetting = "CosmosConnectionString",
                      SqlQuery = "SELECT * FROM c WHERE c.partitionKey = 'topping'")] IEnumerable<Topping> toppings,
            ILogger log)
        {
            return new OkObjectResult(toppings);
        }
    }
}
