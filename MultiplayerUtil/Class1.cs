
namespace MultiplayerUtil;

[BepInPlugin("DolfeMods.Ultrakill.MultiplayerUtil", "ULTRAKILL MultiplayersUtil", "1.0.0")]
public class Class1 : BaseUnityPlugin
{
    public static string modName = "MultiplayerUtil";

    public static Class1 instance;
    public static bool cracked = false;
    public static uint appId => cracked ? 480u : 1229490u;
    private GameObject smObj = null!;
    void Awake()
    {
        instance = this;
        
        Semtings.Init();

        Harmony har = new Harmony("MultiplayerUtil");
        har.PatchAll();

        SceneManager.sceneLoaded += (Scene scene, LoadSceneMode lsm) =>
        {
            if (SceneHelper.CurrentScene == "Main Menu")
            {
                if (smObj != null) return;

                smObj = new GameObject("SteamManager PVP mod");
                smObj.AddComponent<SteamManager>();
                DontDestroyOnLoad(smObj);
            }
        };
    }
}