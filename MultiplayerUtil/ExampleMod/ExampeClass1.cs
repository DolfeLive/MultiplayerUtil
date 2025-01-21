using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using MU = MultiplayerUtil;
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

        MU.Callbacks.TimeToSendImportantData.AddListener(() => 
        {

            object data = MU.Data.Serialize(counter);

            MU.LobbyManager.SendData(data);
        });


        StartCoroutine(Couting());
    }
    public int counter = 0;
    public bool Server = false;

    IEnumerator Couting()
    {
        while (true)
        {
            counter++;
            


            yield return null;
        }
    }
    // 109775242898874045
}
