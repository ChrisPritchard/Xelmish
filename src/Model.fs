module Xelmish.Model

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Input
open Microsoft.Xna.Framework.Graphics

/// Possible keys on the keyboard - aliased here so games don't need to understand/reference XNA namespaces
type Keys = Microsoft.Xna.Framework.Input.Keys
/// Colours - aliased here so that games don't need to understand/reference XNA namespaces (also to fix the spelling)
type Colour = Microsoft.Xna.Framework.Color
/// Creates an xna rect from x, y, w, h values
let rect x y w h = new Microsoft.Xna.Framework.Rectangle(x, y, w, h)

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
with member __.totalGameTime with get() = int64 __.gameTime.TotalGameTime.TotalMilliseconds

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

/// If a game throws this exception, the gameloop will catch it and quit.
type QuitGame() =
    inherit System.Exception()

let exit () = raise (QuitGame ())