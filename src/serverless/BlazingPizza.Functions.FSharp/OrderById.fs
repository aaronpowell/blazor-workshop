namespace BlazingPizza.Functions.FSharp

open Microsoft.AspNetCore.Mvc
open Microsoft.Azure.WebJobs
open Microsoft.Azure.WebJobs.Extensions.Http
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Logging

open BlazingPizza

module OrderById =
    [<FunctionName("order-by-id")>]
    let run
        ([<HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "order-by-id/{orderId:int}")>] req: HttpRequest)
        ([<CosmosDB("blazingPizza",
                    "pizza",
                    ConnectionStringSetting = "CosmosConnectionString",
                    SqlQuery = "SELECT * FROM c WHERE c.OrderId = {orderId}")>] requestedOrder: Order seq)
        (orderId: int)
        (log: ILogger)
        =
        OkObjectResult(OrderWithStatus.FromOrder(requestedOrder |> Seq.head))
