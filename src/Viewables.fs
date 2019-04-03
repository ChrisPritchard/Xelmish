module Xelmish.Viewables

open Model
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework.Input

type Viewable =
| Position of x: int * y: int * width: int * height: int * Viewable
| Window of x: int * y: int * width: int * height: int * Viewable list
| Row of Viewable list
| Text of font: string * text: string
| Button of font: string * text: string * onClick: (unit -> unit)

//let private buttonBack: Texture2D = null

let private vector2 x y = Vector2(float32 x, float32 y)
//let private rectangle x y w h = Rectangle(x, y, w, h)
let private isInside tx ty tw th x y = x >= tx && x <= tx + tw && y >= ty && y <= ty + th

let rec internal renderViewable drawState (px, py, pw, ph) viewable =
    match viewable with
    | Position (x, y, w, h, sv) ->
        renderViewable drawState (px + x, py + y, w, h) sv
    | Window (x, y, w, h, svl) ->
        let div = h / svl.Length
        svl |> List.iteri (fun i -> renderViewable drawState (px + x, py + y + (i * div), w, div))
    | Row svl ->
        let div = pw / svl.Length
        svl |> List.iteri (fun i -> renderViewable drawState (px + (i * div), py, div, ph))
    | Text (f, s) ->
        let font = Map.find f drawState.fonts
        drawState.spriteBatch.DrawString(font, s, vector2 px py, Color.White)
    | Button (f, s, evt) ->
        //drawState.spriteBatch.Draw(buttonBack, rectangle px py pw ph, Color.White)
        let font = Map.find f drawState.fonts
        drawState.spriteBatch.DrawString(font, s, vector2 px py, Color.White)
        if (drawState.mouseState.X, drawState.mouseState.Y) ||> isInside px py pw ph then
            if drawState.mouseState.LeftButton = ButtonState.Pressed then
                evt ()