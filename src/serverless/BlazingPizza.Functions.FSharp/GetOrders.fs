namespace BlazingPizza.Functions.FSharp

open Microsoft.AspNetCore.Mvc
open Microsoft.Azure.WebJobs
open Microsoft.Azure.WebJobs.Extensions.Http
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Logging

open BlazingPizza

module GetOrders =
    [<FunctionName("get-orders")>]
    let runGetOrders
        ([<HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "orders")>] req: HttpRequest)
        ([<CosmosDB("blazingPizza",
                    "pizza",
                    ConnectionStringSetting = "CosmosConnectionString",
                    SqlQuery = "SELECT * FROM c WHERE IS_DEFINED(c.OrderId)")>] placedOrders: Order seq)
        (log: ILogger)
        =
        let userOrders =
            placedOrders
            |> Seq.filter (fun o -> o.UserId = "Mr Awesome")
            |> Seq.map OrderWithStatus.FromOrder

        OkObjectResult userOrders
