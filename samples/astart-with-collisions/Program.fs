open System
open Elmish
open Xelmish.Model 
open Xelmish.Viewables 

type Model = 
    { 
        tileLayer: Tiles.tileLayer 
        collisions: Collisions.bvhTree
    } 

let init () = 

    let rndMap cols rows = 
        [|
            for y = 0 to rows - 1 do 
                for x = 0 to cols - 1 do 
                    // [0, n) or [0, n - 1]. 
                    yield Random.Shared.Next(0, 4)
        |]

    let tileLayer = Tiles.TileLayer.fromArea 30 30 (0, 0) (600, 600) (rndMap 30 30)

    let collisionId = Guid.NewGuid() 

    { 
        tileLayer = tileLayer        
        collisions = 
            tileLayer.tiles 
            |> Seq.indexed
            |> Seq.filter(fun (i, e) -> e = 0)
            |> Seq.map(fun (i, c) -> collisionId, Tiles.TileLayer.destRect tileLayer (i / tileLayer.cols) (i % tileLayer.cols))
            |> Collisions.bvhTree.fromSeq
    }, Cmd.none

type Message =
    | E

let update message model =
    match message with
    | E -> model, Cmd.none

let view model dispatch =
    [
        Tiles.TileLayer.renderTileLayerColor (
            function 
            | 0 -> Colour.Transparent 
            | _ -> Colour.Aqua
        ) model.tileLayer

        // draw mouse 
        OnDraw(fun ast inps sb -> 
            let (mx, my) = inps.mouseState.X - model.tileLayer.x, 
                            inps.mouseState.Y - model.tileLayer.y
            let (x, y) = (mx / model.tileLayer.tileWidth) * model.tileLayer.tileWidth + model.tileLayer.x, 
                            (my / model.tileLayer.tileHeight) * model.tileLayer.tileWidth + model.tileLayer.y 
            sb.Draw(ast.whiteTexture, 
                    Rectangle(x, y, model.tileLayer.tileWidth, model.tileLayer.tileHeight), 
                    Colour.Red))

        onkeydown Keys.Escape exit
    ]

Program.mkProgram init update view
|> Xelmish.Program.runSimpleGameLoop [] (600, 600) Colour.Black