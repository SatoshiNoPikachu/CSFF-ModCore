namespace ModCore.Games;

internal static class DataRegister
{
    private static readonly GameLoad GameLoad;

    static DataRegister()
    {
        GameLoad = GameLoad.Instance;
    }

    public static void OnUidObjInit(UniqueIDScriptable uo)
    {
        if (uo is NPCPerkGroup npcPerkGroup)
        {
            GameLoad.AllNPCPerkGroups.Add(npcPerkGroup);
        }
    }
}