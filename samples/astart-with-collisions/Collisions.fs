module Collisions

open System
open Xelmish.Model

/// the direction of vector
///
/// is the same as the entering direction of r1
///
/// and the same as the leaving direction of r2.
let penetrationVector (r1: Rectangle) (r2: Rectangle) =
    let intersection = Rectangle.Intersect(r1, r2)

    if intersection.Width < intersection.Height then
        (if r1.Center.X < r2.Center.X then
             intersection.Width
         else
             -intersection.Width),
        0
    else
        0,
        (if r1.Center.Y < r2.Center.Y then
             intersection.Height
         else
             -intersection.Height)

type bvhTree =
    | Node of left: bvhTree * right: bvhTree * aabb: Rectangle * height: int
    | Leaf of aabb: Rectangle * id: Guid
    | Nil
    static member empty = Nil

    member inline private x.getBalanceFactor() =
        match x with
        | Node (l, r, _, _) -> l.height () - r.height ()
        | _ -> 0

    member inline private x.getBound() =
        match x with
        | Node (_, _, b, _) -> b
        | Leaf (b, _) -> b
        | Nil -> Rectangle.Empty

    member x.insert(id: Guid, rect: Rectangle) =
        match x with
        | Node (l, r, b, h) ->
            let bl' = Rectangle.Union(l.getBound (), rect)
            let br' = Rectangle.Union(r.getBound (), rect)

            let ur =
                if bl'.Height + bl'.Width < br'.Height + br'.Width then
                    let l = l.insert (id, rect)
                    Node(l, r, Rectangle.Union(b, rect), (max (l.height ()) (r.height ())) + 1)
                else
                    let r = r.insert (id, rect)
                    Node(l, r, Rectangle.Union(b, rect), (max (l.height ()) (r.height ())) + 1)

            match ur with
            | Node (l, r, _, _) ->
                let bf = ur.getBalanceFactor ()
                let lbf = l.getBalanceFactor ()
                let rbf = r.getBalanceFactor ()

                if bf > 1 && lbf >= 0 then
                    match (l, r) with
                    | Node (Node (_, _, bz, zh) as z, t3, _, _), t4 ->
                        let nb =
                            Rectangle.Union(t3.getBound (), t4.getBound ())

                        let n =
                            Node(t3, t4, nb, (max (t3.height ()) (t4.height ())) + 1)

                        Node(z, n, Rectangle.Union(nb, bz), (max zh (n.height ())) + 1)
                    | _ -> ur
                elif bf > 1 && lbf < 0 then
                    match (l, r) with
                    | Node (t1, Node (t2, t3, _, _), _, _), t4 ->
                        let bl =
                            Rectangle.Union(t1.getBound (), t2.getBound ())

                        let br =
                            Rectangle.Union(t3.getBound (), t4.getBound ())

                        let nl =
                            Node(t1, t2, bl, (max (t1.height ()) (t2.height ())) + 1)

                        let nr =
                            Node(t3, t4, br, (max (t3.height ()) (t4.height ())) + 1)

                        Node(nl, nr, Rectangle.Union(bl, br), (max (nl.height ()) (nr.height ())) + 1)
                    | _ -> ur
                elif bf < -1 && rbf <= 0 then
                    match (l, r) with
                    | t1, Node (t2, (Node (_, _, bz, zh) as z), _, _) ->
                        let nb =
                            Rectangle.Union(t1.getBound (), t2.getBound ())

                        let n =
                            Node(t1, t2, nb, (max (t1.height ())) (t2.height ()) + 1)

                        Node(n, z, Rectangle.Union(nb, bz), (max (n.height ()) zh) + 1)
                    | _ -> ur
                elif bf < -1 && rbf > 0 then
                    match (l, r) with
                    | t1, Node (Node (t2, t3, _, _), t4, _, _) ->
                        let bl =
                            Rectangle.Union(t1.getBound (), t2.getBound ())

                        let br =
                            Rectangle.Union(t3.getBound (), t4.getBound ())

                        let nl =
                            Node(t1, t2, bl, (max (t1.height ()) (t2.height ())) + 1)

                        let nr =
                            Node(t3, t4, br, (max (t3.height ()) (t4.height ())) + 1)

                        Node(nl, nr, Rectangle.Union(bl, br), (max (nl.height ()) (nr.height ())) + 1)
                    | _ -> ur
                else
                    ur
            | _ -> ur
        | Leaf (b, lid) -> Node(x, Leaf(rect, id), Rectangle.Union(b, rect), x.height () + 1)
        | Nil -> Leaf(rect, id)

    static member fromSeq =
        Seq.fold (fun (s: bvhTree) (id, x) -> s.insert (id, x)) Nil

    static member fromMap =
        Map.fold (fun (s: bvhTree) k v -> s.insert (k, v)) Nil

    member x.query (rect: Rectangle) act =
        match x with
        | Node (l, r, b, _) ->
            if b.Intersects rect then
                l.query rect act
                r.query rect act
            else
                ()
        | Leaf (b, lid) ->
            if b.Intersects rect then
                act lid b
            else 
                ()
        | Nil -> ()

    member x.count() =
        match x with
        | Nil -> 0
        | Leaf _ -> 1
        | Node (l, r, _, _) -> l.count () + r.count ()

    member x.height() =
        match x with
        | Node (_, _, _, h) -> h
        | Leaf _ -> 1
        | Nil -> 0
