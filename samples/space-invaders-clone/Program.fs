open Elmish
open Xelmish.Model
open Xelmish.Viewables
open Config

type PlayingModel = {
    playerX: int
    playerProjectile: Projectile option
    invaders: Row []
    invaderDirection: ShuffleState
    invaderProjectiles: Projectile list
    lastShuffle: int64
    shuffleInterval: int64
    shuffleMod: int
    explosions: Explosion list
    freeze: bool
    score: int
} 
and Row = { kind: InvaderKind; y: int; xs: int [] }
and ShuffleState = Across of row:int * dir:int | Down of row:int * nextDir:int
and Projectile = { x: int; y: int }
and Explosion = { x: int; y: int; life: int }

let invaderImpact x y w h model =
    let testRect = rect x y w h
    model.invaders 
    |> Seq.indexed 
    |> Seq.collect (fun (r, row) -> 
        row.xs 
        |> Seq.indexed 
        |> Seq.map (fun (c, x) -> (r, c), rect x row.y row.kind.width row.kind.height))
    |> Seq.tryFind (fun (_, rect) -> rect.Intersects testRect)
    |> Option.map fst

let init () = 
    {
        playerX = resWidth / 2 - (playerWidth / 2)
        playerProjectile = None
        invaders = 
            [|0..invaderRows-1|]
            |> Array.map (fun row -> 
                let kind = match row with 0 -> smallSize | 1 | 2 -> mediumSize | _ -> largeSize
                {
                    kind = kind
                    y = invaderTop + row * (kind.height + invaderSpacing)
                    xs = 
                        [|0..invadersPerRow-1|] 
                        |> Array.map (fun col -> 
                            padding + col * (largeSize.width + invaderSpacing) + kind.offset) 
                })
        invaderDirection = Across (invaderRows - 1, 1)
        invaderProjectiles = []
        lastShuffle = 0L
        shuffleInterval = 500L
        shuffleMod = 0
        explosions = []
        freeze = false
        score = 0
    }, Cmd.none

type Message = 
    | MovePlayer of dir: int
    | PlayerShoot
    | InvaderShoot of x: int * y: int
    | ShuffleInvaders of int64
    | MoveProjectiles
    | InvaderHit of row: int * index: int
    | PlayerHit
    | Victory

let rec shuffleInvaders time model = 
    // the shuffle mod is used for animations
    let model = { model with shuffleMod = (model.shuffleMod + 1) % 2 }
    
    let (newInvaders, newDirection) = 
        match model.invaderDirection with
        | Across (targetRow, dir) ->
            let newInvaders = 
                model.invaders 
                |> Array.mapi (fun i row -> 
                    if i <> targetRow then row
                    else
                        { row with xs = row.xs |> Array.map (fun x -> x + (invaderShuffleAmount * dir)) })
            // if the new shuffle has resulted in out of bounds, then use the old shuffle and start down
            if newInvaders.[targetRow].xs |> Array.exists (fun x -> x < padding || x + largeSize.width > (resWidth - padding))
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
        // update explosions
        let newExplosions = 
            model.explosions 
            |> List.map (fun explosion -> 
                { explosion with life = explosion.life - 1 })
            |> List.filter (fun explosion -> explosion.life > 0)

        // check to see if, as a result of this shuffle, the player has been touched.
        let command = 
            let playerHit = invaderImpact model.playerX playerY playerWidth playerHeight model
            if playerHit <> None then Cmd.ofMsg PlayerHit else Cmd.none

        { model with 
            invaders = newInvaders
            invaderDirection = newDirection
            explosions = newExplosions
            lastShuffle = time }, command

let moveProjectiles model =
    let nextPlayerProjectile, cmdResult =
        match model.playerProjectile with
        | None -> None, Cmd.none
        | Some p ->
            let next = { p with y = p.y - projectileSpeed }
            if next.y < 0 then None, Cmd.none
            else
                match invaderImpact p.x p.y 1 projectileHeight model with
                | Some invaderIndex -> None, Cmd.ofMsg (InvaderHit invaderIndex)
                | None -> Some next, Cmd.none

    let nextInvaderProjectiles, cmdResult =
        (([], cmdResult), model.invaderProjectiles)
        ||> List.fold (fun (acc, cmdResult) p ->
            let next = { p with y = p.y + projectileSpeed }
            if next.y > resHeight then acc, cmdResult
            else
                if 
                    p.x >= model.playerX && p.x < model.playerX + playerWidth
                    && p.y >= playerY && p.y < playerY + playerHeight then
                        acc, Cmd.batch [cmdResult; Cmd.ofMsg PlayerHit]
                else
                    next::acc, cmdResult)

    { model with 
        playerProjectile = nextPlayerProjectile 
        invaderProjectiles = nextInvaderProjectiles }, cmdResult

let destroyInvader targetRow index model =
    let xOffset = model.invaders.[targetRow].kind.width / 2 - explosionWidth / 2
    let newExplosion = 
        { x = model.invaders.[targetRow].xs.[index] + xOffset
          y = model.invaders.[targetRow].y
          life = explosionDuration }

    let newInvaders =
        if model.invaders.[targetRow].xs.Length = 1 then 
            Array.append model.invaders.[0..targetRow - 1] model.invaders.[targetRow + 1..]
        else
            model.invaders 
            |> Array.mapi (fun i row ->
                if i <> targetRow then row
                else
                    let newXs = Array.append row.xs.[0..index - 1] row.xs.[index + 1..]
                    { row with xs = newXs })

    // if an invader was the last of the last row, and the current shuffle index is that row,
    // then update the shuffle index to avoid a out of range exception
    let newInvaderDirection =
        match model.invaderDirection with
        | Across (n, dir) when n = newInvaders.Length -> Across (n - 1, dir)
        | Down (n, nextDir) when n = newInvaders.Length -> Down (n - 1, nextDir)
        | other -> other

    let scoreIncrease = model.invaders.[targetRow].kind.score

    { model with
        invaders = newInvaders
        invaderDirection = newInvaderDirection
        explosions = newExplosion::model.explosions
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
    | InvaderShoot (x, y) ->
        let newProjectiles = { x = x; y = y }::model.invaderProjectiles
        { model with invaderProjectiles = newProjectiles }, Cmd.none
    | ShuffleInvaders time -> shuffleInvaders time model        
    | MoveProjectiles -> moveProjectiles model
    | InvaderHit (row, index) -> destroyInvader row index model
    | PlayerHit -> { model with freeze = true }, Cmd.none
    | Victory -> { model with freeze = true }, Cmd.none
    
let sprite (sw, sh, sx, sy) (w, h) (x, y) colour =
    fun loadedAssets _ (spriteBatch: SpriteBatch) ->
        let texture = loadedAssets.textures.["sprites"]
        spriteBatch.Draw (texture, rect x y w h, System.Nullable(rect sx sy sw sh), colour)

let text = text "connection" 24. Colour.White (0., 0.)

let view model dispatch =
    [
        yield text "SCORE" (10, 10)
        yield text (sprintf "%04i" model.score) (10, 44)

        yield! model.invaders 
            |> Array.collect (fun row ->
                let spriteRect = row.kind.animations.[model.shuffleMod]
                row.xs 
                |> Array.map (fun x -> 
                    sprite spriteRect 
                        (row.kind.width, row.kind.height) 
                        (x, row.y) row.kind.colour))

        yield! model.explosions
            |> List.map (fun explosion ->
                sprite spritemap.["invader-death"] 
                    (explosionWidth, explosionHeight) 
                    (explosion.x, explosion.y) Colour.White)

        yield sprite spritemap.["player"] (playerWidth, playerHeight) (model.playerX, playerY) Colour.White

        yield! model.invaderProjectiles
            |> List.map (fun projectile ->
                colour Colour.White (1, projectileHeight) (projectile.x, projectile.y))

        match model.playerProjectile with
            | Some p -> 
                yield colour Colour.White (1, projectileHeight) (p.x, p.y)
            | _ -> 
                yield onkeydown Keys.Space (fun () -> dispatch PlayerShoot)

        if not model.freeze then
            yield fun _ inputs _ -> 
                if not (Array.isEmpty model.invaders) && inputs.totalGameTime - model.lastShuffle > model.shuffleInterval then
                    dispatch (ShuffleInvaders inputs.totalGameTime)

            yield fun _ _ _ -> dispatch MoveProjectiles

            yield whilekeydown Keys.Left (fun () -> dispatch (MovePlayer -1))
            yield whilekeydown Keys.A (fun () -> dispatch (MovePlayer -1))
            yield whilekeydown Keys.Right (fun () -> dispatch (MovePlayer 1))
            yield whilekeydown Keys.D (fun () -> dispatch (MovePlayer 1))

        yield onkeydown Keys.Escape exit
    ]

[<EntryPoint>]
let main _ =
    let config: GameConfig = {
        clearColour = Some Colour.Black
        resolution = Windowed (resWidth, resHeight)
        assetsToLoad = [ 
            Texture ("sprites", "./sprites.png")
            Font ("connection", "./connection") ]
        mouseVisible = false
        showFpsInConsole = true
    }

    Program.mkProgram init update view
    |> Xelmish.Program.runGameLoop config

    0