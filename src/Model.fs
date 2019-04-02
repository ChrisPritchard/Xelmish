module Xelmish.Model

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Input
open Microsoft.Xna.Framework.Graphics

/// Represents colour, best constructed via the rgb and rgba helper methods
type Colour = { r: byte; g: byte; b: byte; a: byte }

let rgb r g b = { r = r; g = g; b = b; a = 255uy }
let rgba r g b a = { r = r; g = g; b = b; a = a }

let internal xnaColor colour =
    new Color(colour.r, colour.g, colour.b, colour.a)

type GameConfig = {
    /// If specified, each draw will be blanked by the colour specified
    clearColour: Colour option
    /// Resolution to render the game (in future this will be changable post init)
    resolution: Resolution
    /// Whether or not the mouse cursor should be visible in the render window
    /// If false and you want a mouse cursor, you will need to render one yourself
    mouseVisible: bool
} 
/// Specifies the resolution to run the game at. For now, this is set once at initiation.
/// Also, presently full screen is not supported.
and Resolution = Windowed of int * int

type DrawState = internal {
    gameTime: GameTime
    keyboardState: KeyboardState
    mouseState: MouseState
    spriteBatch: SpriteBatch
}