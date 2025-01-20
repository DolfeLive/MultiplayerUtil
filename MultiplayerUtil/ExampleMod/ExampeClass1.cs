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
using UnityEngine.SceneManagement;

namespace ExampleMod;

[BepInPlugin("DolfeMods.Ultrakill.MultiplayerUtilExampleMod", "ULTRAKILL MultiplayersUtilExampleMod", "1.0.0")]
class ExampleClass1 : BaseUnityPlugin
{
    public static ExampleClass1 instance;
    void Start()
    {
        this.gameObject.hideFlags = HideFlags.HideAndDontSave;
        instance = this;

        SceneManager.sceneLoaded += (Scene scene, LoadSceneMode lsm) =>
        {
            if (SceneHelper.CurrentScene == "Main Menu")
            {
                MultiplayerUtil.LobbyManager.SetSettings("GAHHHHHHHHHHH", 3, true, true, false, false, ("Idk", "idk"));
                MultiplayerUtil.LobbyManager.CreateLobby();

                GetLobbyStuff();
            }
        };
        
    }
    // 109775242898874045
    async void GetLobbyStuff()
    {        
        List<Lobby> getthingy = getthingy = await MultiplayerUtil.LobbyManager.FetchLobbies(("Idk", "idk"));


        print($"{
            JsonUtility.ToJson(
                new {
                        Owners = getthingy.Select(_ => _.Owner).ToArray()
                    } 
                )
            }");
    }
}
