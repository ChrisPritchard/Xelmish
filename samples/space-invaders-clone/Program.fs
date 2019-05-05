open Elmish
open Xelmish.Model
open Common

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

    Program.mkProgram Game.init Game.update Game.view
    |> Xelmish.Program.runGameLoop config

    0