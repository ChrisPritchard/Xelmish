
open System
open Elmish
open Xelmish.Model
open Constants

type Model = {
    playing: Playing.Model
    shouldQuit: bool
}

type Message = 
| PlayingMessage of Playing.Message

let init () =
    { playing = Playing.init (); shouldQuit = false }, Cmd.none

let update message model =
    match message with
    | PlayingMessage msg -> 
        let newPlaying, newMessage, parentMessage = Playing.update msg model.playing
        match parentMessage with
        | Playing.NoOp -> { model with playing = newPlaying }, Cmd.map PlayingMessage newMessage
        | Playing.Quit -> { model with shouldQuit = true }, Cmd.none
        | Playing.GameOver -> { model with shouldQuit = true }, Cmd.none

let view model dispatch =
    let gameMessage = if model.shouldQuit then Exit else NoOp
    Playing.view model.playing (PlayingMessage >> dispatch), gameMessage

let timerTick dispatch =
    let timer = new System.Timers.Timer(50.)
    timer.Elapsed.Add (fun _ -> dispatch (PlayingMessage Playing.Tick))
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

    Program.mkProgram init update view
    |> Program.withSubscription (fun m -> Cmd.ofSub timerTick)
    //|> Program.withConsoleTrace
    |> Xelmish.Program.runGameLoop config
    0