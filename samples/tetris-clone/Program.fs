
open System
open Elmish
open Xelmish.Model

[<EntryPoint; STAThread>]
let main _ =
    let config = {
        resolution = Windowed (400, 600)
        clearColour = Some (rgb 100uy 100uy 100uy)
        mouseVisible = true
        assetsToLoad = [
            Font ("connection", "./connection")
        ]
    }

    let quit () = ()

    Program.mkProgram Playing.init (Playing.update quit) Playing.view
    //|> Program.withSubscription (fun m -> Cmd.ofSub timerTick)
    |> Program.withConsoleTrace
    |> Xelmish.Program.runGameLoop config
    0