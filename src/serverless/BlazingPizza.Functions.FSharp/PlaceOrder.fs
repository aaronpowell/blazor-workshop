namespace BlazingPizza.Functions.FSharp

open Microsoft.AspNetCore.Mvc
open Microsoft.Azure.WebJobs
open Microsoft.Azure.WebJobs.Extensions.Http
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Logging

open BlazingPizza
open System.IO
open Newtonsoft.Json
open System

module PlaceOrder =
    let validatePizza (sp: Pizza) (pizzas: PizzaSpecial seq) =
        let pizza =
            pizzas
            |> Seq.tryFind (fun p -> p.Id = sp.Special.Id)

        match pizza with
        | Some pizza ->
            sp.SpecialId <- pizza.Id
            sp.Special <- pizza
        | None -> failwith "You tried to submit a pizza we don't make"

    let validateToppings (toppings: Topping seq) (st: PizzaTopping) =
        let topping =
            toppings
            |> Seq.tryFind (fun t -> t.Id = st.Topping.Id)

        match topping with
        | Some topping ->
            st.ToppingId <- topping.Id
            st.Topping <- topping
        | None -> failwith "You tried to submit a topping we don't have"

    [<FunctionName("place-orders")>]
    let run
        ([<HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)>] req: HttpRequest)
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
        async {
            use sr = new StreamReader(req.Body)
            let! rawBody = sr.ReadToEndAsync() |> Async.AwaitTask

            let order =
                JsonConvert.DeserializeObject<Order> rawBody

            order.CreatedTime <- DateTime.Now
            order.DeliveryLocation <- LatLong(51.5001, -0.1239)
            order.UserId <- "Mr Awesome"

            let validateToppings' = validateToppings toppings
            order.Pizzas
            |> Seq.iter (fun sp ->
                validatePizza sp pizzas
                sp.Toppings |> Seq.iter validateToppings')

            let random = Random()
            order.OrderId <- random.Next()

            do! orders.AddAsync(order) |> Async.AwaitTask

            return OkObjectResult(order.OrderId)
        }
        |> Async.StartAsTask
