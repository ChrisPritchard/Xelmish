module Invaders

open Elmish
open Xelmish.Model
open Xelmish.Viewables
open Common

type Model =
    {
        rows: Row []
        direction: ShuffleState
        lasers: (int * int) list
        lastShuffle: int64
        shuffleInterval: int64
        shuffleMod: int
    }
and Row = { kind: InvaderKind; y: int; xs: (int * InvaderState) [] }
and InvaderState = Alive | Dying | Dead
and ShuffleState = Across of row:int * dir:int | Down of row:int * nextDir:int

let init () =
    {
        rows = 
            [|0..invaderRows-1|]
            |> Array.map (fun row -> 
                let kind = match row with 0 -> smallSize | 1 | 2 -> mediumSize | _ -> largeSize
                {
                    kind = kind
                    y = invaderTop + row * (kind.height + invaderSpacing)
                    xs = [|0..invadersPerRow-1|] |> Array.map (fun col -> 
                        padding + col * (largeSize.width + invaderSpacing) + kind.offset, Alive) 
                })
        direction = Across (invaderRows - 1, 1)
        lasers = []
        lastShuffle = 0L
        shuffleInterval = 500L
        shuffleMod = 0
    }

type Message = 
    | Shoot
    | Shuffle of atTime:int64
    | MoveLasers
    | Destroy of row:int * col:int

let shootFromRandom model =
    let possibleShooters = 
        [|0..invadersPerRow-1|]
        |> Array.choose (fun col ->
            [invaderRows-1..-1..0]
            |> List.tryFind (fun row -> 
                let row = model.rows.[row]
                row.y + row.kind.height < playerY && snd row.xs.[col] = Alive)
            |> Option.map (fun row -> 
                let row = model.rows.[row]
                fst row.xs.[col] + row.kind.width / 2, row.y + row.kind.height))
    // pick a random shooter
    let x, y = possibleShooters.[pick possibleShooters.Length]
    { model with lasers = (x, y)::model.lasers }

let shuffleAcross targetRow dir model =
    let newInvaders = model.rows |> Array.mapi (fun i row -> 
        if i <> targetRow then row
        else
            let shuffled = row.xs |> Array.map (fun (x, state) -> 
                match state with
                    | Alive -> x + (invaderShuffleAmount * dir), Alive
                    | _ -> (x, state)) // the dying and dead don't shuffle
            { row with xs = shuffled })
    // if the new shuffle has resulted in out of bounds, then use the old shuffle and start down
    if newInvaders.[targetRow].xs |> Array.exists (fun (x, state) -> state = Alive && x < padding || x + largeSize.width > (resWidth - padding))
    then model.rows, Down (model.rows.Length - 1, dir * -1)
    else newInvaders, Across ((if targetRow = 0 then newInvaders.Length - 1 else targetRow - 1), dir)

let shuffleDown targetRow nextDir model =
    let newInvaders = 
        model.rows 
        |> Array.mapi (fun i row -> 
            if i <> targetRow then row
            else
                { row with y = row.y + invaderShuffleAmount })
    let nextDirection = 
        if targetRow = 0 then Across (newInvaders.Length - 1, nextDir) 
        else Down (targetRow - 1, nextDir)
    newInvaders, nextDirection

let rec shuffleRows time model = 
    // the shuffle mod is used for animations
    let model = { model with shuffleMod = (model.shuffleMod + 1) % 2 }
    let (newRows, newDirection) = 
        match model.direction with
        | Across (targetRow, dir) -> shuffleAcross targetRow dir model
        | Down (targetRow, nextDir) -> shuffleDown targetRow nextDir model
    match model.direction, newDirection with
    | Across _, Down _ -> 
        // immediately do another shuffle, to eliminate the pause between going from across to down.
        shuffleRows time { model with direction = newDirection }
    | _ ->
        { model with 
            rows = newRows
            direction = newDirection
            lastShuffle = time }
            
let destroyInvader targetRow index model =
    let (x, _) = model.rows.[targetRow].xs.[index]
    model.rows.[targetRow].xs.[index] <- (x, Dying)
    let newShuffleInterval = max minShuffle (model.shuffleInterval - shuffleDecrease)
    { model with shuffleInterval = newShuffleInterval }

let update message model =
    match message with
    | Shoot -> shootFromRandom model
    | Shuffle atTime -> shuffleRows atTime model
    | MoveLasers ->
        let rec advance acc rem =
            match rem with
            | [] -> acc
            | (x, y)::tail ->
                let ny = y + invaderProjectileSpeed
                if ny > resHeight then advance acc tail
                else advance ((x, ny)::acc) tail
        { model with lasers = advance [] model.lasers }
    | Destroy (row, col) -> destroyInvader row col model

let view model dispatch freeze =
    [
        for row in model.rows do
            let spriteRect = row.kind.animations.[model.shuffleMod]
            for (x, state) in row.xs do
                if state <> Dead then
                    match state with
                    | Alive ->
                        yield sprite spriteRect 
                            (row.kind.width, row.kind.height) 
                            (x, row.y) row.kind.colour
                    | _ ->
                        yield sprite spritemap.["invader-death"] 
                            (explosionWidth, explosionHeight) 
                            (x, row.y) Colour.White
        
        yield! 
            model.lasers
            |> List.map (fun pos ->
                colour Colour.White (projectileWidth, projectileHeight) pos)

        if not freeze then
            yield onupdate (fun inputs -> 
                if inputs.totalGameTime - model.lastShuffle > model.shuffleInterval then
                    dispatch (Shuffle inputs.totalGameTime))

            yield onupdate (fun _ ->
                dispatch MoveLasers
                if  List.length model.lasers < maxInvaderProjectiles
                    && check invaderShootChance then
                        dispatch Shoot)
    ]        