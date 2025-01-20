using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using MultiplayerUtil;
using Steamworks.Data;
using UnityEngine;

namespace ExampleMod;

[BepInPlugin("DolfeMods.Ultrakill.MultiplayerUtilExampleMod", "ULTRAKILL MultiplayersUtilExampleMod", "1.0.0")]
class ExampleClass1 : BaseUnityPlugin
{
    public static ExampleClass1 instance;
    void Awake()
    {
        instance = this;
        MultiplayerUtil.LobbyManager.SetSettings("GAHHHHHHHHHHH", 3, true, true, false, false, ("Idk", "idk"));
        MultiplayerUtil.LobbyManager.CreateLobby();


        MultiplayerUtil.SteamManager.instance.StartupComplete += () =>
        {
            StartCoroutine(GetLobbyStuff());
        };
    }

    IEnumerator GetLobbyStuff()
    {
        yield return null;
        
        List<Lobby> getthingy = new List<Lobby>();
        
        getthingy = MultiplayerUtil.LobbyManager.FetchLobbies(("Idk", "idk")).Result;
        
        print($"{JsonUtility.ToJson(getthingy.Select(_ => _.Owner))}");
    }
}
