/*using FlareItemMod;
using HarmonyLib;
using Unity.Netcode;

namespace FlareMod.Patches
{
    [HarmonyPatch(typeof(GameNetworkManager))]
    internal class GameNetworkManagerPatch
    {
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        public static void StartPatch(GameNetworkManager __instance)
        {
            Plugin.mls.LogInfo("Adding flare to network prefab");
            NetworkManager.Singleton.AddNetworkPrefab(Plugin.FlarePrefab);
        }

        //[HarmonyPostfix]
        //[HarmonyPatch(typeof(GameNetworkManager), "StartDisconnect")]
        //public static void StartDisconnectPatch()
        //{}
    }
}
*/