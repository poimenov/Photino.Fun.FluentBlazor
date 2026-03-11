[<AutoOpen>]
module Photino.Fun.FluentBlazor.App

open System
open System.Linq
open Microsoft.AspNetCore.Components.Web
open Microsoft.AspNetCore.Components.Routing
open Microsoft.FluentUI.AspNetCore.Components
open Microsoft.Extensions.Localization
open Microsoft.Extensions.Options
open FSharp.Data
open Fun.Blazor
open Fun.Blazor.Router

type WeatherForecastProvider = JsonProvider<""" [{"date": "2018-05-06", "temperatureC": 1, "summary": "Freezing" }] """>

type IShareStore with
    member store.Count = store.CreateCVal(nameof store.Count, 0)
    member store.IsMenuOpen = store.CreateCVal(nameof store.IsMenuOpen, true)
    member store.Theme = store.CreateCVal(nameof store.Theme, DesignThemeModes.Light)

    member store.WeatherData =
        store.CreateCVal(nameof store.WeatherData, Enumerable.Empty<WeatherForecastProvider.Root>().AsQueryable())

let homePage =
    html.inject (fun (localizer: IStringLocalizer<SharedResources>) ->
        fragment {
            FluentLabel'' {
                Typo Typography.H1
                Color Color.Accent
                localizer["Home"]
            }

            p { localizer["HomeText"] }
        })

let fetchDataPage =
    html.inject (fun (store: IShareStore, hook: IComponentHook, localizer: IStringLocalizer<SharedResources>) ->
        hook.AddInitializedTask(fun () ->
            task {
                if not (store.WeatherData.Value.Any()) then
                    let! data = WeatherForecastProvider.AsyncLoad "wwwroot/sample-data/weather.json"
                    store.WeatherData.Publish(data.AsQueryable<WeatherForecastProvider.Root>())
            })

        fragment {
            FluentLabel'' {
                Typo Typography.H1
                Color Color.Accent
                localizer["WeatherHeader"]
            }

            p { localizer["WeatherText"] }

            adapt {
                FluentDataGrid'' {
                    Items store.WeatherData.Value

                    PropertyColumn'' {
                        Title(string localizer["Date"])
                        Property(fun (x: WeatherForecastProvider.Root) -> x.Date.ToString "dd MMM yyyy")
                    }

                    PropertyColumn'' {
                        Title(string localizer["TempC"])
                        Property(fun (x: WeatherForecastProvider.Root) -> x.TemperatureC)
                    }

                    PropertyColumn'' {
                        Title(string localizer["TempF"])

                        Property(fun (x: WeatherForecastProvider.Root) ->
                            Math.Round(float x.TemperatureC * (9.0 / 5.0) + 32.0, 2))
                    }

                    PropertyColumn'' {
                        Title(string localizer["Summary"])
                        Property(fun (x: WeatherForecastProvider.Root) -> x.Summary)
                    }
                }
            }
        })

let counterPage =
    html.inject (fun (store: IShareStore, snackbar: IToastService, localizer: IStringLocalizer<SharedResources>) ->
        fragment {
            FluentLabel'' {
                Typo Typography.H1
                Color Color.Accent
                localizer["Counter"]
            }

            adapt {
                let! count = store.Count

                p {
                    localizer["CurrentCount"]
                    count
                }
            }

            FluentButton'' {
                Appearance Appearance.Accent

                OnClick(fun _ ->
                    store.Count.Publish((+) 1)
                    let currCount = string localizer["CurrentCount"]
                    snackbar.ShowSuccess $"{currCount} {store.Count.Value}")

                localizer["ClickMe"]
            }
        })

let appHeader =
    html.inject
        (fun (store: IShareStore, options: IOptions<AppSettings>, localizer: IStringLocalizer<SharedResources>) ->
            FluentHeader'' {
                FluentStack'' {
                    Orientation Orientation.Horizontal
                    HorizontalGap 2

                    img {
                        src AppSettings.FavIconFileName
                        style { height "40px" }
                    }

                    FluentLabel'' {
                        Typo Typography.H2
                        Color Color.Fill
                        style' "cursor: default;"
                        AppSettings.ApplicationName
                    }

                    FluentSpacer''

                    adapt {
                        let! theme = store.Theme

                        FluentDesignTheme'' {
                            StorageName "theme"
                            Mode store.Theme.Value
                            OfficeColor options.Value.AccentColor

                            OnLoaded(fun args ->
                                if args.IsDark then
                                    store.Theme.Publish DesignThemeModes.Dark)
                        }

                        FluentButton'' {
                            Appearance Appearance.Accent
                            IconStart(Icons.Regular.Size20.DarkTheme())
                            title' (string (localizer["SwitchTheme"]))

                            OnClick(fun _ ->
                                store.Theme.Publish(
                                    if theme = DesignThemeModes.Dark then
                                        DesignThemeModes.Light
                                    else
                                        DesignThemeModes.Dark
                                ))
                        }
                    }
                }
            })

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
    html.injectWithNoKey (fun (store: IShareStore, localizer: IStringLocalizer<SharedResources>) ->
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
                    Tooltip(string (localizer["Home"]))
                    localizer["Home"]
                }

                FluentNavLink'' {
                    Href "/counter"
                    Match NavLinkMatch.Prefix
                    Icon(Icons.Regular.Size20.NumberSymbolSquare())
                    Tooltip(string (localizer["Counter"]))
                    localizer["Counter"]
                }

                FluentNavLink'' {
                    Href "/fetchdata"
                    Match NavLinkMatch.Prefix
                    Icon(Icons.Regular.Size20.Temperature())
                    Tooltip(string (localizer["Weather"]))
                    localizer["Weather"]
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

    override _.Render() = app
