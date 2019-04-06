module Xelmish.Viewables

open Model
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics

type Viewable =
| Colour of colour: Colour * size: (int * int) * pos: (int * int)
| Image of key: string * colour: Colour * size: (int * int) * pos: (int * int)
| Text of text: string * font: string * size: float * colour: Colour * pos: (int * int)

let colour colour size pos = Colour (colour, size, pos)
let image key colour size pos = Image (key, colour, size, pos)
let text text font size colour pos = Text(text, font, size, colour, pos)

let stack (x, y) height (children: ((int * int) -> Viewable) list) =
    let div = height / children.Length
    children |> List.mapi (fun i child ->
        let pos = (x, y + i * div)
        child pos)

let row (x, y) width (children: ((int * int) -> Viewable) list) =
    let div = width / children.Length
    children |> List.mapi (fun i child ->
        let pos = (x + i * div, y)
        child pos)


let private vector2 x y = Vector2(float32 x, float32 y)
let private isInside tx ty tw th x y = x >= tx && x <= tx + tw && y >= ty && y <= ty + th

let rec internal renderViewable (spriteBatch: SpriteBatch) gameTime gameState (px, py, pw, ph) viewable =
    match viewable with
    | Colour (colour, (width, height), (x, y)) ->
        spriteBatch.Draw(gameState.whiteTexture, xnaRect x y width height, xnaColor colour)
    | Image (key, colour, (width, height), (x, y)) ->
        spriteBatch.Draw(gameState.textures.[key], xnaRect x y width height, xnaColor colour)
    | Text (text, font, size, colour, (x, y)) ->
        let font = gameState.fonts.[font]
        //let scale = scaleFor size font
        spriteBatch.DrawString(font, text, vector2 x y, xnaColor colour)

        //if (gameState.mouseState.X, gameState.mouseState.Y) ||> isInside px py pw ph then
        //    if gameState.mouseState.LeftButton = ButtonState.Pressed 
        //    && gameState.lastMouseState.LeftButton <> ButtonState.Pressed then
        //        evt ()