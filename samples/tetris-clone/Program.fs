
open System
open Elmish
open Xelmish.Model
open Constants

type Model = 
    | Start of StartScreen.Model
    | Playing of PlayScreen.Model
    | GameOver of GameOverScreen.Model

type Message = 
    | StartScreenMessage of StartScreen.Message
    | PlayScreenMessage of PlayScreen.Message
    | GameOverScreenMessage of GameOverScreen.Message

let init () =
    Start (StartScreen.init ()), Cmd.none

let update message model =
    match model, message with
    | Start _, StartScreenMessage msg ->
        match msg with
        | StartScreen.StartGame -> Playing (PlayScreen.init ()), Cmd.none

    | Playing playScreen, PlayScreenMessage msg -> 
        match msg with
        | PlayScreen.GameOver score -> GameOver (GameOverScreen.init score), Cmd.none
        | _ -> 
            let newModel, newCommand = PlayScreen.update msg playScreen
            Playing newModel, Cmd.map PlayScreenMessage newCommand

    | GameOver _, GameOverScreenMessage msg ->
        match msg with
        | GameOverScreen.StartGame -> Playing (PlayScreen.init ()), Cmd.none

    | _ -> model, Cmd.none // invalid combination

let view model dispatch =
    match model with
    | Start startScreen ->
        StartScreen.view startScreen (StartScreenMessage >> dispatch)
    | Playing playScreen ->
        PlayScreen.view playScreen (PlayScreenMessage >> dispatch)
    | GameOver gameOverScreen ->
        GameOverScreen.view gameOverScreen (GameOverScreenMessage >> dispatch)

[<EntryPoint; STAThread>]
let main _ =
    let config = {
        resolution = Windowed (resWidth, resHeight)
        clearColour = Some Colour.Gray
        mouseVisible = true
        assetsToLoad = [
            Font ("connection", "./connection")
        ]
    }

    Program.mkProgram init update view
    //|> Program.withConsoleTrace
    |> Xelmish.Program.runGameLoop config
    0