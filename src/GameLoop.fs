module internal Xelmish.GameLoop

open System.IO
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework.Input
open Microsoft.Xna.Framework.Audio
open Microsoft.Xna.Framework.Media
open Model
open Helpers

type GameLoop (config: GameConfig) as this = 
    inherit Game ()

    let graphics = new GraphicsDeviceManager (this)
    let mutable spriteBatch = Unchecked.defaultof<_>

    let mutable assets = Unchecked.defaultof<_>
    let mutable inputs = {
        keyboardState = Unchecked.defaultof<_>
        lastKeyboardState = Unchecked.defaultof<_>
        mouseState = Unchecked.defaultof<_>
        lastMouseState = Unchecked.defaultof<_>
        gameTime = Unchecked.defaultof<_>
    }
    
    let mutable fps = 0
    let mutable lastFpsUpdate = 0.
    let fpsUpdateInterval = 200.

    let mutable updatable: (Inputs -> Unit) list = []
    let mutable drawable: (LoadedAssets -> SpriteBatch -> Unit) list = []

    do 
        match config.resolution with
        | Windowed (w, h) -> 
            graphics.PreferredBackBufferWidth <- w
            graphics.PreferredBackBufferHeight <- h

        this.IsMouseVisible <- config.mouseVisible
        graphics.SynchronizeWithVerticalRetrace <- true
        this.IsFixedTimeStep <- false 
        
    member __.View
        with set value = 
            let newUpdatables, newDrawables =
                (([], []), Seq.rev value)
                ||> Seq.fold (fun (updatable, drawable) -> 
                    function
                    | OnUpdate f -> 
                        f::updatable, drawable
                    | OnDraw f ->
                        updatable, f::drawable)
            updatable <- newUpdatables
            drawable <- newDrawables

    override __.LoadContent () = 
        spriteBatch <- new SpriteBatch (graphics.GraphicsDevice)
        
        let loadedAssets = 
            { whiteTexture = new Texture2D (this.GraphicsDevice, 1, 1)
              textures = Map.empty 
              fonts = Map.empty 
              sounds = Map.empty 
              music = Map.empty }
        loadedAssets.whiteTexture.SetData<Color> [| Color.White |]

        let loadedAssets =
            (loadedAssets, config.assetsToLoad)
            ||> List.fold (fun assets ->
                function
                | Texture (key, path) -> 
                    use stream = File.OpenRead path
                    let texture = Texture2D.FromStream (this.GraphicsDevice, stream)
                    { assets with textures = Map.add key texture assets.textures }
                | Font (key, path) -> 
                    let font = this.Content.Load<SpriteFont> path
                    { assets with fonts = Map.add key font assets.fonts }
                | Sound (key, path) -> 
                    use stream = File.OpenRead path
                    let sound = SoundEffect.FromStream stream
                    { assets with sounds = Map.add key sound assets.sounds }
                | Music (key, path) -> 
                    let uri = new System.Uri (path, System.UriKind.RelativeOrAbsolute)
                    let music = Song.FromUri (key, uri)
                    { assets with music = Map.add key music assets.music })
        assets <- loadedAssets

    override __.Update gameTime =
        inputs <- 
            {   lastKeyboardState = inputs.keyboardState
                keyboardState = Keyboard.GetState ()
                lastMouseState = inputs.mouseState
                mouseState = Mouse.GetState ()
                gameTime = gameTime }

        try
            for updateFunc in updatable do updateFunc inputs
        with
            | :? QuitGame -> __.Exit()

    override __.Draw gameTime =
        Option.iter this.GraphicsDevice.Clear config.clearColour

        spriteBatch.Begin (SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp)
        for drawFunc in drawable do drawFunc assets spriteBatch
        spriteBatch.End ()
        
        if config.showFpsInConsole then 
            if gameTime.TotalGameTime.TotalMilliseconds - lastFpsUpdate > fpsUpdateInterval then
                fps <- int (1. / gameTime.ElapsedGameTime.TotalSeconds)
                lastFpsUpdate <- gameTime.TotalGameTime.TotalMilliseconds
                printFps fps