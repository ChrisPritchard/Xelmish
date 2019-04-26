open Elmish
open Xelmish.Model
open Xelmish.Viewables
open Config

type PlayingModel = {
    playerX: int
    invaders: (InvaderKind * int * int []) []
    invaderDirection: ShuffleState
    bunkers: (int * int) list
    projectiles: (int * int * int) list
    lastShuffle: int64
    shuffleInterval: int64
    shuffleMod: int
    freeze: bool
} 
and ShuffleState = Across of row:int * dir:int | Down of row:int * nextDir:int

let init () = 
    {
        playerX = resWidth / 2 - (playerWidth / 2)
        invaders = 
            [|0..invaderRows-1|]
            |> Array.map (fun row ->
                let kind = match row with 0 -> smallSize | 1 | 2 -> mediumSize | _ -> largeSize
                let y = padding + row * (kind.height + invaderSpacing)
                let xs = 
                    [|0..invadersPerRow-1|] 
                    |> Array.map (fun col -> 
                        padding + col * (largeSize.width + invaderSpacing) + kind.offset)
                kind, y, xs)
        invaderDirection = Across (invaderRows - 1, 1)
        bunkers = []
        projectiles = []
        lastShuffle = 0L
        shuffleInterval = 500L
        shuffleMod = 0
        freeze = false
    }, Cmd.none

type Message = 
    | MovePlayer of dir: int
    | FireProjectile of x: int * y: int * velocity: int
    | ShuffleInvaders of int64
    | MoveProjectiles
    | PlayerHit
    | Victory

let rec shuffleInvaders time model = 
    let model = { model with shuffleMod = (model.shuffleMod + 1) % 2 }
    
    let (newInvaders, newDirection) = 
        match model.invaderDirection with
        | Across (row, dir) ->
            let newInvaders = 
                model.invaders 
                |> Array.mapi (fun i (kind, y, xs) -> 
                    if i <> row then (kind, y, xs) 
                    else
                        kind, y, xs |> Array.map (fun x -> x + (invaderShuffleAmount * dir)))
            let _, _, xs = newInvaders.[row]
            if Array.exists (fun x -> x < padding || x + largeSize.width > (resWidth - padding)) xs 
            then model.invaders, Down (model.invaders.Length - 1, dir * -1)
            else newInvaders, Across ((if row = 0 then newInvaders.Length - 1 else row - 1), dir)
        | Down (row, nextDir) ->
            let newInvaders = 
                model.invaders 
                |> Array.mapi (fun i (kind, y, xs) -> 
                    if i <> row then (kind, y, xs) 
                    else
                        kind, y + invaderShuffleAmount, xs)
            let nextDirection = 
                if row = 0 then Across (newInvaders.Length - 1, nextDir) 
                else Down (row - 1, nextDir)
            newInvaders, nextDirection

    match model.invaderDirection, newDirection with
    | Across _, Down _ -> shuffleInvaders time { model with invaderDirection = newDirection }
    | _ ->
        let command = 
            let playerRect = rect model.playerX playerY playerWidth playerHeight
            let playerHit =
                newInvaders 
                |> Seq.collect (fun (kind, y, xs) -> 
                    xs |> Seq.map (fun x -> rect x y kind.width kind.height))
                |> Seq.exists (fun (rect: Rectangle) -> rect.Intersects playerRect)
            if playerHit then Cmd.ofMsg PlayerHit else Cmd.none

        { model with 
            invaders = newInvaders
            invaderDirection = newDirection
            lastShuffle = time }, command

//let moveProjectiles model =
//    let playerProjectile (acc, playerHit, invadersHit) (x, y, v) =
//        let newY = y + v
//        if newY < 0 then acc, false, invadersHit
//        else
//            let projectileRect = rect x y 1 projectileHeight
//            let hitInvaders = 
//                model.invaders 
//                |> List.filter (fun (ix, iy, iw, ih, _) -> 
//                    projectileRect.Intersects(rect ix iy iw ih))
//            if hitInvaders <> [] then
//                acc, playerHit, hitInvaders @ invadersHit
//            else
//                (x, newY, v)::acc, playerHit, invadersHit

//    let invaderProjectile (acc, playerHit, invadersHit) (x, y, v) =
//        let newY = y + v
//        if newY > resHeight then acc, false, invadersHit
//        else
//            let overlapsPlayer = 
//                x >= model.playerX && x < model.playerX + playerWidth
//                && newY >= playerY
//            if overlapsPlayer then acc, true, invadersHit
//            else (x, newY, v)::acc, playerHit, invadersHit

//    let newProjectiles, playerHit, invadersHit =
//        (([], false, []), model.projectiles)
//        ||> List.fold (fun (acc, playerHit, invadersHit) (x, y, v) ->
//            if v > 0 then invaderProjectile (acc, playerHit, invadersHit) (x, y, v)
//            else playerProjectile (acc, playerHit, invadersHit) (x, y, v))
            
//    let newInvaders = List.except invadersHit model.invaders
//    let command = 
//        if playerHit then Cmd.ofMsg PlayerHit 
//        elif newInvaders = [] then Cmd.ofMsg Victory 
//        else Cmd.none
//    { model with projectiles = newProjectiles; invaders = newInvaders }, command

let update message model =
    match message with
    | MovePlayer dir ->
        let newPos = min (resWidth - padding - playerWidth) (max padding (model.playerX + dir * playerSpeed))
        { model with playerX = newPos }, Cmd.none
    | FireProjectile (x, y, v) ->
        { model with projectiles = (x, y, v)::model.projectiles }, Cmd.none
    | ShuffleInvaders time -> shuffleInvaders time model        
    //| MoveProjectiles -> moveProjectiles model
    | PlayerHit -> { model with freeze = true }, Cmd.none
    | Victory -> { model with freeze = true }, Cmd.none
    
let sprite (sw, sh, sx, sy) (w, h) (x, y) colour =
    fun loadedAssets _ (spriteBatch: SpriteBatch) ->
        let texture = loadedAssets.textures.["sprites"]
        spriteBatch.Draw (texture, rect x y w h, System.Nullable(rect sx sy sw sh), colour)

let view model dispatch =
    [
        yield! model.invaders 
            |> Array.collect (fun (kind, y, xs) ->
                let spriteRect = kind.animations.[model.shuffleMod]
                xs |> Array.map (fun x -> sprite spriteRect (kind.width, kind.height) (x, y) kind.colour))

        yield sprite spritemap.["player"] (playerWidth, playerHeight) (model.playerX, playerY) Colour.White

        yield! model.projectiles
            |> List.map (fun (x, y, _) ->
                colour Colour.White (1, projectileHeight) (x, y))

        if not model.freeze then
            yield fun _ inputs _ -> 
                if inputs.totalGameTime - model.lastShuffle > model.shuffleInterval then
                    dispatch (ShuffleInvaders inputs.totalGameTime)

            //yield fun _ _ _ -> dispatch MoveProjectiles

            yield whilekeydown Keys.Left (fun () -> dispatch (MovePlayer -1))
            yield whilekeydown Keys.A (fun () -> dispatch (MovePlayer -1))
            yield whilekeydown Keys.Right (fun () -> dispatch (MovePlayer 1))
            yield whilekeydown Keys.D (fun () -> dispatch (MovePlayer 1))

        yield onkeydown Keys.Space (fun () -> 
            if not (List.exists (fun (_, _, v) -> v < 0) model.projectiles) then
                let x = model.playerX + playerWidth / 2
                let y = resHeight - (playerHeight + padding) - projectileHeight - 1
                dispatch (FireProjectile (x, y, -projectileSpeed)))

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