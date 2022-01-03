open Elmish
open Xelmish.Model 
open Xelmish.Viewables 

type Model = { x: int; y: int; w: int; h: int } 

let init () = { x = 250; y = 250; w = 100; h = 100 } 

type Message =
    | MoveVertical of dir: int
    | MoveHorizontal of dir: int
    | Resize of dir: int

let update message model =
    match message with
    | MoveVertical dir -> { model with y = model.y + 2 * dir }
    | MoveHorizontal dir -> { model with x = model.x + 2 * dir } 
    | Resize dir -> { x = model.x - dir; y = model.y - dir; w = model.w + 2 * dir; h = model.h + 2 * dir }

let view model dispatch =
    [
        colour Colour.Aqua (model.w, model.h) (model.x, model.y)
        whilekeydown Keys.Up (fun _ -> dispatch (MoveVertical -1))
        whilekeydown Keys.Down (fun _ -> dispatch (MoveVertical 1))
        whilekeydown Keys.Left (fun _ -> dispatch (MoveHorizontal -1))
        whilekeydown Keys.Right (fun _ -> dispatch (MoveHorizontal 1))

        whilekeydown Keys.OemPlus (fun _ -> dispatch (Resize 1))
        whilekeydown Keys.OemMinus (fun _ -> dispatch (Resize -1))
        onkeydown Keys.Escape exit
    ]

Program.mkSimple init update view
|> Xelmish.Program.runSimpleGameLoop [] (600, 600) Colour.Black