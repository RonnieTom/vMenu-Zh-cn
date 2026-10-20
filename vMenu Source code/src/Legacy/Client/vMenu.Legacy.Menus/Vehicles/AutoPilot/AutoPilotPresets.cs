using vMenu.Enhanced.Data.VehicleData;

namespace vMenu.Enhanced.Menus.Vehicles.AutoPilot;

public static class AutoPilotPresets
{
    public static IReadOnlyList<SavedDrivingProfile> For(AutoPilotCategory category) => category switch
    {
        AutoPilotCategory.Plane => Planes,
        AutoPilotCategory.Boat => Boats,
        AutoPilotCategory.Helicopter => Helicopters,
        _ => Vehicles,
    };

    public static SavedDrivingProfile Default(AutoPilotCategory category) => For(category)[0];

    public static bool IsPreset(AutoPilotCategory category, string name)
    {
        foreach (var preset in For(category))
        {
            if (string.Equals(preset.Name, name, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    public static SavedDrivingProfile? Find(AutoPilotCategory category, string name)
    {
        foreach (var preset in For(category))
        {
            if (string.Equals(preset.Name, name, StringComparison.Ordinal))
            {
                return preset;
            }
        }

        return null;
    }

    private static readonly IReadOnlyList<SavedDrivingProfile> Vehicles = FromFlags(
        AutoPilotCategory.Vehicle, DrivingFlags.DrivingPresets);

    private static readonly IReadOnlyList<SavedDrivingProfile> Boats = FromFlags(
        AutoPilotCategory.Boat, DrivingFlags.BoatPresets);

    private static readonly IReadOnlyList<SavedDrivingProfile> Helicopters = FromFlags(
        AutoPilotCategory.Helicopter, DrivingFlags.HeliPresets);

    private static readonly IReadOnlyList<SavedDrivingProfile> Planes =
    [
        new()
        {
            Name = "正常飞行",
            Description = "以合理高度巡航，并精确对准目标。",
            Category = AutoPilotCategory.Plane,
            Flags = PlaneTaxi,
            FlightHeight = 60,
            MinHeightAboveTerrain = 20,
            Precise = true,
        },
        new()
        {
            Name = "高空巡航",
            Description = "先爬升到远离地形的高度再飞向目标。",
            Category = AutoPilotCategory.Plane,
            Flags = PlaneTaxi,
            FlightHeight = 400,
            MinHeightAboveTerrain = 100,
            Precise = false,
        },
        new()
        {
            Name = "低空掠过",
            Description = "全程贴近地面飞行。",
            Category = AutoPilotCategory.Plane,
            Flags = PlaneTaxi,
            FlightHeight = 30,
            MinHeightAboveTerrain = 10,
            Precise = true,
        },
    ];

    // DF_PlaneTaxiMode | DF_UseShortCutLinks, the only driving flags that mean anything to a plane.
    private const int PlaneTaxi = 8388608 | 262144;

    private static List<SavedDrivingProfile> FromFlags(AutoPilotCategory category, IReadOnlyList<DrivingFlag> presets)
    {
        var profiles = new List<SavedDrivingProfile>(presets.Count);

        foreach (var preset in presets)
        {
            profiles.Add(new SavedDrivingProfile
            {
                Name = preset.Label,
                Description = preset.Label,
                Category = category,
                Flags = preset.Value,
            });
        }

        return profiles;
    }
}
