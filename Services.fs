[<AutoOpen>]
module Photino.Fun.FluentBlazor.Services

open System.Diagnostics
open System.Runtime.InteropServices

type Platform =
    | Windows
    | Linux
    | MacOS
    | Unknown

type IPlatformService =
    abstract member GetPlatform: unit -> Platform

type PlatformService() =
    interface IPlatformService with
        member this.GetPlatform() =
            if RuntimeInformation.IsOSPlatform(OSPlatform.Windows) then
                Windows
            elif RuntimeInformation.IsOSPlatform(OSPlatform.Linux) then
                Linux
            elif RuntimeInformation.IsOSPlatform(OSPlatform.OSX) then
                MacOS
            else
                Unknown

type IProcessService =
    abstract member Run: command: string * arguments: string -> unit

type ProcessService() =
    interface IProcessService with
        member this.Run(command, arguments) =
            let psi = new ProcessStartInfo(command)
            psi.RedirectStandardOutput <- false
            psi.UseShellExecute <- false
            psi.CreateNoWindow <- false
            psi.Arguments <- arguments

            let p = new Process()
            p.StartInfo <- psi
            p.Start() |> ignore

type ILinkOpeningService =
    abstract member OpenUrl: url: string -> unit

type LinkOpeningService(platformService: IPlatformService, processService: IProcessService) =
    interface ILinkOpeningService with
        member this.OpenUrl(url) =
            match platformService.GetPlatform() with
            | Windows -> processService.Run("cmd", $"/c start {url}")
            | Linux -> processService.Run("xdg-open", url)
            | MacOS -> processService.Run("open", url)
            | _ -> ()
