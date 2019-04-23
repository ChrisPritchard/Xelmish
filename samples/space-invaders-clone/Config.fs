/// Constants and loaded configuration (e.g. the spritemap) are specified in here
module Config

let resWidth = 800
let resHeight = 600

let padding = 30

let playerDim = 40
let playerY = resHeight - (playerDim + padding)
let playerSpeed = 5

let invaderSpacing = 10
let invadersPerRow = 11
let invaderRows = 5
let invaderShuffleAmount = 20
let invaderShuffleIncrease = 25L

let projectileHeight = 10
let projectileSpeed = 6

let spritemap = 
    System.IO.File.ReadAllLines "./spritemap.txt"
    |> Array.map (fun line -> 
        let sa = line.Split ([|'\t';','|], System.StringSplitOptions.RemoveEmptyEntries)
        sa.[0], (int sa.[1], int sa.[2], int sa.[3], int sa.[4]))
    |> Map.ofArray

let smallSize = let (w, h, _, _) = spritemap.["invader-small-0"] in w, h
let mediumSize = let (w, h, _, _) = spritemap.["invader-medium-0"] in w, h
let largeSize = let (w, h, _, _) = spritemap.["invader-large-0"] in w, h