using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    private Boolean pausedAudio = false;
    
    public AudioSource src;
    public AudioClip music;

    public void PlayAudio() {
        src.clip = music;
        if (!src.isPlaying && !pausedAudio) {
            src.Play();
        }
    }

    public void StopAudio() {
        src.Stop();
        pausedAudio = true;
    }

    public void PauseAudio() {
        src.Pause();
        pausedAudio = true;
    }

    public void ResumeAudio() {
        pausedAudio = false;
        src.Play();
    }
}