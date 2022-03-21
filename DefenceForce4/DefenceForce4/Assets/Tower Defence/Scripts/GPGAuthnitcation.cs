using UnityEngine;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine.SocialPlatforms;

public class GPGAuthnitcation : MonoBehaviour
{
#if UNITY_ANDROID
    public static PlayGamesPlatform platform;
#endif
    public static GPGAuthnitcation instance = null;

    private void Awake()
    {
        if (instance == null)
            instance = this;
    }
    internal void GoogleSignIn()
    {
        if (PhotonEventScript.IsInternetConnected())
        {
#if UNITY_ANDROID
            PlayGamesClientConfiguration config = new PlayGamesClientConfiguration.Builder().Build();
            PlayGamesPlatform.InitializeInstance(config);
            PlayGamesPlatform.DebugLogEnabled = true;
            platform = PlayGamesPlatform.Activate();
#endif
            Social.Active.localUser.Authenticate(success =>
            {
                if (success)
                {
                    Debug.Log("logged in successfully");
                    UserData.SetUsername(Social.Active.localUser.userName);
                    UiManager.instance.SetPlayernameOnUI();
                    GameManager.instance.StartCoroutine(GameManager.instance.SocialSignIn(UserData.GetUsername(), Social.Active.localUser.id));
                }
                else
                    Debug.Log("logged in failed");
            });
        }
        else
            GameManager.instance.StartCoroutine(GameManager.instance.ShowCustomMessage(Constant.str_no_internet));
    }
    internal void LogOutGoogleSignIn()
    {
        if (Social.Active.localUser.authenticated)
        {
#if UNITY_ANDROID
            PlayGamesPlatform.Instance.SignOut();
#endif
        }
    }
}