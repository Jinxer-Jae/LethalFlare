using BepInEx;
using BepInEx.Logging;
using FlareItemMod.Behaviours;
using LethalLib.Modules;
using System.IO;
using System.Reflection;
using UnityEngine;

/* NETWORKING */
//using HarmonyLib;
//using FlareMod.Patches;

namespace FlareItemMod
{
    [BepInPlugin(GUID, NAME, VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        const string GUID = "Jae.flareMod";
        const string NAME = "Flare Mod";
        const string VERSION = "0.9.0";
        public static Plugin instance;
        public static ManualLogSource mls;

        /* NETWORKING */
        //public static GameObject FlarePrefab;
        //private readonly Harmony harmony = new Harmony(GUID);

        void Awake()
        {
            /* NETWORKING */
            //NetcodeWeaver();

            /* SETUP AND INSTANTIATION */
            mls = BepInEx.Logging.Logger.CreateLogSource(GUID);
            instance = this;
            var DLLDirectoryName = Path.GetDirectoryName(this.Info.Location);
            AssetBundle bundle = AssetBundle.LoadFromFile(Path.Combine(DLLDirectoryName, "flaremod"));
            Item flare = bundle.LoadAsset<Item>("Assets/Flare/FlareItem.asset");

            /* SOLO */
            FlareItemBehaviorSolo script = flare.spawnPrefab.AddComponent<FlareItemBehaviorSolo>();

            /* NETWORKING */
            //FlareItemBehavior script = flare.spawnPrefab.AddComponent<FlareItemBehavior>();

            foreach (string assetName in bundle.GetAllAssetNames())
            {
                mls.LogInfo("Asset in Bundle: " + assetName);
            }

            /* ADDING AUDIO FILES TO THE SCRIPT */
            script.initialBurningClip = LoadAssetFromAssetBundleAndLogInfo<AudioClip>(bundle, "flarestrike");
            script.loopBurningClip = LoadAssetFromAssetBundleAndLogInfo<AudioClip>(bundle, "flareburnloop");
            script.deadClip = LoadAssetFromAssetBundleAndLogInfo<AudioClip>(bundle, "flareburnout");

            /* ENABLING PROPERTIES OF THE OBJECT */
            script.grabbable = true;
            script.grabbableToEnemies = true;
            script.itemProperties = flare;

            /* SOLO */
            NetworkPrefabs.RegisterNetworkPrefab(flare.spawnPrefab);
            Utilities.FixMixerGroups(flare.spawnPrefab);

            /* NETWORKING */
            //FlarePrefab = flare.spawnPrefab;
            //Utilities.FixMixerGroups(FlarePrefab);

            /* SCRAP AND STORE PROPERTIES */
            Items.RegisterScrap(flare, 0, Levels.LevelTypes.None);
            TerminalNode node = ScriptableObject.CreateInstance<TerminalNode>();
            node.clearPreviousText = true;
            node.displayText = "Handy when dealing with dark and foggy areas.\n\n";
            Items.RegisterShopItem(flare, null, null, node, 12);

            mls.LogInfo("Loaded Flare Mod");

            /* NETWORKING */
            //harmony.PatchAll(typeof(Plugin));
            //harmony.PatchAll(typeof(GameNetworkManagerPatch));
        }

        /* NETWORKING */
        private static void NetcodeWeaver()
        {
            var types = Assembly.GetExecutingAssembly().GetTypes();
            foreach (var type in types)
            {
                var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                foreach (var method in methods)
                {
                    var attributes = method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);
                    if (attributes.Length > 0)
                    {
                        method.Invoke(null, null);
                    }
                }
            }
        }

        private T LoadAssetFromAssetBundleAndLogInfo<T>(AssetBundle bundle, string assetName) where T : UnityEngine.Object
        {
            var loadedAsset = bundle.LoadAsset<T>(assetName);

            if (!loadedAsset)
            {
                mls.LogError($"{assetName} asset failed to load");
            }
            else
            {
                mls.LogInfo($"{assetName} asset successfully loaded");
            }

            return loadedAsset;
        }

    }
}