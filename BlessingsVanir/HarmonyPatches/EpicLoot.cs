using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlessingsVanir;
using BlessingsVanir.Configs;
using HarmonyLib;
using BlessingsVanir.ReflectiveHooks;

namespace BlessingsVanir.HarmonyPatches
{
    class EpicLoot
    {
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
                if (BlessingsVanir.powerfulCreatures.Contains(__instance.m_name))
                {
                    BlessingsVanir.instance.StartCoroutine(BlessingsVanir.DelayedStatusEffect("miniboss"));
                }
            }
        }
    }
}
