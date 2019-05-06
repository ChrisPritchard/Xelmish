open Elmish
open Xelmish.Model
open Common

type Model = 
    | Start of StartScreen.Model
    | Playing of Game.Model
    | GameOver of GameOver.Model

type Message = 
    | StartMessage of StartScreen.Message
    | PlayingMessage of Game.Message
    | GameOverMessage of GameOver.Message

let init () =
    Start (StartScreen.init ()), Cmd.none

let update message model =
    match model, message with
    | Start _, StartMessage _ -> let model, command = Game.init () in Playing model, command
    | Playing game, PlayingMessage msg -> 
        match msg with
        | Game.GameOver score -> GameOver (GameOver.init score), Cmd.none
        | _ -> 
            let newModel, newCommand = Game.update msg game
            Playing newModel, Cmd.map PlayingMessage newCommand
    | GameOver _, GameOverMessage _ -> let model, command = Game.init () in Playing model, command

    | _ -> model, Cmd.none // invalid combination

let view model dispatch =
    match model with
    | Start startScreen -> StartScreen.view startScreen (StartMessage >> dispatch)
    | Playing game -> Game.view game (PlayingMessage >> dispatch)
    | GameOver gameOverScreen -> GameOver.view gameOverScreen (GameOverMessage >> dispatch)

[<EntryPoint>]
let main _ =
    let config: GameConfig = {
        clearColour = Some Colour.Black
        resolution = Windowed (resWidth, resHeight)
        assetsToLoad = [ 
            FileTexture ("sprites", "./content/sprites.png")
            PipelineFont ("PressStart2P", "./content/PressStart2P")
            FileSound ("shoot", "./content/siclone_shoot.wav")
            FileSound ("shoot-enemy", "./content/siclone_shoot_enemy.wav")
            FileSound ("explosion", "./content/siclone_explosion.wav")
            FileSound ("explosion-small", "./content/siclone_explosion_small.wav") 
            FileSound ("beep", "./content/siclone_menu.wav") ]
        mouseVisible = false
        showFpsInConsole = true
    }

    Program.mkProgram init update view
    |> Xelmish.Program.runGameLoop config

    0