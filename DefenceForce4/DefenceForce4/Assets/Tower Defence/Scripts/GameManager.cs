using LitJson;
using System;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

    public class GameManager : MonoBehaviour
    {
        public static GameManager instance { get; set; }
        public static bool isGameStart { get; set; }
        public static bool isWaveStart { get; set; }
        public static bool iswallAdd { get; set; }
       [SerializeField]
        private PhotonView photonview = null;
        private float countdownTimer = 6f;
        internal static float player_life = 50f, current_life = 0f;
        internal static int wave = 0, ground_npc_kills = 0, air_npc_kills = 0, total_coins = 100, currency_spent = 0, turret_build_count = 0, wall_produced = 0, detonated_mines = 0, coinmultiplier = 1;

        public static string str_gamestart = string.Empty;
        public ParticleSystem objectplace_particle = null, deathparticle = null, turret1_hit_effect = null, turret2_hit_effect = null, turret3_hit_effect = null,
                                             turret5_hit_effect = null, turret6_hit_effect = null, ground_mine_particle = null, air_mine_particle = null;

        private string registrationType = string.Empty, phone_no = string.Empty;
        public List<PlayerData> playerData = null;
        public delegate void OnGameOver();
        public static event OnGameOver gameover;
    public enum playerInfo
    {
        award_title,
        air_kills,
        ground_kills,
        total_money_held,
        walls_produced,
        wall_destroyed,
        turret_destroyed,
        turret_build,
        detonated_mines,
        currency_Spent
    };
    public enum Screens
    {
        MainUI = 0,
        GameoverUI = 1
    };
    public enum GameState
    {
        Win = 0,
        Lose = 1
    };
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }       
        SceneManager.LoadScene(1, LoadSceneMode.Additive);
    }
    private void Start()
    {
        SetQualitySetting();
        StartCoroutine(GetGameData());
        CameraManager.instance.Inititalize();
        AdsManager.instance.Initialize();
        PhotonEventScript.instance.LogintoPhoton();
        UiManager.instance.InitButtonSounds();
        UiManager.instance.InitButtonEvents();
        GridManager.instance.InitGridData();
        SoundManager.instance.PlayMusic();
        UiManager.instance.SetPlayernameOnUI();
        UiManager.instance.SetplayerProfileOnUI(UserData.GetProfileIndex());
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
       
#if UNITY_EDITOR
        Application.targetFrameRate = 60;
#endif
    }
    private void Update()
        {
            //if (Input.GetKey(KeyCode.R))
            //    UserData.DeleteData();
            if (!SoundManager.instance.musicSource.isPlaying)
                SoundManager.instance.PlayMusic();
            if (Input.GetKeyDown(KeyCode.Escape))
                UiManager.instance.AndroidbackEvent();

            if (!isGameStart) 
                return;
            ShowCountdownTimer();
        }
        internal void InitData()
        {
            int length = SpawnEnemy.instance.Enemy.gamedata.Length;
            for (int i = 0; i < length; i++)
            {
                if (SpawnEnemy.instance.Enemy.gamedata[i].name.Equals("Player Life"))
                { 
                    player_life = float.Parse(SpawnEnemy.instance.Enemy.gamedata[i].value);
                    current_life = player_life;
                    UiManager.instance.player_life_text.text = string.Concat(current_life.ToString(), "/", player_life.ToString());
                }
                else if (SpawnEnemy.instance.Enemy.gamedata[i].name.Equals("Initial Coin"))
                    total_coins = int.Parse(SpawnEnemy.instance.Enemy.gamedata[i].value);
                else if (SpawnEnemy.instance.Enemy.gamedata[i].name.Equals("Starting Text"))
                    str_gamestart = SpawnEnemy.instance.Enemy.gamedata[i].value;
            }
            UiManager.instance.InitTurretScrollerData();
        }
    private void ShowCountdownTimer()
    {
        if (countdownTimer > 0 && !isWaveStart)
        {
            countdownTimer -= Time.deltaTime;
            if (countdownTimer >= 1)
                UiManager.instance.countdown_timer_text.text = ((int)countdownTimer).ToString();
            else
                UiManager.instance.countdown_timer_text.text = "Go!";
            if (countdownTimer <= 0)
            {
                isWaveStart = true;
                UiManager.instance.ShowStarttingText(false);
                SpawnEnemy.instance.Inititalize();
            }
        }
    }
    public void SetAvtarOnUI(int avtarindex)
    {
        UserData.SetProfileIndex(avtarindex);
        UiManager.instance.SetplayerProfileOnUI(UserData.GetProfileIndex());
        PhotonEventScript.instance.SetPlayerCustomProperties();
        SoundManager.instance.PlaySfx(SoundManager.instance.button_sfx, 1f);
    }
    internal void GameOver(GameState state)
        {
            photonview.RPC("ResetData", RpcTarget.All, Screens.GameoverUI, state);
            Debug.Log("Game Over");
        }
        [PunRPC]
        internal void ResetData(Screens tempscreen, GameState state)
        {
            isGameStart = false;
            gameover();
            GridManager.instance.ResetGridData();
            CameraManager.instance.ResetData();
            StopAllParticles();
            if (tempscreen.Equals(Screens.GameoverUI))
            {
                if (state.Equals(GameState.Win))
                    UiManager.instance.gameover_title_text.text = Constant.str_win;
                else if (state.Equals(GameState.Lose))
                    UiManager.instance.gameover_title_text.text = Constant.str_lose;
                ShowGameOverScreen();
            }
            else if (tempscreen.Equals(Screens.MainUI))
                PhotonEventScript.instance.LeaveRoom();
            SoundManager.instance.StopSfx();
        }
    internal void ResetDefaultData()
    {
        playerData.Clear();
        countdownTimer = 6;
        isWaveStart = false;
        iswallAdd = false;
        wave = 0;
        ground_npc_kills = 0;
        air_npc_kills = 0;
        total_coins = 0;
        turret_build_count = 0;
        wall_produced = 0;
        currency_spent = 0;
        coinmultiplier = 1;
        detonated_mines = 0;
    }
    internal void ShowWavesOnUI(int waves)
        {
            wave = waves;
            UiManager.instance.wave_text.text = string.Concat(Constant.str_wave, "    ", (wave + 1), "/", SpawnEnemy.instance.maxSpawnWave);
            UiManager.instance.units_count_text.text = string.Concat(Constant.str_units, "     ", SpawnEnemy.instance.Enemy.uints[wave].maxSpawn);
        }
        internal void UpdatePlayerHealth(float damage)
        {
            current_life -= damage;
            UiManager.instance.health_slider.fillAmount = (current_life / player_life);
            UiManager.instance.player_life_text.text = string.Concat(current_life.ToString(), "/", player_life.ToString());
            if (UiManager.instance.health_slider.fillAmount <= 0 && PhotonNetwork.IsMasterClient)
                GameOver(GameState.Lose);
        }
        internal IEnumerator ShowCustomMessage(string message)
        {
            if (UiManager.instance.message_box.activeInHierarchy)
                UiManager.instance.message_box.SetActive(false);
            if (!UiManager.instance.message_box.activeInHierarchy)
            {
                UiManager.instance.message_text.text = message;
                UiManager.instance.message_box.SetActive(true);
                yield return new WaitForSecondsRealtime(2.5f);
                UiManager.instance.message_box.SetActive(false);
            }
        }
    internal IEnumerator ShowCoinFly(int coins, Vector3 pos)
    {
        GameObject tempObject = UiManager.instance.GetCoinUiObject();
        if (tempObject != null)
        {
            tempObject.transform.GetChild(0).GetChild(0).GetComponent<Text>().text = string.Concat("+ ", coins.ToString());
            tempObject.transform.position = CameraManager.instance.maincamera.WorldToScreenPoint(pos);
            tempObject.SetActive(true);
            yield return new WaitForSecondsRealtime(2.5f);
            tempObject.SetActive(false);
        }
      
    }
    internal void PlayParticle(ParticleSystem tempparticle, Vector3 position)
        {
            if (!isGameStart)
                return;
            tempparticle.gameObject.transform.position = new Vector3(position.x, position.y + 0.1f, position.z);
            tempparticle.Play();
        }
        private void StopAllParticles()
        {
            if (turret1_hit_effect.isPlaying)
                turret1_hit_effect.Stop();
            if (turret2_hit_effect.isPlaying)
                turret2_hit_effect.Stop();
            if (turret3_hit_effect.isPlaying)
                turret3_hit_effect.Stop();
            if (turret5_hit_effect.isPlaying)
                turret5_hit_effect.Stop();
            if (turret6_hit_effect.isPlaying)
                turret6_hit_effect.Stop();
            if (objectplace_particle.isPlaying)
                objectplace_particle.Stop();
            if (deathparticle.isPlaying)
                deathparticle.Stop();
            if (ground_mine_particle.isPlaying)
                ground_mine_particle.Stop();
            if (air_mine_particle.isPlaying)
                air_mine_particle.Stop();
        }
        internal void UserLogin()
        {
            if (PhotonEventScript.IsInternetConnected())
            {
                registrationType = "login";
                phone_no = UiManager.instance.login_phone_no_field.text;
                StartCoroutine(SignIn());
            }
            else
                StartCoroutine(ShowCustomMessage(Constant.str_no_internet));
        }
    internal void UserRegister()
    {
        if (PhotonEventScript.IsInternetConnected())
        {
            if (UiManager.instance.termstoggle.isOn)
            {
                registrationType = "register";
                phone_no = UiManager.instance.register_phone_no_field.text;
                StartCoroutine(SignUp());
            }
            else
                StartCoroutine(ShowCustomMessage("Please accept terms and conditions."));
        }
        else
            StartCoroutine(ShowCustomMessage(Constant.str_no_internet));
    }
    internal void VerifyOTP()
        {
            if (PhotonEventScript.IsInternetConnected())
            {
                if (UiManager.instance.verify_otp_field.text != string.Empty)
                    StartCoroutine(Otp_Verify());
                else
                    StartCoroutine(ShowCustomMessage("OTP field is required."));
            }
            else
                StartCoroutine(ShowCustomMessage(Constant.str_no_internet));
        }
    private IEnumerator SignIn()
    {
        WWWForm form = new WWWForm();
        form.AddField("contact", phone_no);
        UiManager.instance.ShowLoadingUI(true, Constant.str_loading);
        using (UnityWebRequest www = UnityWebRequest.Post(Constant.loginUrl, form))
        {
            yield return www.SendWebRequest();
            if (www.isNetworkError || www.isHttpError)
            {
                StartCoroutine(ShowCustomMessage(www.error));
                UiManager.instance.ShowLoadingUI(false, Constant.str_loading);
            }
            else
            {
               // Debug.Log(www.downloadHandler.text);
                JsonData data = JsonMapper.ToObject(www.downloadHandler.text);
                UiManager.instance.ShowLoadingUI(false, Constant.str_loading);
                if (data["status"].ToString() == "True")
                    UiManager.instance.ShowVerifyOtpUI(data[2]["otp"].ToString());
                else
                    StartCoroutine(ShowCustomMessage(data["message"].ToString()));
            }
        }
    }
    private IEnumerator SignUp()
    {
        WWWForm form = new WWWForm();
        form.AddField("user_name", UiManager.instance.register_username_field.text.ToString());
        form.AddField("phone_no", phone_no);
        UiManager.instance.ShowLoadingUI(true, Constant.str_loading);
        using (UnityWebRequest www = UnityWebRequest.Post(Constant.registerUrl, form))
        {
            yield return www.SendWebRequest();
            if (www.isNetworkError || www.isHttpError)
            {
                UiManager.instance.ShowLoadingUI(false, Constant.str_loading);
                StartCoroutine(ShowCustomMessage(www.error));
            }
            else
            {
               // Debug.Log(www.downloadHandler.text);
                UiManager.instance.ShowLoadingUI(false, Constant.str_loading);
                JsonData jval = JsonMapper.ToObject(www.downloadHandler.text);
                if (jval["status"].ToString() == "True")
                    UiManager.instance.ShowVerifyOtpUI(jval[2]["otp"].ToString());
                else
                    StartCoroutine(ShowCustomMessage(jval["message"].ToString()));
            }
        }
    }
    private IEnumerator Otp_Verify()
    {
        WWWForm form = new WWWForm();
        form.AddField("type", registrationType);
        form.AddField("otp", UiManager.instance.verify_otp_field.text);
        form.AddField("phone_no", phone_no);

        UiManager.instance.ShowLoadingUI(true, Constant.str_loading);
        using (UnityWebRequest www = UnityWebRequest.Post(Constant.verify_otp_Url, form))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                UiManager.instance.ShowLoadingUI(false, Constant.str_loading);
                StartCoroutine(ShowCustomMessage(www.error));
            }
            else
            {
                // Debug.Log(www.downloadHandler.text);
                JsonData jval = JsonMapper.ToObject(www.downloadHandler.text);
                if (jval["status"].ToString() == "True")
                {
                    UserData.SetLoginId(jval[2]["id"].ToString());
                    UserData.SetUsername(jval[2]["user_name"].ToString());
                    UserData.GetUserId();
                    UiManager.instance.SetPlayernameOnUI();
                    UserData.SetPhoneNo(phone_no);
                    UserData.SetloginState(true);
                    StartCoroutine(ShowCustomMessage(Constant.str_login_success));
                    if (PhotonNetwork.IsConnectedAndReady)
                    {
                        PhotonEventScript.instance.StartCoroutine(PhotonEventScript.instance.JoinRandomRooms(2.5f));
                        UiManager.instance.ShowMainScreen();
                    }
                    else
                        UiManager.instance.ShowLoadingUI(false, Constant.str_loading);
                }
                else
                {
                    UiManager.instance.ShowLoadingUI(false, Constant.str_loading);
                    StartCoroutine(ShowCustomMessage("Incorrect OTP."));
                }
            }
        }
    }
    internal IEnumerator SocialSignIn(string socialId, string userName)
    {
        WWWForm form = new WWWForm();
        form.AddField("user name", userName);
        form.AddField("social id", socialId);
        UiManager.instance.ShowLoadingUI(true, Constant.str_loading);
        using (UnityWebRequest www = UnityWebRequest.Post(Constant.social_signin_Url, form))
        {
            yield return www.SendWebRequest();
            if (www.isNetworkError || www.isHttpError)
            {
                StartCoroutine(ShowCustomMessage(www.error));
                UiManager.instance.ShowLoadingUI(false, Constant.str_loading);
            }
            else
            {
                JsonData data = JsonMapper.ToObject(www.downloadHandler.text);
                UiManager.instance.ShowLoadingUI(false, Constant.str_loading);
                if (data["status"].ToString() == "True")
                {
                    UserData.GetUserId();
                    UserData.SetloginState(true);
                    StartCoroutine(ShowCustomMessage(Constant.str_login_success));
                    if (PhotonNetwork.IsConnectedAndReady)
                    {
                        PhotonEventScript.instance.StartCoroutine(PhotonEventScript.instance.JoinRandomRooms(2.5f));
                        UiManager.instance.ShowMainScreen();
                    }
                    else
                        UiManager.instance.ShowLoadingUI(false, Constant.str_loading);
                }
                else
                {
                    UiManager.instance.ShowLoadingUI(false, Constant.str_loading);
                    StartCoroutine(ShowCustomMessage(data["message"].ToString()));
                }     
            }
        }
    }
    internal IEnumerator GetGameData()
        {
            if (PhotonEventScript.IsInternetConnected())
            {
                WWWForm form = new WWWForm();
                using (UnityWebRequest www = UnityWebRequest.Get(Constant.GameData_Url))
                {
                    yield return www.SendWebRequest();
                    if (www.isNetworkError || www.isHttpError)
                        Debug.Log("GetGameData : " + www.error);
                    else
                    {
                        // Debug.Log(www.downloadHandler.text);
                        JsonData data = JsonMapper.ToObject(www.downloadHandler.text);
                        if (data["status"].ToString() == "True")
                        {
                            int wavecount = SpawnEnemy.instance.maxSpawnWave;//data[2][0].Count;
                            for (int i = 0; i < wavecount; i++)
                            {
                                SpawnEnemy.instance.Enemy.uints[i].waveTitle = data[2][0][i]["waves_title"].ToString();
                                SpawnEnemy.instance.Enemy.uints[i].maxSpawn = (int)data[2][0][i]["spawn"];
                                SpawnEnemy.instance.Enemy.uints[i].health = (int)data[2][0][i]["health"];
                            }
                            int turretcount = data[2][1].Count;
                            for (int i = 0; i < turretcount; i++)
                            {
                                SpawnEnemy.instance.Enemy.turrets[i].turretName = data[2][1][i]["turrets_type"].ToString();
                                SpawnEnemy.instance.Enemy.turrets[i].turretRange = (int)data[2][1][i]["turret_range"];
                                SpawnEnemy.instance.Enemy.turrets[i].damage = (int)data[2][1][i]["damage"];
                                SpawnEnemy.instance.Enemy.turrets[i].attackSpeed = (int)data[2][1][i]["attack_speed"];
                                SpawnEnemy.instance.Enemy.turrets[i].cost = (int)data[2][1][i]["cost"];
                                SpawnEnemy.instance.Enemy.turrets[i].health = (int)data[2][1][i]["health"];
                            }
                            int gamedatacount = data[2][2].Count;
                            for (int i = 0; i < gamedatacount; i++)
                            {
                                SpawnEnemy.instance.Enemy.gamedata[i].name = data[2][2][i]["name"].ToString();
                                SpawnEnemy.instance.Enemy.gamedata[i].value = data[2][2][i]["value"].ToString();
                            }
                            Debug.Log("Game Data get Successfully");
                        }
                        else
                            Debug.Log("GetGameData : " + data["message"].ToString());
                    }
                }
            }
        }
        internal void Logout()
        {
            UserData.DeleteData();
            GPGAuthnitcation.instance.LogOutGoogleSignIn();
            UiManager.instance.ShowLoginUI();
        }
    internal void SetPlayerInfoRPC(playerInfo info, int data, string userId)
    {
        if (PhotonNetwork.IsConnectedAndReady)
            photonview.RPC("SetPlayerData", RpcTarget.All, info, data, userId);
    }
    [PunRPC]
    private void SetPlayerData(playerInfo info, int data, string userId)
    {
        try
        {
            for (int i = 0; i < playerData.Count; i++)
            {
                if (playerData[i].user_id.Equals(userId))
                {
                    if (info.Equals(playerInfo.air_kills))
                        playerData[i].air_kills = data;
                    else if (info.Equals(playerInfo.ground_kills))
                        playerData[i].ground_kills = data;
                    else if (info.Equals(playerInfo.walls_produced))
                        playerData[i].walls_produced = data;
                    else if (info.Equals(playerInfo.wall_destroyed))
                        playerData[i].wall_destroyed = data;
                    else if (info.Equals(playerInfo.turret_destroyed))
                        playerData[i].turret_destroyed = data;
                    else if (info.Equals(playerInfo.turret_build))
                        playerData[i].turret_build = data;
                    else if (info.Equals(playerInfo.detonated_mines))
                        playerData[i].detonated_mines = data;
                    else if (info.Equals(playerInfo.total_money_held))
                        playerData[i].total_money = data;
                    else if (info.Equals(playerInfo.currency_Spent))
                        playerData[i].currencySpent = data;
                }
            }
        }
        catch (Exception e)
        {
            Debug.Log("SetPlayerData : " + e.Message);
        }
    }
    internal void ShowGameOverScreen()
    {
        UiManager.instance.CloseAllUI();
        UiManager.instance.ResetGameoverData();
        UiManager.instance.gameover_Screen.SetActive(true);
        int length = playerData.Count;
        for (int i = 0; i < length; i++)
        {
            PlayerInfo tempinfo = UiManager.instance.gameover_playerprofile_container.transform.GetChild(i).GetComponent<PlayerInfo>();
            tempinfo.awards_title_text.text = GetAward(playerData[i].turret_build, playerData[i].walls_produced, playerData[i].ground_kills, playerData[i].air_kills, playerData[i].detonated_mines, playerData[i].currencySpent);
            tempinfo.playername_text.text = playerData[i].player_name;
            tempinfo.profile.sprite = playerData[i].profileSprite;
            tempinfo.coin_text.text = playerData[i].total_money.ToString();
            tempinfo.air_kills_text.text = playerData[i].air_kills.ToString();
            tempinfo.ground_kills_text.text = playerData[i].ground_kills.ToString();
            tempinfo.walls_produced_text.text = playerData[i].walls_produced.ToString();
            tempinfo.wall_destroyed_text.text = playerData[i].wall_destroyed.ToString();
            tempinfo.turret_build_text.text = playerData[i].turret_build.ToString();
            tempinfo.turret_destroyed_text.text = playerData[i].turret_destroyed.ToString();
            tempinfo.detonated_mines_text.text = playerData[i].detonated_mines.ToString();
        }
    }
    internal string GetAward(int turretBuildCount, int wallProduced, int groundKills, int airKills, int detonatedMines, int currencySpent)
    {
        string tempreward = string.Empty;
        if (turretBuildCount > 10)
            tempreward = Constant.Awards_Title[0];
        else if ((groundKills + airKills) < 5)
            tempreward = Constant.Awards_Title[6];
        else if ((groundKills + airKills) > 15)
            tempreward = Constant.Awards_Title[5];
        else if (detonatedMines > 5)
            tempreward = Constant.Awards_Title[4];
        else if (currencySpent < 100)
            tempreward = Constant.Awards_Title[2];
        else if (currencySpent > 100)
            tempreward = Constant.Awards_Title[3];
        else if ((wallProduced > 5 && wallProduced < 10) || turret_build_count < 5)
            tempreward = Constant.Awards_Title[8];
        else if (wallProduced > 10)
            tempreward = Constant.Awards_Title[7];
        else
            tempreward = Constant.Awards_Title[1];
        return tempreward;
    }
    private void SetQualitySetting()
    {
        if (SystemInfo.systemMemorySize <= 3048)
            QualitySettings.SetQualityLevel(1);
        else
            QualitySettings.SetQualityLevel(2);
    }
    private void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            if (!PhotonNetwork.IsConnectedAndReady)
                return;
            if (PhotonNetwork.IsMasterClient && PhotonNetwork.InRoom && PhotonNetwork.CurrentRoom.PlayerCount > 1)
            {
                SpawnEnemy.instance.TrasnferWaveOwnerships();
                PhotonNetwork.SetMasterClient(PhotonNetwork.LocalPlayer.GetNext());
                PhotonNetwork.SendAllOutgoingCommands();
            }
        }
    }
    private void OnApplicationFocus(bool focus)
    {
        if (!PhotonEventScript.IsInternetConnected() && !isGameStart)
            UiManager.instance.ShowLoadingUI(true, Constant.str_waitforinternet);
        else if (PhotonEventScript.IsInternetConnected() && !isGameStart)
        {
            UiManager.instance.ShowLoadingUI(false, Constant.str_waitforinternet);
            if (!PhotonNetwork.IsConnectedAndReady)
                PhotonEventScript.instance.LogintoPhoton();
        }
    }
}
    [Serializable]
    public class PlayerData
    {
        public Sprite profileSprite = null;
        public string player_name = string.Empty, user_id = string.Empty;
        public int player_actornumber, total_money, air_kills, ground_kills, walls_produced, wall_destroyed, turret_destroyed, turret_build, detonated_mines, currencySpent;
    }
