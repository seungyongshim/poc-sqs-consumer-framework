#nowarn "20"
open Microsoft.AspNetCore.Builder
open Giraffe

let webApp =
    choose [ route "/ping"  >=> text "pong"
             route "/"      >=> htmlFile "/pages/index.html" ]


let builder = WebApplication.CreateBuilder()

builder.Services.AddGiraffe() |> ignore

let app = builder.Build()
app.UseGiraffe webApp
app.RunAsync() |> Async.AwaitTask |> Async.RunSynchronously

0
