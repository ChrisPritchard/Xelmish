open Elmish
open Xelmish.Model
open Xelmish.Viewables
open Config

type PlayingModel = {
    player: Player.Model
    bunkers: Rectangle list
    invaders: Invaders.Model
    freeze: bool
    score: int
} 

and Projectile = { x: int; y: int }

let defaultBunkers =
    let dim = bunkerBitDim
    let bunkerAt x y =
        bunkerPattern 
        |> Seq.indexed 
        |> Seq.collect (fun (row, elems) ->
            elems 
            |> Seq.indexed 
            |> Seq.filter snd
            |> Seq.map (fun (col, _) -> rect (x + col * dim) (y + row * dim) dim dim))
        |> Seq.toList

    let bunkerY = playerY - bunkerSpace - bunkerHeight
    let spacing = resWidth / 5
    List.concat [
        bunkerAt (spacing - bunkerOffset) bunkerY
        bunkerAt (spacing * 2 - bunkerOffset) bunkerY
        bunkerAt (spacing * 3 - bunkerOffset) bunkerY
        bunkerAt (spacing * 4 - bunkerOffset) bunkerY
    ]

let init () = 
    {
        player = Player.init ()
        bunkers = defaultBunkers
        invaders = Invaders.init ()
        freeze = false
        score = 0
    }, Cmd.none

type Message = 
    | PlayerMessage of Player.Message
    | InvadersMessage of Invaders.Message
    | UpdateDying
    | MoveProjectiles
    | InvaderHit of row: int * index: int
    | PlayerHit
    | Victory
    | Restart

let playerRect model = rect model.player.x playerY playerWidth playerHeight

let invaderImpact x y w h model =
    let testRect = rect x y w h
    model.invaders 
    |> Seq.indexed 
    |> Seq.collect (fun (r, row) -> 
        row.xs 
        |> Seq.indexed 
        |> Seq.filter (fun (_, (_, state)) -> state = Alive)
        |> Seq.map (fun (c, (x, _)) -> (r, c), rect x row.y row.kind.width row.kind.height))
    |> Seq.tryFind (fun (_, rect) -> rect.Intersects testRect)
    |> Option.map fst

let eraseBunkers invaders model =
    let invaderRects = 
        invaders 
        |> Seq.collect (fun row -> 
            row.xs 
            |> Seq.filter (fun (_, state) -> state = Alive) 
            |> Seq.map (fun (x, _) -> rect x row.y row.kind.width row.kind.height))
        |> Seq.toList
    model.bunkers 
    |> List.filter (fun bunker -> 
        invaderRects 
        |> List.exists (fun invaderRect -> 
            invaderRect.Intersects bunker) 
        |> not)

let movePlayerProjectile model =
    match model.player.laser with
    | None -> None, Cmd.none, model.bunkers
    | Some (x, y) ->
        let (nx, ny) = x, y - playerProjectileSpeed
        if ny < 0 then None, Cmd.none, model.bunkers
        else
            match invaderImpact nx ny projectileWidth projectileHeight model with
            | Some invaderIndex -> None, Cmd.ofMsg (InvaderHit invaderIndex), model.bunkers
            | None -> 
                let shotRect = rect nx ny projectileWidth projectileHeight
                let destroyed, newBunkers = model.bunkers |> List.partition (fun b -> b.Intersects shotRect)
                if List.isEmpty destroyed then
                    Some (nx, ny), Cmd.none, model.bunkers
                else
                    None, Cmd.none, newBunkers

let moveInvaderProjectiles model =
    let playerRect = playerRect model
    (([], Cmd.none, model.bunkers), model.invaders.lasers)
    ||> List.fold (fun (acc, cmdResult, bunkers) p ->
        let next = { p with y = p.y + invaderProjectileSpeed }
        if next.y > resHeight then acc, cmdResult, bunkers
        else
            let shotRect = rect p.x p.y projectileWidth projectileHeight
            if shotRect.Intersects playerRect then
                acc, Cmd.ofMsg PlayerHit, bunkers
            else
                let destroyed, newBunkers = bunkers |> List.partition (fun b -> b.Intersects shotRect)
                if List.isEmpty destroyed then
                    next::acc, cmdResult, bunkers
                else
                    acc, cmdResult, newBunkers)

let moveProjectiles model =
    let nextPlayerProjectile, firstCommand, newBunkers = 
        movePlayerProjectile model
    let nextInvaderProjectiles, secondCommand, newBunkers = 
        moveInvaderProjectiles { model with bunkers = newBunkers }
        
    { model with 
        player = { model.player with laser = nextPlayerProjectile }
        bunkers = newBunkers
        invaderProjectiles = nextInvaderProjectiles }, Cmd.batch [firstCommand;secondCommand]

let destroyInvader targetRow index model =
    let newInvaders =
        model.invaders 
            |> Array.mapi (fun i row ->
                if i <> targetRow then row
                else
                    let newXs = 
                        row.xs 
                        |> Array.mapi (fun i (x, state) -> 
                            if i <> index then (x, state) else (x, Dying))
                    { row with xs = newXs })
    let scoreIncrease = model.invaders.[targetRow].kind.score
    { model with
        invaders = newInvaders
        shuffleInterval = max minShuffle (model.shuffleInterval - shuffleDecrease)
        score = model.score + scoreIncrease }, Cmd.none
        
let update message model =
    match message with
    | PlayerMessage message -> 
        { model with player = Player.update message model.player }, Cmd.none
    | InvaderShoot -> shootFromInvader model
    | ShuffleInvaders time -> shuffleInvaders time model   
    | UpdateDying -> 
        let newInvaders = 
            model.invaders 
            |> Array.map (fun row ->
                { row with xs = row.xs |> Array.map (fun (x, state) -> if state = Dying then x, Dead else x, state) })
        { model with invaders = newInvaders }, Cmd.none
    | MoveProjectiles -> moveProjectiles model
    | InvaderHit (row, index) -> destroyInvader row index model
    | PlayerHit -> { model with freeze = true }, Cmd.none
    | Victory -> { model with freeze = true }, Cmd.none
    | Restart -> init ()
    
let sprite (sw, sh, sx, sy) (w, h) (x, y) colour =
    OnDraw (fun loadedAssets (spriteBatch: SpriteBatch) ->
        let texture = loadedAssets.textures.["sprites"]
        spriteBatch.Draw (texture, rect x y w h, System.Nullable(rect sx sy sw sh), colour))

let text = text "PressStart2P" 24. Colour.White (0., 0.)

let view model dispatch =
    [
        yield text "SCORE" (10, 10)
        yield text (sprintf "%04i" model.score) (10, 44)

        yield! model.bunkers
            |> List.map (fun r -> 
                colour bunkerColour (r.Width, r.Height) (r.Left, r.Top))

        yield! Player.view model.player (PlayerMessage >> dispatch)

        if not model.freeze then
            yield onupdate (fun _ -> dispatch MoveProjectiles)

        yield onkeydown Keys.Escape exit
        yield onkeydown Keys.R (fun () -> dispatch Restart)
    ]

[<EntryPoint>]
let main _ =
    let config: GameConfig = {
        clearColour = Some Colour.Black
        resolution = Windowed (resWidth, resHeight)
        assetsToLoad = [ 
            Texture ("sprites", "./sprites.png")
            Font ("PressStart2P", "./PressStart2P") ]
        mouseVisible = false
        showFpsInConsole = true
    }

    Program.mkProgram init update view
    |> Xelmish.Program.runGameLoop config

    0