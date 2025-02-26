#if DEBUG
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using MU = MultiplayerUtil;
using Steamworks.Data;
using UnityEngine;
using MultiplayerUtil;
using Clogger = MultiplayerUtil.Logger;
using static ExampleMod.ExampleClass1;
using UnityEngine.Events;


namespace ExampleMod;

[BepInPlugin("DolfeMods.Ultrakill.MultiplayerUtilExampleMod", "ULTRAKILL MultiplayersUtilExampleMod", "1.0.0")]
class ExampleClass1 : BaseUnityPlugin
{
    public static ExampleClass1 instance;
    
    

    void Start()
    {
        instance = this;
        counter = new CounterClass();
        //player = new Player();


        /*MU.Callbacks.TimeToSendImportantData.AddListener(() => // use for things like player positions where they need to update often
        {
            if (!MU.LobbyManager.isLobbyOwner) return; // Only run if lobby owner

            try
            {
                var wrapper = new NetworkWrapper
                {
                    ClassType = "Player",
                    ClassData = MU.Data.Serialize(player)
                };
                byte[] wrappedData = MU.Data.Serialize(wrapper);

                Debug.Log($"Sending player pos: {player.position.ToVector3()}");
                MU.LobbyManager.SendData(wrappedData);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to send data: {e.Message}");
            }
        });*/

        MU.Callbacks.TimeToSendUnimportantData.AddListener(() =>  // UnimportantData Runs less than important (x times a seconds), use for things like leaderboards
        {
            if (!MU.LobbyManager.isLobbyOwner) return; // Only run if lobby owner

            try
            {
                MU.LobbyManager.SendData(counter);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to send data: {e.Message}");
            }
        });
        

        MU.ObserveManager.SubscribeToType(typeof(CounterClass), out UnityEvent<byte[]> CounterDetected);
        CounterDetected.AddListener(_ =>
        {
            print("counter recived!");
            var counter = Data.Deserialize<ExampleMod.ExampleClass1.CounterClass>(_);
            print($"Value: {counter.counter}");
        });

        StartCoroutine(Couting());
        StartCoroutine(UpdatePlayerPos());
    }
    bool firstTimeP2p = true;

    


    // Counter example
    public CounterClass counter;
    [Serializable] // if you want to use this class to store values IT HAS TO BE [Serializable]
    public class CounterClass
    {
        public CounterClass()
        {
            this.counter = 0;
        }
        public int counter = 0;
    }
    IEnumerator Couting()
    {
        while (true)
        {
            counter.counter++;

            yield return new WaitForSeconds(1f);
        }
    }

    // Player Pos example
    [System.Serializable]
    public class SerializableVector3// You normally cant Serialize a vector3 so this is a lil workaround
    {
        public float x;
        public float y;
        public float z;

        public SerializableVector3(Vector3 vector)
        {
            x = vector.x;
            y = vector.y;
            z = vector.z;
        }

        public Vector3 ToVector3()
        {
            return new Vector3(x, y, z);
        }
    }


    public Player player;
    [Serializable]
    public class Player
    {
        public Player() { this.position = new SerializableVector3(Vector3.zero); }

        public SerializableVector3 position = new SerializableVector3(Vector3.zero);
    }

    IEnumerator UpdatePlayerPos()
    {

        while (true)
        {
            if (NewMovement.Instance == null)
            {
                yield return null;
                continue;
            }
            player.position = new SerializableVector3(NewMovement.Instance?.gameObject?.transform.position ?? Vector3.zero);
            yield return new WaitForSeconds(1f);
        }
    }
}
#endif