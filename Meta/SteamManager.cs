using UnityEngine;
using Steamworks;

public class SteamManager : MonoBehaviour
{
    static SteamManager Instance;
    static Callback<GameOverlayActivated_t> GameOverlayActivated;

    public static readonly bool RESTART_IF_NECESSARY = true;

    public static bool Initialized => SteamAPI.IsSteamRunning() && SteamAPI.Init();

    private void Awake()
    {
#if !UNITY_EDITOR
        if (RESTART_IF_NECESSARY && SteamAPI.RestartAppIfNecessary((AppId_t)2589990))
        {
            Debug.Log("Restarting steam to access overlay");
            Application.Quit();
            return;
        }
#endif

        if (Instance != null)
        {
            Debug.Log("Steam Manager found, destroying duplicate");
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (!Initialized)
        {
            if (!SteamAPI.Init())
            {
                Debug.LogError("SteamAPI.Init() failed. Steamworks will not be initialized.");
                return;
            }
            Debug.Log("Steamworks initialized");
        }
        Debug.Log("Steamworks already initialized");
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SteamAPI.Shutdown();
            Instance = null;
        }
    }

    private void OnEnable()
    {
        GameOverlayActivated = Callback<GameOverlayActivated_t>.Create(OnGameOverlayActivated);
    }

    private void OnGameOverlayActivated(GameOverlayActivated_t param)
    {
        if (param.m_bActive != 0)
        {
            if (!V_HUDManager.Instance.IsPaused)
                V_HUDManager.Instance.ForcePauseOverlay();
        }
        else
        {
            // Steam overlay has been deactivated.
            // Handle overlay deactivation, if needed.
        }
    }
}
