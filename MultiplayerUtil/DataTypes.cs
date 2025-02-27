using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml.Linq;
using UnityEngine;
using Newtonsoft.Json;
using Clogger = MultiplayerUtil.Logger;


namespace MultiplayerUtil;

/*
// An example of a data packet that i made for pvp multiplayer

[Serializable]
public class DataPacket
{
    // Player Core stuff
    public byte PlayerHealth;

    // Pos and Movement
    public float PositionX;
    public float PositionY;
    public float PositionZ;
    public float VelocityX;
    public float VelocityY;
    public float VelocityZ;
    public short RotationX;
    public short RotationY;

    // Combat State
    public byte CurrentWeapon;
    public byte CurrentVariation;
    public bool IsSliding;
    public bool IsPunching;

    // Movement State
    public bool IsWallJumping;
    public bool IsSlamStorage;

    public DataPacket(
        int Health,
        Vector3 Position,
        Vector3 Velocity,
        Vector3 Rotation,
        int CurrentWeapon,
        int CurrentVariation,
        bool IsSliding,
        bool IsPunching,
        bool IsWallJumping,
        bool IsSlamStorage)
    {
        this.PlayerHealth = (byte)Health;
        this.PositionX = Position.x;
        this.PositionY = Position.y;
        this.PositionZ = Position.z;
        this.VelocityX = Velocity.x;
        this.VelocityY = Velocity.y;
        this.VelocityZ = Velocity.z;
        this.RotationX = (short)Rotation.x;
        this.RotationY = (short)Rotation.y;
        this.CurrentWeapon = (byte)CurrentWeapon;
        this.CurrentVariation = (byte)CurrentVariation;
        this.IsSliding = IsSliding;
        this.IsPunching = IsPunching;
        this.IsWallJumping = IsWallJumping;
        this.IsSlamStorage = IsSlamStorage;
    }

    public void Display()
    {
        Console.WriteLine($"Health: {PlayerHealth}");
        Console.WriteLine($"Position: ({PositionX:F2}, {PositionY:F2}, {PositionZ:F2})");
        Console.WriteLine($"Velocity: ({VelocityX:F2}, {VelocityY:F2}, {VelocityZ:F2})");
        Console.WriteLine($"Rotation: ({RotationX:F2}, {RotationY:F2}");
        Console.WriteLine($"Weapon: {CurrentWeapon} | Variation: {CurrentVariation}");
        Console.WriteLine($"States: Sliding={IsSliding}, WallJump={IsWallJumping}, IsSlamStorage;={IsSlamStorage}");
    }
}

*/


public static class Data
{
    public static T Deserialize<T>(byte[] serializedData)
    {
        if (serializedData == null || serializedData.Length == 0)
        {
            Clogger.LogError("Failed to deserialize data: Empty or null data received");
            throw new ArgumentException("Serialized data cannot be null or empty.", nameof(serializedData));
        }

        try
        {
            using (MemoryStream ms = new MemoryStream(serializedData))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                return (T)formatter.Deserialize(ms);
            }
        }
        catch (Exception ex)
        {
            Clogger.LogError($"Failed to deserialize data: {ex.Message}");
            throw;
        }
    }

    public static byte[] Serialize(object data)
    {
        if (data == null)
        {
            Clogger.LogError("Failed to serialize data: Null object provided");
            throw new ArgumentNullException(nameof(data));
        }

        try
        {
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(ms, data);
                return ms.ToArray();
            }
        }
        catch (Exception ex)
        {
            Clogger.LogError($"Failed to serialize data: {ex.Message}");
            throw;
        }
    }
}
