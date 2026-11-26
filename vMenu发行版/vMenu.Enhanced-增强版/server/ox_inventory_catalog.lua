-- Echo911: build vMenu weapon categories from live ox_inventory Items().
-- Top groups: 原版武器 / 添加式武器
-- Leaf groups: ammoname | 投掷物 | 近战

local catalogJson = '[]'

-- Stock weapons from official ox_inventory data/weapons.lua (GTA / default ox set).
local VANILLA = {
  ['WEAPON_ADVANCEDRIFLE'] = true,
  ['WEAPON_APPISTOL'] = true,
  ['WEAPON_ASSAULTRIFLE'] = true,
  ['WEAPON_ASSAULTRIFLE_MK2'] = true,
  ['WEAPON_ASSAULTSHOTGUN'] = true,
  ['WEAPON_ASSAULTSMG'] = true,
  ['WEAPON_AUTOSHOTGUN'] = true,
  ['WEAPON_BALL'] = true,
  ['WEAPON_BAT'] = true,
  ['WEAPON_BATTLEAXE'] = true,
  ['WEAPON_BATTLERIFLE'] = true,
  ['WEAPON_BOTTLE'] = true,
  ['WEAPON_BULLPUPRIFLE'] = true,
  ['WEAPON_BULLPUPRIFLE_MK2'] = true,
  ['WEAPON_BULLPUPSHOTGUN'] = true,
  ['WEAPON_BZGAS'] = true,
  ['WEAPON_CANDYCANE'] = true,
  ['WEAPON_CARBINERIFLE'] = true,
  ['WEAPON_CARBINERIFLE_MK2'] = true,
  ['WEAPON_CERAMICPISTOL'] = true,
  ['WEAPON_COMBATMG'] = true,
  ['WEAPON_COMBATMG_MK2'] = true,
  ['WEAPON_COMBATPDW'] = true,
  ['WEAPON_COMBATPISTOL'] = true,
  ['WEAPON_COMBATSHOTGUN'] = true,
  ['WEAPON_COMPACTLAUNCHER'] = true,
  ['WEAPON_COMPACTRIFLE'] = true,
  ['WEAPON_CROWBAR'] = true,
  ['WEAPON_DAGGER'] = true,
  ['WEAPON_DBSHOTGUN'] = true,
  ['WEAPON_DOUBLEACTION'] = true,
  ['WEAPON_EMPLAUNCHER'] = true,
  ['WEAPON_FERTILIZERCAN'] = true,
  ['WEAPON_FIREEXTINGUISHER'] = true,
  ['WEAPON_FIREWORK'] = true,
  ['WEAPON_FLARE'] = true,
  ['WEAPON_FLAREGUN'] = true,
  ['WEAPON_FLASHLIGHT'] = true,
  ['WEAPON_GADGETPISTOL'] = true,
  ['WEAPON_GOLFCLUB'] = true,
  ['WEAPON_GRENADE'] = true,
  ['WEAPON_GRENADELAUNCHER'] = true,
  ['WEAPON_GUSENBERG'] = true,
  ['WEAPON_HAMMER'] = true,
  ['WEAPON_HATCHET'] = true,
  ['WEAPON_HAZARDCAN'] = true,
  ['WEAPON_HEAVYPISTOL'] = true,
  ['WEAPON_HEAVYRIFLE'] = true,
  ['WEAPON_HEAVYSHOTGUN'] = true,
  ['WEAPON_HEAVYSNIPER'] = true,
  ['WEAPON_HEAVYSNIPER_MK2'] = true,
  ['WEAPON_HOMINGLAUNCHER'] = true,
  ['WEAPON_KNIFE'] = true,
  ['WEAPON_KNUCKLE'] = true,
  ['WEAPON_MACHETE'] = true,
  ['WEAPON_MACHINEPISTOL'] = true,
  ['WEAPON_MARKSMANPISTOL'] = true,
  ['WEAPON_MARKSMANRIFLE'] = true,
  ['WEAPON_MARKSMANRIFLE_MK2'] = true,
  ['WEAPON_METALDETECTOR'] = true,
  ['WEAPON_MG'] = true,
  ['WEAPON_MICROSMG'] = true,
  ['WEAPON_MILITARYRIFLE'] = true,
  ['WEAPON_MINIGUN'] = true,
  ['WEAPON_MINISMG'] = true,
  ['WEAPON_MOLOTOV'] = true,
  ['WEAPON_MUSKET'] = true,
  ['WEAPON_NAVYREVOLVER'] = true,
  ['WEAPON_NIGHTSTICK'] = true,
  ['WEAPON_PETROLCAN'] = true,
  ['WEAPON_PIPEBOMB'] = true,
  ['WEAPON_PISTOL'] = true,
  ['WEAPON_PISTOL_MK2'] = true,
  ['WEAPON_PISTOL50'] = true,
  ['WEAPON_PISTOLXM3'] = true,
  ['WEAPON_POOLCUE'] = true,
  ['WEAPON_PRECISIONRIFLE'] = true,
  ['WEAPON_PROXMINE'] = true,
  ['WEAPON_PUMPSHOTGUN'] = true,
  ['WEAPON_PUMPSHOTGUN_MK2'] = true,
  ['WEAPON_RAILGUN'] = true,
  ['WEAPON_RAILGUNXM3'] = true,
  ['WEAPON_RAYCARBINE'] = true,
  ['WEAPON_RAYMINIGUN'] = true,
  ['WEAPON_RAYPISTOL'] = true,
  ['WEAPON_REVOLVER'] = true,
  ['WEAPON_REVOLVER_MK2'] = true,
  ['WEAPON_RPG'] = true,
  ['WEAPON_SAWNOFFSHOTGUN'] = true,
  ['WEAPON_SMG'] = true,
  ['WEAPON_SMG_MK2'] = true,
  ['WEAPON_SMOKEGRENADE'] = true,
  ['WEAPON_SNIPERRIFLE'] = true,
  ['WEAPON_SNOWBALL'] = true,
  ['WEAPON_SNOWLAUNCHER'] = true,
  ['WEAPON_SNSPISTOL'] = true,
  ['WEAPON_SNSPISTOL_MK2'] = true,
  ['WEAPON_SPECIALCARBINE'] = true,
  ['WEAPON_SPECIALCARBINE_MK2'] = true,
  ['WEAPON_STICKYBOMB'] = true,
  ['WEAPON_STONE_HATCHET'] = true,
  ['WEAPON_STUNGUN'] = true,
  ['WEAPON_SWITCHBLADE'] = true,
  ['WEAPON_TACTICALRIFLE'] = true,
  ['WEAPON_TEARGAS'] = true,
  ['WEAPON_TECPISTOL'] = true,
  ['WEAPON_VINTAGEPISTOL'] = true,
  ['WEAPON_WRENCH'] = true,
}

local AMMO_LABELS = {
  ['ammo-9'] = '9mm 弹药',
  ['ammo-22'] = '.22 弹药',
  ['ammo-38'] = '.38 弹药',
  ['ammo-44'] = '.44 弹药',
  ['ammo-45'] = '.45 弹药',
  ['ammo-50'] = '.50 弹药',
  ['ammo-rifle'] = '步枪弹药',
  ['ammo-rifle2'] = '步枪弹药 2',
  ['ammo-shotgun'] = '霰弹',
  ['ammo-sniper'] = '狙击弹药',
  ['ammo-heavysniper'] = '重型狙击弹药',
  ['ammo-musket'] = '火枪弹药',
  ['ammo-rocket'] = '火箭弹',
  ['ammo-grenade'] = '榴弹',
  ['ammo-flare'] = '信号弹',
  ['ammo-firework'] = '烟花弹',
  ['ammo-railgun'] = '电磁弹药',
  ['ammo-laser'] = '激光弹药',
  ['ammo-emp'] = '电磁脉冲弹药',
}

local function started()
  return GetResourceState('ox_inventory') == 'started'
end

local function isWeaponName(name)
  return type(name) == 'string' and name:sub(1, 7):upper() == 'WEAPON_'
end

local function leafFor(data, items)
  if data.throwable then
    return '投掷物'
  end

  local ammo = data.ammoname
  if type(ammo) == 'string' and ammo ~= '' then
    -- Ammo that is itself a weapon (e.g. snowballs) still counts as that ammo type.
    if AMMO_LABELS[ammo] then
      return AMMO_LABELS[ammo]
    end

    local ammoItem = items[ammo] or items[ammo:lower()]
    if ammoItem and type(ammoItem.label) == 'string' and ammoItem.label ~= '' then
      return ammoItem.label
    end

    return ammo
  end

  return '近战'
end

local function sortKeys(map)
  local keys = {}
  for key in pairs(map) do
    keys[#keys + 1] = key
  end
  table.sort(keys)
  return keys
end

local function rebuild()
  if not started() then
    catalogJson = '[]'
    TriggerEvent('vMenu:oxInv:catalogBuilt', catalogJson)
    return
  end

  local items = exports.ox_inventory:Items()
  if type(items) ~= 'table' then
    catalogJson = '[]'
    TriggerEvent('vMenu:oxInv:catalogBuilt', catalogJson)
    return
  end

  -- buckets[group][leaf] = { { spawnName, label }, ... }
  local buckets = {
    ['原版武器'] = {},
    ['添加式武器'] = {},
  }

  for name, data in pairs(items) do
    if type(data) == 'table' and isWeaponName(name) and name:upper() ~= 'WEAPON_UNARMED' then
      local itemName = name:upper()
      local group = VANILLA[itemName] and '原版武器' or '添加式武器'
      local leaf = leafFor(data, items)
      local spawn = itemName:lower()
      local label = (type(data.label) == 'string' and data.label ~= '') and data.label or itemName

      buckets[group][leaf] = buckets[group][leaf] or {}
      buckets[group][leaf][#buckets[group][leaf] + 1] = {
        spawnName = spawn,
        label = label,
      }
    end
  end

  local categories = {}
  for _, group in ipairs({ '原版武器', '添加式武器' }) do
    local leaves = buckets[group]
    for _, leaf in ipairs(sortKeys(leaves)) do
      local weapons = leaves[leaf]
      table.sort(weapons, function(a, b)
        return a.label < b.label
      end)
      categories[#categories + 1] = {
        name = leaf,
        group = group,
        weapons = weapons,
      }
    end
  end

  catalogJson = json.encode(categories)

  print(('[vMenu] ox_inventory catalog: %s categories from live Items()'):format(#categories))
  TriggerEvent('vMenu:oxInv:catalogBuilt', catalogJson)
end

exports('GetOxWeaponCatalog', function()
  return catalogJson
end)

AddEventHandler('vMenu:oxInv:rebuildCatalog', function()
  rebuild()
end)

AddEventHandler('onResourceStart', function(resource)
  if resource == 'ox_inventory' or resource == GetCurrentResourceName() then
    CreateThread(function()
      Wait(750)
      rebuild()
    end)
  end
end)

CreateThread(function()
  while not started() do
    Wait(400)
  end
  Wait(750)
  rebuild()
end)
