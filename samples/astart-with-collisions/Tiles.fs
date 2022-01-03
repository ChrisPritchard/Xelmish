module Tiles

open Xelmish.Model

type tileSet =
    { tileWidth: int
      tileHeight: int
      rows: int
      cols: int
      textureKey: string
      sourceRects: Rectangle array }

module TileSet =
    let init twidth theight rows cols tkey =
        { tileWidth = twidth
          tileHeight = theight
          rows = rows
          cols = cols
          textureKey = tkey
          sourceRects =
            [| for y = 0 to rows - 1 do
                   for x = 0 to cols - 1 do
                       yield Rectangle(x * twidth, y * theight, twidth, theight) |] }


type tileLayer =
    { x: int 
      y: int
      tileWidth: int
      tileHeight: int
      tiles: int array
      cols: int
      rows: int }

module TileLayer =
    let fromRect rows cols tiles (rect:Rectangle) = 
        let pos = rect.Location.ToVector2()
        { x = pos.X |> int 
          y = pos.Y |> int 
          rows = rows 
          cols = cols 
          tiles = tiles
          tileWidth = rect.Width / cols 
          tileHeight = rect.Height / rows }

    let fromArea rows cols (x, y) (width, height) tiles = 
        { x = x 
          y = y
          rows = rows 
          cols = cols 
          tiles = tiles 
          tileWidth = width / cols 
          tileHeight = height / rows }

    let inline destRect tileLayer (x, y) =
        Rectangle(
            tileLayer.x + x * tileLayer.tileWidth,
            tileLayer.y
            + y * tileLayer.tileHeight,
            tileLayer.tileWidth,
            tileLayer.tileHeight)

    let getXYByIndex tileLayer i = 
        (i % tileLayer.cols, i / tileLayer.cols)

    /// if you use "tiled" then startId would be 1. 
    let renderTileLayer tileSet tileLayer startId =
        OnDraw (fun ast _ sb ->
            let texture = ast.textures.[tileSet.textureKey]

            for y = 0 to tileLayer.rows - 1 do
                for x = 0 to tileLayer.cols - 1 do
                    let ind = y * tileLayer.cols + x
                    let srcInd = tileLayer.tiles.[ind]

                    if srcInd >= startId then 
                        let srcRect = tileSet.sourceRects.[srcInd - startId]
                        let destRect = destRect tileLayer (x, y)
                        sb.Draw(texture, destRect, srcRect, Colour.White))

    let renderTileLayerColor getColor tileLayer = 
        OnDraw (fun ast _ sb -> 
            let texture = ast.whiteTexture 

            for y = 0 to tileLayer.rows - 1 do
                for x = 0 to tileLayer.cols - 1 do
                    let ind = y * tileLayer.cols + x
                    if ind = 0 then 
                        () 
                    else 
                        let destRect = destRect tileLayer (x, y)
                        sb.Draw(texture, destRect, getColor tileLayer.tiles.[ind]))
