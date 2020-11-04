namespace BlazingPizza.Functions.FSharp

open Microsoft.AspNetCore.Mvc
open Microsoft.Azure.WebJobs
open Microsoft.Azure.WebJobs.Extensions.Http
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Logging

open BlazingPizza
open System.Security.Claims

module OrderById =
    [<FunctionName("order-by-id")>]
    let run ([<HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "orders/{orderId:int}")>] req: HttpRequest)
            ([<CosmosDB("blazingPizza",
                        "pizza",
                        ConnectionStringSetting = "CosmosConnectionString",
                        SqlQuery = "SELECT * FROM c WHERE c.OrderId = {orderId}")>] requestedOrder: Order seq)
            (orderId: int)
            (log: ILogger)
            =
        let principal = ClientPrincipal.parse req

        let userId =
            principal.Claims
            |> Seq.find (fun c -> c.Type = ClaimTypes.NameIdentifier)

        let order = requestedOrder |> Seq.tryHead

        match order with
        | Some order when order.UserId = userId.Value ->
            OkObjectResult(OrderWithStatus.FromOrder(order)) :> IActionResult
        | Some _
        | None -> NotFoundResult() :> IActionResult
