using BepInEx;
using BepInEx.Bootstrap;
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
using System.Reflection;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using UnityEngine;
using UnityEngine.TextCore;
using UnityEngine.SceneManagement;
using BlessingsVanir.Configs;
using BlessingsVanir.HarmonyPatches;
using BlessingsVanir.ReflectiveHooks;
using static Humanoid;
using static ItemDrop;
using static ItemSets;
using Logger = Jotunn.Logger;
using Object = UnityEngine.Object;

namespace BlessingsVanir
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    [BepInDependency("randyknapp.mods.epicloot", BepInDependency.DependencyFlags.SoftDependency)]
    //[NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    internal class BlessingsVanir : BaseUnityPlugin
    {
        public const string PluginGUID = "nihilhalla.mods.BlessingsVanir";
        public const string PluginName = "BlessingsVanir";
        public const string PluginVersion = "1.4.3";
        private Harmony harmony;

        public static List<string> elderBlessedTeleportable = new List<string>();
        public static List<string> bonemassBlessedTeleportable = new List<string>();
        public static List<string> moderBlessedTeleportable = new List<string>();
        public static List<string> yagluthBlessedTeleportable = new List<string>();
        public static List<string> powerfulCreatures = new List<string>();

        public static CustomStatusEffect VanirElderBlessing;
        public static CustomStatusEffect VanirBonemassBlessing;
        public static CustomStatusEffect VanirModerBlessing;
        public static CustomStatusEffect VanirYagluthBlessing;
        public static SE_Stats VanirEliteBlessing;
        public static CustomStatusEffect VanirMinibossBlessing;
        public static CustomStatusEffect VanirAbominationBlessing;
        public static CustomStatusEffect VanirTrollBlessing;
        public static CustomStatusEffect VanirGolemBlessing;

        public static BaseUnityPlugin epicLootInstance = null;

        private static Sprite iconBuff;
        private static Sprite iconEliteBuff;

        public static BlessingsVanir instance;

        // Use this class to add your own localization to the game
        // https://valheim-modding.github.io/Jotunn/tutorials/localization.html

        //public Dictionary<string, string> defaultDict = Localization.instance.m_translations;
        public static CustomLocalization localization = LocalizationManager.Instance.GetLocalization();
        public static BaseUnityPlugin GetPluginInstance(string pluginGUID)
        {
            if (Chainloader.PluginInfos.TryGetValue(pluginGUID, out PluginInfo test))
            {
                BaseUnityPlugin pluginInstance = test.Instance;
                //Jotunn.Logger.LogInfo("We found " + test + " was installed, modifying patches.");
                return pluginInstance;
            }
            return null;
        }
        public interface IEpicLootIntegration
        {
            // Define methods or properties you want to use
            void HasActiveMagicEffect();
            Type MagicEffectType();
        }
        private void Awake()
        {

            LoadAssets();
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
            BuildCreatureList();

            if (GetPluginInstance("randyknapp.mods.epicloot") != null)
            {
                epicLootInstance = GetPluginInstance("randyknapp.mods.epicloot");
                Jotunn.Logger.LogInfo("Epic Loot was found, adding reflective patches for status effects.");
                ConfigSetup();
                AddStatusEffects();
                harmony = Harmony.CreateAndPatchAll(new HarmonyPatches.EpicLoot().GetType(), PluginGUID);
            }
            else
            {
                epicLootInstance = null;
                Jotunn.Logger.LogInfo("Epic Loot was NOT found, using original status effects");
                ConfigSetup();
                AddStatusEffects();
                harmony = Harmony.CreateAndPatchAll(new NoEpicLoot().GetType(), PluginGUID);
            }



            // Jotunn comes with its own Logger class to provide a consistent Log style for all mods using it
            Jotunn.Logger.LogInfo("The Vanir are speaking, we hear their wisdom");

            // To learn more about Jotunn's features, go to
            // https://valheim-modding.github.io/Jotunn/tutorials/overview.html
        }
        private void Update()
        {
            //TestCode();
        }
        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }
        private void LoadAssets()
        {
            string modPath = Path.GetDirectoryName(Info.Location);
            iconBuff = AssetUtils.LoadSpriteFromFile(Path.Combine(modPath, "Assets/Resources/vanirbuff.png"));
            iconEliteBuff = AssetUtils.LoadSpriteFromFile(Path.Combine(modPath, "Assets/Resources/vanirelitebuff.png"));
        }
        private void ConfigSetup()
        {
            BaseConfig.InitializeConfigs();
        }
        private void AddStatusEffects()
        {
            if (iconBuff != null && iconEliteBuff != null)
            {
                Logger.LogInfo("Found the sprites for Vanir buffs");
            }
            else
            {
                Logger.LogError("Failed to load the icon sprites for Vanir buffs.");
            }
            AddElderVanirBuff();
            AddBonemassVanirBuff();
            AddModerVanirBuff();
            AddYagluthVanirBuff();
            AddEliteVanirBuff();
            if (epicLootInstance != null)
            {
                AddMinibossVanirBuff();
            }
            else
            {
                AddAbominationVanirBuff();
                AddTrollVanirBuff();
                AddGolemVanirBuff();
            }


        }

        private void BuildCreatureList()
        {
            powerfulCreatures.Add("$enemy_troll");
            powerfulCreatures.Add("$enemy_abomination");
            powerfulCreatures.Add("$enemy_stonegolem");
            powerfulCreatures.Add("$enemy_goblinbrute");
            powerfulCreatures.Add("$enemy_gjall");
        }
        private void AddAbominationVanirBuff()
        {
            float readAbominationValue = BaseConfig.AbominationDuration.Value;
            SE_Stats VanirBlessedAbomination = ScriptableObject.CreateInstance<SE_Stats>();
            VanirBlessedAbomination.name = "VanirAbomination";
            VanirBlessedAbomination.m_name = "$abominationvanir_effectname";
            VanirBlessedAbomination.m_startMessage = "$abominationvanir_effectstart";
            VanirBlessedAbomination.m_tooltip = "$abominationvanir_desc";
            //VanirBlessedAbomination.m_stopMessage = "$elitevanir_effectstop";
            VanirBlessedAbomination.m_startMessageType = MessageHud.MessageType.Center;
            //VanirBlessedAbomination.m_stopMessageType = MessageHud.MessageType.Center;
            VanirBlessedAbomination.m_icon = iconEliteBuff;
            VanirBlessedAbomination.m_ttl = readAbominationValue;

            VanirBlessedAbomination.m_addMaxCarryWeight = BaseConfig.VanirWeightBuffAmount.Value;

            VanirAbominationBlessing = new CustomStatusEffect(VanirBlessedAbomination, fixReference: false);
            ItemManager.Instance.AddStatusEffect(VanirAbominationBlessing);
        }
        private void AddGolemVanirBuff()
        {
            float readGolemValue = BaseConfig.GolemDuration.Value;
            SE_Stats VanirBlessedGolem = ScriptableObject.CreateInstance<SE_Stats>();
            VanirBlessedGolem.name = "VanirAbomination";
            VanirBlessedGolem.m_name = "$abominationvanir_effectname";
            VanirBlessedGolem.m_startMessage = "$abominationvanir_effectstart";
            VanirBlessedGolem.m_tooltip = "$abominationvanir_desc";
            //VanirBlessedAbomination.m_stopMessage = "$elitevanir_effectstop";
            VanirBlessedGolem.m_startMessageType = MessageHud.MessageType.Center;
            //VanirBlessedAbomination.m_stopMessageType = MessageHud.MessageType.Center;
            VanirBlessedGolem.m_icon = iconEliteBuff;
            VanirBlessedGolem.m_ttl = readGolemValue;

            VanirBlessedGolem.m_addMaxCarryWeight = BaseConfig.VanirWeightBuffAmount.Value;

            VanirGolemBlessing = new CustomStatusEffect(VanirBlessedGolem, fixReference: false);
            ItemManager.Instance.AddStatusEffect(VanirGolemBlessing);
        }
        private void AddTrollVanirBuff()
        {
            float readTrollValue = BaseConfig.TrollDuration.Value;
            SE_Stats VanirBlessedTroll = ScriptableObject.CreateInstance<SE_Stats>();
            VanirBlessedTroll.name = "VanirAbomination";
            VanirBlessedTroll.m_name = "$abominationvanir_effectname";
            VanirBlessedTroll.m_startMessage = "$abominationvanir_effectstart";
            VanirBlessedTroll.m_tooltip = "$abominationvanir_desc";
            //VanirBlessedAbomination.m_stopMessage = "$elitevanir_effectstop";
            VanirBlessedTroll.m_startMessageType = MessageHud.MessageType.Center;
            //VanirBlessedAbomination.m_stopMessageType = MessageHud.MessageType.Center;
            VanirBlessedTroll.m_icon = iconEliteBuff;
            VanirBlessedTroll.m_ttl = readTrollValue;

            VanirBlessedTroll.m_addMaxCarryWeight = BaseConfig.VanirWeightBuffAmount.Value;

            VanirTrollBlessing = new CustomStatusEffect(VanirBlessedTroll, fixReference: false);
            ItemManager.Instance.AddStatusEffect(VanirTrollBlessing);
        }
        private void AddMinibossVanirBuff()
        {
            float readMinibossValue = BaseConfig.MinibossDuration.Value;
            SE_Stats VanirBlessedMiniboss = ScriptableObject.CreateInstance<SE_Stats>();
            VanirBlessedMiniboss.name = "VanirMiniboss";
            VanirBlessedMiniboss.m_name = "$minibossvanir_effectname";
            VanirBlessedMiniboss.m_startMessage = "$minibossvanir_effectstart";
            VanirBlessedMiniboss.m_tooltip = "$minibossvanir_desc";
            //VanirBlessedMiniboss.m_stopMessage = "$minibossvanir_effectstop";
            VanirBlessedMiniboss.m_startMessageType = MessageHud.MessageType.Center;
            //VanirBlessedMiniboss.m_stopMessageType = MessageHud.MessageType.Center;

            VanirBlessedMiniboss.m_icon = iconEliteBuff;
            VanirBlessedMiniboss.m_ttl = readMinibossValue;
            
            VanirBlessedMiniboss.m_addMaxCarryWeight = BaseConfig.VanirWeightBuffAmount.Value;

            VanirMinibossBlessing = new CustomStatusEffect(VanirBlessedMiniboss, fixReference: false);
            ItemManager.Instance.AddStatusEffect(VanirMinibossBlessing);
        }
        private void AddEliteVanirBuff()
        {
            float readEliteValue = BaseConfig.EliteDuration.Value;
            SE_Stats VanirBlessedElite = ScriptableObject.CreateInstance<SE_Stats>();
            VanirBlessedElite.name = "VanirElite";
            VanirBlessedElite.m_name = "$elitevanir_effectname";
            VanirBlessedElite.m_startMessage = "$elitevanir_effectstart";
            VanirBlessedElite.m_tooltip = "$elitevanir_desc";
            //VanirBlessedElite.m_stopMessage = "$elitevanir_effectstop";
            VanirBlessedElite.m_startMessageType = MessageHud.MessageType.Center;
            //VanirBlessedElite.m_stopMessageType = MessageHud.MessageType.Center;
            VanirBlessedElite.m_icon = iconEliteBuff;
            VanirBlessedElite.m_ttl = readEliteValue;

            VanirBlessedElite.m_staminaRegenMultiplier = BaseConfig.VanirEliteStaminaRegen.Value;
            VanirBlessedElite.m_healthOverTime = BaseConfig.VanirEliteHealthRegen.Value;
            VanirBlessedElite.m_healthOverTimeDuration = readEliteValue;
            VanirBlessedElite.m_healthOverTimeInterval = 1f;
            VanirBlessedElite.m_healthOverTimeTickHP = 1f;

            CustomStatusEffect VanirEliteBlessing = new CustomStatusEffect(VanirBlessedElite, fixReference: false);
            ItemManager.Instance.AddStatusEffect(VanirEliteBlessing);
        }

        private void AddElderVanirBuff()
        {

            // add effect
            float readElderValue = BaseConfig.ElderDuration.Value;
            SE_Stats VanirBlessedElder = ScriptableObject.CreateInstance<SE_Stats>();
            VanirBlessedElder.name = "VanirElder";
            VanirBlessedElder.m_name = "$eldervanir_effectname";
            VanirBlessedElder.m_startMessage = "$eldervanir_effectstart";
            VanirBlessedElder.m_stopMessage = "$eldervanir_effectstop";
            VanirBlessedElder.m_startMessageType = MessageHud.MessageType.Center;
            VanirBlessedElder.m_stopMessageType = MessageHud.MessageType.Center;
            VanirBlessedElder.m_icon = iconBuff;
            VanirBlessedElder.m_ttl = readElderValue;
            VanirBlessedElder.m_tooltip = "$eldervanir_desc";

            VanirBlessedElder.m_addMaxCarryWeight = BaseConfig.VanirWeightBuffAmount.Value;


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
            float readBonemassValue = BaseConfig.BonemassDuration.Value;
            // add effect
            SE_Stats VanirBlessedBonemass = ScriptableObject.CreateInstance<SE_Stats>();
            VanirBlessedBonemass.name = "VanirBonemass";
            VanirBlessedBonemass.m_name = "$bonemassvanir_effectname";
            VanirBlessedBonemass.m_startMessage = "$bonemassvanir_effectstart";
            VanirBlessedBonemass.m_stopMessage = "$bonemassvanir_effectstop";
            VanirBlessedBonemass.m_tooltip = "$bonemassvanir_desc";
            VanirBlessedBonemass.m_startMessageType = MessageHud.MessageType.Center;
            VanirBlessedBonemass.m_stopMessageType = MessageHud.MessageType.Center;
            VanirBlessedBonemass.m_icon = iconBuff;
            VanirBlessedBonemass.m_ttl = readBonemassValue;

            VanirBlessedBonemass.m_addMaxCarryWeight = BaseConfig.VanirWeightBuffAmount.Value;


            VanirBonemassBlessing = new CustomStatusEffect(VanirBlessedBonemass, fixReference: false);  // We dont need to fix refs here, because no mocks were used
            ItemManager.Instance.AddStatusEffect(VanirBonemassBlessing);

            bonemassBlessedTeleportable.Add("$item_iron");
            bonemassBlessedTeleportable.Add("$item_ironscrap");
        }

        private void AddModerVanirBuff()
        {
            float readModerValue = BaseConfig.ModerDuration.Value;
            // add effect
            SE_Stats VanirBlessedModer = ScriptableObject.CreateInstance<SE_Stats>();
            VanirBlessedModer.name = "VanirModer";
            VanirBlessedModer.m_name = "$modervanir_effectname";
            VanirBlessedModer.m_startMessage = "$modervanir_effectstart";
            VanirBlessedModer.m_stopMessage = "$modervanir_effectstop";
            VanirBlessedModer.m_tooltip = "$modervanir_desc";
            VanirBlessedModer.m_startMessageType = MessageHud.MessageType.Center;
            VanirBlessedModer.m_stopMessageType = MessageHud.MessageType.Center;
            VanirBlessedModer.m_icon = iconBuff;
            VanirBlessedModer.m_ttl = readModerValue;

            VanirBlessedModer.m_addMaxCarryWeight = BaseConfig.VanirWeightBuffAmount.Value;

            VanirModerBlessing = new CustomStatusEffect(VanirBlessedModer, fixReference: false);  // We dont need to fix refs here, because no mocks were used
            ItemManager.Instance.AddStatusEffect(VanirModerBlessing);

            moderBlessedTeleportable.Add("$item_silver");
            moderBlessedTeleportable.Add("$item_silverore");
        }

        private void AddYagluthVanirBuff()
        {
            float readYagluthValue = BaseConfig.YagluthDuration.Value;
            // add effect
            SE_Stats VanirBlessedYagluth = ScriptableObject.CreateInstance<SE_Stats>();
            VanirBlessedYagluth.name = "VanirYagluth";
            VanirBlessedYagluth.m_name = "$yagluthvanir_effectname";
            VanirBlessedYagluth.m_startMessage = "$yagluthvanir_effectstart";
            VanirBlessedYagluth.m_stopMessage = "$yagluthvanir_effectstop";
            VanirBlessedYagluth.m_tooltip = "$yagluthvanir_desc";
            VanirBlessedYagluth.m_startMessageType = MessageHud.MessageType.Center;
            VanirBlessedYagluth.m_stopMessageType = MessageHud.MessageType.Center;
            VanirBlessedYagluth.m_icon = iconBuff;
            VanirBlessedYagluth.m_ttl = readYagluthValue;

            VanirBlessedYagluth.m_addMaxCarryWeight = BaseConfig.VanirWeightBuffAmount.Value;

            VanirYagluthBlessing = new CustomStatusEffect(VanirBlessedYagluth, fixReference: false);  // We dont need to fix refs here, because no mocks were used
            ItemManager.Instance.AddStatusEffect(VanirYagluthBlessing);

            yagluthBlessedTeleportable.Add("$item_blackmetal");
            yagluthBlessedTeleportable.Add("$item_blackmetalscrap");
        }
        public static IEnumerator<WaitForSeconds> DelayedStatusEffect(string str)
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
                if (str.Equals("elite"))
                {
                    player.GetSEMan().AddStatusEffect(VanirEliteBlessing.GetHashCode());
                }
                if (str.Equals("miniboss"))
                {
                    //Jotunn.Logger.LogInfo("Player has slain a powerful creature.");
                    if (player.m_guardianPower != null)
                    {
                        //Jotunn.Logger.LogInfo("Player has a guardian power, applying buff.");
                        float tempGPCD = player.m_guardianPowerCooldown;
                        player.m_guardianPowerCooldown = 0f;
                        player.GetSEMan().AddStatusEffect(player.m_guardianSE.NameHash(), resetTime: false);
                        player.m_guardianPowerCooldown = tempGPCD;
                    }
                    player.GetSEMan().AddStatusEffect(VanirMinibossBlessing.GetHashCode());
                }
                if (str.Equals("troll"))
                {
                    player.GetSEMan().AddStatusEffect(VanirTrollBlessing.GetHashCode());
                }
                if (str.Equals("abomination"))
                {
                    player.GetSEMan().AddStatusEffect(VanirAbominationBlessing.GetHashCode());
                }
                if (str.Equals("golem"))
                {
                    player.GetSEMan().AddStatusEffect(VanirGolemBlessing.GetHashCode());
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

 
    }
}

