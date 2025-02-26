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

        //MU.Callbacks.TimeToSendImportantData.AddListener(() => // use for things like player positions
        //{
        //    if (!MU.LobbyManager.isLobbyOwner) return; // Only run if lobby owner
            
        //    try
        //    {
        //        var wrapper = new NetworkWrapper
        //        {
        //            ClassType = "Player",
        //            ClassData = MU.Data.Serialize(player)
        //        };
        //        byte[] wrappedData = MU.Data.Serialize(wrapper);

        //        Debug.Log($"Sending player pos: {player.position.ToVector3()}");
        //        MU.LobbyManager.SendData(wrappedData);
        //    }
        //    catch (Exception e)
        //    {
        //        Debug.LogError($"Failed to send data: {e.Message}");
        //    }
        //});

        MU.Callbacks.TimeToSendUnimportantData.AddListener(() =>  // UnimportantData Runs less than important (you can change the interval), use for things like leaderboards
        {
            if (!MU.LobbyManager.isLobbyOwner) return; // Only run if lobby owner

            try
            {
                //var wrapper = new NetworkWrapper
                //{
                //    ClassType = "Counter",
                //    ClassData = MU.Data.Serialize(counter)
                //};
                byte[] wrappedData = MU.Data.Serialize(counter);

                //Debug.Log($"Sending counter value: {counter.counter}");
                MU.LobbyManager.SendData(wrappedData);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to send data: {e.Message}");
            }
        });
        MU.Callbacks.p2pMessageRecived.AddListener((dual) => // (byte[], SteamId?), dual is a array of bytes and the steamid of the sender
        {
            if (firstTimeP2p)
            {
                firstTimeP2p = false;
                return;
            }
            var (data, sender) = dual; // (byte[], SteamId?)

            //if (data == null || !sender.Value.IsValid)
            //{
            //    Debug.LogError($"Received invalid P2P message: data or sender is null. data:{data == null}, Sender:{sender.HasValue}");
            //    return;
            //}
            //if (!sender.HasValue || sender.Value == LobbyManager.selfID) // Check if sender isnt null or if the sender isnt yourself
            //{
            //    Debug.Log($"Failed at reciving, why: {(sender.HasValue ? "Has value": "Does not have value")} {(data.Length  > 0 ? string.Join("", data) : "")}, Sender: {(sender.HasValue ? sender.Value.ToString() : "null")}");
            //    return;
            //}

            try
            {
                
                //NetworkWrapper wrapper = MU.Data.Deserialize<NetworkWrapper>(data);
                //if (wrapper == null)
                //{
                //    Debug.LogError("Failed to deserialize NetworkWrapper.");
                //    return;
                //}

                //switch (wrapper.ClassType)
                //{
                //    case "Player":
                //        var playerData = MU.Data.Deserialize<Player>(wrapper.ClassData);
                //        Debug.Log($"Received player position: {playerData.position.ToVector3()}");
                //        break;

                //    case "Counter":
                //        var counterData = MU.Data.Deserialize<CounterClass>(wrapper.ClassData);
                //        //Debug.Log($"Received counter value: {counterData.counter}");
                //        break;
                //    default:
                //        Debug.LogWarning($"Unknown class type received: {wrapper.ClassType}");
                //        break;
                //}

                CounterClass receivedCounter = MU.Data.Deserialize<CounterClass>(data); // Deserialize into the counter class
                Debug.Log($"Received counter value: {receivedCounter.counter}");
                
                
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to process received data: {ex.Message}");
            }
        });


        StartCoroutine(Couting());
        StartCoroutine(UpdatePlayerPos());
    }
    bool firstTimeP2p = true;

    // Wrapper so i can handle multiple classes
    [System.Serializable]
    public class NetworkWrapper
    {
        public string ClassType { get; set; }
        public byte[] ClassData { get; set; }
    }


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
            player.position = new SerializableVector3(NewMovement.Instance?.gameObject?.transform.position ?? Vector3.zero);
            yield return new WaitForSeconds(1f);
        }
    }
}
