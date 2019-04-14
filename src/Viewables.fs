module Xelmish.Viewables

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework.Input
open Model

let private vector2 x y = Vector2(float32 x, float32 y)
let private isInside tx ty tw th x y = x >= tx && x <= tx + tw && y >= ty && y <= ty + th

let colour colour (width, height) (x, y) : Viewable = 
    fun loadedAssets _ spriteBatch -> 
        spriteBatch.Draw(loadedAssets.whiteTexture, xnaRect x y width height, colour)

let image key colour (width, height) (x, y) : Viewable = 
    fun loadedAssets _ spriteBatch -> 
        spriteBatch.Draw(loadedAssets.textures.[key], xnaRect x y width height, colour)

let text font (fontSize: float) colour (ox, oy) (text: string) (x, y) : Viewable =
    fun loadedAssets _ spriteBatch -> 
        let font = loadedAssets.fonts.[font]
        let measured = font.MeasureString (text)
        let scale = let v = float32 fontSize / measured.Y in Vector2(v, v)
        let origin = Vector2(float32 (ox % 1.) * measured.X * scale.X, float32 (oy % 1.) * measured.Y * scale.Y)
        let position = Vector2.Add(origin, vector2 x y)
        spriteBatch.DrawString(font, text, position, colour, 0.f, Vector2.Zero, scale, SpriteEffects.None, 0.f)

let onclick event (width, height) (x, y) : Viewable =
    fun _ inputs _ -> 
        if (inputs.mouseState.X, inputs.mouseState.Y) ||> isInside x y width height then
            if inputs.mouseState.LeftButton = ButtonState.Pressed 
            && inputs.lastMouseState.LeftButton <> ButtonState.Pressed then
                event ()

let onkeydown key event: Viewable =
   fun _ inputs _ -> 
        if inputs.keyboardState.IsKeyDown key 
        && not (inputs.lastKeyboardState.IsKeyDown key) then 
            event ()

let whilekeydown key event: Viewable =
   fun _ inputs _ -> 
        if inputs.keyboardState.IsKeyDown key 
        && inputs.lastKeyboardState.IsKeyDown key then 
            event ()

let onkeyup key event : Viewable =
    fun _ inputs _ ->
        if not (inputs.keyboardState.IsKeyDown key)
        && inputs.lastKeyboardState.IsKeyDown key then 
            event ()        