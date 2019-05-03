open Elmish
open Xelmish.Model
open Xelmish.Viewables
open Config

let tickInterval = 500L

type PlayingModel = {
    lastTick: int64
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
        lastTick = 0L
        player = Player.init ()
        bunkers = defaultBunkers
        invaders = Invaders.init ()
        freeze = false
        score = 0
    }, Cmd.none

type Message = 
    | PlayerMessage of Player.Message
    | InvadersMessage of Invaders.Message
    | UpdateDying of int64
    | CheckLaserCollisions
    | InvaderHit of row: int * index: int
    | PlayerHit
    | Victory
    | Restart

let playerRect model = rect model.player.x playerY playerWidth playerHeight

let invaderImpact x y w h model =
    let testRect = rect x y w h
    model.invaders.rows
    |> Seq.indexed 
    |> Seq.collect (fun (r, row) -> 
        row.xs 
        |> Seq.indexed 
        |> Seq.filter (fun (_, (_, state)) -> state = Invaders.Alive)
        |> Seq.map (fun (c, (x, _)) -> (r, c), rect x row.y row.kind.width row.kind.height))
    |> Seq.tryFind (fun (_, rect) -> rect.Intersects testRect)
    |> Option.map fst

let eraseBunkers (invaderRows: Invaders.Row []) bunkers =
    let invaderRects = 
        invaderRows 
        |> Seq.collect (fun row -> 
            row.xs 
            |> Seq.filter (fun (_, state) -> state = Invaders.Alive) 
            |> Seq.map (fun (x, _) -> rect x row.y row.kind.width row.kind.height))
        |> Seq.toList
    bunkers 
    |> List.filter (fun bunker -> 
        invaderRects 
        |> List.exists (fun invaderRect -> 
            invaderRect.Intersects bunker) 
        |> not)

let movePlayerLaserCollisions model =
    match model.player.laser with
    | None -> None, Cmd.none, model.bunkers
    | Some (x, y) ->
        match invaderImpact x y projectileWidth projectileHeight model with
        | Some invaderIndex -> None, Cmd.ofMsg (InvaderHit invaderIndex), model.bunkers
        | None -> 
            let shotRect = rect x y projectileWidth projectileHeight
            let destroyed, newBunkers = model.bunkers |> List.partition (fun b -> b.Intersects shotRect)
            if List.isEmpty destroyed then
                Some (x, y), Cmd.none, model.bunkers
            else
                None, Cmd.none, newBunkers

let checkInvaderLaserCollisions model =
    let playerRect = playerRect model
    (([], Cmd.none, model.bunkers), model.invaders.lasers)
    ||> List.fold (fun (acc, cmdResult, bunkers) (x, y) ->
        let shotRect = rect x y projectileWidth projectileHeight
        if shotRect.Intersects playerRect then
            acc, Cmd.ofMsg PlayerHit, bunkers
        else
            let destroyed, newBunkers = bunkers |> List.partition (fun b -> b.Intersects shotRect)
            if List.isEmpty destroyed then
                (x, y)::acc, cmdResult, bunkers
            else
                acc, cmdResult, newBunkers)

let checkLaserCollisions model =
    let nextPlayerLaser, firstCommand, newBunkers = 
        movePlayerLaserCollisions model
    let nextInvaderLasers, secondCommand, newBunkers = 
        checkInvaderLaserCollisions { model with bunkers = newBunkers }
    let newBunkers = eraseBunkers model.invaders.rows newBunkers
        
    { model with 
        player = { model.player with laser = nextPlayerLaser }
        bunkers = newBunkers
        invaders = { model.invaders with lasers = nextInvaderLasers } }, Cmd.batch [firstCommand;secondCommand]

let destroyInvader targetRow index model =
    let newInvaders =
        model.invaders.rows
            |> Array.mapi (fun i row ->
                if i <> targetRow then row
                else
                    let newXs = 
                        row.xs 
                        |> Array.mapi (fun i (x, state) -> 
                            if i <> index then (x, state) else (x, Invaders.Dying))
                    { row with xs = newXs })
    let scoreIncrease = model.invaders.rows.[targetRow].kind.score
    let newShuffleInterval = max minShuffle (model.invaders.shuffleInterval - shuffleDecrease)
    { model with
        invaders = { model.invaders with rows = newInvaders; shuffleInterval = newShuffleInterval }
        score = model.score + scoreIncrease }, Cmd.none
        
let update message model =
    match message with
    | PlayerMessage message -> 
        { model with player = Player.update message model.player }, Cmd.none
    | InvadersMessage message -> 
        { model with invaders = Invaders.update message model.invaders }, Cmd.none
    | UpdateDying atTime -> 
        let newInvaders = 
            model.invaders.rows
            |> Array.map (fun row ->
                { row with xs = row.xs |> Array.map (fun (x, state) -> if state = Invaders.Dying then x, Invaders.Dead else x, state) })
        { model with 
            invaders = { model.invaders with rows = newInvaders }
            lastTick = atTime }, Cmd.none
    | CheckLaserCollisions -> checkLaserCollisions model
    | InvaderHit (row, index) -> destroyInvader row index model
    | PlayerHit -> { model with freeze = true }, Cmd.none
    | Victory -> { model with freeze = true }, Cmd.none
    | Restart -> init ()

let text = text "PressStart2P" 24. Colour.White (0., 0.)

let view model dispatch =
    [
        yield text "SCORE" (10, 10)
        yield text (sprintf "%04i" model.score) (10, 44)

        yield! Player.view model.player (PlayerMessage >> dispatch) model.freeze

        yield! model.bunkers
            |> List.map (fun r -> 
                colour bunkerColour (r.Width, r.Height) (r.Left, r.Top))

        yield! Invaders.view model.invaders (InvadersMessage >> dispatch) model.freeze

        if not model.freeze then
            yield onupdate (fun _ -> dispatch CheckLaserCollisions)
            yield onupdate (fun inputs -> 
                if inputs.totalGameTime - model.lastTick > tickInterval then
                    dispatch (UpdateDying inputs.totalGameTime))

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