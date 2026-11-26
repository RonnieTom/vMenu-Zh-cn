-- Echo911: server-side ox_inventory mutations for the vMenu weapon menu.

local function started()
    return GetResourceState('ox_inventory') == 'started'
end

local function notify(src, message, nType)
    TriggerClientEvent('vMenu:oxNotify', src, json.encode({
        title = 'vMenu',
        description = message,
        type = nType or 'inform',
        duration = 6500,
        position = 'top-right',
    }))
end

local function done(src)
    TriggerClientEvent('vMenu:oxInv:serverDone', src)
end

local function firstWeaponSlot(src, weaponItem)
    local slots = exports.ox_inventory:Search(src, 'slots', weaponItem)
    if type(slots) ~= 'table' then
        return nil
    end

    for _, slot in pairs(slots) do
        if type(slot) == 'table' and slot.slot then
            return slot
        end
    end

    return nil
end

RegisterNetEvent('vMenu:oxInv:giveWeapon', function(item, ammo, componentsJson)
    local src = source
    if not started() or type(item) ~= 'string' then return end

    local components = {}
    local decoded = json.decode(componentsJson or '[]')
    if type(decoded) == 'table' then
        for i = 1, #decoded do
            if type(decoded[i]) == 'string' and decoded[i] ~= '' then
                components[#components + 1] = decoded[i]
            end
        end
    end

    local metadata = {
        ammo = math.max(0, math.floor(tonumber(ammo) or 250)),
        components = components,
    }

    local ok = exports.ox_inventory:AddItem(src, item, 1, metadata)
    if not ok then
        notify(src, ('无法放入 ox 库存：%s（负重或空间不足）'):format(item), 'error')
    end
    done(src)
end)

RegisterNetEvent('vMenu:oxInv:removeWeapon', function(item)
    local src = source
    if not started() or type(item) ~= 'string' then return end

    exports.ox_inventory:RemoveItem(src, item, 1)
    done(src)
end)

RegisterNetEvent('vMenu:oxInv:refillWeapon', function(item, ammo)
    local src = source
    if not started() or type(item) ~= 'string' then return end

    local slot = firstWeaponSlot(src, item)
    if not slot then
        notify(src, '库存中没有该武器物品。', 'error')
        done(src)
        return
    end

    local meta = slot.metadata or {}
    meta.ammo = math.max(0, math.floor(tonumber(ammo) or 250))
    exports.ox_inventory:SetMetadata(src, slot.slot, meta)
    done(src)
end)

RegisterNetEvent('vMenu:oxInv:toggleComponent', function(weaponItem, componentItem, attach)
    local src = source
    if not started() or type(weaponItem) ~= 'string' or type(componentItem) ~= 'string' then return end

    local slot = firstWeaponSlot(src, weaponItem)
    if not slot then
        notify(src, '请先把该武器加入 ox 库存，再安装配件。', 'error')
        done(src)
        return
    end

    local meta = slot.metadata or {}
    local components = {}
    if type(meta.components) == 'table' then
        for i = 1, #meta.components do
            components[#components + 1] = meta.components[i]
        end
    end

    local want = attach and true or false
    local found = false
    for i = #components, 1, -1 do
        if components[i] == componentItem then
            found = true
            if not want then
                table.remove(components, i)
            end
        end
    end

    if want and not found then
        components[#components + 1] = componentItem
    end

    meta.components = components
    exports.ox_inventory:SetMetadata(src, slot.slot, meta)
    done(src)
end)

RegisterNetEvent('vMenu:oxInv:giveComponentItem', function(componentItem)
    local src = source
    if not started() or type(componentItem) ~= 'string' then return end

    local ok = exports.ox_inventory:AddItem(src, componentItem, 1)
    if not ok then
        notify(src, ('无法放入配件：%s'):format(componentItem), 'error')
    end
    done(src)
end)

RegisterNetEvent('vMenu:oxInv:removeAllWeapons', function(itemsJson)
    local src = source
    if not started() then return end

    local items = json.decode(itemsJson or '[]')
    if type(items) ~= 'table' then
        done(src)
        return
    end

    for i = 1, #items do
        local item = items[i]
        if type(item) == 'string' then
            local count = exports.ox_inventory:GetItemCount(src, item) or 0
            if count > 0 then
                exports.ox_inventory:RemoveItem(src, item, count)
            end
        end
    end

    done(src)
end)

RegisterNetEvent('vMenu:oxInv:giveAllWeapons', function(itemsJson)
    local src = source
    if not started() then return end

    local items = json.decode(itemsJson or '[]')
    if type(items) ~= 'table' then
        done(src)
        return
    end

    local failed = 0
    for i = 1, #items do
        local item = items[i]
        if type(item) == 'string' then
            local ok = exports.ox_inventory:AddItem(src, item, 1, { ammo = 250, components = {} })
            if not ok then
                failed = failed + 1
            end
        end
    end

    if failed > 0 then
        notify(src, ('部分武器未能放入库存（%s 件失败）'):format(failed), 'warning')
    end
    done(src)
end)
