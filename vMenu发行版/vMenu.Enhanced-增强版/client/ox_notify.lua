-- Echo911: route vMenu notifications through ox_lib.
-- C# sends a JSON string on vMenu:oxNotify; this bridge calls lib.notify.

local function show(payload)
    if type(payload) ~= 'string' or payload == '' then
        return
    end

    if GetResourceState('ox_lib') ~= 'started' then
        return
    end

    local data = json.decode(payload)
    if type(data) ~= 'table' then
        return
    end

    lib.notify(data)
end

RegisterNetEvent('vMenu:oxNotify', show)
