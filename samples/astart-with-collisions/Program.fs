open System
open Elmish
open Xelmish.Model
open Xelmish.Viewables
open Tiles
open Collisions
open AStar

type Model =
    { tileLayer: tileLayer
      collisions: bvhTree
      player: Player.Model }

let init () =

    let rndMap cols rows =
        [| for y = 0 to rows - 1 do
               for x = 0 to cols - 1 do
                   // [0, n) or [0, n - 1].
                   yield Random.Shared.Next(0, 5) |]

    let tileLayer =
        let (tw, th) = 25, 25
        let (cols, rows) = 1600 / tw, 900 / th 

        { tileLayer.tileWidth = tw
          tileHeight = th
          x = 0
          y = 0
          rows = rows
          cols = cols
          tiles = rndMap cols rows }

    let collisionId = Guid.NewGuid()

    { tileLayer = tileLayer
      collisions =
        tileLayer.tiles
        |> Seq.indexed
        |> Seq.filter (fun (i, e) -> e = 0)
        |> Seq.map (fun (i, c) -> collisionId, TileLayer.destRect tileLayer (Tiles.TileLayer.getXYByIndex tileLayer i))
        |> bvhTree.fromSeq
      player = Player.init },
    Cmd.none

type Message = SubMsg of Player.Msg

let update msg model =
    match msg with
    | SubMsg pmsg ->
        let (newPlayer, newMsg) = Player.update pmsg model.player
        { model with player = newPlayer }, Cmd.map SubMsg newMsg

let gcost _ _ = 1. 
let fcost (x, y) (gx, gy) = 
    (abs (gx - x) + abs (gy - y)) |> float 

let neighbors model (x, y) = 
    let (tcols, trows) = model.tileLayer.cols, model.tileLayer.rows
    let ns = 
        [ (x - 1, y)
          (x, y - 1)
          (x + 1, y)
          (x, y + 1) ]
        |> Seq.filter(fun (x, y) -> 
            let id = TileLayer.getIndexByXY model.tileLayer (x, y)
            x >= 0 && x < tcols && 
            y >= 0 && y < trows && 
            model.tileLayer.tiles.[id] <> 0)
    in ns 

let astarConfig model = 
    { neighbours = neighbors model
      gCost = gcost
      fCost = fcost
      maxIterations = None }

let view model dispatch =
    [ TileLayer.renderTileLayerColor
        (function
        | 0 -> Colour.White
        | _ -> Colour.DarkGreen)
        model.tileLayer

      yield! Player.view model.player (SubMsg >> dispatch) model.collisions

      // draw mouse
      OnDraw (fun ast inps sb ->
          let (mx, my) =
              inps.mouseState.X - model.tileLayer.x, inps.mouseState.Y - model.tileLayer.y

          let (x, y) =
              (mx / model.tileLayer.tileWidth)
              * model.tileLayer.tileWidth
              + model.tileLayer.x,
              (my / model.tileLayer.tileHeight)
              * model.tileLayer.tileWidth
              + model.tileLayer.y

          sb.Draw(ast.whiteTexture, 
                    Rectangle(x, y, model.tileLayer.tileWidth, model.tileLayer.tileHeight), 
                    Colour.Red))
      
      // draw path. 
      OnDraw(fun ast inps sb -> 
        let start = 
            model.player.pivot 
            |> TileLayer.getTileXYbyAbsoluteXY model.tileLayer
        let goal = 
            (inps.mouseState.X, inps.mouseState.Y)
            |> TileLayer.getTileXYbyAbsoluteXY model.tileLayer
        
        let search = model |> astarConfig |> search
        match search start goal with 
        | Some s -> 
            s |> Seq.iter (fun (x, y) -> 
                let (ax, ay) = 
                    x * model.tileLayer.tileWidth + model.tileLayer.x, 
                    y * model.tileLayer.tileHeight + model.tileLayer.y
                sb.Draw(ast.whiteTexture, 
                        Rectangle(ax, ay, model.tileLayer.tileWidth, model.tileLayer.tileHeight), 
                        Colour.MonoGameOrange * 0.5f))
        | None -> ())

      onkeydown Keys.Escape exit ]

Program.mkProgram init update view
|> Xelmish.Program.runSimpleGameLoop [] (1600, 900) Colour.White
