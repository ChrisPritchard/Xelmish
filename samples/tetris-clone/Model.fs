module Model

open Xelmish.Model

type GameModel =
| StartPage
| Playing of PlayingState
| GameOver of score:int
| Quit
and PlayingState = {
    grid: Colour[,]
    block: (int * int) list * Colour
    score: int
}