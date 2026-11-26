-- Echo911: bridge vMenu weapon menu ↔ ox_inventory items (WEAPON_*, at_*).
-- C# talks through local events; mutations go to the server script.

local tracked = {}

local function started()
    return GetResourceState('ox_inventory') == 'started'
end

local function countOf(item)
    if not started() or type(item) ~= 'string' or item == '' then
        return 0
    end

    local count = exports.ox_inventory:Search('count', item)
    return tonumber(count) or 0
end

local function fittedComponents(weaponItem)
    if not started() then
        return {}
    end

    local slots = exports.ox_inventory:Search('slots', weaponItem)
    if type(slots) ~= 'table' then
        return {}
    end

    for _, slot in pairs(slots) do
        if type(slot) == 'table' and type(slot.metadata) == 'table' and type(slot.metadata.components) == 'table' then
            return slot.metadata.components
        end
    end

    return {}
end

local function sync()
    local payload = {
        available = started(),
        items = {},
        fitted = {},
    }

    if payload.available then
        for item in pairs(tracked) do
            payload.items[item] = countOf(item)
            if item:sub(1, 7) == 'WEAPON_' then
                payload.fitted[item] = fittedComponents(item)
            end
        end
    end

    TriggerEvent('vMenu:oxInv:sync', json.encode(payload))
end

AddEventHandler('vMenu:oxInv:track', function(itemsJson)
    tracked = {}
    local list = json.decode(itemsJson or '[]')
    if type(list) == 'table' then
        for i = 1, #list do
            local item = list[i]
            if type(item) == 'string' and item ~= '' then
                tracked[item] = true
            end
        end
    end
    sync()
end)

AddEventHandler('vMenu:oxInv:requestSync', function()
    sync()
end)

AddEventHandler('vMenu:oxInv:giveWeapon', function(item, ammo, componentsJson)
    if type(item) ~= 'string' then return end
    tracked[item] = true
    TriggerServerEvent('vMenu:oxInv:giveWeapon', item, tonumber(ammo) or 250, componentsJson or '[]')
end)

AddEventHandler('vMenu:oxInv:removeWeapon', function(item)
    if type(item) ~= 'string' then return end
    TriggerServerEvent('vMenu:oxInv:removeWeapon', item)
end)

AddEventHandler('vMenu:oxInv:refillWeapon', function(item, ammo)
    if type(item) ~= 'string' then return end
    TriggerServerEvent('vMenu:oxInv:refillWeapon', item, tonumber(ammo) or 250)
end)

AddEventHandler('vMenu:oxInv:toggleComponent', function(weaponItem, componentItem, attach)
    if type(weaponItem) ~= 'string' or type(componentItem) ~= 'string' then return end
    tracked[weaponItem] = true
    tracked[componentItem] = true
    TriggerServerEvent('vMenu:oxInv:toggleComponent', weaponItem, componentItem, attach and true or false)
end)

AddEventHandler('vMenu:oxInv:giveComponentItem', function(componentItem)
    if type(componentItem) ~= 'string' then return end
    tracked[componentItem] = true
    TriggerServerEvent('vMenu:oxInv:giveComponentItem', componentItem)
end)

AddEventHandler('vMenu:oxInv:removeAllWeapons', function(itemsJson)
    TriggerServerEvent('vMenu:oxInv:removeAllWeapons', itemsJson)
end)

AddEventHandler('vMenu:oxInv:giveAllWeapons', function(itemsJson)
    TriggerServerEvent('vMenu:oxInv:giveAllWeapons', itemsJson)
end)

RegisterNetEvent('vMenu:oxInv:serverDone', function()
    sync()
end)

AddEventHandler('ox_inventory:updateInventory', function()
    sync()
end)

CreateThread(function()
    while GetResourceState('ox_inventory') ~= 'started' do
        Wait(500)
    end
    Wait(250)
    sync()
end)
