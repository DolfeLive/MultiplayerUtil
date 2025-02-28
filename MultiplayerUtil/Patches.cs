using HarmonyLib;
using plog.Models;
using Steamworks;
using System;
using System.Reflection;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MultiplayerUtil;

[HarmonyPatch(typeof(SteamController), "Awake")]
public class SteamControllerAwakePatch
{
    public static bool Prefix(SteamController __instance)
    {
        if (SteamController.Instance)
        {
            UnityEngine.Object.Destroy(__instance.gameObject);
            return false;
        }

        SteamController.Instance = __instance;
        __instance.transform.SetParent(null);
        UnityEngine.Object.DontDestroyOnLoad(__instance.gameObject);

        try
        {
            SteamClient.Init(Class1.appId, true);
            SteamManager.instance.selfID = SteamClient.SteamId;
            SteamController.Log.Info("Steam initialized!");
        }
        catch (Exception)
        {
            SteamController.Log.Info("Couldn't initialize Steam");
        }

        return false;
    }
}