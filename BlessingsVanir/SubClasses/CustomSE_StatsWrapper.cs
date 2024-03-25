using System;
using System.Reflection;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using HarmonyLib;
using BlessingsVanir;
using Jotunn;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using UnityEngine;
using static Humanoid;


namespace BlessingsVanir.SubClasses
{

    public class CustomSE_StatsWrapper : CustomStatusEffect
    {
        public string name;
        public string m_name;
        public string m_startMessage;
        public string m_tooltip;
        public MessageHud.MessageType m_startMessageType;
        public Sprite m_icon;
        public float m_ttl;
        public float m_staminaRegenMultiplier;
        public float m_healthOverTime;
        public float m_healthOverTimeDuration;
        public float m_healthOverTimeInterval;
        public float m_healthOverTimeTickHP;
        public SE_Stats _internalSEStats { get; set; }

        // Assuming CustomStatusEffect has a constructor that accepts a name and a bool for fixing references.
        public CustomSE_StatsWrapper(SE_Stats seStats, bool fixReference)
            : base(seStats, fixReference) // Adjust parameters as necessary for CustomStatusEffect's constructor
        {
            _internalSEStats = seStats;

            name = _internalSEStats.name;
            m_name = _internalSEStats.m_name;
            m_startMessage = _internalSEStats.m_startMessage;
            m_tooltip = _internalSEStats.m_tooltip;
            m_startMessageType = _internalSEStats.m_startMessageType;
            m_icon = _internalSEStats.m_icon;
            m_ttl = _internalSEStats.m_ttl;
            m_staminaRegenMultiplier = _internalSEStats.m_staminaRegenMultiplier;
            m_healthOverTime = _internalSEStats.m_healthOverTime;
            m_healthOverTimeDuration = _internalSEStats.m_healthOverTimeDuration;
            m_healthOverTimeInterval = _internalSEStats.m_healthOverTimeInterval;
            m_healthOverTimeTickHP = _internalSEStats.m_healthOverTimeTickHP;
        }


    }

}