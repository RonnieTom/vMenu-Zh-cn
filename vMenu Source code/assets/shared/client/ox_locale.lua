-- Echo911: keep ox_lib UI (input dialog Confirm/Cancel, etc.) in Simplified Chinese.

local function apply()
    if GetResourceState('ox_lib') ~= 'started' then
        return
    end

    if lib and lib.setLocale then
        lib.setLocale('zh-cn')
    end
end

CreateThread(function()
    apply()
end)

AddEventHandler('onClientResourceStart', function(resource)
    if resource == 'ox_lib' or resource == GetCurrentResourceName() then
        apply()
    end
end)
