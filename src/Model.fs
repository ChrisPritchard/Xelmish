﻿module Xelmish.Model

open Microsoft.Xna.Framework.Input
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework.Audio
open Microsoft.Xna.Framework.Media

// Aliases here so that games don't need to understand/reference XNA namespaces (also to fix the spelling)

type Keys = Microsoft.Xna.Framework.Input.Keys
type ButtonState = Microsoft.Xna.Framework.Input.ButtonState
type Colour = Microsoft.Xna.Framework.Color
type Rectangle = Microsoft.Xna.Framework.Rectangle
type SpriteBatch = Microsoft.Xna.Framework.Graphics.SpriteBatch
type GameTime = Microsoft.Xna.Framework.GameTime

let rgba (r: byte) (g: byte) (b: byte) (a: byte) = Colour (r, g, b, a)

/// Creates an xna rect from x, y, w, h values
let rect x y w h = new Rectangle(x, y, w, h)

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
and Resolution = 
    | Windowed of int * int
    | FullScreen of int * int 
    | Borderless of int * int 
/// Definitions of assets to load on start, e.g. named texture files.
/// IMPORTANT: all paths are relative paths to content files, e.g. /Content/Sprite.png, 
/// except for fonts, which MUST be relative paths (without extensions) to spritefonts built using the content pipeline.
/// This is because fonts cannot be direct loaded, and must be processed via the pipeline.
and Loadable =
/// key (how it is referenced) and path (full relative path to texture file)
| FileTexture of key:string * path:string
/// key (how it is referenced) and path (full relative path (without extension) to texture source)
| PipelineTexture of key:string * path:string
/// key (how it is referenced) and path (full relative path (without extension) to spriteFont)
| PipelineFont of key:string * path:string
/// key (how it is referenced) and path (full relative path to sound file)
| FileSound of key:string * path:string
/// key (how it is referenced) and path (full relative path (without extension) to sound source)
| PipelineSound of key:string * path:string
/// key (how it is referenced) and path (full relative path to music file)
| FileMusic of key:string * path:string
/// key (how it is referenced) and path (full relative path (without extension) to music source)
| PipelineMusic of key:string * path:string

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
    whiteTexture: Texture2D
    textures: Map<string, Texture2D>
    fonts: Map<string, SpriteFont>
    sounds: Map<string, SoundEffect>
    music: Map<string, Song>
}

/// On each draw and update of the core loop, a list of viewables provided by the main Xelmish component will be run 
/// if appropriate, in the order provided.
/// Each is given the set of loaded assets (e.g. textures) and the spriteBatch object if drawable to draw with, or
/// the current input state if on update.
/// In the Viewables module, there are functions that create viewables for common tasks, like drawing colours or images.
type Viewable = 
    | OnDraw of (LoadedAssets -> Inputs -> SpriteBatch -> unit)
    | OnUpdate of (Inputs -> unit)

/// If a game throws this exception during the update call (e.g. via an OnUpdate viewable), the gameloop will catch it and quit.
/// Note: when referencing via Nuget or .dll, debuggers will halt on this exception. However, you can safely continue if they do.
type QuitGame() =
    inherit System.Exception()

let exit () = raise (QuitGame ())
