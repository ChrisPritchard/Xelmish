open System
open Elmish
open Xelmish.Model
open Xelmish.Viewables
open Tiles
open Collisions

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
        let (cols, rows) = 5, 5

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

let view model dispatch =
    [ TileLayer.renderTileLayerColor
        (function
        | 0 -> Colour.Transparent
        | _ -> Colour.Aqua)
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

          sb.Draw(ast.whiteTexture, Rectangle(x, y, model.tileLayer.tileWidth, model.tileLayer.tileHeight), Colour.Red))

      onkeydown Keys.Escape exit ]

Program.mkProgram init update view
|> Program.withConsoleTrace
|> Xelmish.Program.runSimpleGameLoop [] (1600, 900) Colour.Black
