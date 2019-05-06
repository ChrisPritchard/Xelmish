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
    highScore: int
    lives: int
    soundQueue: KeyQueue
}

let earthLine = rect padding (resHeight - padding) (resWidth - padding * 2) 2

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

let init highScore = 
    {
        lastTick = 0L
        player = Player.init ()
        bunkers = defaultBunkers
        invaders = Invaders.init ()
        score = 0
        highScore = highScore
        lives = 3
        soundQueue = KeyQueue (["startgame"])
    }, Cmd.none

type Message = 
    | PlayerMessage of Player.Message
    | InvadersMessage of Invaders.Message
    | UpdateDying of tickTime:int64
    | CheckCollisions
    | InvaderHit of row:int * index:int
    | PlayerHit
    | Victory of score:int * highScore:int
    | GameOver of score:int * highScore:int
    | Restart

let indexedInvaders model =
    model.invaders.rows
    |> Seq.indexed 
    |> Seq.collect (fun (r, row) -> 
        row.xs 
        |> Seq.indexed 
        |> Seq.filter (fun (_, (_, state)) -> state = Invaders.Alive)
        |> Seq.map (fun (c, (x, _)) -> (r, c), rect x row.y row.kind.width row.kind.height))
    |> Seq.toList

let checkPlayerLaserCollisions indexedInvaders model =
    match model.player.laser with
    | None -> None, Cmd.none, model.bunkers
    | Some (x, y) ->
        let laserRect = rect x y projectileWidth projectileHeight
        match indexedInvaders |> List.tryFind (fun (_, invaderRect) -> laserRect.Intersects invaderRect) with
        | Some (invaderIndex, _) -> None, Cmd.ofMsg (InvaderHit invaderIndex), model.bunkers
        | None -> 
            let shotRect = rect x y projectileWidth projectileHeight
            let destroyed, newBunkers = model.bunkers |> List.partition (fun b -> b.Intersects shotRect)
            if List.isEmpty destroyed then
                Some (x, y), Cmd.none, model.bunkers
            else
                None, Cmd.none, newBunkers

let checkInvaderLaserCollisions playerRect bunkers model =
    (([], Cmd.none, bunkers), model.invaders.lasers)
    ||> List.fold (fun (acc, cmdResult, bunkers) (x, y) ->
        let shotRect = rect x y projectileWidth projectileHeight
        if shotRect.Intersects playerRect then
            model.soundQueue.Enqueue "explosion"
            acc, Cmd.ofMsg PlayerHit, bunkers
        else
            let destroyed, newBunkers = 
                bunkers |> List.partition (fun bunker -> 
                    shotRect.Intersects bunker)
            if List.isEmpty destroyed then
                (x, y)::acc, cmdResult, bunkers
            else
                acc, cmdResult, newBunkers)

let checkCollisions model =
    if model.invaders.alive = 0 then
        model, Cmd.ofMsg (Victory (model.score, model.highScore))
    else
        let indexedInvaders = indexedInvaders model
        let playerRect = rect model.player.x playerY playerWidth playerHeight
        let nextPlayerLaser, invaderHit, newBunkers = checkPlayerLaserCollisions indexedInvaders model
        let nextInvaderLasers, playerHit, newBunkers = checkInvaderLaserCollisions playerRect newBunkers model
        let finalBunkers = 
            newBunkers |> List.filter (fun bunker -> 
                not (indexedInvaders |> List.exists (fun (_, invaderRect) -> invaderRect.Intersects bunker)))
    
        let lives, gameOver =
            let fatalImpact = 
                indexedInvaders 
                |> List.tryFind (fun (_, invaderRect) -> 
                    invaderRect.Intersects earthLine || invaderRect.Intersects playerRect)
            match fatalImpact with
            | None -> model.lives, Cmd.none
            | Some _ -> 
                model.soundQueue.Enqueue "explosion"
                0, Cmd.ofMsg PlayerHit
        
        { model with 
            player = { model.player with laser = nextPlayerLaser }
            lives = lives
            bunkers = finalBunkers
            invaders = { model.invaders with lasers = nextInvaderLasers } }, 

        Cmd.batch [invaderHit; playerHit; gameOver]
        
let updateDying atTime model = 
    match model.player.state with
    | Player.Dying 0 when model.lives = 1 || model.lives = 0 ->
        model, Cmd.ofMsg (GameOver (model.score, model.highScore))
    | Player.Dying 0 ->
        { model with
            player = Player.init ()
            lives = model.lives - 1
            invaders = { model.invaders with lasers = [] } }, Cmd.none
    | _ ->
        let newInvaders = 
            model.invaders.rows
            |> Array.map (fun row ->
                { row with 
                    xs = row.xs |> Array.map (fun (x, state) -> 
                        if state = Invaders.Dying then x, Invaders.Dead 
                        else x, state) })
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
    | CheckCollisions -> checkCollisions model
    | InvaderHit (row, index) -> 
        model.soundQueue.Enqueue "explosion-small"
        { model with score = model.score + model.invaders.rows.[row].kind.score },
        Cmd.ofMsg (InvadersMessage (Invaders.Destroy (row, index)))
    | PlayerHit -> { model with player = { model.player with state = Player.Dying playerTimeToDie } }, Cmd.none
    | Restart -> init model.highScore
    | _ -> failwith "unhandled combination" // these messages (victory/gameover) should be caught by parent

let text = text "PressStart2P" 24.

let view model dispatch =
    [
        yield playQueuedSound model.soundQueue

        yield text Colour.White (0., 0.) "SCORE" (10, 10)
        yield text Colour.White (0., 0.) (sprintf "%04i" model.score) (10, 44)
        
        yield text Colour.Cyan (0., 0.)  "HIGH  SCORE" (150, 10)
        yield text Colour.White (0., 0.) (sprintf "%04i" model.highScore) (150, 44)

        yield text Colour.White (-1., 0.) "LIVES" (resWidth - 10, 10)
        yield text Colour.White (-1., 0.) (sprintf "%02i" model.lives) (resWidth - 10, 44)

        yield! Player.view model.player (PlayerMessage >> dispatch)
        yield! Invaders.view model.invaders (InvadersMessage >> dispatch) (model.player.state <> Player.Alive)

        yield! model.bunkers
            |> List.map (fun r -> 
                colour bunkerColour (r.Width, r.Height) (r.Left, r.Top))

        if model.player.state = Player.Alive then
            yield onupdate (fun _ -> dispatch CheckCollisions)

        yield colour Colour.OrangeRed (earthLine.Width, earthLine.Height) (earthLine.Left, earthLine.Top)

        yield onupdate (fun inputs -> 
            if inputs.totalGameTime - model.lastTick > dyingTickInterval then
                dispatch (UpdateDying inputs.totalGameTime))

        yield onkeydown Keys.Escape exit
        yield onkeydown Keys.R (fun () -> dispatch Restart)
    ]