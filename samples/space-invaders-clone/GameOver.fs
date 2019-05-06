module GameOver

open System.IO
open Common
open Xelmish.Viewables
open Xelmish.Model

type Model = {
    highScore: int
    score: int
    wasVictory: bool
    soundQueue: KeyQueue
}

let init wasVictory score highScore =
    if highScore < score then
        File.WriteAllText (highScoreFile, score.ToString())
    {   highScore = highScore
        score = score
        wasVictory = wasVictory
        soundQueue = KeyQueue (if wasVictory then ["victory"] else ["gameover"]) }

type Message = | StartGame of highScore: int

let view model dispatch = 
    let centredText colour = text "PressStart2P" 24. colour (-0.5, 0.)
    let textMid = resWidth / 2
    [
        yield playQueuedSound model.soundQueue

        if not model.wasVictory then
            yield centredText Colour.Red "GAME  OVER!" (textMid, 90)
            yield centredText Colour.OrangeRed "YOU  FAILED  TO  FEND  OFF  THE  INVADERS  :(" (textMid, 130)
        else
            yield centredText (rgba 0uy 255uy 0uy 255uy) "VICTORY!" (textMid, 90)
            yield centredText Colour.OrangeRed "YOU  DEFEATED  THE  INVASION!" (textMid, 130)

        yield centredText Colour.White "SCORE" (textMid, 200)
        yield centredText Colour.White (sprintf "%04i" model.score) (textMid, 240)

        if model.highScore >= model.score then
            yield centredText Colour.Cyan "HIGH  SCORE" (textMid, 280)
            yield centredText Colour.Cyan (sprintf "%04i" model.highScore) (textMid, 320)
        else
            yield centredText Colour.Cyan "NEW  HIGH  SCORE!" (textMid, 280)

        yield centredText Colour.OrangeRed "(P)LAY  AGAIN" (textMid, 450)
        yield centredText Colour.OrangeRed "(Q)UIT" (textMid, 480)

        yield onkeydown Keys.P (fun () -> dispatch (StartGame (if model.highScore < model.score then model.score else model.highScore)))
        yield onkeydown Keys.Q exit
        yield onkeydown Keys.Escape exit
    ]