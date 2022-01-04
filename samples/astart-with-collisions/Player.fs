module Player

open Elmish
open Xelmish.Model
open Xelmish.Viewables

type Model =
    { x: int
      y: int
      width: int
      height: int
      speed: int // this mean we donnot normalize the velocity
      color: Colour
      rectangle: Rectangle }
    member x.pivot = 
        (x.x + x.width / 2, x.y + x.height / 2)

let init =
    let (x, y, w, h) = 0, 0, 25, 25

    { x = x
      y = y
      width = w
      height = h
      speed = 3
      color = Colour.Blue
      rectangle = Rectangle(x, y, w, h) }

type Msg = Movement of x: int * y: int

let update msg model =
    match msg with
    | Movement (x, y) ->
        let (nx, ny) = model.x + x, model.y + y

        { model with
            x = nx
            y = ny
            rectangle = Rectangle(nx, ny, model.width, model.height) },
        Cmd.none

let view (model: Model) dispatch (collisions: Collisions.bvhTree) =
    [ OnUpdate (fun inps ->
        let (vx, vy) =
            [ 
              if inps.keyboardState.IsKeyDown Keys.W then
                  (0, -model.speed)
              if inps.keyboardState.IsKeyDown Keys.S then
                  (0, model.speed)
              if inps.keyboardState.IsKeyDown Keys.D then
                  (model.speed, 0)
              if inps.keyboardState.IsKeyDown Keys.A then
                  (-model.speed, 0)
            ]
            |> List.fold (fun (x, y) (x', y') -> (x + x', y + y')) (0, 0)

        let mr =
            Rectangle(model.x + vx, model.y + vy, model.width, model.height)

        let (px, py) = collisions.queryPV mr (fun _ _ -> true)
        let (vx, vy) = (vx + px, vy + py)

        if (vx = 0 && vy = 0) |> not then
            (vx, vy) |> Movement |> dispatch)

      colour model.color (model.width, model.height) (model.x, model.y) ]
