-- Manifest data
fx_version 'cerulean'
games { 'gta5' }

-- Resource stuff
name 'vMenu Enhanced'
description 'vMenu Enhanced for regular FiveM (GTA V Legacy). Server sided trainer/menu with custom permissions. Echo911: notifications via ox_lib.'
version 'versiongoeshere'
author 'Tom Grobbe'
url 'https://github.com/TomGrobbe/vMenu/'

dependency 'ox_lib'
dependency 'ox_inventory'

ui_page 'ui/index.html'

files {
    'ui/**/*',

    -- A wildcard, so a language an owner adds is picked up without touching this file.
    'language/*.json',

    'config/tattoos.json',

    'client/*.dll',
}

shared_script '@ox_lib/init.lua'

-- Client assembly
client_script 'client/vMenu.Legacy.Core.net.dll'
client_script 'client/struct_natives.lua'
client_script 'client/ox_notify.lua'
client_script 'client/ox_input.lua'
client_script 'client/ox_locale.lua'
client_script 'client/ox_inventory_weapons.lua'

node_version '22'

-- Server assembly.
server_scripts {
    'server/net_bridge.js',
    'server/vMenu.Legacy.Core.Server.net.dll',
    'server/ox_inventory_weapons.lua',
    'server/ox_inventory_catalog.lua',
}
