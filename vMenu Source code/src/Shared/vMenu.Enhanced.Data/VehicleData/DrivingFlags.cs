namespace vMenu.Enhanced.Data.VehicleData;

// One flag bit, or one ready made combination of them. Name is the identifier Rockstar's own script
// headers use, which is what gets shown as the row description, and Label is the short human title.
//
// A class, not a record: the sandbox refuses the generated EqualityComparer<T>.Default.
public sealed class DrivingFlag(string name, string label, int value)
{
    public string Name { get; } = name;

    public string Label { get; } = label;

    public int Value { get; } = value;
}

// The three flag bitfields the vehicle task natives take, transcribed from commands_vehicle.sch in
// the leaked script source. Values are int rather than uint because every native takes an int, and
// the top bit does not fit in a positive one.
public static class DrivingFlags
{
    // DRIVINGMODE. Taken by nearly every driving task, and by the boat and plane tasks for the part
    // of the job that happens on the ground. Bits 15, 16, 17, 20, 26, 27, 28 and 31 are not named in
    // the script headers, so those names come from the old vMenu instead.
    public static readonly IReadOnlyList<DrivingFlag> Driving =
    [
        /* 01 */ new("DF_StopForCars", "遇车停车", 1),
        /* 02 */ new("DF_StopForPeds", "遇行人停车", 2),
        /* 03 */ new("DF_SwerveAroundAllCars", "绕开所有车辆", 4),
        /* 04 */ new("DF_SteerAroundStationaryCars", "绕开停放车辆", 8),
        /* 05 */ new("DF_SteerAroundPeds", "绕开行人", 16),
        /* 06 */ new("DF_SteerAroundObjects", "绕开物体", 32),
        /* 07 */ new("DF_DontSteerAroundPlayerPed", "不绕开玩家", 64),
        /* 08 */ new("DF_StopAtLights", "红灯停车", 128),
        /* 09 */ new("DF_GoOffRoadWhenAvoiding", "避障时可驶离道路", 256),
        /* 10 */ new("DF_DriveIntoOncomingTraffic", "可驶入对向车道", 512),
        /* 11 */ new("DF_DriveInReverse", "倒车行驶", 1024),
        /* 12 */ new("DF_UseWanderFallbackInsteadOfStraightLine", "无路线时游荡", 2048),
        /* 13 */ new("DF_AvoidRestrictedAreas", "避开禁区", 4096),
        /* 14 */ new("DF_PreventBackgroundPathfinding", "禁止后台寻路", 8192),
        /* 15 */ new("DF_AdjustCruiseSpeedBasedOnRoadSpeed", "按道路限速巡航", 16384),
        /* 16 */ new("DF_PreventJoinInRoadDirectionWhenMoving", "移动时不汇入道路方向", 32768),
        /* 17 */ new("DF_DontAvoidTarget", "不避开目标", 65536),
        /* 18 */ new("DF_TargetPositionOverridesEntity", "目标坐标优先于实体", 131072),
        /* 19 */ new("DF_UseShortCutLinks", "使用捷径节点", 262144),
        /* 20 */ new("DF_ChangeLanesAroundObstructions", "遇障变道", 524288),
        /* 21 */ new("DF_AvoidTargetCoors", "远离目标坐标", 1048576),
        /* 22 */ new("DF_UseSwitchedOffNodes", "使用已关闭节点", 2097152),
        /* 23 */ new("DF_PreferNavmeshRoute", "优先导航网格路线", 4194304),
        /* 24 */ new("DF_PlaneTaxiMode", "飞机滑行模式", 8388608),
        /* 25 */ new("DF_ForceStraightLine", "强制直线", 16777216),
        /* 26 */ new("DF_UseStringPullingAtJunctions", "路口平滑转弯", 33554432),
        /* 27 */ new("DF_AvoidAdverseConditions", "避开恶劣路况", 67108864),
        /* 28 */ new("DF_AvoidTurns", "尽量少转弯", 134217728),
        /* 29 */ new("DF_ExtendRouteWithWanderResults", "用游荡延长路线", 268435456),
        /* 30 */ new("DF_AvoidHighways", "避开高速公路", 536870912),
        /* 31 */ new("DF_ForceJoinInRoadDirection", "强制汇入道路方向", 1073741824),
        /* 32 */ new("DF_DontTerminateTaskWhenAchieved", "到达后不结束任务", int.MinValue),
    ];

    // BOATMODE, taken only by TASK_BOAT_MISSION.
    public static readonly IReadOnlyList<DrivingFlag> Boat =
    [
        new("BCF_StopAtEnd", "终点停车", 1),
        new("BCF_StopAtShore", "靠岸停车", 2),
        new("BCF_AvoidShore", "避开岸边", 4),
        new("BCF_PreferForward", "优先向前", 8),
        new("BCF_NeverStop", "永不停车", 16),
        new("BCF_NeverNavMesh", "不使用导航网格", 32),
        new("BCF_NeverRoute", "不使用路线", 64),
        new("BCF_ForceBeached", "强制冲滩", 128),
        new("BCF_UseWanderRoute", "使用游荡路线", 256),
        new("BCF_UseFleeRoute", "使用逃离路线", 512),
        new("BCF_NeverPause", "永不暂停", 1024),
    ];

    // HELIMODE, taken by TASK_HELI_MISSION and TASK_VEHICLE_HELI_PROTECT.
    public static readonly IReadOnlyList<DrivingFlag> Heli =
    [
        new("HF_AttainRequestedOrientation", "转向指定朝向", 1),
        new("HF_DontModifyOrientation", "不改变朝向", 2),
        new("HF_DontModifyPitch", "不改变俯仰", 4),
        new("HF_DontModifyThrottle", "不改变油门", 8),
        new("HF_DontModifyRoll", "不改变横滚", 16),
        new("HF_LandOnArrival", "到达后降落", 32),
        new("HF_DontDoAvoidance", "不做任何避障", 64),
        new("HF_StartEngineImmediately", "立即启动引擎", 128),
        new("HF_ForceHeightMapAvoidance", "强制高度图避障", 256),
        new("HF_DontClampProbesToDestination", "探测不锁定目标", 512),
        new("HF_EnableTimeslicingWhenPossible", "允许分时处理", 1024),
        new("HF_CircleOppositeDirection", "反向盘旋", 2048),
        new("HF_MaintainHeightAboveTerrain", "保持离地高度", 4096),
        new("HF_IgnoreHiddenEntitiesDuringLand", "降落时忽略隐藏实体", 8192),
        new("HF_DisableAllHeightMapAvoidance", "关闭全部高度图避障", 16384),
    ];

    // The eight combinations the game's own missions reuse, with the values straight out of the
    // script headers rather than recomputed, so they match what Rockstar actually shipped.
    public static readonly IReadOnlyList<DrivingFlag> DrivingPresets =
    [
        new("DRIVINGMODE_STOPFORCARS", "正常", 786603),
        new("DRIVINGMODE_STOPFORCARS_STRICT", "正常（绝不偏离）", 262275),
        new("DRIVINGMODE_STOPFORCARS_IGNORELIGHTS", "正常（闯红灯）", 786475),
        new("DRIVINGMODE_AVOIDCARS", "警觉", 786469),
        new("DRIVINGMODE_AVOIDCARS_RECKLESS", "鲁莽", 786468),
        new("DRIVINGMODE_AVOIDCARS_OBEYLIGHTS", "警觉（遵守信号灯）", 786597),
        new("DRIVINGMODE_AVOIDCARS_STOPFORPEDS_OBEYLIGHTS", "警觉（礼让行人）", 786599),
        new("DRIVINGMODE_PLOUGHTHROUGH", "横冲直撞", 262144),
    ];

    public static readonly IReadOnlyList<DrivingFlag> BoatPresets =
    [
        new("BCF_DEFAULTSETTINGS", "默认", 7),
        new("BCF_OPENOCEANSETTINGS", "开阔海域", 111),
        new("BCF_BOATTAXISETTINGS", "船只载客", 1071),
    ];

    // Combinations rather than bits of their own, which is why they sit here and not in the Heli list.
    // Only the first two are the game's own: HF_HEIGHTMAPONLYAVOIDANCE is HF_DontDoAvoidance |
    // HF_ForceHeightMapAvoidance. The third is ours, because a helicopter that starts its engine and
    // keeps its distance from the ground is what almost everybody actually wants.
    public static readonly IReadOnlyList<DrivingFlag> HeliPresets =
    [
        new("HF_NONE", "无标志", 0),
        new("HF_HEIGHTMAPONLYAVOIDANCE", "仅高度图避障", 320),

        // HF_StartEngineImmediately | HF_MaintainHeightAboveTerrain | HF_AttainRequestedOrientation.
        new("HF_StartEngineImmediately | HF_MaintainHeightAboveTerrain | HF_AttainRequestedOrientation", "平稳飞行", 4225),
    ];
}
