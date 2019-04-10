module Playing

open Xelmish.Model
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
    {   rotations = [|  [0,0; 1,0; 0,1; 1,1] |]
        colour = Colours.cyan }
    {   rotations = [|  [0,0; 1,0; 2,0; 3,0]
                        [2,0; 2,1; 2,2; 2,3] |]
        colour = Colours.red }
    {   rotations = [|  [0,0; 1,0; 1,1; 2,1]
                        [2,0; 2,1; 1,1; 1,2] |] 
        colour = Colours.green }
    {   rotations = [|  [1,0; 2,0; 1,1; 0,1]
                        [1,0; 1,1; 2,1; 2,2] |] 
        colour = Colours.blue }
    {   rotations = [|  [0,0; 1,0; 2,0; 0,1]
                        [1,0; 1,1; 1,2; 2,2] |] 
        colour = Colours.yellow }
    {   rotations = [|  [0,0; 1,0; 2,0; 2,1]
                        [1,0; 1,1; 1,2; 2,0] |] 
        colour = Colours.magenta }
    {   rotations = [|  [0,0; 1,0; 2,0; 1,1]
                        [1,0; 1,1; 1,2; 0,1]
                        [0,1; 1,1; 2,1; 1,0]
                        [1,0; 1,1; 1,2; 2,1] |]
        colour = Colours.silver }
]

let init () = {
        staticBlocks = Map.empty
        blockPosition = startPos
        shapeType = shapes.[random.Next(shapes.Length)]
        rotationIndex = 0
        score = 0
    }

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

let outOfBounds (x, y) =
    x < 0 || x >= gridWidth || y < 0 || y >= gridHeight

let overlap staticBlocks tiles =
    tiles |> List.exists (fun tile -> 
        Map.containsKey tile staticBlocks)

let moveShape (dx, dy) model =
    let newModel = { model with blockPosition = let (x, y) = model.blockPosition in x + dx, y + dy }
    let newTiles = tilesFor newModel

    if List.exists outOfBounds newTiles then model, Cmd.none
    elif overlap model.staticBlocks newTiles then
        let oldTiles = tilesFor model
        let newStatics = 
            (model.staticBlocks, oldTiles) 
            ||> List.fold (fun statics tile -> 
                Map.add tile model.shapeType.colour statics)
        { model with
            staticBlocks = newStatics
            rotationIndex = 0
            blockPosition = startPos }, Cmd.ofMsg CheckLines
    else
        newModel, Cmd.none

let rotateShape model =
    let newModel = { model with rotationIndex = (model.rotationIndex + 1) % model.shapeType.rotations.Length }
    let newTiles = tilesFor newModel

    if  List.exists outOfBounds newTiles 
        || overlap model.staticBlocks newTiles then
        model, Cmd.none
    else
        newModel, Cmd.none

let checkLines model = // TODO remove lines and add score
    model, Cmd.ofMsg SpawnBlock

let spawnBlock model gameOver =
    let newModel = { model with shapeType = shapes.[random.Next(shapes.Length)] }
    let spawnedTiles = tilesFor newModel
    if overlap model.staticBlocks spawnedTiles then gameOver ()
    newModel, Cmd.none

let update message model gameOver =
    match message with
    | Tick | Drop -> moveShape (0, 1) model
    | Left -> moveShape (-1, 0) model
    | Right -> moveShape (1, 0) model
    | Rotate -> rotateShape model
    | CheckLines -> checkLines model
    | SpawnBlock -> spawnBlock model gameOver