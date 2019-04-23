open Elmish
open Xelmish.Model
open Xelmish.Viewables

let resWidth = 800
let resHeight = 600
let padding = 30
let playerDim = 40
let playerY = resHeight - (playerDim + padding)
let playerSpeed = 5
let invaderDim = 40
let invaderSpacing = 20
let invaderShuffleAmount = 20
let projectileHeight = 10
let projectileSpeed = 6

type Model = {
    playerX: int
    invaders: (int * int) list
    invaderDirection: int
    bunkers: (int * int) list
    projectiles: (int * int * int) list
    lastShuffle: int64
    shuffleInterval: int64
    freeze: bool
}

let init () = 
    {
        playerX = resWidth / 2 - (playerDim / 2)
        invaders = 
            [0..8*5-1] 
            |> List.map (fun i ->
                let y = padding + (i / 8) * (invaderDim + invaderSpacing)
                let x = padding + (i % 8) * (invaderDim + invaderSpacing)
                x, y)
        invaderDirection = 1
        bunkers = []
        projectiles = []
        lastShuffle = 0L
        shuffleInterval = 500L
        freeze = false
    }, Cmd.none

type Message = 
    | MovePlayer of dir: int
    | FireProjectile of x: int * y: int * velocity: int
    | ShuffleInvaders of int64
    | MoveProjectiles
    | PlayerHit
    | Victory

let shuffleInvaders time model = 
    let (newInvaders, valid) = 
        (([], true), model.invaders)
        ||> List.fold (fun (acc, valid) (x, y) ->
            if not valid then (acc, valid)
            else
                let nx = x + invaderShuffleAmount * model.invaderDirection
                if nx < padding || nx + invaderDim > (resWidth - padding) then acc, false
                else (nx, y)::acc, true)
    if not valid then
        { model with 
            invaders = model.invaders |> List.map (fun (x, y) -> x, y + invaderDim / 2)
            invaderDirection = model.invaderDirection * -1
            lastShuffle = time
            shuffleInterval = max 50L (model.shuffleInterval - 50L) }, 
        Cmd.none
    else
        let command = 
            let playerRect = rect model.playerX playerY playerDim playerDim
            if List.exists (fun (x, y) -> (rect x y invaderDim invaderDim).Intersects(playerRect)) model.invaders 
            then Cmd.ofMsg PlayerHit else Cmd.none
        { model with invaders = newInvaders; lastShuffle = time }, command

let moveProjectiles model =
    let newProjectiles, playerHit, invadersHit =
        (([], false, []), model.projectiles)
        ||> List.fold (fun (acc, playerHit, invadersHit) (x, y, v) ->
            let newY = y + v
            if newY > resHeight || newY < -projectileHeight then acc, playerHit, invadersHit // out of bounds
            elif v > 0 then // invader projectile
                let overlapsPlayer = 
                    x >= model.playerX && x < model.playerX + playerDim
                    && newY >= playerY
                if overlapsPlayer then acc, true, invadersHit
                else (x, newY, v)::acc, playerHit, invadersHit
            else // player projectile
                let projectileRect = rect x y 1 projectileHeight
                let hitInvaders = 
                    model.invaders 
                    |> List.filter (fun (ix, iy) -> 
                        projectileRect.Intersects(rect ix iy invaderDim invaderDim))
                if hitInvaders <> [] then
                    acc, playerHit, hitInvaders @ invadersHit
                else
                    (x, newY, v)::acc, playerHit, invadersHit)
    let newInvaders = List.except invadersHit model.invaders
    let command = 
        if playerHit then Cmd.ofMsg PlayerHit 
        elif newInvaders = [] then Cmd.ofMsg Victory 
        else Cmd.none
    { model with projectiles = newProjectiles; invaders = newInvaders }, command

let update message model =
    match message with
    | MovePlayer dir ->
        let newPos = min (resWidth - padding - playerDim) (max padding (model.playerX + dir * playerSpeed))
        { model with playerX = newPos }, Cmd.none
    | FireProjectile (x, y, v) ->
        { model with projectiles = (x, y, v)::model.projectiles }, Cmd.none
    | ShuffleInvaders time -> shuffleInvaders time model        
    | MoveProjectiles -> moveProjectiles model
    | PlayerHit -> { model with freeze = true }, Cmd.none
    | Victory -> { model with freeze = true }, Cmd.none

let view model dispatch =
    [
        yield! model.invaders 
            |> List.map (fun invaderPos ->
                colour Colour.Green (invaderDim, invaderDim) invaderPos)

        yield colour Colour.Red (playerDim, playerDim) (model.playerX, playerY)

        yield! model.projectiles
            |> List.map (fun (x, y, _) ->
                colour Colour.White (1, projectileHeight) (x, y))

        if not model.freeze then
            yield fun _ inputs _ -> 
                if inputs.totalGameTime - model.lastShuffle > model.shuffleInterval then
                    dispatch (ShuffleInvaders inputs.totalGameTime)

            yield fun _ _ _ -> dispatch MoveProjectiles

            yield whilekeydown Keys.Left (fun () -> dispatch (MovePlayer -1))
            yield whilekeydown Keys.Right (fun () -> dispatch (MovePlayer 1))

        yield onkeydown Keys.Space (fun () -> 
            if not (List.exists (fun (_, _, v) -> v < 0) model.projectiles) then
                let x = model.playerX + playerDim / 2
                let y = resHeight - (playerDim + padding) - projectileHeight - 1
                dispatch (FireProjectile (x, y, -projectileSpeed)))

        yield onkeydown Keys.Escape exit
    ]

[<EntryPoint>]
let main _ =
    let config: GameConfig = {
        clearColour = Some Colour.Black
        resolution = Windowed (resWidth, resHeight)
        assetsToLoad = []
        mouseVisible = false
        showFpsInConsole = true
    }

    Program.mkProgram init update view
    |> Xelmish.Program.runGameLoop config

    0