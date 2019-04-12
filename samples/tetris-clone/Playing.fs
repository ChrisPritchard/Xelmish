module Playing

open Xelmish.Model
open Xelmish.Viewables
open Elmish
open Constants

let startPos = (gridWidth / 2, 0)
let random = System.Random ()

type Model = {
    staticBlocks: Map<int * int, Colour>
    blockPosition: (int * int)
    shapeType: Shape
    nextShapeType: Shape
    rotationIndex: int
    lastDrop: int
    dropInterval: int
    dropPressed: bool
    score: int
}

let init () = 
    {
        staticBlocks = Map.empty
        blockPosition = startPos
        shapeType = shapes.[random.Next(shapes.Length)]
        nextShapeType = shapes.[random.Next(shapes.Length)]
        rotationIndex = 0
        score = 0
        lastDrop = 0
        dropInterval = 1000
        dropPressed = false
    }

type Message = 
| Tick
| Drop of bool
| Left
| Right
| Rotate
| CheckLines
| SpawnBlock
| QuitGame

type ParentMessage = 
| NoOp
| Quit
| GameOver

let tilesFor (x, y) shape rotation =
    shape.rotations.[rotation]
    |> List.map (fun (dx, dy) -> x + dx, y + dy)

let tilesForModel model = tilesFor model.blockPosition model.shapeType model.rotationIndex

let outOfBounds =
    List.exists (fun (x, y) -> x < 0 || x >= gridWidth || y < 0)

let belowFloor =
    List.exists (fun (_, y) -> y >= gridHeight)

let overlap staticBlocks tiles =
    tiles |> List.exists (fun tile -> 
        Map.containsKey tile staticBlocks)

let moveShape (dx, dy) model =
    let newModel = { model with blockPosition = let (x, y) = model.blockPosition in x + dx, y + dy }
    let newTiles = tilesForModel newModel

    if outOfBounds newTiles then model, Cmd.none, NoOp
    elif overlap model.staticBlocks newTiles || belowFloor newTiles then
        let oldTiles = tilesForModel model
        let newStatics = 
            (model.staticBlocks, oldTiles) 
            ||> List.fold (fun statics tile -> 
                Map.add tile model.shapeType.colour statics)
        { model with staticBlocks = newStatics }, Cmd.ofMsg CheckLines, NoOp
    else
        newModel, Cmd.none, NoOp

let timedDrop model =
    let interval = if model.dropPressed then 100 else model.dropInterval
    let time = int (System.DateTime.Now.Ticks / 10000L)
    if time - model.lastDrop > interval then
        moveShape (0, 1) { model with lastDrop = time }
    else
        model, Cmd.none, NoOp

let rotateShape model =
    let newModel = { model with rotationIndex = (model.rotationIndex + 1) % model.shapeType.rotations.Length }
    let newTiles = tilesForModel newModel

    if  outOfBounds newTiles 
        || belowFloor newTiles
        || overlap model.staticBlocks newTiles then
        model, Cmd.none, NoOp
    else
        newModel, Cmd.none, NoOp

let scoreFor count =
    match count with
    | 1 -> 10
    | 2 -> 30
    | 3 -> 60
    | 4 -> 100
    | _ -> 0

let checkLines model =
    let (_, y) = model.blockPosition
    let complete =
        [y..y+3] 
        |> List.filter (fun line ->
            line < gridHeight
            && List.forall (fun x -> 
                Map.containsKey (x, line) model.staticBlocks) [0..gridWidth-1])
    let dropAbove staticBlocks line =
        staticBlocks 
        |> Map.toList
        |> List.map (fun ((x, y), v) -> 
            if y > line then ((x, y), v) else ((x, y + 1), v))
        |> Map.ofList
    let newStatics = (model.staticBlocks, complete) ||> List.fold dropAbove
    let newScore = model.score + scoreFor complete.Length
    { model with staticBlocks = newStatics; score = newScore }, Cmd.ofMsg SpawnBlock, NoOp

let spawnBlock model =
    let newModel = 
        { model with 
            blockPosition = startPos
            shapeType = model.nextShapeType
            nextShapeType = shapes.[random.Next(shapes.Length)]
            rotationIndex = 0 }
    let spawnedTiles = tilesForModel newModel
    let parentMessage =
        if overlap model.staticBlocks spawnedTiles then GameOver
        else NoOp
    newModel, Cmd.none, parentMessage

let update message model =
    match message with
    | Tick -> timedDrop model
    | Left -> moveShape (-1, 0) model
    | Right -> moveShape (1, 0) model
    | Rotate -> rotateShape model
    | Drop v -> { model with dropPressed = v }, Cmd.none, NoOp
    | CheckLines -> checkLines model
    | SpawnBlock -> spawnBlock model
    | QuitGame -> model, Cmd.none, Quit

let view model dispatch =
    [
        let blockTiles = tilesForModel model |> Set.ofList
        for x = 0 to gridWidth - 1 do
            for y = 0 to gridHeight - 1 do
                let tx, ty = padding + x * tiledim, padding + y * tiledim
                if blockTiles.Contains (x, y) then
                    yield colour model.shapeType.colour (tiledim, tiledim) (tx, ty)
                else
                    match Map.tryFind (x, y) model.staticBlocks with
                    | Some c ->
                        yield colour c (tiledim, tiledim) (tx, ty)
                    | _ ->
                        yield colour Colours.whiteSmoke (tiledim, tiledim) (tx, ty)

        let previewStart = (padding * 2) + (tiledim * gridWidth)
        let previewPos = if model.nextShapeType.rotations.Length = 1 then (2, 1) else (1, 1)
        let nextBlockTiles = tilesFor previewPos model.nextShapeType 0 |> Set.ofList
        for x = 0 to 5 do
            for y = 0 to 3 do
                let tx, ty = previewStart + x * tiledim, padding + y * tiledim
                if nextBlockTiles.Contains (x, y) then
                    yield colour model.nextShapeType.colour (tiledim, tiledim) (tx, ty)
                else
                    yield colour Colours.whiteSmoke (tiledim, tiledim) (tx, ty)

        yield onkeydown Keys.Left (fun () -> dispatch Left)
        yield onkeydown Keys.Right (fun () -> dispatch Right)
        yield onkeydown Keys.Up (fun () -> dispatch Rotate)
        yield onkeydown Keys.Down (fun () -> dispatch (Drop true))
        yield onkeyup Keys.Down (fun () -> dispatch (Drop false))
        yield onkeydown Keys.Escape (fun () -> dispatch QuitGame)
    ]