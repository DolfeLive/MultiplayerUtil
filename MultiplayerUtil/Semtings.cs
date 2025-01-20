using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BepInEx;
using UnityEngine;
using Clogger = MultiplayerUtil.Logger;

namespace MultiplayerUtil;

public static class Semtings
{
    static string modSettingPath;
    public static void Init()
    {
        try
        {
            if (string.IsNullOrEmpty(Paths.ConfigPath))
            {
                Clogger.LogError("Config path is not initialized!");
                return;
            }

            modSettingPath = Path.Combine(Paths.ConfigPath, "UKCM", "Settings.cfg");

            Directory.CreateDirectory(Path.GetDirectoryName(modSettingPath));

            if (!File.Exists(modSettingPath))
            {
                CreateDefaultSettingsFile();
            }
            else
            {
                LoadExistingSettings();
            }
        }
        catch (Exception ex)
        {
            Clogger.LogError($"Error in Init(): {ex.Message}");
            Debug.LogException(ex);
        }
    }

    private static void CreateDefaultSettingsFile()
    {
        try
        {
            var settings = new Settings
            {
                cracked = false
            };

            string json = JsonUtility.ToJson(settings, prettyPrint: true);
            File.WriteAllText(modSettingPath, json);
        }
        catch (Exception ex)
        {
            Clogger.LogError($"Failed to create settings file: {ex.Message}");
        }
    }

    private static void LoadExistingSettings()
    {
        try
        {
            string json = File.ReadAllText(modSettingPath);
            var settings = JsonUtility.FromJson<Settings>(json);

            if (settings == null)
            {
                Clogger.LogError("Failed to deserialize settings. Creating default.");
                CreateDefaultSettingsFile();
                return;
            }

            Class1.cracked = settings.cracked;
        }
        catch (Exception ex)
        {
            Clogger.LogError($"Error loading settings: {ex.Message}");
            CreateDefaultSettingsFile();
        }
    }

}

[System.Serializable]
public class Settings
{ 
    public bool cracked = false;
}
