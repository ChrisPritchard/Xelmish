module Xelmish.Viewables

type Viewable =
| Position of x: int * y: int * width: int * height: int * Viewable
| Window of x: int * y: int * width: int * height: int * Viewable list
| Text of string
| Button of text: string * onClick: (unit -> unit)
| Row of Viewable list

let rec renderViewable drawState bounds viewable =
    match viewable with
    | Position (x, y, w, h, sv) ->
        renderViewable drawState (x, y, w, h) sv
    | Window (x, y, w, h, svl) ->
        let div = h / svl.Length
        svl |> List.iteri (fun i -> renderViewable drawState (x, y + (i * div), w, div))