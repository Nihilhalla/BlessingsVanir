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
using System.Linq;
using System.Collections.Specialized;
using System.Reflection;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.TextCore;
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
        public const string PluginGUID = "nihilhalla.mods.BlessingsVanir";
        public const string PluginName = "BlessingsVanir";
        public const string PluginVersion = "1.4.2";
        private readonly Harmony harmony = new Harmony("Harmony.BlessingsVanir");

        public static List<string> elderBlessedTeleportable = new List<string>();
        public static List<string> bonemassBlessedTeleportable = new List<string>();
        public static List<string> moderBlessedTeleportable = new List<string>();
        public static List<string> yagluthBlessedTeleportable = new List<string>();
        public static List<string> powerfulCreatures = new List<string>();

        private static CustomStatusEffect VanirElderBlessing;
        private static CustomStatusEffect VanirBonemassBlessing;
        private static CustomStatusEffect VanirModerBlessing;
        private static CustomStatusEffect VanirYagluthBlessing;
        private static SE_Stats VanirEliteBlessing;
        private static CustomStatusEffect VanirMinibossBlessing;
        private static CustomStatusEffect VanirAbominationBlessing;

        private static ConfigFile serverConfig = new ConfigFile(Path.Combine(BepInEx.Paths.ConfigPath, "BlessingsVanir.cfg"), true);

        private static ConfigEntry<float> ElderDuration;
        private static ConfigEntry<float> BonemassDuration;
        private static ConfigEntry<float> ModerDuration;
        private static ConfigEntry<float> YagluthDuration;
        private static ConfigEntry<float> EliteDuration;
        private static ConfigEntry<float> MinibossDuration;
        //private static ConfigEntry<float> VanirMinibossStaminaCostReduction;
        private static ConfigEntry<float> VanirEliteStaminaRegen;
        private static ConfigEntry<float> VanirEliteHealthRegen;
        private static ConfigEntry<float> VanirWeightBuffAmount;

        private static Sprite iconBuff;
        private static Sprite iconEliteBuff;

        private static BlessingsVanir instance;

        // Use this class to add your own localization to the game
        // https://valheim-modding.github.io/Jotunn/tutorials/localization.html

        //public Dictionary<string, string> defaultDict = Localization.instance.m_translations;
        public static CustomLocalization localization = LocalizationManager.Instance.GetLocalization();
        public static bool PluginInstalled(string pluginGUID)
        {
            if (Chainloader.PluginInfos.TryGetValue(pluginGUID, out PluginInfo test))
            {
                Jotunn.Logger.LogInfo("We found " + test + " was installed, modifying patches.");
                return true;
            }
            return false;
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
            ConfigSetup();
            AddStatusEffects();
            /*if (PluginInstalled("randyknapp.mods.epicloot"))
            {
                Jotunn.Logger.LogInfo("EpicLoot installed, changing status effect patches to avoid clobbering.");
                //harmony.Patch(Character.OnDeath(), CharacterDeathPatch.Prefix, CharacterDeathPatch.Postfix);
            }*/
            harmony.PatchAll();



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
            AddConfigValues();
            SynchronizationManager.Instance.RegisterCustomConfig(serverConfig);
        }
        private static void UpdateConfigs(object obj, EventArgs args)
        {
            VanirElderBlessing.StatusEffect.m_ttl = ElderDuration.Value;
            VanirBonemassBlessing.StatusEffect.m_ttl = BonemassDuration.Value;
            VanirModerBlessing.StatusEffect.m_ttl = ModerDuration.Value;
            VanirYagluthBlessing.StatusEffect.m_ttl = YagluthDuration.Value;

            VanirEliteBlessing.m_staminaRegenMultiplier = VanirEliteStaminaRegen.Value;
            VanirEliteBlessing.m_healthOverTime = VanirEliteHealthRegen.Value;
            VanirEliteBlessing.m_ttl = EliteDuration.Value;
            VanirMinibossBlessing.StatusEffect.m_ttl = MinibossDuration.Value;

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
            AddMinibossVanirBuff();
            //AddAbominationVanirBuff();

        }

        private void BuildCreatureList()
        {
            powerfulCreatures.Add("$enemy_troll");
            powerfulCreatures.Add("$enemy_abomination");
            powerfulCreatures.Add("$enemy_stonegolem");
            powerfulCreatures.Add("$enemy_goblinbrute");
            powerfulCreatures.Add("$enemy_gjall");
        }
        private void AddConfigValues()
        {
            ConfigurationManagerAttributes isAdminOnly = new ConfigurationManagerAttributes { IsAdminOnly = true };

            ElderDuration = serverConfig.Bind("Server config", "ElderDuration", 300f,
                new ConfigDescription("Server side float controlling the Elder Buff Duration, Time in Seconds",
                    new AcceptableValueRange<float>(5f, 3600f), isAdminOnly));
            ElderDuration.SettingChanged += UpdateConfigs;

            BonemassDuration = serverConfig.Bind("Server config", "BonemassDuration", 300f,
                new ConfigDescription("Server side float controlling the Bonemass Buff Duration, Time in Seconds",
                    new AcceptableValueRange<float>(5f, 3600f), isAdminOnly));
            BonemassDuration.SettingChanged += UpdateConfigs;

            ModerDuration = serverConfig.Bind("Server config", "ModerDuration", 300f,
                new ConfigDescription("Server side float controlling the Moder Buff Duration, Time in Seconds",
                    new AcceptableValueRange<float>(5f, 3600f), isAdminOnly));
            ModerDuration.SettingChanged += UpdateConfigs;

            YagluthDuration = serverConfig.Bind("Server config", "YagluthDuration", 300f,
                new ConfigDescription("Server side float controlling the Yagluth Buff Duration, Time in Seconds",
                    new AcceptableValueRange<float>(5f, 3600f), isAdminOnly));
            YagluthDuration.SettingChanged += UpdateConfigs;

            EliteDuration = serverConfig.Bind("Server config", "EliteDuration", 15f,
                new ConfigDescription("Server side float controlling the 1+ Star slain Buff Duration, Time in Seconds",
                    new AcceptableValueRange<float>(5f, 600f), isAdminOnly));
            EliteDuration.SettingChanged += UpdateConfigs;

            MinibossDuration = serverConfig.Bind("Server config", "MinibossDuration", 300f,
                new ConfigDescription("Server side float controlling the slain Powerful Creature Buff Duration, Time in Seconds",
                    new AcceptableValueRange<float>(5f, 600f), isAdminOnly));
            MinibossDuration.SettingChanged += UpdateConfigs;

            /*AbominationDuration = serverConfig.Bind("Server config", "AbominationDuration", 300f,
                new ConfigDescription("Server side float controlling the slain abomination Buff Duration, Time in Seconds",
                    new AcceptableValueRange<float>(5f, 600f), isAdminOnly));
            AbominationDuration.SettingChanged += UpdateConfigs;*/

            //Buff tweaks section

            /*VanirTrollStaminaCostReduction = serverConfig.Bind("Buff tweaks", "VanirTrollStaminaCostReduction", .5f,
                new ConfigDescription("Server side float, flat value for stamina cost reduction during troll slain buff", new AcceptableValueRange<float>(0.01f, 20f), isAdminOnly));
            VanirTrollStaminaCostReduction.SettingChanged += UpdateConfigs;
            */
            VanirEliteStaminaRegen = serverConfig.Bind("Buff tweaks", "VanirEliteStaminaRegen", 1.5f,
                new ConfigDescription("Server side float, multiplier for stamina regen during elite slain buff **Requires Restart**", new AcceptableValueRange<float>(1f, 5f), isAdminOnly));
            VanirEliteStaminaRegen.SettingChanged += UpdateConfigs;

            VanirEliteHealthRegen = serverConfig.Bind("Buff tweaks", "VanirEliteHealthRegen", 50f,
                new ConfigDescription("Server side float, multiplier for health regen during elite slain buff **Requires Restart**", new AcceptableValueRange<float>(1f, 500f), isAdminOnly));
            VanirEliteHealthRegen.SettingChanged += UpdateConfigs;

            VanirWeightBuffAmount = serverConfig.Bind("Buff tweaks", "VanirWeightBuffAmount", 60f,
                new ConfigDescription("Server side float, multiplier for health regen during elite slain buff **Requires Restart**", new AcceptableValueRange<float>(10f, 500f), isAdminOnly));
            VanirWeightBuffAmount.SettingChanged += UpdateConfigs;

            /*
            VanirAbominationDamageReduce = serverConfig.Bind("Buff tweaks", "VanirAbominationDamageReduce", 0.5f,
                new ConfigDescription("Server side float, multiplier for damage reduction during abomination slain buff", new AcceptableValueRange<float>(0.01f, 1f), isAdminOnly));
            VanirAbominationDamageReduce.SettingChanged += UpdateConfigs;
            */
        }
        /*private void AddAbominationVanirBuff()
        {
            float readAbominationValue = AbominationDuration.Value;
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

            VanirBlessedAbomination.m_addMaxCarryWeight = 30f;

            VanirAbominationBlessing = new CustomStatusEffect(VanirBlessedAbomination, fixReference: false);
            ItemManager.Instance.AddStatusEffect(VanirAbominationBlessing);
        }
        private static IEnumerator<object> VanirAbominationBlessingSpecial()
        {
            foreach (Player player in Player.s_players)
            {
                while (player.GetSEMan().HaveStatusEffect(VanirAbominationBlessing.GetHashCode()))
                {
                    if (player.GetSEMan().HaveStatusEffect(Player.s_statusEffectWet))
                    {
                        player.GetSEMan().RemoveStatusEffect(Player.s_statusEffectWet, quiet: true);
                    }
                }
            }
            return null;
        }*/
        private void AddMinibossVanirBuff()
        {
            float readMinibossValue = MinibossDuration.Value;
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
            
            VanirBlessedMiniboss.m_addMaxCarryWeight = VanirWeightBuffAmount.Value;

            VanirMinibossBlessing = new CustomStatusEffect(VanirBlessedMiniboss, fixReference: false);
            ItemManager.Instance.AddStatusEffect(VanirMinibossBlessing);
        }
        private void AddEliteVanirBuff()
        {
            float readEliteValue = EliteDuration.Value;
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

            VanirBlessedElite.m_staminaRegenMultiplier = VanirEliteStaminaRegen.Value;
            VanirBlessedElite.m_healthOverTime = VanirEliteHealthRegen.Value;
            VanirBlessedElite.m_healthOverTimeDuration = readEliteValue;
            VanirBlessedElite.m_healthOverTimeInterval = 1f;
            VanirBlessedElite.m_healthOverTimeTickHP = 1f;

            CustomStatusEffect VanirEliteBlessing = new CustomStatusEffect(VanirBlessedElite, fixReference: false);
            ItemManager.Instance.AddStatusEffect(VanirEliteBlessing);
        }

        private void AddElderVanirBuff()
        {

            // add effect
            float readElderValue = ElderDuration.Value;
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

            VanirBlessedElder.m_addMaxCarryWeight = VanirWeightBuffAmount.Value;


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

            VanirBlessedBonemass.m_addMaxCarryWeight = VanirWeightBuffAmount.Value;


            VanirBonemassBlessing = new CustomStatusEffect(VanirBlessedBonemass, fixReference: false);  // We dont need to fix refs here, because no mocks were used
            ItemManager.Instance.AddStatusEffect(VanirBonemassBlessing);

            bonemassBlessedTeleportable.Add("$item_iron");
            bonemassBlessedTeleportable.Add("$item_ironscrap");
        }

        private void AddModerVanirBuff()
        {
            float readModerValue = ModerDuration.Value;
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

            VanirBlessedModer.m_addMaxCarryWeight = VanirWeightBuffAmount.Value;

            VanirModerBlessing = new CustomStatusEffect(VanirBlessedModer, fixReference: false);  // We dont need to fix refs here, because no mocks were used
            ItemManager.Instance.AddStatusEffect(VanirModerBlessing);

            moderBlessedTeleportable.Add("$item_silver");
            moderBlessedTeleportable.Add("$item_silverore");
        }

        private void AddYagluthVanirBuff()
        {
            float readYagluthValue = YagluthDuration.Value;
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

            VanirBlessedYagluth.m_addMaxCarryWeight = VanirWeightBuffAmount.Value;

            VanirYagluthBlessing = new CustomStatusEffect(VanirBlessedYagluth, fixReference: false);  // We dont need to fix refs here, because no mocks were used
            ItemManager.Instance.AddStatusEffect(VanirYagluthBlessing);

            yagluthBlessedTeleportable.Add("$item_blackmetal");
            yagluthBlessedTeleportable.Add("$item_blackmetalscrap");
        }
        private static IEnumerator<WaitForSeconds> DelayedStatusEffect(string str)
        {
            yield return new WaitForSeconds(3f);
            foreach (Player player in Player.s_players)
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
                    string gp = "GP_Eikthyr";
                    int guardianPowerHash = gp.GetStableHashCode();
                    StatusEffect guardianSE = ObjectDB.instance.GetStatusEffect(guardianPowerHash);
                    player.GetSEMan().AddStatusEffect(guardianSE.NameHash(), resetTime: true);
                    player.GetSEMan().AddStatusEffect(VanirMinibossBlessing.GetHashCode());
                }
                /*
                if (str.Equals("abomination"))
                {
                    player.GetSEMan().AddStatusEffect(VanirAbominationBlessing.GetHashCode());
                    instance.StartCoroutine(VanirAbominationBlessingSpecial());
                }
                */
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
        /*
        [HarmonyPatch(typeof(Player), nameof(Player.ApplyArmorDamageMods))]
        public static class DamageResistMod
        {
            private static Player player = Player.m_localPlayer;
            public static void Postfix(Player __instance, ref HitData.DamageModifiers mods)
            {
                var damageMods = new List<HitData.DamageModPair>();
                if (__instance.GetSEMan().HaveStatusEffect(VanirAbominationBlessing.StatusEffect.name))
                {
                    damageMods.Add(new HitData.DamageModPair() { m_type = HitData.DamageType.Pierce, m_modifier = HitData.DamageModifier.Resistant });
                    damageMods.Add(new HitData.DamageModPair() { m_type = HitData.DamageType.Slash, m_modifier = HitData.DamageModifier.Resistant });
                    damageMods.Add(new HitData.DamageModPair() { m_type = HitData.DamageType.Blunt, m_modifier = HitData.DamageModifier.Resistant });
                    damageMods.Add(new HitData.DamageModPair() { m_type = HitData.DamageType.Chop, m_modifier = HitData.DamageModifier.Resistant });
                }

                mods.Apply(damageMods);
            }
        }
        [HarmonyPatch(typeof(Character), nameof(Character.RPC_Damage))]
        public static class ModifyDamageTaken
        {
            public static void Prefix(Character __instance, HitData hit)
            {
                if (!(__instance is Player player))
                {
                    return;
                }

                hit.m_damage.m_pierce *= VanirAbominationDamageReduce.Value;
                hit.m_damage.m_slash *= VanirAbominationDamageReduce.Value;
                hit.m_damage.m_blunt *= VanirAbominationDamageReduce.Value;
                hit.m_damage.m_chop *= VanirAbominationDamageReduce.Value;


            }
        }
        */
        [HarmonyPatch(typeof(Character), nameof(Character.OnDeath))]

        public static class CharacterDeathPatch
        {

            public static void Prefix(ref Humanoid __instance)
            {
               /*if (__instance.m_name.Equals("$enemy_troll"))
                {
                    trollDied = true;
                }
                else
                {
                    trollDied = false;
                }*/
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
                if ((__instance.m_level == 2 && __instance.m_tamed.Equals(false) && UnityEngine.Random.Range(1, 7).Equals(6)) || (__instance.m_level >= 3 && __instance.m_tamed.Equals(false)))
                {
                    instance.StartCoroutine(DelayedStatusEffect("elite"));
                }
                if (powerfulCreatures.Contains(__instance.m_name))
                {
                    instance.StartCoroutine(DelayedStatusEffect("miniboss"));
                }
                /*if (__instance.m_name.Equals("$enemy_abomination"))
                {
                    instance.StartCoroutine(DelayedStatusEffect("abomination"));
                }*/
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

