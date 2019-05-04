
open Elmish
open Xelmish.Model // required for config types used when using program.run
open Xelmish.Viewables // required to get access to helpers like 'colour'

type Model = { x: int; y: int; w: int; h: int } // our model is the position and dimensions of our shape

let init () = { x = 250; y = 250; w = 100; h = 100 } // start in middle of screen

type Message =
    | MoveVertical of dir: int
    | MoveHorizontal of dir: int
    | Resize of dir: int

let update message model =
    match message with
    | MoveVertical dir -> { model with y = model.y + 2 * dir }
    | MoveHorizontal dir -> { model with x = model.x + 2 * dir }    
    // we resize by not only changing dims but also pos, so the shape stays in the same place
    | Resize dir -> { x = model.x - dir; y = model.y - dir; w = model.w + 2 * dir; h = model.h + 2 * dir }

let view model dispatch =
    [
        // this is the only 'drawable' of our sample, nice and simple. it uses no loaded assets
        colour Colour.Aqua (model.w, model.h) (model.x, model.y)

        // various event helpers are in the Viewables module. whilekeydown will trigger every update
        whilekeydown Keys.Up (fun _ -> dispatch (MoveVertical -1))
        whilekeydown Keys.Down (fun _ -> dispatch (MoveVertical 1))
        whilekeydown Keys.Left (fun _ -> dispatch (MoveHorizontal -1))
        whilekeydown Keys.Right (fun _ -> dispatch (MoveHorizontal 1))

        whilekeydown Keys.OemPlus (fun _ -> dispatch (Resize 1))
        whilekeydown Keys.OemMinus (fun _ -> dispatch (Resize -1))

        // as a nice simple way to exit the app, we use onkeydown (triggers just the first press)
        // with the exit helper method (which has the signature fun _ -> throw exit exception)
        onkeydown Keys.Escape exit
    ]

[<EntryPoint>]
let main _ =
    // in this simple example, we can use the Elmish mkSimple and
    // the Xelmish runSimple, which preconfigures a windowed game for you
    Program.mkSimple init update view
    |> Xelmish.Program.runSimpleGameLoop [] (600, 600) Colour.Black
    0
