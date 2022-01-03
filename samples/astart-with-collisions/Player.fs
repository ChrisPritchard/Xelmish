module Player

open Elmish 
open Xelmish.Model
open Xelmish.Viewables

type Player = 
    {
        x: int 
        y: int 
        width: int 
        height: int 
        speed: int // this mean we donnot normalize the velocity 
        color: Colour
    }
    member model.getAABB () = 
        Rectangle(model.x, model.y, model.width, model.height)

let init = 
    {
        x = 0 
        y = 0 
        width = 25
        height = 25
        speed = 2
        color = Colour.Blue
    }

type Msg = 
    | Movement of x:int * y:int 

let update msg model = 
    match msg with 
    | Movement(x, y) -> 
        { model with x = model.x + x
                     y = model.y + y }, Cmd.none 

let view model dispatch (collisions:Collisions.bvhTree) = 
    [
        

        whilekeydown Keys.D (fun _ -> Movement(model.speed, 0) |> dispatch)
        whilekeydown Keys.A (fun _ -> Movement(-model.speed, 0) |> dispatch)
        whilekeydown Keys.W (fun _ -> Movement(0, -model.speed) |> dispatch)
        whilekeydown Keys.S (fun _ -> Movement(0, model.speed) |> dispatch)

        OnUpdate(fun inps -> 
            collisions.query (model.getAABB())
                             (fun id rect -> 
                                Collisions.penetrationVector rect (model.getAABB())
                                |> Movement
                                |> dispatch))

        colour model.color (model.width, model.height) (model.x, model.y)
    ]