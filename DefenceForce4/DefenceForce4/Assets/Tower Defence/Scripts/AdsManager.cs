using UnityEngine;
using System.Collections;
using UnityEngine.Advertisements;

public class AdsManager : MonoBehaviour, IUnityAdsListener
{
    public static AdsManager instance;
    private bool istestmode = false, isinitilize = false;

    internal string android_gameId = "4642311", ios_gameId = "4642310", intertitialId = string.Empty, intertitialId_Android = "Interstitial_Android", intertialId_Ios = "Interstitial_iOS";

    public enum AdType { Null, GameOver, VideoRewarded };
    internal AdType adsType = AdType.Null;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
    internal void Initialize()
    {
        if (PhotonEventScript.IsInternetConnected())
        {

            Advertisement.AddListener(this);
#if UNITY_ANDROID
            intertitialId = intertitialId_Android;
            Advertisement.Initialize(android_gameId, istestmode);
#endif
#if UNITY_IOS
            intertitialId = intertialId_Ios;
            Advertisement.Initialize(ios_gameId, istestmode);
#endif
            isinitilize = true;
        }
        RequestIntertitalAds();
    }
    internal void RequestIntertitalAds()
    {
        if (UserData.GetPremiumUser())
            return;
        if (PhotonEventScript.IsInternetConnected())
        {
            if (!isinitilize)
                Initialize();
            else
            {
                if (!Advertisement.IsReady(intertitialId))
                {
                    Advertisement.Load(intertitialId);
                    Debug.Log("Request Intertial ads Loaded");
                }
            }
        }
    }
    public void ShowIntertialAds(AdType adtype)
    {
        if (UserData.GetPremiumUser())
            return;
        adsType = adtype;
        if (PhotonEventScript.IsInternetConnected())
        {
            if (Advertisement.IsReady(intertitialId))
                Advertisement.Show(intertitialId);
            else
            {
                StartCoroutine(OnIntertialClose(0f));
                Debug.Log("intertitialId is not ready at the moment! Please try again later!");
            }
        }
        else
            StartCoroutine(OnIntertialClose(0f));
    }
    private IEnumerator OnIntertialClose(float time)
    {
        yield return new WaitForSeconds(time);
        if (adsType.Equals(AdType.GameOver))
            PhotonEventScript.instance.LeaveRoom();
    }
    public void OnUnityAdsDidFinish(string surfacingId, ShowResult showResult)
    {
        Debug.Log(surfacingId + "  " + showResult);
        if (showResult == ShowResult.Finished)
        {
            if (adsType.Equals(AdType.VideoRewarded))
                Debug.Log("AdTyp.Video Rewarded");
            else
            {
                StartCoroutine(OnIntertialClose(0f));
                RequestIntertitalAds();
            }
        }
        else if (showResult == ShowResult.Skipped)
        {
            if (adsType.Equals(AdType.VideoRewarded))
                Debug.Log("Rewarded video skipped!");
            else
            {
                StartCoroutine(OnIntertialClose(0f));
                RequestIntertitalAds();
            }
        }
        else if (showResult == ShowResult.Failed)
        {
            if (adsType.Equals(AdType.VideoRewarded))
                Debug.Log("Rewarded video failed!");
            else
            {
                StartCoroutine(OnIntertialClose(0f));
                RequestIntertitalAds();
            }
        }
    }
    public void OnUnityAdsReady(string surfacingId)
    {
        // If the ready Ad Unit or legacy Placement is rewarded, show the ad:
        //if (surfacingId == rewardedvideoId)
        //{
        //    Debug.Log("Ads is ready");
        //    // Optional actions to take when theAd Unit or legacy Placement becomes ready (for example, enable the rewarded ads button)
        //    //Advertisement.Show(intertitialId);
        //}
    }
    public void OnUnityAdsDidError(string message)
    {
        // Log the error.
        Debug.Log("OnUnityAdsDidError " + message);
    }
    public void OnUnityAdsDidStart(string surfacingId)
    {
        // Optional actions to take when the end-users triggers an ad.
    }
}
