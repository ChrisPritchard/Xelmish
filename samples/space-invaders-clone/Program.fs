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
    freeze: bool
} 
and Row = { kind: InvaderKind; y: int; xs: int [] }
and ShuffleState = Across of row:int * dir:int | Down of row:int * nextDir:int
and Projectile = { x: int; y: int }

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
                    y = padding + row * (kind.height + invaderSpacing)
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
        freeze = false
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
        // check to see if, as a result of this shuffle, the player has been touched.
        let command = 
            let playerRect = rect model.playerX playerY playerWidth playerHeight
            let playerHit =
                newInvaders 
                |> Seq.collect (fun row -> 
                    row.xs |> Seq.map (fun x -> rect x row.y row.kind.width row.kind.height))
                |> Seq.exists (fun (rect: Rectangle) -> rect.Intersects playerRect)
            if playerHit then Cmd.ofMsg PlayerHit else Cmd.none

        { model with 
            invaders = newInvaders
            invaderDirection = newDirection
            lastShuffle = time }, command

let moveProjectiles model =
    model, Cmd.none
//    let playerProjectile (acc, playerHit, invadersHit) (projectile: Projectile) =
//        let next = { projectile with y = projectile.y + projectile.velocity }
//        if next.y < 0 then acc, false, invadersHit
//        else
//            //let projectileRect = rect next.x next.y 1 projectileHeight
//            //let hitInvaders = 
//            //    model.invaders 
//            //    |> List.filter (fun (ix, iy, iw, ih, _) -> 
//            //        projectileRect.Intersects(rect ix iy iw ih))
//            //if hitInvaders <> [] then
//            //    acc, playerHit, hitInvaders @ invadersHit
//            //else
//            next::acc, playerHit, invadersHit

//    let invaderProjectile (acc, playerHit, invadersHit) (projectile: Projectile) =
//        let next = { projectile with y = projectile.y + projectile.velocity }
//        if next.y > resHeight then acc, playerHit, invadersHit
//        else
//            let overlapsPlayer = 
//                projectile.x >= model.playerX && projectile.x < model.playerX + playerWidth
//                && next.y >= playerY
//            if overlapsPlayer then acc, true, invadersHit
//            else next::acc, playerHit, invadersHit

//    let newProjectiles, playerHit, invadersHit =
//        (([], false, []), model.projectiles)
//        ||> List.fold (fun (acc, playerHit, invadersHit) projectile ->
//            if projectile.velocity > 0 then 
//                invaderProjectile (acc, playerHit, invadersHit) projectile
//            else 
//                playerProjectile (acc, playerHit, invadersHit) projectile)
            
//    //let newInvaders = List.except invadersHit model.invaders
//    let command = 
//        if playerHit then Cmd.ofMsg PlayerHit 
//        //elif newInvaders = [] then Cmd.ofMsg Victory 
//        else Cmd.none
//    //{ model with projectiles = newProjectiles; invaders = newInvaders }, command
//    { model with projectiles = newProjectiles }, command

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
    | InvaderHit (row, index) -> model, Cmd.none // TODO
    | PlayerHit -> { model with freeze = true }, Cmd.none
    | Victory -> { model with freeze = true }, Cmd.none
    
let sprite (sw, sh, sx, sy) (w, h) (x, y) colour =
    fun loadedAssets _ (spriteBatch: SpriteBatch) ->
        let texture = loadedAssets.textures.["sprites"]
        spriteBatch.Draw (texture, rect x y w h, System.Nullable(rect sx sy sw sh), colour)

let view model dispatch =
    [
        yield! model.invaders 
            |> Array.collect (fun row ->
                let spriteRect = row.kind.animations.[model.shuffleMod]
                row.xs |> Array.map (fun x -> sprite spriteRect (row.kind.width, row.kind.height) (x, row.y) row.kind.colour))

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
                if inputs.totalGameTime - model.lastShuffle > model.shuffleInterval then
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
        assetsToLoad = [ Texture ("sprites", "./sprites.png") ]
        mouseVisible = false
        showFpsInConsole = true
    }

    Program.mkProgram init update view
    |> Xelmish.Program.runGameLoop config

    0