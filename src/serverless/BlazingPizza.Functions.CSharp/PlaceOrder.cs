using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System;
using System.Security.Claims;

namespace BlazingPizza.Functions.CSharp
{
    public static class PlaceOrder
    {
        private static void ValidatePizza(Pizza sp, IEnumerable<PizzaSpecial> pizzas)
        {
            var pizza = pizzas.FirstOrDefault(p => p.Id == sp.Special.Id);

            if (pizza == default(PizzaSpecial))
            {
                throw new KeyNotFoundException("You tried to submit a pizza we don't make");
            }

            sp.SpecialId = pizza.Id;
            sp.Special = pizza;
        }

        private static void ValidateToppings(IEnumerable<Topping> toppings, PizzaTopping st)
        {
            var topping = toppings.FirstOrDefault(t => t.Id == st.Topping.Id);

            if (topping == default(Topping))
            {
                throw new KeyNotFoundException("You tried to submit a topping we don't have");
            }

            st.ToppingId = topping.Id;
            st.Topping = topping;
        }

        [FunctionName("orders")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            [CosmosDB("blazingPizza",
                      "pizza",
                      ConnectionStringSetting = "CosmosConnectionString",
                      SqlQuery = "SELECT * FROM c WHERE c.partitionKey = 'specials'")] IEnumerable<PizzaSpecial> pizzas,
            [CosmosDB("blazingPizza",
                      "pizza",
                      ConnectionStringSetting = "CosmosConnectionString",
                      SqlQuery = "SELECT * FROM c WHERE c.partitionKey = 'topping'")] IEnumerable<Topping> toppings,
            [CosmosDB("blazingPizza", "pizza", ConnectionStringSetting = "CosmosConnectionString")] IAsyncCollector<Order> orders,
            ILogger log)
        {
            using (var sr = new StreamReader(req.Body))
            {
                var rawBody = await sr.ReadToEndAsync();
                var order = JsonConvert.DeserializeObject<Order>(rawBody);

                order.CreatedTime = DateTime.Now;
                order.DeliveryLocation = new LatLong(51.5001, -0.1239);

                var principal = StaticWebAppsAuth.Parse(req);
                var userId = principal.Claims.First(c => c.Type == ClaimTypes.NameIdentifier);
                order.UserId = userId.Value;

                foreach (var pizza in order.Pizzas)
                {
                    ValidatePizza(pizza, pizzas);

                    foreach (var topping in pizza.Toppings)
                    {
                        ValidateToppings(toppings, topping);
                    }
                }

                order.OrderId = new Random().Next();

                await orders.AddAsync(order);

                return new OkObjectResult(order.OrderId);
            }
        }
    }
}
