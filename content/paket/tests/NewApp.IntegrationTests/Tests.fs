namespace NewApp.IntegrationTests

open Expecto
open Expecto.Flip

module GetItem =
    let knownItemId = "item-001"
    let unknownItemId = "item-999"

    [<Tests>]
    let tests =
        testList
            "getItem"
            [ test "returns item for known ID" {
                  let dto: RequestDTOs.GetItem =
                      { Purpose = "IntegrationTest"
                        ItemId = knownItemId }

                  let item =
                      Workflows.getItem testContext dto
                      |> AsyncWriter.getItem
                      |> Expect.wantOk ""

                  item.ItemId |> Expect.equal "" "item-001"
                  item.Name |> Expect.equal "" "Sample Item"
              }

              test "returns error for unknown item" {
                  let dto: RequestDTOs.GetItem =
                      { Purpose = "IntegrationTest"
                        ItemId = unknownItemId }

                  Workflows.getItem testContext dto
                  |> AsyncWriter.getItem
                  |> Expect.wantError ""
                  |> _.Type
                  |> Expect.equal "" ErrorType.LambdaInvoke
              } ]
