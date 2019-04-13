
open System
open Elmish
open Xelmish.Model
open Constants

type Model = {
    screen: Screen
    shouldQuit: bool
} and Screen = 
    | Start of StartScreen.Model
    | Playing of PlayScreen.Model
    | GameOver of GameOverScreen.Model

type Message = 
    | StartScreenMessage of StartScreen.Message
    | PlayScreenMessage of PlayScreen.Message
    | GameOverScreenMessage of GameOverScreen.Message

let init () =
    { screen = Start (StartScreen.init ()); shouldQuit = false }, Cmd.none

let update message model =
    match model.screen, message with
    | Start _, StartScreenMessage msg ->
        match StartScreen.update msg with
        | StartScreen.Start -> { model with screen = Playing (PlayScreen.init ()) }, Cmd.none
        | StartScreen.Quit -> { model with shouldQuit = true }, Cmd.none

    | Playing playScreen, PlayScreenMessage msg -> 
        let newPlaying, newMessage, parentMessage = PlayScreen.update msg playScreen
        match parentMessage with
        | PlayScreen.NoOp -> { model with screen = Playing newPlaying }, Cmd.map PlayScreenMessage newMessage
        | PlayScreen.Quit -> { model with shouldQuit = true }, Cmd.none
        | PlayScreen.GameOver score -> { model with screen = GameOver (GameOverScreen.init score) }, Cmd.none

    | GameOver _, GameOverScreenMessage msg ->
        match GameOverScreen.update msg with
        | GameOverScreen.Start -> { model with screen = Playing (PlayScreen.init ()) }, Cmd.none
        | GameOverScreen.Quit -> { model with shouldQuit = true }, Cmd.none

    | _ -> model, Cmd.none // invalid combination

let view model dispatch =
    let gameMessage = if model.shouldQuit then Exit else NoOp
    match model.screen with
    | Start startScreen ->
        StartScreen.view startScreen (StartScreenMessage >> dispatch), gameMessage
    | Playing playScreen ->
        PlayScreen.view playScreen (PlayScreenMessage >> dispatch), gameMessage
    | GameOver gameOverScreen ->
        GameOverScreen.view gameOverScreen (GameOverScreenMessage >> dispatch), gameMessage

let timerTick dispatch =
    let timer = new System.Timers.Timer(50.)
    timer.Elapsed.Add (fun _ -> dispatch (PlayScreenMessage PlayScreen.Tick))
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