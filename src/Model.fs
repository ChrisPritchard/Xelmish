module Xelmish.Model

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Input
open Microsoft.Xna.Framework.Graphics

/// Represents colour, best constructed via the rgb and rgba helper methods
type Colour = { r: byte; g: byte; b: byte; a: byte }

let rgb r g b = { r = r; g = g; b = b; a = 255uy }
let rgba r g b a = { r = r; g = g; b = b; a = a }

module Colours =
    let white = rgb 255uy 255uy 255uy
    let red = rgb 255uy 0uy 0uy
    let orange = rgb 255uy 165uy 0uy
    let yellow = rgb 255uy 255uy 0uy
    let green = rgb 0uy 255uy 0uy
    let indigo = rgb 75uy 0uy 130uy
    let violet = rgb 238uy 130uy 238uy
    let blue = rgb 0uy 0uy 255uy
    let black = rgb 0uy 0uy 0uy
    let cyan = rgb 0uy 255uy 255uy
    let magenta = rgb 255uy 0uy 255uy
    let silver = rgb 192uy 192uy 192uy
    let whiteSmoke = rgb 245uy 245uy 245uy

let internal xnaColor colour =
    new Color(colour.r, colour.g, colour.b, colour.a)

type Keys = Microsoft.Xna.Framework.Input.Keys

/// Represents a rectangle on the screen, used for destinations like
/// where to draw a colour
type Rectangle = { x: int; y: int; width: int; height: int }
let rect x y w h = { x = x; y = y; width = w; height = h }

let internal xnaRect x y w h = new Microsoft.Xna.Framework.Rectangle(x, y, w, h)
let internal rectToXnaRect r = new Microsoft.Xna.Framework.Rectangle(r.x, r.y, r.width, r.height)

type GameConfig = {
    /// If specified, each draw will be blanked by the colour specified
    clearColour: Colour option
    /// Resolution to render the game (in future this will be changable post init)
    resolution: Resolution
    /// Whether or not the mouse cursor should be visible in the render window
    /// If false and you want a mouse cursor, you will need to render one yourself
    mouseVisible: bool
    /// All assets (like images, fonts etc) that the game will use
    assetsToLoad: Loadable list
} 
/// Specifies the resolution to run the game at. For now, this is set once at initiation.
/// Also, presently fullscreen is not supported.
and Resolution = Windowed of int * int
/// Definitions of assets to load on start, e.g. named texture files.
/// IMPORTANT: all paths are relative paths to content files, e.g. /Content/Sprite.png, 
/// except for fonts, which MUST be relative paths (without extensions) to spritefonts built using the content pipeline.
/// This is because fonts cannot be direct loaded, and must be processed via the pipeline.
and Loadable =
/// key (how it is referenced) and path (full relative path to file)
| Texture of key:string * path:string
/// key (how it is referenced) and path (full relative path (without extension) to spriteFont)
| Font of key:string * path:string

type GameState = internal {
    keyboardState: KeyboardState
    lastKeyboardState: KeyboardState
    mouseState: MouseState
    lastMouseState: MouseState
    textures: Map<string, Texture2D>
    whiteTexture: Texture2D
    fonts: Map<string, SpriteFont>
}