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
      rectangle: Rectangle}

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
            rectangle = Rectangle(nx, ny, model.width, model.height)},
        Cmd.none

let view (model: Model) dispatch (collisions: Collisions.bvhTree) =
    [ whilekeydown Keys.D (fun _ -> Movement(model.speed, 0) |> dispatch)
      whilekeydown Keys.A (fun _ -> Movement(-model.speed, 0) |> dispatch)
      whilekeydown Keys.W (fun _ -> Movement(0, -model.speed) |> dispatch)
      whilekeydown Keys.S (fun _ -> Movement(0, model.speed) |> dispatch)

      OnUpdate (fun inps ->
          collisions.query model.rectangle (fun id rect ->
              Collisions.penetrationVector rect model.rectangle
              |> Movement
              |> dispatch))

      colour model.color (model.width, model.height) (model.x, model.y) ]
