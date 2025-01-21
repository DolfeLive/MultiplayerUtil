using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using MU = MultiplayerUtil;
using Steamworks.Data;
using UnityEngine;
using MultiplayerUtil;

namespace ExampleMod;

[BepInPlugin("DolfeMods.Ultrakill.MultiplayerUtilExampleMod", "ULTRAKILL MultiplayersUtilExampleMod", "1.0.0")]
class ExampleClass1 : BaseUnityPlugin
{
    public static ExampleClass1 instance;
    void Start()
    {
        this.gameObject.hideFlags = HideFlags.HideAndDontSave;
        instance = this;
        counter = new CounterClass();
        
        MU.Callbacks.TimeToSendUnimportantData.AddListener(() => 
        {
            if (!MU.LobbyManager.isLobbyOwner) return;

            try
            {
                byte[] data = MU.Data.Serialize(counter);
                Debug.Log($"Sending counter value: {counter.counter}");
                MU.LobbyManager.SendData(data);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to send data: {ex.Message}");
            }
        });

        MU.Callbacks.p2pMessageRecived.AddListener((dual) =>
        {
            if ((dual.Item2.HasValue && dual.Item2.Value != LobbyManager.selfID) || !dual.Item2.HasValue)
            {
                return;
            }

            try
            {
                if (dual.Item1 != null)
                {
                    CounterClass receivedCounter = MU.Data.Deserialize<CounterClass>((byte[])dual.Item1);
                    Debug.Log($"Received counter value: {receivedCounter.counter}");
                }
                else
                {
                    if (dual.Item2.HasValue)
                    {
                        Debug.LogWarning($"Received null p2p message, {dual.Item1}, sender: {dual.Item2.Value}");
                    }
                    else
                    {
                        Debug.LogWarning($"Received null p2p message, {dual.Item1}, sender: Unknown");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to process received data: {ex.Message}");
            }
        });


        StartCoroutine(Couting());
    }

    public CounterClass counter;

    [Serializable]
    public class CounterClass
    {
        public CounterClass()
        {
            this.counter = 0;
        }
        public int counter = 0;
    }

    public bool Server = false;

    IEnumerator Couting()
    {
        while (true)
        {
            counter.counter++;

            yield return new WaitForSeconds(1f);
        }
    }
    // 109775242898874045
}
