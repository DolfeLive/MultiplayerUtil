using Steamworks;
using System.Collections.Generic;
using UnityEngine;
using Clogger = MultiplayerUtil.Logger;
using System;

namespace MultiplayerUtil.Server;

public class Serveier // Read it like its french, also yes i named it this on purpose
{
    public List<Friend> besties = new List<Friend>(); // People in lobby

    public Serveier()
    {
        SteamManager.instance.dataLoop = SteamManager.instance.StartCoroutine(SteamManager.instance.DataLoopInit());
    }
    public void Send(object data)
    {
        byte[] serializedData;

        if (data is byte[])
        {
            serializedData = (byte[])data;
        }
        else
        {
            serializedData = Data.Serialize(data);
        }


        foreach (var bestie in besties)
        {
            var peerId = bestie.Id;

            if (peerId == LobbyManager.selfID) return;

            bool success = SteamNetworking.SendP2PPacket(
                peerId,
                serializedData,
                serializedData.Length,
                0,
                P2PSend.Reliable
            );

            if (!success)
            {
                Clogger.LogError($"Failed to send P2P packet to {peerId}", false);
            }
        }
    }
}
