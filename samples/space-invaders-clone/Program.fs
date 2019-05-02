open Elmish
open Xelmish.Model
open Xelmish.Viewables
open Config

let random = System.Random()
let check chance =
    random.NextDouble () <= chance
let pick from =
    random.Next (0, from)

type PlayingModel = {
    playerX: int
    playerProjectile: Projectile option
    bunkers: Rectangle list
    invaders: Row []
    invaderDirection: ShuffleState
    invaderProjectiles: Projectile list
    lastShuffle: int64
    shuffleInterval: int64
    shuffleMod: int
    freeze: bool
    score: int
} 
and Row = { kind: InvaderKind; y: int; xs: (int * InvaderState) [] }
and InvaderState = Alive | Dying | Dead
and ShuffleState = Across of row:int * dir:int | Down of row:int * nextDir:int
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
        playerX = resWidth / 2 - (playerWidth / 2)
        playerProjectile = None
        bunkers = defaultBunkers
        invaders = 
            [|0..invaderRows-1|]
            |> Array.map (fun row -> 
                let kind = match row with 0 -> smallSize | 1 | 2 -> mediumSize | _ -> largeSize
                {
                    kind = kind
                    y = invaderTop + row * (kind.height + invaderSpacing)
                    xs = [|0..invadersPerRow-1|] |> Array.map (fun col -> 
                        padding + col * (largeSize.width + invaderSpacing) + kind.offset, Alive) 
                })
        invaderDirection = Across (invaderRows - 1, 1)
        invaderProjectiles = []
        lastShuffle = 0L
        shuffleInterval = 500L
        shuffleMod = 0
        freeze = false
        score = 0
    }, Cmd.none

type Message = 
    | MovePlayer of dir: int
    | PlayerShoot
    | InvaderShoot
    | ShuffleInvaders of int64
    | MoveProjectiles
    | InvaderHit of row: int * index: int
    | PlayerHit
    | Victory
    | Restart

let playerRect model = rect model.playerX playerY playerWidth playerHeight

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

let rec shuffleInvaders time model = 
    // the shuffle mod is used for animations
    let model = { model with shuffleMod = (model.shuffleMod + 1) % 2 }
    
    let (newInvaders, newDirection) = 
        match model.invaderDirection with
        | Across (targetRow, dir) ->
            let newInvaders = model.invaders |> Array.mapi (fun i row -> 
                if i <> targetRow then row
                else
                    let shuffled = row.xs |> Array.map (fun (x, state) -> 
                        match state with
                            | Alive -> x + (invaderShuffleAmount * dir), Alive
                            | _ -> (x, state)) // the dying and dead don't shuffle
                    { row with xs = shuffled })

            // if the new shuffle has resulted in out of bounds, then use the old shuffle and start down
            if newInvaders.[targetRow].xs |> Array.exists (fun (x, state) -> state = Alive && x < padding || x + largeSize.width > (resWidth - padding))
            then model.invaders, Down (model.invaders.Length - 1, dir * -1)
            else newInvaders, Across ((if targetRow = 0 then newInvaders.Length - 1 else targetRow - 1), dir)

        | Down (targetRow, nextDir) ->
            let newInvaders = 
                model.invaders 
                |> Array.mapi (fun i row -> 
                    if i <> targetRow then row
                    else
                        { row with y = row.y + invaderShuffleAmount })
            let nextDirection = 
                if targetRow = 0 then Across (newInvaders.Length - 1, nextDir) 
                else Down (targetRow - 1, nextDir)
            newInvaders, nextDirection

    match model.invaderDirection, newDirection with
    | Across _, Down _ -> 
        // immediately do another shuffle, to eliminate the pause between going from across to down.
        shuffleInvaders time { model with invaderDirection = newDirection }
    | _ ->
        // update dying
        let newInvaders = 
            newInvaders 
            |> Array.map (fun row ->
                { row with xs = row.xs |> Array.map (fun (x, state) -> if state = Dying then x, Dead else x, state) })
        // check to see if, as a result of this shuffle, the player has been touched.
        let command = 
            let playerHit = invaderImpact model.playerX playerY playerWidth playerHeight model
            if playerHit <> None then Cmd.ofMsg PlayerHit else Cmd.none
        { model with 
            invaders = newInvaders
            invaderDirection = newDirection
            lastShuffle = time }, command

let shootFromInvader model = 
    // only the invaders with a clear shot can shoot
    // start at the bottom of each column and look up until a live one is found
    let possibleShooters = 
        [|0..invadersPerRow-1|]
        |> Array.choose (fun col ->
            [invaderRows-1..-1..0]
            |> List.tryFind (fun row -> 
                let row = model.invaders.[row]
                row.y + row.kind.height < playerY && snd row.xs.[col] = Alive)
            |> Option.map (fun row -> 
                let row = model.invaders.[row]
                fst row.xs.[col] + row.kind.width / 2, row.y + row.kind.height))
    // pick a random shooter
    let x, y = possibleShooters.[pick possibleShooters.Length]
    let newProjectiles = { x = x; y = y }::model.invaderProjectiles
    { model with invaderProjectiles = newProjectiles }, Cmd.none

let moveProjectiles model =
    let nextPlayerProjectile, cmdResult =
        match model.playerProjectile with
        | None -> None, Cmd.none
        | Some p ->
            let next = { p with y = p.y - playerProjectileSpeed }
            if next.y < 0 then None, Cmd.none
            else
                match invaderImpact p.x p.y 1 projectileHeight model with
                | Some invaderIndex -> None, Cmd.ofMsg (InvaderHit invaderIndex)
                | None -> Some next, Cmd.none

    let playerRect = playerRect model

    let nextInvaderProjectiles, cmdResult, newBunkers =
        (([], cmdResult, model.bunkers), model.invaderProjectiles)
        ||> List.fold (fun (acc, cmdResult, bunkers) p ->
            let next = { p with y = p.y + invaderProjectileSpeed }
            if next.y > resHeight then acc, cmdResult, bunkers
            else
                let shotRect = rect p.x p.y projectileWidth projectileHeight
                if shotRect.Intersects playerRect then
                    acc, Cmd.batch [cmdResult; Cmd.ofMsg PlayerHit], bunkers
                else
                    let destroyed, newBunkers = bunkers |> List.partition (fun b -> b.Intersects shotRect)
                    if List.isEmpty destroyed then
                        next::acc, cmdResult, bunkers
                    else
                        acc, cmdResult, newBunkers)

    { model with 
        playerProjectile = nextPlayerProjectile 
        bunkers = newBunkers
        invaderProjectiles = nextInvaderProjectiles }, cmdResult

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
    | MovePlayer dir ->
        let newPos = min (resWidth - padding - playerWidth) (max padding (model.playerX + dir * playerSpeed))
        { model with playerX = newPos }, Cmd.none
    | PlayerShoot ->
        let newProjectile = 
            {   x = model.playerX + playerWidth / 2
                y = resHeight - (playerHeight + padding) - projectileHeight - 1 }
        { model with playerProjectile = Some newProjectile }, Cmd.none
    | InvaderShoot -> shootFromInvader model
    | ShuffleInvaders time -> shuffleInvaders time model        
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

        yield! model.invaders 
            |> Array.collect (fun row ->
                let spriteRect = row.kind.animations.[model.shuffleMod]
                row.xs 
                |> Array.filter (fun (_, state) -> state <> Dead)
                |> Array.map (fun (x, state) -> 
                    match state with
                    | Alive ->
                        sprite spriteRect 
                            (row.kind.width, row.kind.height) 
                            (x, row.y) row.kind.colour
                    | _ -> 
                        sprite spritemap.["invader-death"] 
                            (explosionWidth, explosionHeight) 
                            (x, row.y) Colour.White))

        yield! model.bunkers
            |> List.map (fun r -> 
                colour bunkerColour (r.Width, r.Height) (r.Left, r.Top))

        yield sprite spritemap.["player"] (playerWidth, playerHeight) (model.playerX, playerY) Colour.White

        yield! model.invaderProjectiles
            |> List.map (fun projectile ->
                colour Colour.White (projectileWidth, projectileHeight) (projectile.x, projectile.y))

        if not model.freeze then
            yield onupdate (fun inputs -> 
                if not (Array.isEmpty model.invaders) && inputs.totalGameTime - model.lastShuffle > model.shuffleInterval then
                    dispatch (ShuffleInvaders inputs.totalGameTime))
            
            match model.playerProjectile with
                | Some p -> 
                    yield colour Colour.White (projectileWidth, projectileHeight) (p.x, p.y)
                | _ -> 
                    yield onkeydown Keys.Space (fun () -> dispatch PlayerShoot)

            yield onupdate (fun _ -> 
                if not (Array.isEmpty model.invaders)
                    && List.length model.invaderProjectiles < maxInvaderProjectiles
                    && check invaderShootChance then
                        dispatch InvaderShoot)

            yield onupdate (fun _ -> dispatch MoveProjectiles)

            yield whilekeydown Keys.Left (fun () -> dispatch (MovePlayer -1))
            yield whilekeydown Keys.A (fun () -> dispatch (MovePlayer -1))
            yield whilekeydown Keys.Right (fun () -> dispatch (MovePlayer 1))
            yield whilekeydown Keys.D (fun () -> dispatch (MovePlayer 1))

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