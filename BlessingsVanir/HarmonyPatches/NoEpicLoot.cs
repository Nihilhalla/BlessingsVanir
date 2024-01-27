using BepInEx;
using BepInEx.Configuration;
using Jotunn;
using Jotunn.Entities;
using Jotunn.Configs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using BlessingsVanir;
using BlessingsVanir.Configs;

namespace BlessingsVanir.HarmonyPatches
{
    class NoEpicLoot
    {
        [HarmonyPatch(typeof(Player), nameof(Player.ApplyArmorDamageMods))]
        public static class DamageResistMod
        {
            private static Player player = Player.m_localPlayer;
            public static void Postfix(Player __instance, ref HitData.DamageModifiers mods)
            {
                var damageMods = new List<HitData.DamageModPair>();
                if (__instance.GetSEMan().HaveStatusEffect(BlessingsVanir.VanirAbominationBlessing.StatusEffect.name))
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

                hit.m_damage.m_pierce *= BaseConfig.VanirAbominationDamageReduce.Value;
                hit.m_damage.m_slash *= BaseConfig.VanirAbominationDamageReduce.Value;
                hit.m_damage.m_blunt *= BaseConfig.VanirAbominationDamageReduce.Value;
                hit.m_damage.m_chop *= BaseConfig.VanirAbominationDamageReduce.Value;


            }
        }
        [HarmonyPatch(typeof(Player), nameof(Player.UpdateTeleport))]
        public static class Teleport
        {
            public static void Prefix()
            {
                if (Player.m_localPlayer.GetSEMan().HaveStatusEffect(BlessingsVanir.VanirElderBlessing.StatusEffect.name))
                {
                    BlessingsVanir.SyncTeleportability("VanirElder", BlessingsVanir.elderBlessedTeleportable);
                    //Jotunn.Logger.LogInfo("The Vanir have seen the death of The Elder");
                }
                else
                {
                    BlessingsVanir.ResetTeleportability("VanirElder", BlessingsVanir.elderBlessedTeleportable);
                }
                if (Player.m_localPlayer.GetSEMan().HaveStatusEffect(BlessingsVanir.VanirBonemassBlessing.StatusEffect.name))
                {
                    BlessingsVanir.SyncTeleportability("VanirBonemass", BlessingsVanir.bonemassBlessedTeleportable);
                    //Jotunn.Logger.LogInfo("The Vanir have seen the death of Bonemass");
                }
                else
                {
                    BlessingsVanir.ResetTeleportability("VanirBonemass", BlessingsVanir.bonemassBlessedTeleportable);
                }
                if (Player.m_localPlayer.GetSEMan().HaveStatusEffect(BlessingsVanir.VanirModerBlessing.StatusEffect.name))
                {
                    BlessingsVanir.SyncTeleportability("VanirModer", BlessingsVanir.moderBlessedTeleportable);
                    //Jotunn.Logger.LogInfo("The Vanir have seen the death of Moder");
                }
                else
                {
                    BlessingsVanir.ResetTeleportability("VanirModer", BlessingsVanir.moderBlessedTeleportable);
                }
                if (Player.m_localPlayer.GetSEMan().HaveStatusEffect(BlessingsVanir.VanirYagluthBlessing.StatusEffect.name))
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
        [HarmonyPatch(typeof(Character), nameof(Character.OnDeath))]

        public static class CharacterDeathPatch
        {

            public static void Prefix(ref Humanoid __instance)
            {

            }

            public static void Postfix(ref Character __instance)
            {
                if (__instance.m_name.Equals("$enemy_gdking"))
                {
                    BlessingsVanir.instance.StartCoroutine(BlessingsVanir.DelayedStatusEffect("elder"));
                }
                if (__instance.m_name.Equals("$enemy_bonemass"))
                {
                    BlessingsVanir.instance.StartCoroutine(BlessingsVanir.DelayedStatusEffect("bonemass"));
                }
                if (__instance.m_name.Equals("$enemy_dragon"))
                {
                    BlessingsVanir.instance.StartCoroutine(BlessingsVanir.DelayedStatusEffect("moder"));
                }
                if (__instance.m_name.Equals("$enemy_goblinking"))
                {
                    BlessingsVanir.instance.StartCoroutine(BlessingsVanir.DelayedStatusEffect("yagluth"));
                }
                if (__instance.m_level == 2 && __instance.m_tamed.Equals(false) && UnityEngine.Random.Range(1, 7).Equals(6) || __instance.m_level == 3 && __instance.m_tamed.Equals(false))
                {
                    BlessingsVanir.instance.StartCoroutine(BlessingsVanir.DelayedStatusEffect("elite"));
                }
                if (__instance.m_name.Equals("$enemy_troll"))
                {
                    BlessingsVanir.instance.StartCoroutine(BlessingsVanir.DelayedStatusEffect("troll"));
                }
                if (__instance.m_name.Equals("$enemy_abomination"))
                {
                    BlessingsVanir.instance.StartCoroutine(BlessingsVanir.DelayedStatusEffect("abomination"));
                }
            }
        }
    }
}
