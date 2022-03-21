using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance { get; set; }

    public AudioClip  bg_music = null, objectplace_sound = null, button_sfx = null, battle_btn_sfx = null,  wave_sfx = null, item_selcet_sfx = null,
                                 turret1_sfx = null, turret2_sfx = null, turret3_sfx = null, turret4_sfx = null, turret5_sfx = null, turret6_sfx = null, turret_6_explode_sfx = null,
                                 ground_mine_sfx = null, ground_mine_explode_sfx = null, wall_destroy_sfx = null;

    public AudioSource sfxSource = null, musicSource = null;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
    internal void PlaySfx(AudioClip clip, float volume)
    {
        if (UserData.GetSfxVolume().Equals(0))
            return;
        if (sfxSource != null)
            sfxSource.PlayOneShot(clip,UserData.GetSfxVolume()); 
    }
    internal void StopSfx()
    {
        if (sfxSource != null && sfxSource.isPlaying)
            sfxSource.Stop();
    }
    internal void PlayMusic()
    {
        try
        {
            if (musicSource != null)
            {
                musicSource.clip = bg_music;
                musicSource.Play();
                musicSource.volume = UserData.GetMusicVolume();
            }
        }
        catch(Exception e)
        {
            Debug.Log("PlayMusic " + e.Message);
        }
    }
    internal void MusicVolUpdate(float vol)
    {
        musicSource.volume = vol;
    }
}
