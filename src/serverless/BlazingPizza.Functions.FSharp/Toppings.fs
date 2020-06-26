namespace BlazingPizza.Functions.FSharp

open Microsoft.AspNetCore.Mvc
open Microsoft.Azure.WebJobs
open Microsoft.Azure.WebJobs.Extensions.Http
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Logging

open MockData

module Toppings =
    [<FunctionName("toppings")>]
    let run ([<HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)>] req: HttpRequest) (log: ILogger) =
        async { return OkObjectResult(toppings) :> IActionResult }
        |> Async.StartAsTask
