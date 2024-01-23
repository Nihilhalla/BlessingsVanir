using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using Jotunn;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Humanoid;
using static ItemDrop;
using static ItemSets;
using Logger = Jotunn.Logger;
using Object = UnityEngine.Object;

namespace BlessingsVanir
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    //[NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    internal class BlessingsVanir : BaseUnityPlugin
    {
        public const string PluginGUID = "com.jotunn.BlessingsVanir";
        public const string PluginName = "BlessingsVanir";
        public const string PluginVersion = "1.3.3";
        private readonly Harmony harmony = new Harmony("test.BlessingsVanir");

        public static List<string> elderBlessedTeleportable = new List<string>();
        public static List<string> bonemassBlessedTeleportable = new List<string>();
        public static List<string> moderBlessedTeleportable = new List<string>();
        public static List<string> yagluthBlessedTeleportable = new List<string>();

        private static CustomStatusEffect VanirElderBlessing;
        private static CustomStatusEffect VanirBonemassBlessing;
        private static CustomStatusEffect VanirModerBlessing;
        private static CustomStatusEffect VanirYagluthBlessing;

        private static ConfigFile serverConfig = new ConfigFile(Path.Combine(BepInEx.Paths.ConfigPath, "BlessingsVanir.cfg"), true);
        private static ConfigEntry<float> ElderDuration;
        private static ConfigEntry<float> BonemassDuration;
        private static ConfigEntry<float> ModerDuration;
        private static ConfigEntry<float> YagluthDuration;

        private static BlessingsVanir instance;

        // Use this class to add your own localization to the game
        // https://valheim-modding.github.io/Jotunn/tutorials/localization.html
        public static CustomLocalization Localization = LocalizationManager.Instance.GetLocalization();

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
            ConfigSetup();
            AddStatusEffects();
            AddLocalizations();
            harmony.PatchAll();

            // Jotunn comes with its own Logger class to provide a consistent Log style for all mods using it
            Jotunn.Logger.LogInfo("The Vanir are speaking, we hear their wisdom");

            // To learn more about Jotunn's features, go to
            // https://valheim-modding.github.io/Jotunn/tutorials/overview.html
        }
        private void Update()
        {

        }
        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }
        private void AddLocalizations()
        {
            /*
            Localization = new CustomLocalization();
            LocalizationManager.Instance.AddLocalization(Localization);
            //Translations and names for the effects
            */
            Localization.AddTranslation("English", new Dictionary<string, string>
            {
                {"eldervanir_effectname", "Blessing of the Vanir, Elder"},
                {"eldervanir_effectstart", "The Vanir have seen the death of The Elder"},
                {"eldervanir_effectstop", "The Vanir have stopped respecting The Elder"},
                {"bonemassvanir_effectname", "Blessing of the Vanir, Bonemass"},
                {"bonemassvanir_effectstart", "The Vanir have seen the death of Bonemass"},
                {"bonemassvanir_effectstop", "The Vanir have stopped respecting Bonemass"},
                {"modervanir_effectname", "Blessing of the Vanir, Moder"},
                {"modervanir_effectstart", "The Vanir have seen the death of Moder"},
                {"modervanir_effectstop", "The Vanir have stopped respecting Moder"},
                {"yagluthvanir_effectname", "Blessing of the Vanir, Yagluth"},
                {"yagluthvanir_effectstart", "The Vanir have seen the death of Yagluth"},
                {"yagluthvanir_effectstop", "The Vanir have stopped respecting Yagluth"}
            });
        }

        private void ConfigSetup()
        {
            AddConfigValues();
            SynchronizationManager.Instance.RegisterCustomConfig(serverConfig);
        }
        private static void UpdateElderDuration(object obj, EventArgs args)
        {
            VanirElderBlessing.StatusEffect.m_ttl = ElderDuration.Value;
        }
        private static void UpdateBonemassDuration(object obj, EventArgs args)
        {
            VanirBonemassBlessing.StatusEffect.m_ttl = BonemassDuration.Value;
        }
        private static void UpdateModerDuration(object obj, EventArgs args)
        {
            VanirModerBlessing.StatusEffect.m_ttl = ModerDuration.Value;
        }
        private static void UpdateYagluthDuration(object obj, EventArgs args)
        {
            VanirYagluthBlessing.StatusEffect.m_ttl = YagluthDuration.Value;
        }
        private void AddStatusEffects()
        {
            Sprite iconSprite = AssetUtils.LoadSpriteFromFile("BlessingsVanir/Assets/vanirbuff.png");
            if (iconSprite != null)
            {
                Logger.LogInfo("Found the Sprite for VanirBuff");
            }
            else
            {
                Logger.LogError("Failed to load the icon sprite for VanirElder buff.");
            }
            AddElderVanirBuff();
            AddBonemassVanirBuff();
            AddModerVanirBuff();
            AddYagluthVanirBuff();
        }
        private void AddConfigValues()
        {
            ConfigurationManagerAttributes isAdminOnly = new ConfigurationManagerAttributes { IsAdminOnly = true };

            ElderDuration = serverConfig.Bind("Server config", "ElderDuration", 300f,
                new ConfigDescription("Server side float controlling the Elder Buff Duration, Time in Seconds",
                    new AcceptableValueRange<float>(30f, 2400f), isAdminOnly));
            ElderDuration.SettingChanged += UpdateElderDuration;

            BonemassDuration = serverConfig.Bind("Server config", "BonemassDuration", 300f,
                new ConfigDescription("Server side float controlling the Bonemass Buff Duration, Time in Seconds",
                    new AcceptableValueRange<float>(30f, 2400f), isAdminOnly));
            BonemassDuration.SettingChanged += UpdateBonemassDuration;

            ModerDuration = serverConfig.Bind("Server config", "ModerDuration", 300f,
                new ConfigDescription("Server side float controlling the Moder Buff Duration, Time in Seconds",
                    new AcceptableValueRange<float>(30f, 2400f), isAdminOnly));
            ModerDuration.SettingChanged += UpdateModerDuration;

            YagluthDuration = serverConfig.Bind("Server config", "YagluthDuration", 300f,
                new ConfigDescription("Server side float controlling the Yagluth Buff Duration, Time in Seconds",
                    new AcceptableValueRange<float>(30f, 2400f), isAdminOnly));
            YagluthDuration.SettingChanged += UpdateYagluthDuration;


        }


        private void AddElderVanirBuff()
        {

            // add effect
            float readElderValue = ElderDuration.Value;
            StatusEffect VanirBlessedElder = ScriptableObject.CreateInstance<StatusEffect>();
            VanirBlessedElder.name = "VanirElder";
            VanirBlessedElder.m_name = "$eldervanir_effectname";
            VanirBlessedElder.m_startMessage = "$eldervanir_effectstart";
            VanirBlessedElder.m_stopMessage = "$eldervanir_effectstop";
            VanirBlessedElder.m_startMessageType = MessageHud.MessageType.Center;
            VanirBlessedElder.m_stopMessageType = MessageHud.MessageType.Center;
            VanirBlessedElder.m_icon = AssetUtils.LoadSpriteFromFile("BlessingsVanir/Assets/vanirbuff.png");
            VanirBlessedElder.m_ttl = readElderValue;



            VanirElderBlessing = new CustomStatusEffect(VanirBlessedElder, fixReference: false);  // We dont need to fix refs here, because no mocks were used
            ItemManager.Instance.AddStatusEffect(VanirElderBlessing);

            elderBlessedTeleportable.Add("$item_copper");
            elderBlessedTeleportable.Add("$item_copperore");
            elderBlessedTeleportable.Add("$item_copperscrap");
            elderBlessedTeleportable.Add("$item_tin");
            elderBlessedTeleportable.Add("$item_tinore");
            elderBlessedTeleportable.Add("$item_bronze");
        }

        private void AddBonemassVanirBuff()
        {
            float readBonemassValue = BonemassDuration.Value;
            // add effect
            StatusEffect VanirBlessedBonemass = ScriptableObject.CreateInstance<StatusEffect>();
            VanirBlessedBonemass.name = "VanirBonemass";
            VanirBlessedBonemass.m_name = "$bonemassvanir_effectname";
            VanirBlessedBonemass.m_startMessage = "$bonemassvanir_effectstart";
            VanirBlessedBonemass.m_stopMessage = "$bonemassvanir_effectstop";
            VanirBlessedBonemass.m_startMessageType = MessageHud.MessageType.Center;
            VanirBlessedBonemass.m_stopMessageType = MessageHud.MessageType.Center;
            VanirBlessedBonemass.m_icon = AssetUtils.LoadSpriteFromFile("BlessingsVanir/Assets/vanirbuff.png");
            VanirBlessedBonemass.m_ttl = readBonemassValue;

            VanirBonemassBlessing = new CustomStatusEffect(VanirBlessedBonemass, fixReference: false);  // We dont need to fix refs here, because no mocks were used
            ItemManager.Instance.AddStatusEffect(VanirBonemassBlessing);

            bonemassBlessedTeleportable.Add("$item_iron");
            bonemassBlessedTeleportable.Add("$item_ironscrap");
        }

        private void AddModerVanirBuff()
        {
            float readModerValue = ModerDuration.Value;
            // add effect
            StatusEffect VanirBlessedModer = ScriptableObject.CreateInstance<StatusEffect>();
            VanirBlessedModer.name = "VanirModer";
            VanirBlessedModer.m_name = "$modervanir_effectname";
            VanirBlessedModer.m_startMessage = "$modervanir_effectstart";
            VanirBlessedModer.m_stopMessage = "$modervanir_effectstop";
            VanirBlessedModer.m_startMessageType = MessageHud.MessageType.Center;
            VanirBlessedModer.m_stopMessageType = MessageHud.MessageType.Center;
            VanirBlessedModer.m_icon = AssetUtils.LoadSpriteFromFile("BlessingsVanir/Assets/vanirbuff.png");
            VanirBlessedModer.m_ttl = readModerValue;

            VanirModerBlessing = new CustomStatusEffect(VanirBlessedModer, fixReference: false);  // We dont need to fix refs here, because no mocks were used
            ItemManager.Instance.AddStatusEffect(VanirModerBlessing);

            moderBlessedTeleportable.Add("$item_silver");
            moderBlessedTeleportable.Add("$item_silverore");
        }

        private void AddYagluthVanirBuff()
        {
            float readYagluthValue = YagluthDuration.Value;
            // add effect
            StatusEffect VanirBlessedYagluth = ScriptableObject.CreateInstance<StatusEffect>();
            VanirBlessedYagluth.name = "VanirYagluth";
            VanirBlessedYagluth.m_name = "$yagluthvanir_effectname";
            VanirBlessedYagluth.m_startMessage = "$yagluthvanir_effectstart";
            VanirBlessedYagluth.m_stopMessage = "$yagluthvanir_effectstop";
            VanirBlessedYagluth.m_startMessageType = MessageHud.MessageType.Center;
            VanirBlessedYagluth.m_stopMessageType = MessageHud.MessageType.Center;
            VanirBlessedYagluth.m_icon = AssetUtils.LoadSpriteFromFile("BlessingsVanir/Assets/vanirbuff.png");
            VanirBlessedYagluth.m_ttl = readYagluthValue;

            VanirYagluthBlessing = new CustomStatusEffect(VanirBlessedYagluth, fixReference: false);  // We dont need to fix refs here, because no mocks were used
            ItemManager.Instance.AddStatusEffect(VanirYagluthBlessing);

            yagluthBlessedTeleportable.Add("$item_blackmetal");
            yagluthBlessedTeleportable.Add("$item_blackmetalscrap");
        }
        private static IEnumerator<WaitForSeconds> DelayedStatusEffect(string str)
        {
            yield return new WaitForSeconds(3f);
            foreach(Player player in Player.s_players)
            {
                if (str.Equals("elder")) 
                {
                    player.GetSEMan().AddStatusEffect(VanirElderBlessing.GetHashCode());
                }
                if (str.Equals("bonemass"))
                {
                    player.GetSEMan().AddStatusEffect(VanirBonemassBlessing.GetHashCode());
                }
                if (str.Equals("moder"))
                {
                    player.GetSEMan().AddStatusEffect(VanirModerBlessing.GetHashCode());
                }
                if (str.Equals("yagluth"))
                {
                    player.GetSEMan().AddStatusEffect(VanirYagluthBlessing.GetHashCode());
                }
            }
        }
        public static void SyncTeleportability(string effectName, List<string> toTeleport)
        {
            foreach (ItemDrop.ItemData item in Player.m_localPlayer.GetInventory().GetAllItems())
            {
                if (toTeleport.Contains(item.m_shared.m_name))
                {
                    item.m_shared.m_teleportable = true;
                }
            }
        }

        public static void ResetTeleportability(string effectName, List<string> toTeleport)
        {
            foreach (ItemDrop.ItemData item in Player.m_localPlayer.GetInventory().GetAllItems())
            {
                if (toTeleport.Contains(item.m_shared.m_name))
                {
                    item.m_shared.m_teleportable = false;
                }
            }

        }

        [HarmonyPatch(typeof(Character), nameof(Character.OnDeath))]

        public static class OnDeath
        {

            public static void Prefix(ref Character __instance)
            {

            }

            public static void Postfix(ref Character __instance)
            {
                if (__instance.m_name.Equals("$enemy_gdking")) 
                {
                    instance.StartCoroutine(DelayedStatusEffect("elder"));
                }
                if (__instance.m_name.Equals("$enemy_bonemass"))
                {
                    instance.StartCoroutine(DelayedStatusEffect("bonemass"));
                }
                if (__instance.m_name.Equals("$enemy_dragon"))
                {
                    instance.StartCoroutine(DelayedStatusEffect("moder"));
                }
                if (__instance.m_name.Equals("$enemy_goblinking"))
                {
                    instance.StartCoroutine(DelayedStatusEffect("yagluth"));
                }
            }
        }

        [HarmonyPatch(typeof(Player), nameof(Player.UpdateTeleport))]
        public static class Teleport
        {
            public static void Prefix()
            {
                if (Player.m_localPlayer.GetSEMan().HaveStatusEffect(VanirElderBlessing.StatusEffect.name)) 
                {
                    BlessingsVanir.SyncTeleportability("VanirElder", BlessingsVanir.elderBlessedTeleportable);
                    //Jotunn.Logger.LogInfo("The Vanir have seen the death of The Elder");
                }
                else
                {
                    BlessingsVanir.ResetTeleportability("VanirElder", BlessingsVanir.elderBlessedTeleportable);
                }
                if (Player.m_localPlayer.GetSEMan().HaveStatusEffect(VanirBonemassBlessing.StatusEffect.name))  
                {
                    BlessingsVanir.SyncTeleportability("VanirBonemass", BlessingsVanir.bonemassBlessedTeleportable);
                    //Jotunn.Logger.LogInfo("The Vanir have seen the death of Bonemass");
                }
                else
                {
                    BlessingsVanir.ResetTeleportability("VanirBonemass", BlessingsVanir.bonemassBlessedTeleportable);
                }
                if (Player.m_localPlayer.GetSEMan().HaveStatusEffect(VanirModerBlessing.StatusEffect.name))
                { 
                    BlessingsVanir.SyncTeleportability("VanirModer", BlessingsVanir.moderBlessedTeleportable);
                    //Jotunn.Logger.LogInfo("The Vanir have seen the death of Moder");
                }
                else
                {
                    BlessingsVanir.ResetTeleportability("VanirModer", BlessingsVanir.moderBlessedTeleportable);
                }
                if (Player.m_localPlayer.GetSEMan().HaveStatusEffect(VanirYagluthBlessing.StatusEffect.name))
                {
                    BlessingsVanir.SyncTeleportability("VanirYagluth", BlessingsVanir.yagluthBlessedTeleportable);
                    //Jotunn.Logger.LogInfo("The Vanir have seen the death of Yagluth");
                }
                else
                {
                    BlessingsVanir.ResetTeleportability("VanirYagluth", BlessingsVanir.yagluthBlessedTeleportable);
                }
            }
            public static void Postfix()
            {

            }
        }
    }
}

