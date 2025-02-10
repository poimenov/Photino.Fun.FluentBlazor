module Program

open Photino.Blazor
open Microsoft.Extensions.DependencyInjection
open Microsoft.FluentUI.AspNetCore.Components
open Photino.Fun.FluentBlazor
open System

[<EntryPoint>]
let main args =
    let builder = PhotinoBlazorAppBuilder.CreateDefault(args)

    builder.Services.AddFunBlazorWasm() |> ignore
    builder.Services.AddFluentUIComponents() |> ignore
    builder.Services.AddSingleton<IPlatformService, PlatformService>() |> ignore
    builder.Services.AddSingleton<IProcessService, ProcessService>() |> ignore

    builder.Services.AddSingleton<ILinkOpeningService, LinkOpeningService>()
    |> ignore

    let application = builder.Build()
    application.RootComponents.AddFunBlazor("#app", app) |> ignore

    // customize window
    application.MainWindow
        .SetSize(1024, 768)
        .SetIconFile("wwwroot/favicon.ico")
        .SetTitle("Photino.Fun.FluentBlazor")
    |> ignore

    AppDomain.CurrentDomain.UnhandledException.Add(fun e ->
        let ex = e.ExceptionObject :?> Exception
        application.MainWindow.ShowMessage(ex.Message, "Error") |> ignore)

    application.Run()
    0
