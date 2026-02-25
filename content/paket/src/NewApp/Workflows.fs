module Workflows

open global.AsyncWriterResult


let getItem
    (appContext: AppContext.AppContext)
    : RequestDTOs.GetItem -> AWR<ResponseDTOs.GetItem>
    =
    fun dto ->
        asyncWriterResult {
            let! itemId = validateItemId dto.ItemId
            do! Write.log "ItemId" itemId

            let! item = appContext.ExternalAPI.GetItem itemId

            do! Write.log "ItemName" item.Name

            return ResponseDTOs.GetItem.ofDomain item
        }
