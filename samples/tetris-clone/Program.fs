
open System
open Elmish
open Xelmish.Model
open Constants

type Model = {
    screen: Screen
    shouldQuit: bool
} and Screen = 
    | Playing of Playing.Model

type Message = 
| PlayingMessage of Playing.Message

let init () =
    { screen = Playing (Playing.init ()); shouldQuit = false }, Cmd.none

let update message model =
    match model.screen, message with
    | Playing playScreen, PlayingMessage msg -> 
        let newPlaying, newMessage, parentMessage = Playing.update msg playScreen
        match parentMessage with
        | Playing.NoOp -> { model with screen = Playing newPlaying }, Cmd.map PlayingMessage newMessage
        | Playing.Quit -> { model with shouldQuit = true }, Cmd.none
        | Playing.GameOver score -> { model with shouldQuit = true }, Cmd.none

let view model dispatch =
    let gameMessage = if model.shouldQuit then Exit else NoOp
    match model.screen with
    | Playing playScreen ->
        Playing.view playScreen (PlayingMessage >> dispatch), gameMessage

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