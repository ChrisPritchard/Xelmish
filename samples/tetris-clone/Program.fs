
open System
open Elmish
open Xelmish.Model
open Constants

let timerTick dispatch =
    let timer = new System.Timers.Timer(1000.)
    timer.Elapsed.Add (fun _ -> dispatch Playing.Tick)
    timer.Start ()

[<EntryPoint; STAThread>]
let main _ =
    let config = {
        resolution = Windowed (resWidth, resHeight)
        clearColour = Some (rgb 100uy 100uy 100uy)
        mouseVisible = true
        assetsToLoad = [
            Font ("connection", "./connection")
        ]
    }

    let quit () = ()

    Program.mkProgram Playing.init (Playing.update quit) Playing.view
    |> Program.withSubscription (fun m -> Cmd.ofSub timerTick)
    //|> Program.withConsoleTrace
    |> Xelmish.Program.runGameLoop config
    0