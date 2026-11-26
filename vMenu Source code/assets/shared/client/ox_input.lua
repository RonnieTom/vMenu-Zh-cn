-- Echo911: vMenu text prompts go through ox_lib inputDialog.
-- C# sends JSON on vMenu:oxInput; this bridge replies on vMenu:oxInputResult.

local function reply(id, values)
    TriggerEvent('vMenu:oxInputResult', json.encode({
        id = id,
        values = values,
    }))
end

local function show(payload)
    if type(payload) ~= 'string' or payload == '' then
        return
    end

    local data = json.decode(payload)
    if type(data) ~= 'table' or data.id == nil then
        return
    end

    if GetResourceState('ox_lib') ~= 'started' then
        reply(data.id, nil)
        return
    end

    CreateThread(function()
        local rows = {}
        local sourceRows = data.rows

        if type(sourceRows) ~= 'table' then
            reply(data.id, nil)
            return
        end

        for index = 1, #sourceRows do
            local row = sourceRows[index]
            local field = {
                type = 'input',
                label = row.label or ('字段 ' .. index),
                description = (row.description and row.description ~= '') and row.description or nil,
                default = (row.default and row.default ~= '') and row.default or nil,
                placeholder = (row.placeholder and row.placeholder ~= '') and row.placeholder or nil,
                max = (type(row.max) == 'number' and row.max > 0) and row.max or nil,
            }

            if type(row.options) == 'table' and #row.options > 0 then
                field.type = 'select'
                field.searchable = true
                field.clearable = true
                field.options = row.options
                field.max = nil
            end

            rows[index] = field
        end

        local result = lib.inputDialog(data.heading or 'vMenu', rows)
        if not result then
            reply(data.id, nil)
            return
        end

        local values = {}
        for index = 1, #rows do
            local value = result[index]
            if value == nil then
                values[index] = ''
            else
                values[index] = tostring(value)
            end
        end

        reply(data.id, values)
    end)
end

AddEventHandler('vMenu:oxInput', show)
