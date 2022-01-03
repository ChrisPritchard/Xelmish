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
    [
      //whilekeydown Keys.D (fun _ -> Movement(model.speed, 0) |> dispatch)
      //whilekeydown Keys.A (fun _ -> Movement(-model.speed, 0) |> dispatch)
      //whilekeydown Keys.W (fun _ -> Movement(0, -model.speed) |> dispatch)
      //whilekeydown Keys.S (fun _ -> Movement(0, model.speed) |> dispatch)

      //OnUpdate (fun inps ->
      //    collisions.query model.rectangle (fun id rect ->
      //        Collisions.penetrationVector rect model.rectangle
      //        |> Movement
      //        |> dispatch))
      OnUpdate (fun inps ->
          let mutable (vx, vy) =
              if inps.keyboardState.IsKeyDown Keys.D then
                  (model.speed, 0)
              elif inps.keyboardState.IsKeyDown Keys.A then
                  (-model.speed, 0)
              elif inps.keyboardState.IsKeyDown Keys.W then
                  (0, -model.speed)
              elif inps.keyboardState.IsKeyDown Keys.S then
                  (0, model.speed)
              else
                  (0, 0)
          
          let mr = Rectangle(model.x + vx, model.y + vy, model.width, model.height)
          collisions.query mr (fun id rect ->
              let mr = Rectangle(model.x + vx, model.y + vy, model.width, model.height)
              let (px, py) =
                  Collisions.penetrationVector rect mr 
              vx <- vx + px
              vy <- vy + py)

          if (vx = 0 && vy = 0) |> not then
              (vx, vy) |> Movement |> dispatch)
          
      colour model.color (model.width, model.height) (model.x, model.y) ]
