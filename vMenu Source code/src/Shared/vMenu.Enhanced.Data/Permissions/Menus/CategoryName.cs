using System.Text;

namespace vMenu.Enhanced.Data.Permissions.Menus;

// Turns a category name a server owner typed into something usable as a permission segment. Used by
// the vehicle spawner and the ped models menu alike.
public static class CategoryName
{
    // Display titles (Chinese or English) mapped to the stable ASCII segments the permission system
    // and permissions.cfg.example expect. Keys are compared case-insensitively for English.
    private static readonly Dictionary<string, string> KnownSegments =
        new(StringComparer.OrdinalIgnoreCase)
        {
            // Ped models — Chinese display → original English slug
            ["动物"] = "animals",
            ["女性"] = "female",
            ["主要"] = "main",
            ["男性"] = "male",
            ["警察与治安官"] = "police_sheriff",
            ["应急服务"] = "emergency_services",
            ["军方与机构"] = "military_agencies",
            ["安保与警卫"] = "security_guards",
            ["帮派"] = "gangs",
            ["罪犯与囚犯"] = "criminals_prisoners",
            ["工人与行业"] = "workers_trades",
            ["商店与服务业"] = "shops_hospitality",
            ["夜生活与娱乐"] = "nightlife_entertainment",
            ["DJ与音乐人"] = "djs_musicians",
            ["僵尸与怪物"] = "zombies_monsters",
            ["怪咖与服装"] = "oddballs_costumes",
            ["尸体"] = "corpses",
            ["Rockstar客串"] = "rockstar_cameos",
            ["独特角色"] = "unique_characters",
            ["剧情角色（A - C）"] = "story_characters_a_c",
            ["剧情角色（D - J）"] = "story_characters_d_j",
            ["剧情角色（K - M）"] = "story_characters_k_m",
            ["剧情角色（N - R）"] = "story_characters_n_r",
            ["剧情角色（S - Z）"] = "story_characters_s_z",
            ["过场角色（A - J）"] = "cutscene_characters_a_j",
            ["过场角色（K - Z）"] = "cutscene_characters_k_z",

            // Ped models — English originals (same slug the ASCII path would produce)
            ["Animals"] = "animals",
            ["Female"] = "female",
            ["Main"] = "main",
            ["Male"] = "male",
            ["Police & Sheriff"] = "police_sheriff",
            ["Emergency Services"] = "emergency_services",
            ["Military & Agencies"] = "military_agencies",
            ["Security & Guards"] = "security_guards",
            ["Gangs"] = "gangs",
            ["Criminals & Prisoners"] = "criminals_prisoners",
            ["Workers & Trades"] = "workers_trades",
            ["Shops & Hospitality"] = "shops_hospitality",
            ["Nightlife & Entertainment"] = "nightlife_entertainment",
            ["DJs & Musicians"] = "djs_musicians",
            ["Zombies & Monsters"] = "zombies_monsters",
            ["Oddballs & Costumes"] = "oddballs_costumes",
            ["Corpses"] = "corpses",
            ["Rockstar Cameos"] = "rockstar_cameos",
            ["Unique Characters"] = "unique_characters",
            ["Story Characters (A - C)"] = "story_characters_a_c",
            ["Story Characters (D - J)"] = "story_characters_d_j",
            ["Story Characters (K - M)"] = "story_characters_k_m",
            ["Story Characters (N - R)"] = "story_characters_n_r",
            ["Story Characters (S - Z)"] = "story_characters_s_z",
            ["Cutscene Characters (A - J)"] = "cutscene_characters_a_j",
            ["Cutscene Characters (K - Z)"] = "cutscene_characters_k_z",

            // Weapons — Chinese display
            ["手枪"] = "handguns",
            ["突击步枪"] = "assault_rifles",
            ["霰弹枪"] = "shotguns",
            ["冲锋枪/轻机枪"] = "sub_light_machine_guns",
            ["投掷物"] = "throwables",
            ["近战武器"] = "melee",
            ["重型武器"] = "heavy_weapons",
            ["狙击步枪"] = "sniper_rifles",

            // Weapons — English originals
            ["Handguns"] = "handguns",
            ["Assault Rifles"] = "assault_rifles",
            ["Shotguns"] = "shotguns",
            ["Sub-/Light Machine Guns"] = "sub_light_machine_guns",
            ["Throwables"] = "throwables",
            ["Melee"] = "melee",
            ["Heavy Weapons"] = "heavy_weapons",
            ["Sniper Rifles"] = "sniper_rifles",
        };

    /// <summary>Lowercased ASCII segment. Empty only when the name was blank. Non-ASCII display
    /// names (e.g. Chinese category titles) are mapped to stable English slugs so ACE permissions
    /// stay valid; anything unknown falls back to an ASCII slug or a short hash.</summary>
    public static string ToPermissionSegment(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return string.Empty;
        }

        var trimmed = name.Trim();

        if (KnownSegments.TryGetValue(trimmed, out var known))
        {
            return known;
        }

        var ascii = AsciiSlug(trimmed);

        if (ascii.Length > 0)
        {
            return ascii;
        }

        // Pure non-ASCII with no known mapping: still produce a valid, stable segment.
        return "c" + StableHash(trimmed);
    }

    private static string AsciiSlug(string name)
    {
        var segment = new StringBuilder(name.Length);

        foreach (var character in name)
        {
            if (char.IsAsciiLetterOrDigit(character))
            {
                segment.Append(char.ToLowerInvariant(character));
                continue;
            }

            if (segment.Length > 0 && segment[^1] != '_')
            {
                segment.Append('_');
            }
        }

        while (segment.Length > 0 && segment[^1] == '_')
        {
            segment.Length--;
        }

        return segment.ToString();
    }

    // FNV-1a over UTF-8 — no crypto APIs, works on net462 and net10 alike.
    private static string StableHash(string name)
    {
        unchecked
        {
            uint hash = 2166136261;

            foreach (var b in Encoding.UTF8.GetBytes(name))
            {
                hash ^= b;
                hash *= 16777619;
            }

            return hash.ToString("x8");
        }
    }
}
