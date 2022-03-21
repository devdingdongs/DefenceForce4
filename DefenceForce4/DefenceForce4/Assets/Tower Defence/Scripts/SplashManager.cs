using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Photon.Pun;

public class SplashManager : MonoBehaviour
{
    [SerializeField]
    private Image loadingBar = null;
    private bool issceneLoaded = false;
    private float temptimer = 0f, loadtimer = 0f, speed = 5f;
    private int[] loadingtimer = new int[5] {40,42,45,46,48};

    private void Start()
    {
        loadtimer = loadingtimer[ Random.Range(0, loadingtimer.Length)];
    }
    private void Update()
    {
        if (issceneLoaded) return;
        if (temptimer < loadtimer && !issceneLoaded)
        {
            temptimer += Time.deltaTime * speed;
            loadingBar.fillAmount = temptimer / loadtimer;
        }
        else
        {
            issceneLoaded = true;
            if (UserData.GetloginState())
            {
                if (PhotonEventScript.IsInternetConnected())
                {
                    if (PhotonNetwork.IsConnectedAndReady)
                    {
                        PhotonEventScript.instance.StartCoroutine(PhotonEventScript.instance.JoinRandomRooms(0f));
                        UiManager.instance.ShowMainScreen();
                    }   
                }
                else
                    UiManager.instance.ShowMainScreen();
            }
            else
                UiManager.instance.ShowLoginUI();
            SceneManager.UnloadSceneAsync(1, UnloadSceneOptions.None);
        }
    }
}
