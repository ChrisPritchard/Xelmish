open Elmish
open Xelmish.Model
open Common

[<EntryPoint>]
let main _ =
    let config: GameConfig = {
        clearColour = Some Colour.Black
        resolution = Windowed (resWidth, resHeight)
        assetsToLoad = [ 
            Texture ("sprites", "./sprites.png")
            Font ("PressStart2P", "./PressStart2P") ]
        mouseVisible = false
        showFpsInConsole = true
    }

    Program.mkProgram Game.init Game.update Game.view
    |> Xelmish.Program.runGameLoop config

    0