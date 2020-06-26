module internal MockData

open BlazingPizza

let makePizza name price desc img id =
    let pizza = PizzaSpecial()
    pizza.Id <- id
    pizza.Name <- name
    pizza.Description <- desc
    pizza.ImageUrl <- img
    pizza.BasePrice <- price
    pizza

let pizzas =
    [ makePizza "Basic Cheese Pizza" 9.99m "It's cheesy and delicious. Why wouldn't you want one?"
          "img/pizzas/cheese.jpg" 1
      makePizza "The Baconatorizor" 11.99m "It has EVERY kind of bacon" "img/pizzas/bacon.jpg" 2
      makePizza "Classic pepperoni" 10.5m "It's the pizza you grew up with, but Blazing hot!" "img/pizzas/pepperoni.jpg"
          3 ]
