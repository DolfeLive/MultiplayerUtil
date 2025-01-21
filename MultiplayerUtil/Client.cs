using Steamworks;
using System.Collections.Generic;
using UnityEngine;
using Clogger = MultiplayerUtil.Logger;

namespace MultiplayerUtil;

public class Client
{
    public List<SteamId> connectedPeers = new List<SteamId>();

    public void Connect(SteamId hostId)
    {
        bool success = SteamNetworking.AcceptP2PSessionWithUser(hostId);
        if (success)
        {
            connectedPeers.Add(hostId);
            SteamManager.instance.dataLoop = SteamManager.instance.StartCoroutine(SteamManager.instance.DataLoopInit());
            Clogger.Log($"P2P Connection established with {hostId}", true);
        }
        else
        {
            Clogger.LogError($"Failed to establish P2P connection with {hostId}", true);
        }
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

        foreach (var peerId in connectedPeers)
        {
            bool success = SteamNetworking.SendP2PPacket(
                peerId,
                serializedData,
                serializedData.Length,
                0,
                P2PSend.Reliable
            );

            if (!success)
            {
                Clogger.LogError($"Failed to send P2P packet to {peerId}", true);
            }
        }
    }
}
