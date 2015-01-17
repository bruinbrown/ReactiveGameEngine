namespace CardboardTaskForceReactive

module CameraModule =
    open CardboardTaskForceReactive.Model.Vector
    open ReactiveGameEngine.Reactive
    
    type CameraState =
        { TopLeftPosition : Vector<int>
          Size : Vector<float32> }

    type CameraMessage =
        | MoveTo of Vector<int>
        | MoveToAndCentre of Vector<int>
        | MoveBy of Vector<int>

    let MAP_TILE_SIZE = 64

    let private message_handler (state,_) message =
        match message with
        | MoveTo vec -> { state with TopLeftPosition = vec }
        | MoveToAndCentre vec -> { state with TopLeftPosition = vec } //TODO: Finish logic here to centre
        | MoveBy vec -> { state with TopLeftPosition = state.TopLeftPosition + vec }

    let spawn_camera position size =
        let state = { TopLeftPosition = position; Size = size }
        reactor {
            path "camera"
            defaultState state
            messageHandler message_handler
        }
