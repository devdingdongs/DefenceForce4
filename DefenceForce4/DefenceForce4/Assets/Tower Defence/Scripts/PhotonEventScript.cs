using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using Photon.Pun;
using UnityEngine.UI;
using System.Linq;
using System.Collections;

public class PhotonEventScript : MonoBehaviourPunCallbacks
{
    private int actornumber = 0;
    public static PhotonEventScript instance { get; set; }
    public PhotonView photonview = null;
    internal string gameVersion = "1";
    public List<GameObject> WaitingPlayerUI;

    private void Awake()
    {
        if (instance == null)
            instance = this;
    }
    public void LogintoPhoton()
    {
        if(!PhotonNetwork.IsConnected && IsInternetConnected())
        {
            PhotonNetwork.AutomaticallySyncScene = true;
            PhotonNetwork.ConnectUsingSettings();
            PhotonNetwork.GameVersion = gameVersion;
        }
    }
    public override void OnConnectedToMaster()
    {
        Debug.Log("Photon connected to master server");
        PhotonNetwork.JoinLobby();
        SetUsername();
    }
    public override void OnJoinedRoom()
    {
        Debug.Log("On Joined Room");
        Debug.Log("Room name : " + PhotonNetwork.CurrentRoom.Name);
        SetPlayerCustomProperties();
        StartCoroutine(WaitingPlayerUI_Update(3f));
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log("left player name : " + otherPlayer.NickName);
        if (GameManager.isGameStart)
            GameManager.instance.StartCoroutine(GameManager.instance.ShowCustomMessage(otherPlayer.CustomProperties["username"].ToString() + " left the game."));
        if (!GameManager.isGameStart && UiManager.instance.hostgame_screen.activeInHierarchy)
            StartCoroutine(WaitingPlayerUI_Update(1f));
    }
    public override void OnLeftRoom()
    {
        Debug.Log("Left room");
        TowerDefence.TowerManager.instance.DestroyTurrets();
        UiManager.instance.ShowMainScreen();
    }
    public void LeaveRoom()
    {
        if (PhotonNetwork.IsConnected)
            PhotonNetwork.LeaveRoom(false);
        else
            UiManager.instance.ShowMainScreen();
    }
    public override void OnJoinedLobby()
    {
        Debug.Log("Join Lobby");
        base.OnJoinedLobby();
    }
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.Log("Join Room : " + message);
        base.OnJoinRoomFailed(returnCode, message);
    }
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("OnJoinRandomFailed " + message);
        CreatePublicRoom();
    }
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        print("createRoomFailed" + message);
    }
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        print("enter room" + newPlayer.NickName);
        StartCoroutine(WaitingPlayerUI_Update(3f));
    }
    public override void OnDisconnected(DisconnectCause cause)
    {
        if (GameManager.isGameStart)
            GameManager.instance.ResetData(GameManager.Screens.MainUI, GameManager.GameState.Lose);
        Debug.LogWarningFormat("PUN Basics Tutorial/Launcher: OnDisconnected() was called by PUN with reason {0}", cause);
    }
    internal void SetUsername()
    {
        if (PhotonNetwork.IsConnectedAndReady)
            PhotonNetwork.LocalPlayer.NickName = UserData.GetUsername();
    }
    internal IEnumerator JoinRandomRooms(float time)
    {
        UiManager.instance.ShowLoadingUI(true, Constant.str_matchfind);
        yield return new WaitForSeconds(time);
        if (IsInternetConnected())
        {
            if (PhotonNetwork.IsConnectedAndReady)
                PhotonNetwork.JoinRandomRoom();
            else
            {
                UiManager.instance.ShowLoadingUI(false, Constant.str_matchfind);
                UiManager.instance.ShowMainScreen();
                LogintoPhoton();
            } 
        }
        else
        {
            UiManager.instance.ShowLoadingUI(false, Constant.str_matchfind);
            UiManager.instance.ShowMainScreen();
            GameManager.instance.StartCoroutine(GameManager.instance.ShowCustomMessage(Constant.str_no_internet));
        }
        UiManager.instance.setting_open_btn.interactable = true;
    }
    private IEnumerator WaitingPlayerUI_Update(float time)
    {
        yield return new WaitForSeconds(time);
        UiManager.instance.ShowLoadingUI(false, Constant.str_matchfind);
        int length = WaitingPlayerUI.Count;
        for (int i = 1; i < length; i++)
        {
            WaitingPlayerUI[i].transform.GetChild(1).GetComponent<Text>().text = "Waiting...";
        }
        WaitingPlayerUI[0].transform.GetChild(1).GetComponent<Text>().text = UserData.GetUsername();
        for (int i = 1; i <= PhotonNetwork.PlayerListOthers.Length; i++)
        {
            WaitingPlayerUI[i].transform.GetChild(1).GetComponent<Text>().text = PhotonNetwork.PlayerListOthers[i-1].CustomProperties["username"].ToString();
            WaitingPlayerUI[i].transform.GetChild(0).GetChild(0).GetComponent<Image>().sprite = UiManager.instance.avtarSprite[int.Parse(PhotonNetwork.PlayerListOthers[i - 1].CustomProperties["profileId"].ToString())];
        }
        if(!UiManager.instance.hostgame_screen.activeInHierarchy && PhotonNetwork.CurrentRoom.PlayerCount < Constant.minplayer)
        {
            UiManager.instance.CloseAllUI();
            UiManager.instance.hostgame_screen.SetActive(true);
        }
        if (PhotonNetwork.IsMasterClient)
        {
            if (PhotonNetwork.CurrentRoom.PlayerCount >= Constant.minplayer)
                photonview.RPC("GameStart", RpcTarget.All);    
        }
    }

    [PunRPC]
    private void GameStart()
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount >= Constant.minplayer)
        {
            GameManager.instance.ResetDefaultData();
            GameManager.instance.InitData();
            UiManager.instance.CloseAllUI();
            GameManager.instance.ShowWavesOnUI(GameManager.wave);
            UiManager.instance.game_bg.SetActive(true);
            UiManager.instance.gameplay_screen.SetActive(true);
            UiManager.instance.ShowStarttingText(true);
            UiManager.instance.ShowCoinsOnUI(GameManager.total_coins);
            if (!UserData.GetTutorialState())
                UiManager.instance.tutorial_icon.SetActive(true);
            else
                UiManager.instance.tutorial_icon.SetActive(false);
            GameManager.isGameStart = true;

            int length = PhotonNetwork.CurrentRoom.PlayerCount;
            for(int i=0; i< length; i++)
            {
                if (UserData.GetUserId().Equals(PhotonNetwork.PlayerList[i].CustomProperties["userId"].ToString()))
                    actornumber = i;    
            }
            if (PhotonNetwork.LocalPlayer.CustomProperties["username"].ToString().Equals(UserData.GetUsername()))
            {
                CameraManager.instance.CamParent.transform.position = Constant.StartMapPos[actornumber];
                GameManager.instance.playerData.Add(new PlayerData() { user_id = UserData.GetUserId(), profileSprite = UiManager.instance.avtarSprite[UserData.GetProfileIndex()], player_actornumber = PhotonNetwork.LocalPlayer.ActorNumber, player_name = UserData.GetUsername() });
            }
            for (int i = 0; i < PhotonNetwork.PlayerListOthers.Length; i++)
            {
                GameManager.instance.playerData.Add(new PlayerData() {user_id = PhotonNetwork.PlayerListOthers[i].CustomProperties["userId"].ToString(), profileSprite = UiManager.instance.avtarSprite[int.Parse(PhotonNetwork.PlayerListOthers[i].CustomProperties["profileId"].ToString())], player_actornumber = PhotonNetwork.PlayerListOthers[i].ActorNumber, player_name = PhotonNetwork.PlayerListOthers[i].CustomProperties["username"].ToString()});
            }
            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.CurrentRoom.IsOpen = false;
                PhotonNetwork.CurrentRoom.IsVisible = false;
            }
        }
    }
    private void CreatePublicRoom()
    {
        PhotonNetwork.CreateRoom(null, new RoomOptions() { MaxPlayers = (byte)Constant.maxplayer,
                                                           IsVisible = true, IsOpen = true, CleanupCacheOnLeave = false },TypedLobby.Default);
    }
    internal void SetPlayerCustomProperties()
    {
        ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable();
        props.Add("profileId", UserData.GetProfileIndex());
        props.Add("username", UserData.GetUsername());
        props.Add("userId", UserData.GetUserId());
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
    }
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        var Tempstr = "OnPlayerPropertiesUpdate => " + targetPlayer.NickName;
        for (int i = 0; i < changedProps.Count; i++)
        {
            var item = changedProps.ElementAt(i);
            DictionaryEntry dictionaryEntry = new DictionaryEntry(item.Key, item.Value);
            Tempstr += dictionaryEntry.Key.ToString() + " = " + dictionaryEntry.Value.ToString() + "  ";
            if (dictionaryEntry.Key.ToString().Equals("profileId"))
                Debug.Log("profile Update ");
        }
    }
    public static bool IsInternetConnected()
    {
        if (Application.internetReachability != NetworkReachability.NotReachable)
            return true;
        else
            return false;
    }
}
