using UnityEngine;

public class UserData : MonoBehaviour
{
    private const string username = "username", userid = "userid", login = "login", loginid = "loginid", sfxvolume = "sfxvolume", musicvolume = "musicvolume",
                                    phonenumber = "phonenumber", premiumuser = "premiumuser", profileIndex = "profileIndex", tutorial = "tutorial";

    internal static void DeleteData()
    {
        PlayerPrefs.DeleteAll();
        Debug.Log("Delete playerprefs Data");
    }
    public static void SetTutorialState(bool state)
    {
        if (!PlayerPrefs.HasKey(tutorial))
            PlayerPrefs.SetInt(tutorial, booltoint(state));
        else
            PlayerPrefs.SetInt(tutorial, booltoint(state));
    }
    public static bool GetTutorialState()
    {
        int temp = 0;
        if (PlayerPrefs.HasKey(tutorial))
            temp = PlayerPrefs.GetInt(tutorial);
        else
        {
            SetTutorialState(false);
            temp = PlayerPrefs.GetInt(tutorial);
        }
        return inttobool(temp);
    }
    public static void SetloginState(bool state)
    {
        if (!PlayerPrefs.HasKey(login))
            PlayerPrefs.SetInt(login, booltoint(state));
        else
            PlayerPrefs.SetInt(login, booltoint(state));
    }
    public static bool GetloginState()
    {
        int temp = 0;
        if (PlayerPrefs.HasKey(login))
            temp = PlayerPrefs.GetInt(login);
        else
        {
            SetloginState(false);
            temp = PlayerPrefs.GetInt(login);
        }
        return inttobool(temp);
    }
    public static void SetLoginId(string id)
    {
        if (!PlayerPrefs.HasKey(loginid))
            PlayerPrefs.SetString(loginid, id);
        else
            PlayerPrefs.SetString(loginid, id);
    }
    public static string GetLoginId()
    {
        string loginId = string.Empty;
        if (PlayerPrefs.HasKey(loginid))
            loginId = PlayerPrefs.GetString(loginid);
        else
        {
            loginId = PlayerPrefs.GetString(loginid);
        }
        return loginId;
    }
    public static void SetPhoneNo(string number)
    {
        if (!PlayerPrefs.HasKey(phonenumber))
            PlayerPrefs.SetString(phonenumber, number);
        else
            PlayerPrefs.SetString(phonenumber, number);
    }
    public static string GetPhoneNo()
    {
        string phoneno = string.Empty;
        if (PlayerPrefs.HasKey(phonenumber))
            phoneno = PlayerPrefs.GetString(phonenumber);
        else
        {
            SetPhoneNo("0123456789");
            phoneno = PlayerPrefs.GetString(phonenumber);
        }
        return phoneno;
    }
    public static void SetProfileIndex(int id)
    {
        if (!PlayerPrefs.HasKey(profileIndex))
            PlayerPrefs.SetInt(profileIndex, id);
        else
            PlayerPrefs.SetInt(profileIndex, id);
    }
    public static int GetProfileIndex()
    {
        int tempIndex = 0;
        if (PlayerPrefs.HasKey(profileIndex))
            tempIndex = PlayerPrefs.GetInt(profileIndex);
        else
        {
            SetProfileIndex(0);
            tempIndex = PlayerPrefs.GetInt(profileIndex);
        }
        return tempIndex;
    }
    public static void SetUsername(string name)
    {
        if (!PlayerPrefs.HasKey(username))
            PlayerPrefs.SetString(username, name);
        else
            PlayerPrefs.SetString(username, name);
    }
    public static string GetUsername()
    {
        string name = string.Empty;

        if (PlayerPrefs.HasKey(username))
            name = PlayerPrefs.GetString(username);
        else
        {
            name = "Gamer " + Random.Range(100, 1000);
            SetUsername(name);
        }
        return name;
    }
    public static void SetUserId(string id)
    {
        if (!PlayerPrefs.HasKey(userid))
            PlayerPrefs.SetString(userid, id);
        else
            PlayerPrefs.SetString(userid, id);
    }
    public static string GetUserId()
    {
        string Userid = string.Empty;
        if (PlayerPrefs.HasKey(userid))
            Userid = PlayerPrefs.GetString(userid);
        else
        {
            SetUserId(string.Concat(GetUsername(), Random.Range(1, 1000000).ToString()));
            Userid = PlayerPrefs.GetString(userid);
        }
        return Userid;
    }
    public static void SetPremiumUser(bool state)
    {
        if (!PlayerPrefs.HasKey(premiumuser))
            PlayerPrefs.SetInt(premiumuser, booltoint(state));
        else
            PlayerPrefs.SetInt(premiumuser, booltoint(state));
    }
    public static bool GetPremiumUser()
    {
        int temp = 0;
        if (PlayerPrefs.HasKey(premiumuser))
            temp = PlayerPrefs.GetInt(premiumuser);
        else
        {
            SetPremiumUser(false);
            temp = PlayerPrefs.GetInt(premiumuser);
        }
        return inttobool(temp);
    }
    public static void SetSfxVolume(float id)
    {
        if (!PlayerPrefs.HasKey(sfxvolume))
            PlayerPrefs.SetFloat(sfxvolume, id);
        else
            PlayerPrefs.SetFloat(sfxvolume, id);
    }
    public static float GetSfxVolume()
    {
        float tempvalue = 0f;
        if (PlayerPrefs.HasKey(sfxvolume))
            tempvalue = PlayerPrefs.GetFloat(sfxvolume);
        else
        {
            SetSfxVolume(1f);
            tempvalue = PlayerPrefs.GetFloat(sfxvolume);
        }
        return tempvalue;
    }
    public static void SetMusicVolume(float id)
    {
        if (!PlayerPrefs.HasKey(musicvolume))
            PlayerPrefs.SetFloat(musicvolume, id);
        else
            PlayerPrefs.SetFloat(musicvolume, id);
    }
    public static float GetMusicVolume()
    {
        float tempvalue = 0f;
        if (PlayerPrefs.HasKey(musicvolume))
            tempvalue = PlayerPrefs.GetFloat(musicvolume);
        else
        {
            SetMusicVolume(1f);
            tempvalue = PlayerPrefs.GetFloat(musicvolume);
        }
        return tempvalue;
    }
    public static int booltoint(bool state)
    {
        if (state)
            return 1;
        else
            return 0;
    }
    public static bool inttobool(int state)
    {
        if (state == 0)
            return false;
        else
            return true;
    }
}
