using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using Jotunn;
using Jotunn.Entities;
using Jotunn.Configs;
using Jotunn.Utils;
using Jotunn.Managers;
using BlessingsVanir;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace BlessingsVanir.Configs
{
    class BaseConfig
    {
        public static ConfigFile serverConfig = new ConfigFile(Path.Combine(BepInEx.Paths.ConfigPath, "BlessingsVanir.cfg"), true);

        public static ConfigEntry<float> ElderDuration;
        public static ConfigEntry<float> BonemassDuration;
        public static ConfigEntry<float> ModerDuration;
        public static ConfigEntry<float> YagluthDuration;
        public static ConfigEntry<float> EliteDuration;
        public static ConfigEntry<float> MinibossDuration;
        public static ConfigEntry<float> AbominationDuration;
        public static ConfigEntry<float> TrollDuration;
        public static ConfigEntry<float> GolemDuration;

        public static ConfigEntry<float> VanirAbominationDamageReduce;
        public static ConfigEntry<float> VanirTrollStaminaCostReduction;
        public static ConfigEntry<float> VanirEliteStaminaRegen;
        public static ConfigEntry<float> VanirEliteHealthRegen;
        public static ConfigEntry<float> VanirWeightBuffAmount;

        public static void InitializeConfigs()
        {
            SynchronizationManager.Instance.RegisterCustomConfig(serverConfig);
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

            if (BlessingsVanir.epicLootInstance != null)
            {
                MinibossDuration = serverConfig.Bind("Server config", "MinibossDuration", 300f,
                    new ConfigDescription("Server side float controlling the slain Powerful Creature Buff Duration, Time in Seconds",
                        new AcceptableValueRange<float>(5f, 600f), isAdminOnly));
                MinibossDuration.SettingChanged += UpdateConfigs;
            }
            else
            {
                TrollDuration = serverConfig.Bind("Server config", "TrollDuration", 300f,
                    new ConfigDescription("Server side float controlling the slain troll Buff Duration, Time in Seconds",
                        new AcceptableValueRange<float>(5f, 600f), isAdminOnly));
                TrollDuration.SettingChanged += UpdateConfigs;

                AbominationDuration = serverConfig.Bind("Server config", "AbominationDuration", 300f,
                    new ConfigDescription("Server side float controlling the slain abomination Buff Duration, Time in Seconds",
                        new AcceptableValueRange<float>(5f, 600f), isAdminOnly));
                AbominationDuration.SettingChanged += UpdateConfigs;

                GolemDuration = serverConfig.Bind("Server config", "GolemDuration", 300f,
                    new ConfigDescription("Server side float controlling the slain stone golem Buff Duration, Time in Seconds",
                        new AcceptableValueRange<float>(5f, 600f), isAdminOnly));
                GolemDuration.SettingChanged += UpdateConfigs;

                //Buff tweaks with epic loot

                VanirTrollStaminaCostReduction = serverConfig.Bind("Buff tweaks", "VanirTrollStaminaCostReduction", .5f,
                    new ConfigDescription("Server side float, flat value for stamina cost reduction during troll slain buff", new AcceptableValueRange<float>(0.01f, 20f), isAdminOnly));
                VanirTrollStaminaCostReduction.SettingChanged += UpdateConfigs;

                VanirAbominationDamageReduce = serverConfig.Bind("Buff tweaks", "VanirAbominationDamageReduce", 0.5f,
                    new ConfigDescription("Server side float, multiplier for damage reduction during abomination slain buff", new AcceptableValueRange<float>(0.01f, 1f), isAdminOnly));
                VanirAbominationDamageReduce.SettingChanged += UpdateConfigs;
            }
            //Buff Tweaks in all cases
            VanirEliteStaminaRegen = serverConfig.Bind("Buff tweaks", "VanirEliteStaminaRegen", 1.5f,
                   new ConfigDescription("Server side float, multiplier for stamina regen during elite slain buff **Requires Restart**", new AcceptableValueRange<float>(1f, 5f), isAdminOnly));
            VanirEliteStaminaRegen.SettingChanged += UpdateConfigs;

            VanirEliteHealthRegen = serverConfig.Bind("Buff tweaks", "VanirEliteHealthRegen", 50f,
                new ConfigDescription("Server side float, multiplier for health regen during elite slain buff **Requires Restart**", new AcceptableValueRange<float>(1f, 500f), isAdminOnly));
            VanirEliteHealthRegen.SettingChanged += UpdateConfigs;

            VanirWeightBuffAmount = serverConfig.Bind("Buff tweaks", "VanirWeightBuffAmount", 60f,
                new ConfigDescription("Server side float, multiplier for health regen during elite slain buff **Requires Restart**", new AcceptableValueRange<float>(10f, 500f), isAdminOnly));
            VanirWeightBuffAmount.SettingChanged += UpdateConfigs;





        }
        private static void UpdateConfigs(object obj, EventArgs args)
        {
            BlessingsVanir.VanirElderBlessing.StatusEffect.m_ttl = ElderDuration.Value;
            BlessingsVanir.VanirBonemassBlessing.StatusEffect.m_ttl = BonemassDuration.Value;
            BlessingsVanir.VanirModerBlessing.StatusEffect.m_ttl = ModerDuration.Value;
            BlessingsVanir.VanirYagluthBlessing.StatusEffect.m_ttl = YagluthDuration.Value;

            BlessingsVanir.VanirEliteBlessing.m_staminaRegenMultiplier = VanirEliteStaminaRegen.Value;
            BlessingsVanir.VanirEliteBlessing.m_healthOverTime = VanirEliteHealthRegen.Value;
            BlessingsVanir.VanirEliteBlessing.m_ttl = EliteDuration.Value;
            BlessingsVanir.VanirMinibossBlessing.StatusEffect.m_ttl = MinibossDuration.Value;

        }
    }
}
