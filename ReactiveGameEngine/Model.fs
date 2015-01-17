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

    let contains_vector (Rectangle(Vector(tlx, tly), Vector(brx, bry))) (Vector(x,y)) =
        x > tlx && x < brx && y > tly && y < bry