using UnityEngine.UI;
using UnityEngine;
using System;

public class UiManager : MonoBehaviour
{
    public static UiManager instance { get; set; }
    public ScrollRect units_scroll = null;
    public Transform turret_container = null;
    public GameObject[] turrets = null,  CoinsflyObject = null;
    public Text[] playername = null;
    public Image[] profile = null;
    public Sprite[] avtarSprite = null;
    public Toggle termstoggle = null;
    public Slider sfx_slider = null, music_slider = null;
    public Text game_start_text = null, loading_text = null ,countdown_timer_text = null, player_life_text = null, wave_text = null, units_count_text = null, waves_coming_text = null, message_text = null, player_name_text = null, totalcoins_text = null, gameover_title_text = null;
    public UIParticleSystem mainscreen_particle = null;
    public ParticleSystem main_screen_particle = null, main_scren_star_particle = null;
    public Image  health_slider = null, wave_icon = null;

    public GameObject game_bg = null, tutorial_icon = null, message_box = null, wave_popup = null, gameover_profile_container  = null,gameover_playerprofile_container = null;
    public InputField login_phone_no_field = null, register_username_field = null, register_phone_no_field = null, verify_otp_field = null;
    public GameObject main_bg_screen = null, login_screen = null, register_screen = null, verify_otp_screen = null, maingame_screen = null, hostgame_screen = null, gameplay_screen = null, loading_screen = null, gameover_Screen = null, profile_screen = null, game_quit_screen = null, setting_screen = null;
    public Button home_screen_profile_btn = null, login_with_apple_btn = null, login_with_google_btn = null, login_btn = null, signup_btn = null,  register_btn = null, verify_otp_btn = null, register_back_btn = null, forget_back_btn = null, join_random_room_btn = null, leave_room_btn = null, home_btn = null, privacy_policy_btn = null, terms_conditions_btn = null, logout_btn = null,
                           profile_back_btn = null, gameplay_home_btn = null, quit_yes_btn = null, quit_no_btn = null, quit_close_btn = null, setting_open_btn = null, setting_close_btn = null;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        setting_open_btn.interactable = false;
    }
    private void OnEnable()
    {
        GameManager.gameover += OnGameOver;
    }
    private void OnDisable()
    {
        GameManager.gameover -= OnGameOver;
    }
    internal void InitButtonEvents()
    {
        login_btn.onClick.AddListener(() => GameManager.instance.UserLogin());
        signup_btn.onClick.AddListener(() => ShowRegisterUI());
        register_btn.onClick.AddListener(() => GameManager.instance.UserRegister());
        register_back_btn.onClick.AddListener(() => ShowLoginUI());
        verify_otp_btn.onClick.AddListener(() => GameManager.instance.VerifyOTP());
        forget_back_btn.onClick.AddListener(() => ShowLoginUI());
        join_random_room_btn.onClick.AddListener(() => PhotonEventScript.instance.StartCoroutine(PhotonEventScript.instance.JoinRandomRooms(0f)));
        leave_room_btn.onClick.AddListener(() => PhotonEventScript.instance.LeaveRoom());
        home_btn.onClick.AddListener(() => OnHomeButtonClick());
        gameplay_home_btn.onClick.AddListener(() => game_quit_screen.SetActive(true));
        home_screen_profile_btn.onClick.AddListener(() => ShowProfileScreen());
        quit_yes_btn.onClick.AddListener(() => OnClickYesBtn());
        quit_no_btn.onClick.AddListener(() => game_quit_screen.SetActive(false));
        quit_close_btn.onClick.AddListener(() => game_quit_screen.SetActive(false));
        profile_back_btn.onClick.AddListener(() => ShowMainScreen());
        logout_btn.onClick.AddListener(() => GameManager.instance.Logout());
        setting_open_btn.onClick.AddListener(() => SettingUIState(true));
        setting_close_btn.onClick.AddListener(()=>SettingUIState(false));
        login_with_google_btn.onClick.AddListener(() => GPGAuthnitcation.instance.GoogleSignIn());
        login_with_apple_btn.onClick.AddListener(() => GPGAuthnitcation.instance.GoogleSignIn());
        privacy_policy_btn.onClick.AddListener(() => OpenUrl(Constant.privacypolicy_Url));
        terms_conditions_btn.onClick.AddListener(() => OpenUrl(Constant.termcondition_Url));
        music_slider.onValueChanged.AddListener(delegate { SoundManager.instance.MusicVolUpdate(music_slider.value); });
    }
    internal void InitButtonSounds()
    {
        login_btn.onClick.AddListener(() => SoundManager.instance.PlaySfx(SoundManager.instance.button_sfx, 1f));
        signup_btn.onClick.AddListener(() => SoundManager.instance.PlaySfx(SoundManager.instance.button_sfx, 1f));
        register_btn.onClick.AddListener(() => SoundManager.instance.PlaySfx(SoundManager.instance.button_sfx, 1f));
        register_back_btn.onClick.AddListener(() => SoundManager.instance.PlaySfx(SoundManager.instance.button_sfx, 1f));
        verify_otp_btn.onClick.AddListener(() => SoundManager.instance.PlaySfx(SoundManager.instance.button_sfx, 1f));
        forget_back_btn.onClick.AddListener(() => SoundManager.instance.PlaySfx(SoundManager.instance.button_sfx, 1f));
        join_random_room_btn.onClick.AddListener(() => SoundManager.instance.PlaySfx(SoundManager.instance.battle_btn_sfx, 0.5f));
        leave_room_btn.onClick.AddListener(() => SoundManager.instance.PlaySfx(SoundManager.instance.button_sfx, 1f));
        home_btn.onClick.AddListener(() => SoundManager.instance.PlaySfx(SoundManager.instance.button_sfx, 1f));
        profile_back_btn.onClick.AddListener(() => SoundManager.instance.PlaySfx(SoundManager.instance.button_sfx, 1f));
        logout_btn.onClick.AddListener(() => SoundManager.instance.PlaySfx(SoundManager.instance.button_sfx, 1f));
        home_screen_profile_btn.onClick.AddListener(() => SoundManager.instance.PlaySfx(SoundManager.instance.button_sfx, 1f));
        gameplay_home_btn.onClick.AddListener(() => SoundManager.instance.PlaySfx(SoundManager.instance.button_sfx, 1f));
        login_with_apple_btn.onClick.AddListener(() => SoundManager.instance.PlaySfx(SoundManager.instance.button_sfx, 1f));
        quit_yes_btn.onClick.AddListener(() => SoundManager.instance.PlaySfx(SoundManager.instance.button_sfx, 1f));
        quit_no_btn.onClick.AddListener(() => SoundManager.instance.PlaySfx(SoundManager.instance.button_sfx, 1f));
        quit_close_btn.onClick.AddListener(() => SoundManager.instance.PlaySfx(SoundManager.instance.button_sfx, 1f));
        login_with_google_btn.onClick.AddListener(() => SoundManager.instance.PlaySfx(SoundManager.instance.button_sfx, 1f));
        privacy_policy_btn.onClick.AddListener(() => SoundManager.instance.PlaySfx(SoundManager.instance.button_sfx, 1f));
        terms_conditions_btn.onClick.AddListener(() => SoundManager.instance.PlaySfx(SoundManager.instance.button_sfx, 1f));
    }
    internal void SetPlayernameOnUI()
    {
        int length = playername.Length;
        for(int i=0; i< length;i++)
        {
            playername[i].text = UserData.GetUsername();
        }
    }
    internal void SetplayerProfileOnUI(int index)
    {
        try
        {
            int length = profile.Length;
            for (int i = 0; i < length; i++)
            {
                profile[i].sprite = avtarSprite[index];
            }
        }
        catch (Exception e)
        {
            Debug.Log("SetplayerProfileOnUI" + e.Message);
        }
    }
    internal void InitTurretScrollerData()
    {
        int length = turrets.Length;
        for(int i=0; i< length; i++)
        {
            TowerData data = turrets[i].GetComponent<TowerData>();
            data.towerprice = (int)TowerDefence.TowerManager.GetTurretData(data.TowerName, TowerDefence.TowerManager.TurretsInfo.cost);
            data.tower_price_text.text = data.towerprice.ToString();
        }
    }
    private void OnHomeButtonClick()
    {
        if (!UserData.GetPremiumUser())
            AdsManager.instance.ShowIntertialAds(AdsManager.AdType.GameOver);
        else
            PhotonEventScript.instance.LeaveRoom();
    }
    private void OpenUrl(string url)
    {
        Application.OpenURL(url);
    }
    internal void ShowCoinsOnUI(int coins)
    {
        UpdateTurretsSprite(coins);
        totalcoins_text.text = coins.ToString();
        GameManager.instance.SetPlayerInfoRPC(GameManager.playerInfo.total_money_held, coins, UserData.GetUserId());
    }
    private void UpdateTurretsSprite(int coins)
    {
        int length = turrets.Length;
        for (int i = 0; i < length; i++)
        {
            TowerData data = turrets[i].GetComponent<TowerData>();
            if (data.towerprice <= coins)
                data.transform.GetChild(0).GetComponent<Image>().sprite = data.unlockSprite;
            else
                data.transform.GetChild(0).GetComponent<Image>().sprite = data.lockSpite;
        }
    }
    private void ShowProfileScreen()
    {
        CloseAllUI();
        main_bg_screen.SetActive(true);
        profile_screen.SetActive(true);
    }
    private void SettingUIState(bool state)
    {
        if(state)
        {
            sfx_slider.value = UserData.GetSfxVolume();
            music_slider.value = UserData.GetMusicVolume();
        }
        else
        {
            ShowMainScreen();
            UserData.SetSfxVolume(sfx_slider.value);
            UserData.SetMusicVolume(music_slider.value);
            SoundManager.instance.musicSource.volume = UserData.GetMusicVolume();
        }
        setting_screen.SetActive(state);
        SoundManager.instance.PlaySfx(SoundManager.instance.button_sfx, 1f);
    }
    internal void ShowLoginUI()
    {
        CloseAllUI();
        login_phone_no_field.text = string.Empty;
        main_bg_screen.SetActive(true);
        login_screen.SetActive(true);
        if (Application.platform == RuntimePlatform.Android)
        {
            login_with_google_btn.gameObject.SetActive(true);
            login_with_apple_btn.gameObject.SetActive(false);
        }
        else if(Application.platform == RuntimePlatform.IPhonePlayer)
        {
            login_with_google_btn.gameObject.SetActive(false);
            login_with_apple_btn.gameObject.SetActive(true);
        }
    }
    internal void ShowRegisterUI()
    {
        CloseAllUI();
        register_username_field.text = string.Empty;
        register_phone_no_field.text = string.Empty;
        main_bg_screen.SetActive(true);
        register_screen.SetActive(true);
    }
    internal void ShowVerifyOtpUI(string otp)
    {
        CloseAllUI();
        verify_otp_field.text = otp;
        main_bg_screen.SetActive(true);
        verify_otp_screen.SetActive(true);
    }
    internal void ShowMainScreen()
    {
        CloseAllUI();
        maingame_screen.SetActive(true);
        if (!main_screen_particle.gameObject.activeInHierarchy)
        {
            main_scren_star_particle.gameObject.SetActive(true);
            main_screen_particle.gameObject.SetActive(true);
            main_screen_particle.Play();
            main_scren_star_particle.Play();
        }
    }
    internal void ShowLoadingUI(bool state, string message)
    {
        loading_text.text = message;
        loading_screen.SetActive(state);
    }
    internal GameObject GetCoinUiObject()
    {
        int length = CoinsflyObject.Length;
        for(int i = 0; i< length; i++)
        {
            if (!CoinsflyObject[i].activeInHierarchy)
                return CoinsflyObject[i];
        }
        return null;
    }
    internal void CloseAllUI()
    {
        login_screen.SetActive(false);
        profile_screen.SetActive(false);
        register_screen.SetActive(false);
        main_bg_screen.SetActive(false);
        hostgame_screen.SetActive(false);
        gameplay_screen.SetActive(false);
        maingame_screen.SetActive(false);
        gameover_Screen.SetActive(false);
        verify_otp_screen.SetActive(false);
        game_quit_screen.SetActive(false);

        if (!main_screen_particle.gameObject.activeInHierarchy)
            return;
        main_scren_star_particle.gameObject.SetActive(false);
        main_screen_particle.gameObject.SetActive(false);
    }
    internal void ShowStarttingText(bool state)
    {
        game_start_text.gameObject.SetActive(state);
        game_start_text.text = string.Concat(UserData.GetUsername() , ",  " , GameManager.str_gamestart);
        countdown_timer_text.gameObject.SetActive(state);
    }
    private void OnClickYesBtn()
    {
        if(gameplay_screen.activeInHierarchy)
            GameManager.instance.ResetData(GameManager.Screens.MainUI, GameManager.GameState.Lose);
        else
        {
            game_quit_screen.SetActive(false);
            Application.Quit();
        }
    }
    internal void AndroidbackEvent()
    {
        if (loading_screen.activeInHierarchy)
            return;
        if (profile_screen.activeInHierarchy)
        {
            profile_screen.SetActive(false);
            ShowMainScreen();
        }
        else if (setting_screen.activeInHierarchy)
            SettingUIState(false);
        else if(maingame_screen.activeInHierarchy && !game_quit_screen.activeInHierarchy)
            game_quit_screen.SetActive(true);
        else if (game_quit_screen.activeInHierarchy)
            game_quit_screen.SetActive(false);
        else if (gameplay_screen.activeInHierarchy)
            game_quit_screen.SetActive(true);
        else if (gameover_Screen.activeInHierarchy || hostgame_screen.activeInHierarchy)
            OnHomeButtonClick();
        SoundManager.instance.PlaySfx(SoundManager.instance.button_sfx, 1f);
    }
    internal void ResetGameoverData()
    {
        int length = gameover_playerprofile_container.transform.childCount;
        for (int i = 0; i < length; i++)
        {
            PlayerInfo tempinfo = gameover_playerprofile_container.transform.GetChild(i).GetComponent<PlayerInfo>();
            tempinfo.awards_title_text.text = "-";
            tempinfo.playername_text.text = "-";
            tempinfo.coin_text.text = "-";
            tempinfo.air_kills_text.text = "-";
            tempinfo.ground_kills_text.text = "-";
            tempinfo.walls_produced_text.text = "-";
            tempinfo.wall_destroyed_text.text = "-";
            tempinfo.turret_build_text.text = "-";
            tempinfo.turret_destroyed_text.text = "-";
            tempinfo.detonated_mines_text.text = "-";
        }
    }
    private void OnGameOver()
    {
        game_bg.SetActive(false);
        health_slider.fillAmount = 1f;
        wave_text.text = "0";
        turret_container.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
    }
}
