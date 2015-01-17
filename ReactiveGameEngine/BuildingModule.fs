namespace CardboardTaskForceReactive

module BuildingModule =
    open CardboardTaskForceReactive.Model
    open ReactiveGameEngine.Reactive
    open Microsoft.Xna.Framework.Graphics
    open Microsoft.Xna.Framework.Content
    open CameraModule

    type BuildingType =
        | HQ
        | Factory
        | Office
        | Dock
        | Airport

    type CaptureStatus =
        | Captured of Team
        | Neutral

    type BuildingState =
        { BuildingType : BuildingType
          CaptureStatus : CaptureStatus }

    type BuildingMessage =
        | CaptureBuilding of Team * int

    let [<Literal>] BUILDING_DRAW_DEPTH = 0.5f

    let private retrieve_texture_name building_type =
        match building_type with
        | HQ -> "hq"
        | Factory -> "factory"
        | Office -> "office"
        | Dock -> "dock"
        | Airport -> "airport"

    let private message_handler (state, context) message =
        let state = match message with
                    | CaptureBuilding(team, amount) -> state
        state

    let private sprite_provider (content_provider:ContentManager) sprite_name =
        content_provider.Load<Texture2D>(sprite_name)

    let private _renderer (sprite_provider:string -> Texture2D) position (spritebatch:SpriteBatch) gametime state =
        let texture = state.BuildingType
                      |> retrieve_texture_name
                      |> sprite_provider
        let camera_position = ((!!"camera").Head.State :?> CameraState).TopLeftPosition
        let screen_position = (position - camera_position) |> to_float32_vector |> to_xvector
        spritebatch.Draw(texture, position = System.Nullable(screen_position), depth = BUILDING_DRAW_DEPTH)

    let spawn_building (content_provider:ContentManager) address building_type position capture_status =
        let state = { BuildingType = building_type; CaptureStatus = Neutral }
        let sprite_provider = sprite_provider content_provider
        let _renderer = _renderer sprite_provider position
        reactor {
            path address
            defaultState state
            messageHandler message_handler
            renderer _renderer
        }