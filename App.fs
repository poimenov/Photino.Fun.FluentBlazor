[<AutoOpen>]
module Photino.Fun.FluentBlazor.App

open System
open System.Linq
open Microsoft.AspNetCore.Components.Web
open Microsoft.AspNetCore.Components.Routing
open Microsoft.FluentUI.AspNetCore.Components
open FSharp.Data
open Fun.Blazor
open Fun.Blazor.Router

type WeatherForecastProvider = JsonProvider<""" [{"date": "2018-05-06", "temperatureC": 1, "summary": "Freezing" }] """>

type IShareStore with
    member store.Count = store.CreateCVal(nameof store.Count, 0)
    member store.IsMenuOpen = store.CreateCVal(nameof store.IsMenuOpen, true)

    member store.WeatherData =
        store.CreateCVal(nameof store.WeatherData, Enumerable.Empty<WeatherForecastProvider.Root>().AsQueryable())

let homePage =
    fragment {
        SectionContent'' {
            SectionName "Title"
            "Home"
        }

        FluentLabel'' {
            Typo Typography.H1
            Color Color.Accent
            "Hi from FunBlazor"
        }
    }

let fetchDataPage =
    html.inject (fun (store: IShareStore, hook: IComponentHook) ->
        hook.AddInitializedTask(fun () ->
            task {
                if not (store.WeatherData.Value.Any()) then
                    let! data = WeatherForecastProvider.AsyncLoad("wwwroot/sample-data/weather.json")
                    store.WeatherData.Publish(data.AsQueryable<WeatherForecastProvider.Root>())
            })

        fragment {
            SectionContent'' {
                SectionName "Title"
                "Async Fetch Data"
            }

            FluentLabel'' {
                Typo Typography.H1
                Color Color.Accent
                "Fetch Data"
            }

            adapt {
                FluentDataGrid'' {
                    Items(store.WeatherData.Value)

                    PropertyColumn'' {
                        Title "Date"
                        Property(fun (x: WeatherForecastProvider.Root) -> x.Date.ToString("dd MMM yyyy"))
                    }

                    PropertyColumn'' {
                        Title "Temp. (C)"
                        Property(fun (x: WeatherForecastProvider.Root) -> x.TemperatureC)
                    }

                    PropertyColumn'' {
                        Title "Temp. (F)"

                        Property(fun (x: WeatherForecastProvider.Root) ->
                            Math.Round((float x.TemperatureC) * (9.0 / 5.0) + 32.0, 2))
                    }

                    PropertyColumn'' {
                        Title "Summary"
                        Property(fun (x: WeatherForecastProvider.Root) -> x.Summary)
                    }
                }
            }
        })

let counterPage =
    html.inject (fun (store: IShareStore, snackbar: IToastService) ->
        fragment {
            SectionContent'' {
                SectionName "Title"
                "Counter"
            }

            FluentLabel'' {
                Typo Typography.H1
                Color Color.Accent
                "Counter"
            }

            adapt {
                let! count = store.Count

                div {
                    "Here is the count: "
                    count
                }
            }

            FluentButton'' {
                Appearance Appearance.Accent

                OnClick(fun _ ->
                    store.Count.Publish((+) 1)
                    snackbar.ShowSuccess($"Count = {store.Count.Value}"))

                "Increase by 1"
            }
        })

let appHeader =
    FluentHeader'' {
        FluentStack'' {
            Orientation Orientation.Horizontal

            img {
                src "favicon.ico"

                style { height "28px" }
            }

            FluentLabel'' {
                Typo Typography.H2
                Color Color.Fill
                "Photino.Fun.FluentBlazor"
            }
        }
    }

let appFooter =
    html.inject (fun (los: ILinkOpeningService) ->
        FluentFooter'' {
            FluentAnchor'' {
                Appearance Appearance.Hypertext
                href "#"
                OnClick(fun _ -> los.OpenUrl "https://slaveoftime.github.io/Fun.Blazor.Docs/")

                "Fun.Blazor"
            }

            FluentSpacer''

            FluentAnchor'' {
                Appearance Appearance.Hypertext
                href "#"
                OnClick(fun _ -> los.OpenUrl "https://www.tryphotino.io")

                "Photino"
            }
        })

let navmenus =
    html.injectWithNoKey (fun (store: IShareStore) ->
        adaptiview () {
            let! binding = store.IsMenuOpen.WithSetter()

            FluentNavMenu'' {
                Width 200
                Collapsible true
                Expanded' binding

                FluentNavLink'' {
                    Href "/"
                    Match NavLinkMatch.All
                    Icon(Icons.Regular.Size20.Home())
                    "Home"
                }

                FluentNavLink'' {
                    Href "/counter"
                    Match NavLinkMatch.Prefix
                    Icon(Icons.Regular.Size20.NumberSymbolSquare())
                    "Counter"
                }

                FluentNavLink'' {
                    Href "/fetchdata"
                    Match NavLinkMatch.Prefix
                    Icon(Icons.Regular.Size20.Temperature())
                    "Fetch data"
                }
            }
        })

let routes =
    html.route
        [| routeCi "/counter" counterPage
           routeCi "/fetchdata" fetchDataPage
           routeAny homePage |]

let app =
    ErrorBoundary'' {
        ErrorContent(fun e ->
            FluentLabel'' {
                Color Color.Error
                string e
            })

        FluentToastProvider''

        FluentLayout'' {
            appHeader

            FluentStack'' {
                Width "100%"
                class' "main"
                Orientation Orientation.Horizontal
                navmenus

                FluentBodyContent'' {
                    class' "body-content"
                    style { overflowHidden }

                    div {
                        class' "content"

                        routes
                    }
                }
            }

            appFooter
        }
    }

type App() =
    inherit FunComponent()

    override _.Render () = app
