namespace CardboardTaskForceReactive

    module private MapParser =
        open CardboardTaskForceReactive.Model
        open System.IO
        open System.Text.RegularExpressions

        type internal Metadata =
            { Name : string
              MapVersion : int
              Description : string
              Size : Vector<int> }

        type internal MapBuilding =
            { BuildingType : int
              TeamCapture : int * int }

        type internal MapEntity =
            { EntityType : int
              EntityHealth : int
              EntityTeam : int }

        type internal MapSquare =
            { SquareType : int
              Building : MapBuilding option
              Entity : MapEntity option }

        type internal MapCamera =
            { Position : int * int }

        type internal MapDefinition =
            { Metadata : Metadata
              Squares : Map<int * int, MapSquare> }

        exception private InvalidMapVersion of string

        let internal parser_v1 serialized_map =
            
            Unchecked.defaultof<MapDefinition>

        let internal parse_map  (lines: string) =
            let string_reader = new System.IO.StringReader(lines)
            let version = string_reader.ReadLine() |> int
            let data = string_reader.ReadToEnd()
            match version with
            | 1 -> parser_v1 data
            | _ -> let error_message = sprintf "Unexpected map version. Parser v%i is not supported" version
                   raise <| InvalidMapVersion(error_message)

        let internal file_based_map_parser = File.ReadAllText >> parse_map

    module MapModule =
        open Microsoft.Xna.Framework.Graphics
        open CardboardTaskForceReactive.Model.Vector
        open EntityModule
        open BuildingModule
        open MapParser
        open ReactiveGameEngine.Reactive

        type Building =
            { BuildingTexture : Texture2D }

        type Square =
            { Unit : string option
              Building : string option
              SquareTexture : Texture2D }
            member this.BuildingState
                with get () = this.Building
                              |> Option.map (fun t -> ((!!t).Head.State :?> BuildingState))
            member this.UnitState
                with get () = this.Unit
                              |> Option.map (fun t -> ((!!t).Head.State :?> EntityState))

        type MapState =
            { Map : Map<Vector<int>, Square>
              DyingUnits : string list }

        let private spawn_unit content_provider state unit_type position =
            ()

        let private convert_map_definition map =
            Unchecked.defaultof<MapState>

        let private message_handler (state, context) message =
            match message with
            | TrimDeadUnits -> let map = state.Map |> Map.map (fun k v -> match v.UnitState with
                                                                          | Some x -> if x.Health < 0 then { v with Unit = None }
                                                                                      else v
                                                                          | None -> v)
                               { state with Map = map }
            | MoveUnit (vec, target) -> state
            | BuildUnit (vec, typ) -> state

        let load_map content_provider map_file =
            let map = file_based_map_parser map_file
                      |> convert_map_definition
            reactor {
                path "map"
                defaultState map
                messageHandler message_handler
            } |> spawn