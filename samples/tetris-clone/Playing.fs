module Playing

open Xelmish.Model
open Xelmish.Viewables
open Elmish

let gridWidth = 10
let gridHeight = 20
let startPos = (gridWidth / 2, 0)
let random = System.Random ()

type Model = {
    staticBlocks: Map<int * int, Colour>
    blockPosition: (int * int)
    shapeType: Shape
    rotationIndex: int
    score: int
}
and Shape = {
    rotations: ((int * int) list) []
    colour: Colour
}

let shapes = [
    {   rotations = [|  [0,0; 1,0; 0,1; 1,1] |] // O
        colour = Colours.cyan }
    {   rotations = [|  [0,0; 1,0; 2,0; 3,0]    // I
                        [2,0; 2,1; 2,2; 2,3] |]
        colour = Colours.red }
    {   rotations = [|  [0,0; 1,0; 1,1; 2,1]    // Z
                        [2,0; 2,1; 1,1; 1,2] |] 
        colour = Colours.green }
    {   rotations = [|  [1,0; 2,0; 1,1; 0,1]    // S
                        [1,0; 1,1; 2,1; 2,2] |] 
        colour = Colours.blue }
    {   rotations = [|  [0,0; 1,0; 2,0; 0,1]    // L
                        [0,0; 1,0; 1,1; 1,2]
                        [0,1; 1,1; 2,1; 2,0]
                        [1,0; 1,1; 1,2; 2,2] |] 
        colour = Colours.orange }
    {   rotations = [|  [0,0; 1,0; 2,0; 2,1]    // J
                        [1,0; 1,1; 1,2; 0,2]
                        [0,0; 0,1; 1,1; 2,1]
                        [1,0; 2,0; 1,1; 1,2] |] 
        colour = Colours.magenta }
    {   rotations = [|  [0,0; 1,0; 2,0; 1,1]    // T
                        [1,0; 1,1; 1,2; 0,1]
                        [0,1; 1,1; 2,1; 1,0]
                        [1,0; 1,1; 1,2; 2,1] |]
        colour = Colours.silver }
]

let init () = 
    {
        staticBlocks = Map.empty
        blockPosition = startPos
        shapeType = shapes.[random.Next(shapes.Length)]
        rotationIndex = 0
        score = 0
    }, Cmd.none

type Message = 
| Tick
| Drop
| Left
| Right
| Rotate
| CheckLines
| SpawnBlock

let tilesFor model =
    let x, y = model.blockPosition
    model.shapeType.rotations.[model.rotationIndex] 
    |> List.map (fun (dx, dy) -> x + dx, y + dy)

let outOfBounds =
    List.exists (fun (x, y) -> x < 0 || x >= gridWidth || y < 0)

let belowFloor =
    List.exists (fun (_, y) -> y >= gridHeight)

let overlap staticBlocks tiles =
    tiles |> List.exists (fun tile -> 
        Map.containsKey tile staticBlocks)

let moveShape (dx, dy) model =
    let newModel = { model with blockPosition = let (x, y) = model.blockPosition in x + dx, y + dy }
    let newTiles = tilesFor newModel

    if outOfBounds newTiles then model, Cmd.none
    elif overlap model.staticBlocks newTiles || belowFloor newTiles then
        let oldTiles = tilesFor model
        let newStatics = 
            (model.staticBlocks, oldTiles) 
            ||> List.fold (fun statics tile -> 
                Map.add tile model.shapeType.colour statics)
        { model with staticBlocks = newStatics }, Cmd.ofMsg CheckLines
    else
        newModel, Cmd.none

let rotateShape model =
    let newModel = { model with rotationIndex = (model.rotationIndex + 1) % model.shapeType.rotations.Length }
    let newTiles = tilesFor newModel

    if  outOfBounds newTiles 
        || belowFloor newTiles
        || overlap model.staticBlocks newTiles then
        model, Cmd.none
    else
        newModel, Cmd.none

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
    { model with staticBlocks = newStatics; score = newScore }, Cmd.ofMsg SpawnBlock

let spawnBlock model gameOver =
    let newModel = 
        { model with 
            blockPosition = startPos
            shapeType = shapes.[random.Next(shapes.Length)]
            rotationIndex = 0 }
    let spawnedTiles = tilesFor newModel
    if overlap model.staticBlocks spawnedTiles then gameOver ()
    newModel, Cmd.none

let update gameOver message model =
    match message with
    | Tick | Drop -> moveShape (0, 1) model
    | Left -> moveShape (-1, 0) model
    | Right -> moveShape (1, 0) model
    | Rotate -> rotateShape model
    | CheckLines -> checkLines model
    | SpawnBlock -> spawnBlock model gameOver

let view model dispatch =
    let gridX, gridY = 30, 30
    let tileW, tileH = 20, 20
    let blockTiles = tilesFor model |> Set.ofList
    [
        for x = 0 to gridWidth - 1 do
            for y = 0 to gridHeight - 1 do
                let tx, ty = gridX + x * tileW, gridY + y * tileH
                if blockTiles.Contains (x, y) then
                    yield colour model.shapeType.colour (tileW, tileH) (tx, ty)
                else
                    match Map.tryFind (x, y) model.staticBlocks with
                    | Some c ->
                        yield colour c (tileW, tileH) (tx, ty)
                    | _ ->
                        yield colour Colours.whiteSmoke (tileW, tileH) (tx, ty)

        yield onkeydown Keys.Left (fun () -> dispatch Left)
        yield onkeydown Keys.Right (fun () -> dispatch Right)
        yield onkeydown Keys.Up (fun () -> dispatch Rotate)
        yield onkeydown Keys.Down (fun () -> dispatch Drop)
    ]