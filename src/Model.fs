module Xelmish.Model

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Input
open Microsoft.Xna.Framework.Graphics

/// Represents colour, best constructed via the rgb and rgba helper methods
type Colour = { r: byte; g: byte; b: byte; a: byte }

/// Takes a rgb value (three bytes for red, green and blue) and produces a colour record, with no transparency
let rgb r g b = { r = r; g = g; b = b; a = 255uy }
/// Takes a rgb value (three bytes for red, green and blue) plus a byte for transparency and produces a colour record
let rgba r g b a = { r = r; g = g; b = b; a = a }

/// Common colours, e.g. white, black, the rainbow and extras
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

/// Possible keys on the keyboard
type Keys = Microsoft.Xna.Framework.Input.Keys

/// Represents a rectangle on the screen, used for destinations like
/// where to draw a colour
type Rectangle = { x: int; y: int; width: int; height: int }

/// Produces a rectangle record for given x, y, width and height values
let rect x y w h = { x = x; y = y; width = w; height = h }

let internal xnaRect x y w h = new Microsoft.Xna.Framework.Rectangle(x, y, w, h)
let internal rectToXnaRect r = new Microsoft.Xna.Framework.Rectangle(r.x, r.y, r.width, r.height)

/// The broad config for the game loop, e.g. resolution and clear colour, plus assets to load at startup
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

/// Current and previous state of input devices
type Inputs = {
    keyboardState: KeyboardState
    lastKeyboardState: KeyboardState
    mouseState: MouseState
    lastMouseState: MouseState
    gameTime: GameTime
}

/// Assets loaded at startup for use
type LoadedAssets = {
    textures: Map<string, Texture2D>
    whiteTexture: Texture2D
    fonts: Map<string, SpriteFont>
}

/// On each draw, a list of viewables provided by the main Xelmish component will be run in the order provided.
/// Each is given the set of loaded asseets (e.g. textures), the current input state, and the spriteBatch object to draw with.
/// In the Viewables module, there are functions that create viewables for common tasks, like drawing colours or images.
type Viewable = LoadedAssets -> Inputs -> SpriteBatch -> unit

/// Messages from the elmish view methods that should effect the game engine, e.g. tell it to quit.
type GameMessage =
/// Do nothing
| NoOp
/// Exit the loop, and thus the program
| Exit