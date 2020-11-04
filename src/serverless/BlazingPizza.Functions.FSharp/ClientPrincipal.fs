namespace BlazingPizza.Functions.FSharp

open Microsoft.AspNetCore.Http
open System.Text.Json
open System.Security.Claims

type ClientPrincipal =
    { IdentityProvider: string
      UserId: string
      UserDetails: string
      UserRoles: string seq }

module ClientPrincipal =
    let parse (req: HttpRequest) =
        let header = req.Headers.["x-ms-client-principal"]
        let data = header.[0]
        let decoded = System.Convert.FromBase64String data

        let json =
            System.Text.ASCIIEncoding.ASCII.GetString decoded

        let principal =
            JsonSerializer.Deserialize<ClientPrincipal>(json)

        match principal.UserRoles
              |> Seq.except [| "anonymous" |] with
        | roles when Seq.isEmpty roles -> ClaimsPrincipal()
        | _ ->
            let identity =
                ClaimsIdentity(principal.IdentityProvider)

            identity.AddClaim(Claim(ClaimTypes.NameIdentifier, principal.UserId))
            identity.AddClaim(Claim(ClaimTypes.Name, principal.UserDetails))
            principal.UserRoles
            |> Seq.map (fun role -> Claim(ClaimTypes.Role, role))
            |> identity.AddClaims

            ClaimsPrincipal identity
