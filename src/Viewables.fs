module Xelmish.Viewables

open Model
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework.Input

type Viewable =
| Colour of colour: Colour * size: (int * int) * pos: (int * int)
| Image of key: string * colour: Colour * size: (int * int) * pos: (int * int)
| Text of text: string * font: string * size: float * colour: Colour * origin: (float * float) * pos: (int * int)
| OnClick of event: (Unit -> Unit) * size: (int * int) * pos: (int * int)
| OnKeyDown of key: Model.Keys * event: (Unit -> Unit)

let colour colour size pos = Colour (colour, size, pos)
let image key colour size pos = Image (key, colour, size, pos)
let text font size colour origin text pos = Text (text, font, size, colour, origin, pos)
let onclick event size pos = OnClick (event, size, pos)
let onkeydown keyCode event = OnKeyDown (keyCode, event)

let private vector2 x y = Vector2(float32 x, float32 y)
let private isInside tx ty tw th x y = x >= tx && x <= tx + tw && y >= ty && y <= ty + th

let rec internal renderViewable (spriteBatch: SpriteBatch) gameState viewable =
    match viewable with
    | Colour (colour, (width, height), (x, y)) ->
        spriteBatch.Draw(gameState.whiteTexture, xnaRect x y width height, xnaColor colour)
    | Image (key, colour, (width, height), (x, y)) ->
        spriteBatch.Draw(gameState.textures.[key], xnaRect x y width height, xnaColor colour)
    | Text (text, font, size, colour, (ox, oy), (x, y)) ->
        let font = gameState.fonts.[font]
        let measured = font.MeasureString (text)
        let scale = let v = float32 size / measured.Y in Vector2(v, v)
        let origin = Vector2(float32 (ox % 1.) * measured.X * scale.X, float32 (oy % 1.) * measured.Y * scale.Y)
        let position = Vector2.Add(origin, vector2 x y)
        spriteBatch.DrawString(font, text, position, xnaColor colour, 0.f, Vector2.Zero, scale, SpriteEffects.None, 0.f)
    | OnClick (event, (width, height), (x, y)) ->
        if (gameState.mouseState.X, gameState.mouseState.Y) ||> isInside x y width height then
            if gameState.mouseState.LeftButton = ButtonState.Pressed 
            && gameState.lastMouseState.LeftButton <> ButtonState.Pressed then
                event ()
    | OnKeyDown (key, event) ->
        if gameState.keyboardState.IsKeyDown key 
           && not (gameState.lastKeyboardState.IsKeyDown key) then event ()