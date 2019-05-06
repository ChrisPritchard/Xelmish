module GameOverScreen

open System
open System.IO
open Constants
open Xelmish.Viewables
open Xelmish.Model

type Model = {
    highScore: (int * DateTime) option
    score: int
    newHighScore: bool
}

let init score =
    let highScore = 
        if File.Exists highScoreFile 
        then Some (int (File.ReadAllText highScoreFile), File.GetLastWriteTime highScoreFile)
        else None
    let newHighScore = match highScore with Some (i, _) when i >= score -> false | _ -> true
    if newHighScore then
        File.WriteAllText (highScoreFile, score.ToString())
    { highScore = highScore; score = score; newHighScore = newHighScore }

type Message = 
    | StartGame

let view model dispatch = 
    let text size = text "connection" size Colour.White (-0.5, 0.)
    let textMid = resWidth / 2
    [
        yield text 80. "GAME OVER!" (textMid, 40)

        yield text 40. (sprintf "Score: %i" model.score) (textMid, 160)

        if not model.newHighScore then
            yield text 25. "you failed to beat your high score" (textMid, 210)
            match model.highScore with
            | Some (score, date) ->
                yield text 30. (sprintf "High Score: %i" score) (textMid, 260)
                yield text 25. (sprintf "Scored on %s" (date.ToString("dd/MM/yyyy"))) (textMid, 300)
            | _ -> ()
        else
            yield text 40. "NEW HIGH SCORE!" (textMid, 210)

        yield text 25. "(P)lay again?" (textMid, 350)
        yield text 25. "(Q)uit" (textMid, 380)

        yield onkeydown Keys.P (fun () -> dispatch StartGame)
        yield onkeydown Keys.Q exit
    ]