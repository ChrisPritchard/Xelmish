module Xelmish.Viewables

open Model
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework.Input

type Viewable = GameState -> SpriteBatch -> unit

let private vector2 x y = Vector2(float32 x, float32 y)
let private isInside tx ty tw th x y = x >= tx && x <= tx + tw && y >= ty && y <= ty + th

let colour colour (width, height) (x, y) : Viewable = 
    fun gameState spriteBatch -> 
        spriteBatch.Draw(gameState.whiteTexture, xnaRect x y width height, xnaColor colour)

let image key colour (width, height) (x, y) : Viewable = 
    fun gameState spriteBatch -> 
        spriteBatch.Draw(gameState.textures.[key], xnaRect x y width height, xnaColor colour)

let text font (fontSize: float) colour (ox, oy) (text: string) (x, y) : Viewable =
    fun gameState spriteBatch -> 
        let font = gameState.fonts.[font]
        let measured = font.MeasureString (text)
        let scale = let v = float32 fontSize / measured.Y in Vector2(v, v)
        let origin = Vector2(float32 (ox % 1.) * measured.X * scale.X, float32 (oy % 1.) * measured.Y * scale.Y)
        let position = Vector2.Add(origin, vector2 x y)
        spriteBatch.DrawString(font, text, position, xnaColor colour, 0.f, Vector2.Zero, scale, SpriteEffects.None, 0.f)

let onclick event (width, height) (x, y) : Viewable =
    fun gameState _ -> 
        if (gameState.mouseState.X, gameState.mouseState.Y) ||> isInside x y width height then
            if gameState.mouseState.LeftButton = ButtonState.Pressed 
            && gameState.lastMouseState.LeftButton <> ButtonState.Pressed then
                event ()

let onkeydown key event: Viewable =
   fun gameState _ -> 
        if gameState.keyboardState.IsKeyDown key 
        && not (gameState.lastKeyboardState.IsKeyDown key) then 
            event ()

let onkeyup key event : Viewable =
    fun gameState _ ->
        if not (gameState.keyboardState.IsKeyDown key)
        && gameState.lastKeyboardState.IsKeyDown key then 
            event ()        