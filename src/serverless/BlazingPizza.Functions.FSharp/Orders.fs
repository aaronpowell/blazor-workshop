namespace BlazingPizza.Functions.FSharp

open Microsoft.AspNetCore.Mvc
open Microsoft.Azure.WebJobs
open Microsoft.Azure.WebJobs.Extensions.Http
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Logging

open BlazingPizza

module GetOrders =
    [<FunctionName("orders")>]
    let run
        ([<HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)>] req: HttpRequest)
        ([<CosmosDB("blazingPizza",
                    "pizza",
                    ConnectionStringSetting = "CosmosConnectionString",
                    SqlQuery = "SELECT * FROM c WHERE c.partitionKey = 'specials'")>] pizzas: PizzaSpecial seq)
        ([<CosmosDB("blazingPizza",
                    "pizza",
                    ConnectionStringSetting = "CosmosConnectionString",
                    SqlQuery = "SELECT * FROM c WHERE c.partitionKey = 'topping'")>] toppings: Topping seq)
        ([<CosmosDB("blazingPizza", "pizza", ConnectionStringSetting = "CosmosConnectionString")>] orders: IAsyncCollector<Order>)
        (log: ILogger)
        =
        log.LogInformation("Started orders request")

        let result =
            match req.Method with
            | "get"
            | "GET" -> async { return OkResult() :> IActionResult }
            | "post"
            | "POST" -> PlaceOrder.run req pizzas toppings orders log
            | _ ->
                async {
                    log.LogInformation
                    <| sprintf "Got a HTTP METHOD of %s that isn't supported" req.Method
                    return BadRequestResult() :> IActionResult
                }

        result |> Async.StartAsTask
