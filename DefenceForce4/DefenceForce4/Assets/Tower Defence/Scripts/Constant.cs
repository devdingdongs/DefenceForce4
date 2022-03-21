using UnityEngine;

public class Constant : MonoBehaviour
{
    public static int  maxplayer = 4, minplayer = 4;
    public static readonly string enemy_str = "Enemy", bullet_str = "Bullet", str_centerwall = "centerwall", str_cannon = "Cannon", str_OuterGrid = "OuterGrid", str_InnerGrid = "InnerGrid", str_turret_1 = "Turret 1",
                                                  str_turret_2 = "Turret 2", str_turret_3 = "Turret 3", str_turret_4 = "Turret 4", str_turret_5 = "Turret 5", str_turret_6 = "Turret 6",
                                                  str_mine = "Mine", str_turret_7 = "Turret 7", str_balloonMine = "Balloon Mine", str_airUnit = "AirUnit", str_wall = "Wall", str_win = "Game Win", str_lose = "Game Lose",
                                                  str_wave = "Wave", str_units = "Units", str_itemplace_errormsg = "Can't place Item at this position!", str_nocoin_msg = "You have insufficient Coins !",
                                                  str_login_success = "You are Successfully login!", str_no_internet = "No internet connection !", str_loading = "Loading...", str_waitforinternet = "Waiting for internet connection...", str_matchfind = "Matching...",
                                                  str_bottomsideWall = "BottomSideWall", str_topsideWall = "TopSideWall", str_leftsideWall = "LeftSideWall", str_rightsideWall = "RightSideWall";

    public static readonly Color32[] turret_glowcolor = new Color32[4] {new Color32(126, 122, 110, 100), new Color32(107, 210, 150, 100), new Color32(192, 143, 54, 100), new Color32(199, 28, 28, 100) };
    public static readonly Vector3[] StartMapPos = new Vector3[4] {new Vector3(-1.55f, 0f,-2.8f), new Vector3(1.7f, 0f, -2.84f), new Vector3(-1.55f, 0f, 2.3f), new Vector3(1.75f, 0f, 2.3f) };
    public static readonly string[] Awards_Title = new string[9] { "Most Cowardly", "Hoarder", "Foolish Investor", "MVP" , "Demolition Man", "Warlord", "Least Prepared", "The Architect" , "Bad Planner" };                                                

    public static readonly string baseUrl = "http://dev9server.com/tower-defense-game/api/";
    public static readonly string privacypolicy_Url = "https://www.termsfeed.com/live/c9fa2091-9e30-496c-a21f-0a231cb041c7";
    public static readonly string termcondition_Url = "https://www.termsfeed.com/live/8c60846a-33d0-4ff9-900d-621038b94be5";
    public static readonly string loginUrl =  string.Concat(baseUrl, "login");
    public static readonly string registerUrl = string.Concat(baseUrl, "register");
    public static readonly string verify_otp_Url = string.Concat(baseUrl, "verify-otp");
    public static readonly string social_signin_Url = string.Concat(baseUrl, "social-login");
    public static readonly string GameData_Url = string.Concat(baseUrl, "settings");
   
}
