﻿#if DEBUG
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
        gameObject.hideFlags = HideFlags.HideAndDontSave;
        instance = this;
        counter = new CounterClass();
        player = new Player();

        // You can send data at any time but these are pre set loops for convenience

        MU.Callbacks.TimeToSendImportantData.AddListener(() => // use for things like player positions where they need to update often
        {
            try
            {
                
                Debug.Log($"Sending player pos: {player.position.ToVector3()}");
                MU.LobbyManager.SendData(player);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to send data: {e.Message}");
            }
        });

        MU.Callbacks.TimeToSendUnimportantData.AddListener(() =>  // UnimportantData Runs less than important (x times a seconds), use for things like leaderboards
        {
            if (!MU.LobbyManager.isLobbyOwner) return; // Only run if lobby owner // only do this if its owner only stuff

            try
            {
                MU.LobbyManager.SendData(counter);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to send data: {e.Message}");
            }
        });
        

        MU.ObserveManager.SubscribeToType(typeof(CounterClass), out Callbacks.SenderUnityEvent CounterDetected);
        CounterDetected.AddListener(_ => // When counter changes run this codeblock
        {
            var counter = Data.Deserialize<ExampleMod.ExampleClass1.CounterClass>(_.Item1);
            print($"Counter value: {counter.counter}, Sender id: {_.Item2.Value}");
        });

        MU.ObserveManager.SubscribeToType(typeof(Player), out Callbacks.SenderUnityEvent PlayerDetected);
        PlayerDetected.AddListener(_ =>
        {
            var player = Data.Deserialize<Player>(_.Item1);
            print($"player Pos: {player.position.ToVector3()}, Sender id: {_.Item2.Value}");
        });

        StartCoroutine(Couting());
        StartCoroutine(UpdatePlayerPos());
    }
    /*
     *  Please note:
     *  Try and keep your classes small
     *  Try and substitue values like health from an int to a byte (0-255)
     *  If you want to put the extra effort you can split up data based on their importance and send them at different times
     *  Try and avoid strings if you can or set limits to the length of strings
     *  
     *  Below is the int types C# supports
     *  
     *  The sbyte type represents signed 8-bit integers with values from -128 to 127, inclusive.
     *  The byte type represents unsigned 8-bit integers with values from 0 to 255, inclusive.
     *  The short type represents signed 16-bit integers with values from -32768 to 32767, inclusive.
     *  The ushort type represents unsigned 16-bit integers with values from 0 to 65535, inclusive.
     *  The int type represents signed 32-bit integers with values from -2147483648 to 2147483647, inclusive.
     *  The uint type represents unsigned 32-bit integers with values from 0 to 4294967295, inclusive.
     *  The long type represents signed 64-bit integers with values from -9223372036854775808 to 9223372036854775807, inclusive.
     *  The ulong type represents unsigned 64-bit integers with values from 0 to 18446744073709551615, inclusive.
     *  
     * - https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/types 8.3.6 Integral types
     *   https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/integral-numeric-types
    */


    // Counter example
    public CounterClass counter;

    [Serializable] // if you want to use this class to store values it has to be [Serializable]
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
    public class SerializableVector3 // You normally cant Serialize a vector3 so this is a lil workaround
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
                yield return new WaitForSeconds(2);
                continue;
            }
            player.position = new SerializableVector3(NewMovement.Instance?.gameObject?.transform.position ?? Vector3.zero);
            yield return new WaitForSeconds(1f);
        }
    }
}
#endif