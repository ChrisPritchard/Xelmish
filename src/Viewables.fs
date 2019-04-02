module Xelmish.Viewables

type Viewable =
| Window of x: int * y: int * width: int * height: int * Viewable list
| Text of string
| Button of text: string * onClick: (unit -> unit)
| Row of Viewable list