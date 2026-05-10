using HarmonyLib;
using ModCore.Data;

namespace ModCore.Patcher;

[HarmonyPatch(typeof(UniqueIDScriptable))]
internal static class UniqueIDScriptablePatch
{
    [HarmonyPrefix, HarmonyPatch("SortUniqueObjectList")]
    public static bool SortUniqueObjectList_Prefix()
    {
        return Loader.IsLoaded;
    }
}

