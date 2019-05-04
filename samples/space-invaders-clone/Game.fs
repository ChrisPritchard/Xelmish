module Game

open Elmish
open Xelmish.Model
open Xelmish.Viewables
open Common

type Model = {
    lastTick: int64
    player: Player.Model
    bunkers: Rectangle list
    invaders: Invaders.Model
    score: int
    lives: int
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
        score = 0
        lives = 3
    }, Cmd.none

type Message = 
    | PlayerMessage of Player.Message
    | InvadersMessage of Invaders.Message
    | UpdateDying of int64
    | CheckLaserCollisions
    | InvaderHit of row: int * index: int
    | PlayerHit
    | Victory
    | GameOver
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

let checkPlayerLaserCollisions model =
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
        checkPlayerLaserCollisions model
    let nextInvaderLasers, secondCommand, newBunkers = 
        checkInvaderLaserCollisions { model with bunkers = newBunkers }
    let newBunkers = eraseBunkers model.invaders.rows newBunkers
        
    { model with 
        player = { model.player with laser = nextPlayerLaser }
        bunkers = newBunkers
        invaders = { model.invaders with lasers = nextInvaderLasers } }, Cmd.batch [firstCommand;secondCommand]
        
let updateDying atTime model = 
    match model.player.state with
    | Player.Dying 0 when model.lives = 0 ->
        model, Cmd.ofMsg GameOver
    | Player.Dying 0 ->
        { model with
            player = Player.init ()
            lives = model.lives - 1
            invaders = { model.invaders with lasers = [] } }, Cmd.none
    | _ ->
        let newInvaders = 
            model.invaders.rows
            |> Array.map (fun row ->
                { row with xs = row.xs |> Array.map (fun (x, state) -> if state = Invaders.Dying then x, Invaders.Dead else x, state) })
        let newPlayer = 
            match model.player.state with 
            | Player.Dying n -> { model.player with state = Player.Dying (n - 1) } 
            | _ -> model.player
        { model with 
            invaders = { model.invaders with rows = newInvaders }
            player = newPlayer
            lastTick = atTime }, Cmd.none

let update message model =
    match message with
    | PlayerMessage message -> 
        { model with player = Player.update message model.player }, Cmd.none
    | InvadersMessage message -> 
        { model with invaders = Invaders.update message model.invaders }, Cmd.none
    | UpdateDying atTime -> updateDying atTime model
    | CheckLaserCollisions -> checkLaserCollisions model
    | InvaderHit (row, index) -> 
        { model with score = model.score + model.invaders.rows.[row].kind.score },
        Cmd.ofMsg (InvadersMessage (Invaders.Destroy (row, index)))
    | PlayerHit -> { model with player = { model.player with state = Player.Dying dieLength } }, Cmd.none
    | Victory -> model, Cmd.none // todo
    | GameOver -> model, Cmd.none // todo
    | Restart -> init ()

let text = text "PressStart2P" 24. Colour.White (0., 0.)

let view model dispatch =
    [
        yield text "SCORE" (10, 10)
        yield text (sprintf "%04i" model.score) (10, 44)

        yield text "LIVES" (150, 10)
        yield text (sprintf "%02i" model.lives) (150, 44)

        yield! Player.view model.player (PlayerMessage >> dispatch)
        yield! Invaders.view model.invaders (InvadersMessage >> dispatch) (model.player.state <> Player.Alive)

        yield! model.bunkers
            |> List.map (fun r -> 
                colour bunkerColour (r.Width, r.Height) (r.Left, r.Top))

        if model.player.state = Player.Alive then
            yield onupdate (fun _ -> dispatch CheckLaserCollisions)

        yield onupdate (fun inputs -> 
            if inputs.totalGameTime - model.lastTick > tickInterval then
                dispatch (UpdateDying inputs.totalGameTime))

        yield onkeydown Keys.Escape exit
        yield onkeydown Keys.R (fun () -> dispatch Restart)
    ]