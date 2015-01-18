namespace CardboardTaskForceReactive

module EntityModule =
    open Microsoft.Xna.Framework.Graphics
    open ReactiveGameEngine.Reactive
    open CardboardTaskForceReactive.Model
    open CameraModule
    open Microsoft.Xna.Framework
    open Microsoft.Xna.Framework.Content

    type MovementState =
        | Moving
        | Stationary

    type EntityType =
        | Soldier
        | Tank
        | Battleship

    type EntityState =
        { MovementState : MovementState
          Health : int
          EntityType : EntityType
          Position : Vector<int>
          Team : Team }

    type EntityMessage =
        | ApplyDamage of int
        | AttackEntity of Vector<int>
        | MoveTo of Vector<int>
        | Select
    
    type DyingEntitiesState =
        { Entities : Map<Vector<int>, string> }

    type DyingEntitiesMessage =
        | AddDyingEntity of Vector<int> * string
        | TrimDeadEntity of Vector<int>

    type MapMessage =
        | BuildUnit of Vector<int> * EntityType
        | MoveUnit of Vector<int> * Vector<int>
        | TrimDeadUnits
        | EndTurn

    let [<Literal>] ALIVE_DRAW_DEPTH = 0.3f
    let [<Literal>] DEAD_DRAW_DEPTH = 0.4f

    let private (|Living|Dead|) entity =
        if entity.Health < 0 then Dead else Living(entity.Health)

    let private retrieve_texture_name entity = 
        let first_part = match entity.Team with
                         | Team1 -> "blue"
                         | Team2 -> "red"
        let second_part = match entity.EntityType with
                          | Soldier -> "soldier"
                          | Tank -> "tank"
                          | Battleship -> "battleship"
        let third_part = match entity.MovementState with
                         | Moving -> "moving"
                         | Stationary -> "stationary"
        sprintf "%s_%s_%s" first_part second_part third_part

    let private apply_damage (state,context:ActorContext) damage =
        let new_health = state.Health - damage
        if new_health < 0 then 
            schedule 1000 "dying" <| TrimDeadEntity(state.Position)
            "map" <! TrimDeadUnits
            "dying" <! AddDyingEntity(state.Position, context.Path)
        { state with Health = new_health }

    let private attack_entity (state, context) vector =
        state

    let private move_to (state, context) vec =
        state

    let private select (state, context) =
        state

    let private message_handler (state:EntityState,context) (message:EntityMessage) =
        match message with
        | ApplyDamage damage -> apply_damage (state,context) damage
        | AttackEntity (Vector(x,y)) -> state
        | MoveTo (Vector(x,y)) -> state
        | Select -> state

    let private default_state team entity_type position =
        { MovementState = Stationary; Health = 100; EntityType = entity_type; Position = position; Team = team }

    let private _renderer (sprite_provider:string -> Texture2D) (spritebatch:SpriteBatch) (gametime:GameTime) (state:EntityState) =
        let tex = retrieve_texture_name state
                  |> sprite_provider
        let camera_position = ((!!"camera").Head.State :?> CameraState).TopLeftPosition
        let screen_position = state.Position - camera_position
        let render_coords = System.Nullable(screen_position |> to_float32_vector |> to_xvector)
        let draw_depth = match state with
                         | Living _ -> ALIVE_DRAW_DEPTH
                         | Dead -> DEAD_DRAW_DEPTH
        spritebatch.Draw(tex, position = render_coords, depth = draw_depth)
        ()

    let private sprite_provider (content:ContentManager) sprite_name =
        content.Load<Texture2D>(sprite_name)

    let spawn_entity (content_provider:ContentManager) address team entity_type position =
        let state = default_state team entity_type position
        reactor {
            path address
            defaultState state
            messageHandler message_handler
            renderer (_renderer (sprite_provider content_provider))
        } |> ignore