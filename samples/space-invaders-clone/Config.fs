/// Constants and loaded configuration (e.g. the spritemap) are specified in here
module Config

open Xelmish.Model

let resWidth = 800
let resHeight = 600

let padding = 30
let invaderTop = padding * 3

let invaderSpacing = 12
let invadersPerRow = 11
let invaderRows = 5
let invaderShuffleAmount = 20
let invaderShuffleIncrease = 25L

let shuffleDecrease = 20L
let minShuffle = 50L

let projectileHeight = 10
let invaderProjectileSpeed = 2
let playerProjectileSpeed = 4

let maxInvaderProjectiles = 3
let invaderShootChance = 0.1

let spritemap = 
    System.IO.File.ReadAllLines "./spritemap.txt"
    |> Array.map (fun line -> 
        let sa = line.Split ([|'\t';','|], System.StringSplitOptions.RemoveEmptyEntries)
        sa.[0], (int sa.[1], int sa.[2], int sa.[3], int sa.[4]))
    |> Map.ofArray

let sizeMulti = 2

type InvaderKind = {
    width: int
    height: int
    offset: int
    colour: Colour
    animations: (int * int * int * int) []
    score: int
}

let largeSize = 
    let width, height = 
        let w, h, _, _ = spritemap.["invader-large-0"]
        w * sizeMulti, h * sizeMulti
    {   width = width
        height = height
        offset = 0
        colour = Colour.OrangeRed
        animations = [|
                spritemap.["invader-large-0"]
                spritemap.["invader-large-1"]
            |]
        score = 10 }

let mediumSize = 
    let width, height = 
        let w, h, _, _ = spritemap.["invader-medium-0"]
        w * sizeMulti, h * sizeMulti
    {   width = width
        height = height
        offset = (largeSize.width - width) / 2
        colour = Colour.Gold
        animations = [|
                spritemap.["invader-medium-0"]
                spritemap.["invader-medium-1"]
            |]
        score = 20 }

let smallSize = 
    let width, height = 
        let w, h, _, _ = spritemap.["invader-small-0"]
        w * sizeMulti, h * sizeMulti
    {   width = width
        height = height
        offset = (largeSize.width - width) / 2
        colour = Colour.Cyan
        animations = [|
                spritemap.["invader-small-0"]
                spritemap.["invader-small-1"]
            |]
        score = 30 }

let animationSpeed = 100L

let playerWidth, playerHeight = 
    let (w, h, _, _) = spritemap.["player"]
    w * sizeMulti, h * sizeMulti

let playerY = resHeight - (playerHeight + padding)
let playerSpeed = 5

let bunkerSpace = 20
let bunkerBitDim = 6
let bunkerPattern = 
    [|
        "   XXXXXXXXX   "
        "  XXXXXXXXXXX  "
        " XXXXXXXXXXXXX "
        "XXXXXXXXXXXXXXX"
        "XXXXXXXXXXXXXXX"
        "XXXXXXXXXXXXXXX"
        "XXX         XXX"
        "XXX         XXX"
    |] |> Array.map (Seq.map (fun c -> if c = 'X' then true else false) >> Seq.toArray)
let bunkerHeight = bunkerBitDim * bunkerPattern.Length
let bunkerOffset = (bunkerBitDim * bunkerPattern.[0].Length) / 2
let bunkerColour = rgba 0uy 255uy 0uy 255uy

let explosionWidth, explosionHeight = 
    let (w, h, _, _) = spritemap.["invader-death"]
    w * sizeMulti, h * sizeMulti
let explosionDuration = 1