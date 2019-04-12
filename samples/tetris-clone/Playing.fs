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

let ifValid model newModel =
    let newTiles = tilesForModel newModel
    if  outOfBounds newTiles 
        || belowFloor newTiles
        || overlap model.staticBlocks newTiles then
        model, Cmd.none, NoOp
    else
        newModel, Cmd.none, NoOp

let moveShape dx model =
    ifValid model { model with blockPosition = let (x, y) = model.blockPosition in x + dx, y }

let rotateShape model =
    ifValid model { model with rotationIndex = (model.rotationIndex + 1) % model.shapeType.rotations.Length }

let dropShape model =
    let newModel = { model with blockPosition = let (x, y) = model.blockPosition in x, y + 1 }
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

let checkForDrop model =
    let interval = if model.dropPressed then 100 else model.dropInterval
    let time = int (System.DateTime.Now.Ticks / 10000L)
    if time - model.lastDrop > interval then
        dropShape { model with lastDrop = time }
    else
        model, Cmd.none, NoOp

let dropAbove staticBlocks line =
    staticBlocks 
    |> Map.toList
    |> List.map (fun ((x, y), v) -> 
        if y > line then ((x, y), v) else ((x, y + 1), v))
    |> Map.ofList

let removeLines toRemove model =
    let newStatics = (model.staticBlocks, toRemove) ||> List.fold dropAbove
    let newScore = model.score + scoreFor toRemove.Length
    let newDrop = 
        if newScore / perLevel > model.score / perLevel 
        then max minDrop (model.dropInterval - dropPerLevel)
        else model.dropInterval
    { model with 
        staticBlocks = newStatics
        score = newScore
        dropInterval = newDrop }, Cmd.ofMsg SpawnBlock, NoOp

let checkLines model =
    let (_, y) = model.blockPosition
    let complete =
        [y..y+3] 
        |> List.filter (fun line ->
            line < gridHeight
            && List.forall (fun x -> 
                Map.containsKey (x, line) model.staticBlocks) [0..gridWidth-1])
    if List.isEmpty complete then
        model, Cmd.ofMsg SpawnBlock, NoOp
    else
        removeLines complete model

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
    | Tick -> checkForDrop model
    | Left -> moveShape -1 model
    | Right -> moveShape 1 model
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