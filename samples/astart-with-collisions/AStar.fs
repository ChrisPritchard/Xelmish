/// a astar path finding algorithm module created by Chris Pritchard. 
/// 
/// see also: https://github.com/ChrisPritchard/astar-search
module AStar

type Config<'a> = 
    {
        /// <summary>
        /// A method that, given a source, will return its neighbours.
        /// </summary>
        neighbours: 'a -> seq<'a>
        /// <summary>
        /// Given two nodes that are next to each other, return the g cost between them.
        /// The g cost is the cost of moving from one to the other directly.
        /// </summary>
        gCost: 'a -> 'a -> float
        /// <summary>
        /// Given two nodes, return the f cost between them. This is a heuristic score used from a given node to the goal.
        /// Line-of-sight distance is an example of how this might be defined.
        /// </summary>
        fCost: 'a -> 'a -> float
        /// <summary>
        /// The maximum number of tiles to check - used to limit overly long searches when accuracy is not paramount
        /// </summary>
        maxIterations: int option
    }

let search<'a when 'a : comparison> config start goal : seq<'a> option =

    let rec reconstructPath cameFrom current =
        seq {
            yield current
            match Map.tryFind current cameFrom with
            | None -> ()
            | Some next -> yield! reconstructPath cameFrom next
        }

    let rec crawler closedSet (openSet, gScores, fScores, cameFrom) =
        match config.maxIterations with 
        | Some n when n = Set.count closedSet -> None
        | _ ->
            match List.sortBy (fun n -> Map.find n fScores) openSet with
            | current::_ when current = goal -> Some <| reconstructPath cameFrom current 
            | current::rest ->
                let gScore = Map.find current gScores
                let next =
                    config.neighbours current 
                    |> Seq.filter (fun n -> closedSet |> Set.contains n |> not)
                    |> Seq.fold (fun (openSet, gScores, fScores, cameFrom) neighbour ->
                        let tentativeGScore = gScore + config.gCost current neighbour
                        if List.contains neighbour openSet && tentativeGScore >= Map.find neighbour gScores 
                        then (openSet, gScores, fScores, cameFrom)
                        else
                            let newOpenSet = if List.contains neighbour openSet then openSet else neighbour::openSet
                            let newGScores = Map.add neighbour tentativeGScore gScores
                            let newFScores = Map.add neighbour (tentativeGScore + config.fCost neighbour goal) fScores
                            let newCameFrom = Map.add neighbour current cameFrom
                            newOpenSet, newGScores, newFScores, newCameFrom
                        ) (rest, gScores, fScores, cameFrom)
                crawler (Set.add current closedSet) next
            | _ -> None

    let gScores = Map.ofList [start, 0.]
    let fScores = Map.ofList [start, config.fCost start goal]
    crawler Set.empty ([start], gScores, fScores, Map.empty)