module Xelmish.Model

open Microsoft.Xna.Framework

/// Represents colour, best constructed via the rgb and rgba helper methods
type Colour = { r: byte; g: byte; b: byte; a: byte }

let rgb r g b = { r = r; g = g; b = b; a = 255uy }
let rgba r g b a = { r = r; g = g; b = b; a = a }

let internal xnaColor colour =
    new Color(colour.r, colour.g, colour.b, colour.a)

type GameLoopConfig = {
    /// If specified, each draw will be blanked by the colour specified
    clearColour: Colour option
    /// Resolution to render the game (in future this will be changable post init)
    resolution: Resolution
    /// Whether or not the mouse cursor should be visible in the render window
    /// If false and you want a mouse cursor, you will need to render one yourself
    mouseVisible: bool
} 
and Resolution = Windowed of int * int