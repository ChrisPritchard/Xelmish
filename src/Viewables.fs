/// This module contains functions that provide some stock viewables for use.
/// For example, drawing a rect of a given colour is defined here.
module Xelmish.Viewables

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework.Input
open Model

/// An alias for the .NET Queue of string class. For 'play once' things like sounds, this
/// is useful to add to your model in order to play effects properly. See the space invaders sample for usage
type KeyQueue = System.Collections.Generic.Queue<string>

let private vector2 x y = Vector2(float32 x, float32 y)
let private isInside tx ty tw th x y = x >= tx && x <= tx + tw && y >= ty && y <= ty + th

/// Draw a coloured rect
let colour colour (width, height) (x, y) = 
    OnDraw (fun loadedAssets _ spriteBatch -> 
        spriteBatch.Draw(loadedAssets.whiteTexture, rect x y width height, colour))

/// Draw the image specified by the given key
let image key colour (width, height) (x, y) = 
    OnDraw (fun loadedAssets _ spriteBatch -> 
        spriteBatch.Draw(loadedAssets.textures.[key], rect x y width height, colour))

/// Draw text at the given size, colour and position. 
/// The ox, oy origin vars determine where the text should be placed relative to the given position,
/// e.g. 0., 0. would be right down from x, y whereas -1., 0. would be left, down, with the last character just before x.
let text font (fontSize: float) colour (ox: float, oy: float) (text: string) (x, y) =
    OnDraw (fun loadedAssets _ spriteBatch -> 
        let font = loadedAssets.fonts.[font]
        let measured = font.MeasureString (text)
        let scale = let v = float32 fontSize / measured.Y in Vector2(v, v)
        let origin = Vector2 (float32 ox * measured.X * scale.X, float32 oy * measured.Y * scale.Y)
        let position = Vector2.Add(origin, vector2 x y)
        spriteBatch.DrawString (font, text, position, colour, 0.f, Vector2.Zero, scale, SpriteEffects.None, 0.f))

/// Play the next sound in a sound queue (a Queue<string> containing keys of sound effects to play)
let playQueuedSound (soundQueue: KeyQueue) =
    OnDraw (fun loadedAssets _ _ ->
        if soundQueue.Count > 0 then 
            let nextSound = soundQueue.Dequeue ()
            ignore (loadedAssets.sounds.[nextSound].Play ()))

/// Run the given event function on every call to Update by the game loop (approx 60 times a second)
let onupdate event = 
    OnUpdate event

/// Run the given event if the left mouse button has just been pressed in the specified area
let onclick event (width, height) (x, y) =
    onupdate (fun inputs -> 
        if (inputs.mouseState.X, inputs.mouseState.Y) ||> isInside x y width height then
            if inputs.mouseState.LeftButton = ButtonState.Pressed 
            && inputs.lastMouseState.LeftButton <> ButtonState.Pressed then
                event ())

/// Run the given event with the current mouse pos if the left mouse button has just been pressed
let onclickpoint event =
    onupdate (fun inputs -> 
        if inputs.mouseState.LeftButton = ButtonState.Pressed 
        && inputs.lastMouseState.LeftButton <> ButtonState.Pressed then
            event (inputs.mouseState.X, inputs.mouseState.Y))

/// Run the given event if the given key has just been pressed
let onkeydown key event =
    onupdate (fun inputs -> 
        if inputs.keyboardState.IsKeyDown key 
        && not (inputs.lastKeyboardState.IsKeyDown key) then 
            event ())

/// Run the given event if the given key is currently pressed
let whilekeydown key event =
    onupdate (fun inputs -> 
        if inputs.keyboardState.IsKeyDown key 
        && inputs.lastKeyboardState.IsKeyDown key then 
            event ())

/// Run the given event if the given key has just been released
let onkeyup key event  =
    onupdate (fun inputs -> 
        if not (inputs.keyboardState.IsKeyDown key)
        && inputs.lastKeyboardState.IsKeyDown key then 
            event ())
