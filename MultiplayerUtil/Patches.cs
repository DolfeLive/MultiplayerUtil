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
            SteamController.Log.Info("Steam initialized!", Array.Empty<Tag>());
        }
        catch (Exception)
        {
            SteamController.Log.Info("Couldn't initialize Steam", Array.Empty<Tag>());
        }

        return false;
    }

}

//[HarmonyPatch]
//class InstantiatePatches
//{
//    // Basic Instantiate(Object)
//    [HarmonyPatch(typeof(UnityEngine.Object), nameof(UnityEngine.Object.Instantiate), new Type[] { typeof(UnityEngine.Object) })]
//    class InstantiateBasicPatch
//    {
//        static void Postfix(UnityEngine.Object __result)
//        {
//            if (__result is GameObject resultObj)
//            {
//                UltraIDK.Logger.Log($"GameObject instantiated: {resultObj.name}");
//            }
//        }
//    }

//    // Instantiate(Object, Vector3, Quaternion)
//    [HarmonyPatch(typeof(UnityEngine.Object), nameof(UnityEngine.Object.Instantiate),
//        new Type[] { typeof(UnityEngine.Object), typeof(Vector3), typeof(Quaternion) })]
//    class InstantiateWithPositionRotationPatch
//    {
//        static void Postfix(UnityEngine.Object __result, Vector3 position, Quaternion rotation)
//        {
//            if (__result is GameObject resultObj)
//            {
//                UltraIDK.Logger.Log($"GameObject instantiated with position {position} and rotation {rotation}: {resultObj.name}");
//            }
//        }
//    }

//    // Instantiate(Object, Vector3, Quaternion, Transform)
//    [HarmonyPatch(typeof(UnityEngine.Object), nameof(UnityEngine.Object.Instantiate),
//        new Type[] { typeof(UnityEngine.Object), typeof(Vector3), typeof(Quaternion), typeof(Transform) })]
//    class InstantiateWithPositionRotationParentPatch
//    {
//        static void Postfix(UnityEngine.Object __result, Vector3 position, Quaternion rotation, Transform parent)
//        {
//            if (__result is GameObject resultObj)
//            {
//                UltraIDK.Logger.Log($"GameObject instantiated with position {position}, rotation {rotation}, and parent {parent?.name ?? "null"}: {resultObj.name}");
//            }
//        }
//    }

//    // Instantiate(Object, Transform)
//    [HarmonyPatch(typeof(UnityEngine.Object), nameof(UnityEngine.Object.Instantiate),
//        new Type[] { typeof(UnityEngine.Object), typeof(Transform) })]
//    class InstantiateWithParentPatch
//    {
//        static void Postfix(UnityEngine.Object __result, Transform parent)
//        {
//            if (__result is GameObject resultObj)
//            {
//                UltraIDK.Logger.Log($"GameObject instantiated with parent {parent?.name ?? "null"}: {resultObj.name}");
//            }
//        }
//    }

//    // Instantiate(Object, Transform, bool)
//    [HarmonyPatch(typeof(UnityEngine.Object), nameof(UnityEngine.Object.Instantiate),
//        new Type[] { typeof(UnityEngine.Object), typeof(Transform), typeof(bool) })]
//    class InstantiateWithParentAndWorldSpacePatch
//    {
//        static void Postfix(UnityEngine.Object __result, Transform parent, bool instantiateInWorldSpace)
//        {
//            if (__result is GameObject resultObj)
//            {
//                UltraIDK.Logger.Log($"GameObject instantiated with parent {parent?.name ?? "null"} and instantiateInWorldSpace={instantiateInWorldSpace}: {resultObj.name}");
//            }
//        }
//    }

//    // Basic generic Instantiate<T>
//    [HarmonyPatch(typeof(UnityEngine.Object))]
//    [HarmonyPatch("Instantiate", typeof(UnityEngine.Object))]
//    class InstantiateGenericBasicPatch
//    {
//        static void Postfix(UnityEngine.Object __result)
//        {
//            if (__result is GameObject resultObj)
//            {
//                UltraIDK.Logger.Log($"Generic GameObject instantiated: {resultObj.name}");
//            }
//        }
//    }

//    // Generic Instantiate<T> with position and rotation
//    [HarmonyPatch(typeof(UnityEngine.Object))]
//    [HarmonyPatch("Instantiate", typeof(UnityEngine.Object), typeof(Vector3), typeof(Quaternion))]
//    class InstantiateGenericWithPositionRotationPatch
//    {
//        static void Postfix(UnityEngine.Object __result, Vector3 position, Quaternion rotation)
//        {
//            if (__result is GameObject resultObj)
//            {
//                UltraIDK.Logger.Log($"Generic GameObject instantiated with position {position} and rotation {rotation}: {resultObj.name}");
//            }
//        }
//    }

//    // Generic Instantiate<T> with position, rotation and parent
//    [HarmonyPatch(typeof(UnityEngine.Object))]
//    [HarmonyPatch("Instantiate", typeof(UnityEngine.Object), typeof(Vector3), typeof(Quaternion), typeof(Transform))]
//    class InstantiateGenericWithPositionRotationParentPatch
//    {
//        static void Postfix(UnityEngine.Object __result, Vector3 position, Quaternion rotation, Transform parent)
//        {
//            if (__result is GameObject resultObj)
//            {
//                UltraIDK.Logger.Log($"Generic GameObject instantiated with position {position}, rotation {rotation}, and parent {parent?.name ?? "null"}: {resultObj.name}");
//            }
//        }
//    }

//    // Generic Instantiate<T> with parent
//    [HarmonyPatch(typeof(UnityEngine.Object))]
//    [HarmonyPatch("Instantiate", typeof(UnityEngine.Object), typeof(Transform))]
//    class InstantiateGenericWithParentPatch
//    {
//        static void Postfix(UnityEngine.Object __result, Transform parent)
//        {
//            if (__result is GameObject resultObj)
//            {
//                UltraIDK.Logger.Log($"Generic GameObject instantiated with parent {parent?.name ?? "null"}: {resultObj.name}");
//            }
//        }
//    }

//    // Generic Instantiate<T> with parent and world space
//    [HarmonyPatch(typeof(UnityEngine.Object))]
//    [HarmonyPatch("Instantiate", typeof(UnityEngine.Object), typeof(Transform), typeof(bool))]
//    class InstantiateGenericWithParentAndWorldSpacePatch
//    {
//        static void Postfix(UnityEngine.Object __result, Transform parent, bool instantiateInWorldSpace)
//        {
//            if (__result is GameObject resultObj)
//            {
//                UltraIDK.Logger.Log($"Generic GameObject instantiated with parent {parent?.name ?? "null"} and instantiateInWorldSpace={instantiateInWorldSpace}: {resultObj.name}");
//            }
//        }
//    }
//}

// Add patches for: player speed, rocket riding, bullets, damage reciving, shotgun bullet location rng
//
//
