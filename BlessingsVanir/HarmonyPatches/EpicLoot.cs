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
        //public override void ApplyArmorDamageMods(ref HitData.DamageModifiers mods)
        [HarmonyPatch(typeof(Player), nameof(Player.ApplyArmorDamageMods))]
        public static class ModifyResistance_Player_ApplyArmorDamageMods_Patch
        {
            public static void Postfix(Player __instance, ref HitData.DamageModifiers mods)
            {
                var damageMods = new List<HitData.DamageModPair>();

                if (__instance.HasActiveMagicEffect(MagicEffectType.AddFireResistance))
                {
                    damageMods.Add(new HitData.DamageModPair() { m_type = HitData.DamageType.Fire, m_modifier = HitData.DamageModifier.Resistant });
                }
                if (__instance.HasActiveMagicEffect(MagicEffectType.AddFrostResistance))
                {
                    damageMods.Add(new HitData.DamageModPair() { m_type = HitData.DamageType.Frost, m_modifier = HitData.DamageModifier.Resistant });
                }
                if (__instance.HasActiveMagicEffect(MagicEffectType.AddLightningResistance))
                {
                    damageMods.Add(new HitData.DamageModPair() { m_type = HitData.DamageType.Lightning, m_modifier = HitData.DamageModifier.Resistant });
                }
                if (__instance.HasActiveMagicEffect(MagicEffectType.AddPoisonResistance))
                {
                    damageMods.Add(new HitData.DamageModPair() { m_type = HitData.DamageType.Poison, m_modifier = HitData.DamageModifier.Resistant });
                }
                if (__instance.HasActiveMagicEffect(MagicEffectType.AddSpiritResistance))
                {
                    damageMods.Add(new HitData.DamageModPair() { m_type = HitData.DamageType.Spirit, m_modifier = HitData.DamageModifier.Resistant });
                }

                mods.Apply(damageMods);
            }
        }

        [HarmonyPatch(typeof(Character), nameof(Character.RPC_Damage))]
        public static class ModifyResistance_Character_RPC_Damage_Patch
        {
            public static void Prefix(Character __instance, HitData hit)
            {
                if (!(__instance is Player player))
                {
                    return;
                }

                float sum(params string[] effects)
                {
                    float value = 1;
                    foreach (var effect in effects)
                    {
                        value -= player.GetTotalActiveMagicEffectValue(effect, 0.01f);
                    }

                    return Math.Max(value, 0);
                }

                // elemental resistances
                hit.m_damage.m_fire *= sum(MagicEffectType.AddFireResistancePercentage, MagicEffectType.AddElementalResistancePercentage);
                hit.m_damage.m_frost *= sum(MagicEffectType.AddFrostResistancePercentage, MagicEffectType.AddElementalResistancePercentage);
                hit.m_damage.m_lightning *= sum(MagicEffectType.AddLightningResistancePercentage, MagicEffectType.AddElementalResistancePercentage);
                hit.m_damage.m_poison *= sum(MagicEffectType.AddPoisonResistancePercentage, MagicEffectType.AddElementalResistancePercentage);
                hit.m_damage.m_spirit *= sum(MagicEffectType.AddSpiritResistancePercentage, MagicEffectType.AddElementalResistancePercentage);

                // physical resistances
                hit.m_damage.m_blunt *= sum(MagicEffectType.AddBluntResistancePercentage, MagicEffectType.AddPhysicalResistancePercentage);
                hit.m_damage.m_slash *= sum(MagicEffectType.AddSlashingResistancePercentage, MagicEffectType.AddPhysicalResistancePercentage);
                hit.m_damage.m_pierce *= sum(MagicEffectType.AddPiercingResistancePercentage, MagicEffectType.AddPhysicalResistancePercentage);
                hit.m_damage.m_chop *= sum(MagicEffectType.AddChoppingResistancePercentage, MagicEffectType.AddPhysicalResistancePercentage);
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
                if (BlessingsVanir.powerfulCreatures.Contains(__instance.m_name))
                {
                    BlessingsVanir.instance.StartCoroutine(BlessingsVanir.DelayedStatusEffect("miniboss"));
                }
            }
        }
    }
}
