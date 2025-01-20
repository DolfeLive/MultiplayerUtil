using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        
    }
    // 109775242898874045
}
