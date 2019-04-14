open Elmish
open Xelmish.Model

let resWidth = 800
let resHeight = 600
let playerSpeed = 5
let minX = 30
let maxX = resWidth - 30
let invaderDim = 40
let invaderSpacing = 20
let invaderSpeed = 3
let playerDim = 40
let projectileHeight = 10
let projectileSpeed = 10

type Model = {
    playerX: int
    invaders: (int * int) list
    invaderSpeed: int
    bunkers: (int * int) list
    projectiles: (int * int * int) list
}

let init () = {
    playerX = resWidth / 2 - (playerDim / 2)
    invaders = 
        [0..8*5-1] 
        |> List.map (fun i ->
            let y = (i / 8) * (invaderDim + invaderSpacing)
            let x = (i % 8) * (invaderDim + invaderSpacing)
            x, y)
    invaderSpeed = invaderSpeed
    bunkers = []
    projectiles = []
}

type Message = 
    | MovePlayer of dir: int
    | FireProjectile of x: int * y: int * velocity: int
    | ShuffleInvaders
    | MoveProjectiles

let update message model =
    match message with
    | MovePlayer dir ->
        let newPos = min maxX (max minX (model.playerX + dir * playerSpeed))
        { model with playerX = newPos }, Cmd.none, NoOp
    | FireProjectile (x, y, v) ->
        { model with projectiles = (x, y, v)::model.projectiles }, Cmd.none, NoOp
    | ShuffleInvaders ->
        let (newInvaders, valid) = 
            (([], true), model.invaders)
            ||> List.fold (fun (acc, valid) (x, y) ->
                if not valid then (acc, valid)
                else
                    let nx = x + model.invaderSpeed
                    if nx < minX || nx + invaderDim > maxX then acc, false
                    else (nx, y)::acc, true)
        if not valid then
            // drop invaders
            // check for player impact
            { model with invaderSpeed = model.invaderSpeed * -1 }, Cmd.none, NoOp
        else
            { model with invaders = newInvaders }, Cmd.none, NoOp
    | MoveProjectiles ->
        let newProjectiles =
            ([], model.projectiles)
            ||> List.fold (fun acc (x, y, v) ->
                let newY = y + v
                if newY > resHeight || newY < -projectileHeight then acc
                else (x, newY, v)::acc)
        // check for player inpact
        // check for invader inpact
        { model with projectiles = newProjectiles }, Cmd.none, NoOp

[<EntryPoint>]
let main argv =
    printfn "Hello World from F#!"
    0
