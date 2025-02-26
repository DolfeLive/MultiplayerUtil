using System;
using Steamworks;
using Steamworks.Data;
using UnityEngine;
using ULTRAKILL;
using UnityEngine.UI;
using BepInEx;
using UnityEngine.SceneManagement;
using TMPro;
using System.IO;
using HarmonyLib;
using MultiplayerUtil;
using Clogger = MultiplayerUtil.Logger;

namespace MultiplayerUtil;

[BepInPlugin("DolfeMods.Ultrakill.MultiplayerUtil", "ULTRAKILL MultiplayersUtil", "1.0.0")]
public class Class1 : BaseUnityPlugin
{
    public static string modName = "MultiplayerUtil";

    public static Class1 instance;
    public static bool cracked = false;
    public static uint appId => cracked ? 480u : 1229490u;
    private GameObject smObj = null!;
    void Awake()
    {
        instance = this;
        
        Semtings.Init();

        Harmony har = new Harmony("MultiplayerUtil");
        har.PatchAll();

        SceneManager.sceneLoaded += (Scene scene, LoadSceneMode lsm) =>
        {
            if (SceneHelper.CurrentScene == "Main Menu")
            {
                if (smObj != null) return;

                smObj = new GameObject("SteamManager PVP mod");
                smObj.AddComponent<SteamManager>();
                DontDestroyOnLoad(smObj);
            }
        };
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            ExampleMod.ExampleClass1.CounterClass counter = new ExampleMod.ExampleClass1.CounterClass();
            counter.counter = 5;

            NetworkWrapper nw = new();
            nw.ClassType = typeof(ExampleMod.ExampleClass1.CounterClass).AssemblyQualifiedName;
            nw.ClassData = Data.Serialize(counter);

            ObserveManager.OnMessageRecived(Data.Serialize(nw), 0);
        }
    }
}