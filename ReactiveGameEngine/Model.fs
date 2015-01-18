namespace CardboardTaskForceReactive.Model

type Team =
    | Team1
    | Team2

[<AutoOpen>]
module Vector =
    open Microsoft.Xna.Framework

    type Vector<'a> =
        | Vector of x:'a * y:'a
        static member (+) (Vector(a1,b1), Vector(a2,b2)) =
            Vector(a1 + a2, b1 + b2)
        static member (-) (Vector(a1,b1), Vector(a2,b2)) =
            Vector(a1-a2, b1-b2)
        static member (*) (Vector(a1,b1), c) =
            Vector(a1*c, b1*c)
        static member (*) (c, Vector(a,b)) =
            Vector(a*c, b*c)
        static member (/) (Vector(a,b), c) =
            Vector(a/c, b/c)

    let map casterFunc (Vector(a,b)) =
        Vector(a |> casterFunc, b |> casterFunc)

    let to_int_vector = map int

    let to_int32_vector = map int32

    let to_float_vector = map float

    let to_float32_vector = map float32

    let to_xvector (Vector(a,b)) = Vector2(a,b)

[<AutoOpen>]
module Rectangle =

    type Rectangle<'a> =
        | Rectangle of Vector<'a> * Vector<'a>
        member this.TopLeft with get () = match this with | Rectangle(tl,_) -> tl
        member this.BottomRight with get () = match this with | Rectangle(_, br) -> br
        member this.TopRight with get () = match this with Rectangle(Vector(tlx,tly), Vector(brx, bry)) -> Vector(brx, tly)
        member this.BottomLeft with get () = match this with Rectangle(Vector(tlx, tly), Vector(brx, bry)) -> Vector(tlx, bry)

    let contains_vector (Vector(x,y)) (Rectangle(Vector(tlx, tly), Vector(brx, bry))) =
        x > tlx && x < brx && y > tly && y < bry

    let to_xrectangle (Rectangle(Vector(tlx,tly), Vector(brx, bry))) =
        Microsoft.Xna.Framework.Rectangle(tlx, tly, brx - tlx, bry - tly)