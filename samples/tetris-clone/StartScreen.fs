module StartScreen

open System
open System.IO
open Constants
open Xelmish.Viewables
open Xelmish.Model

type Model = {
    highScore: (int * DateTime) option
}

let init () =
    let score = 
        if File.Exists highScoreFile 
        then Some (int (File.ReadAllText highScoreFile), File.GetLastWriteTime highScoreFile)
        else None
    { highScore = score }

type Message = 
    | StartGame

let view model dispatch = 
    let text size = text "connection" size Colours.white (-0.5, 0.)
    let textMid = resWidth / 2
    [
        yield text 100. "TETRIS!" (textMid, 40)

        match model.highScore with
        | Some (score, date) ->
            yield text 30. (sprintf "High Score: %i" score) (textMid, 150)
            yield text 25. (sprintf "Scored on %s" (date.ToString("dd/MM/yyyy"))) (textMid, 190)
        | _ -> 
            yield text 25. "No high score set yet" (textMid, 170)

        yield text 25. "(S)tart game" (textMid, 250)
        yield text 25. "(Q)uit" (textMid, 280)

        yield onkeydown Keys.S (fun () -> dispatch StartGame)
        yield onkeydown Keys.Q exit
    ]