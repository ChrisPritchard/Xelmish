open Elmish
open Xelmish.Model
open Common

[<EntryPoint>]
let main _ =
    let config: GameConfig = {
        clearColour = Some Colour.Black
        resolution = Windowed (resWidth, resHeight)
        assetsToLoad = [ 
            Texture ("sprites", "./content/sprites.png")
            Font ("PressStart2P", "./content/PressStart2P")
            Sound ("shoot", "./content/siclone_shoot.wav")
            Sound ("shoot-enemy", "./content/siclone_shoot_enemy.wav")
            Sound ("explosion", "./content/siclone_explosion.wav")
            Sound ("explosion-small", "./content/siclone_explosion_small.wav") 
            Sound ("beep", "./content/siclone_menu.wav") ]
        mouseVisible = false
        showFpsInConsole = true
    }

    Program.mkProgram Game.init Game.update Game.view
    |> Xelmish.Program.runGameLoop config

    0