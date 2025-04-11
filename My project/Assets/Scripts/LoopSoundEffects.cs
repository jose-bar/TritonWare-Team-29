using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoopSoundEffects : MonoBehaviour
{
    public AudioSource src;
    public AudioClip sfx1;

    public void PlayMoveAudio() {
        src.clip = sfx1;
        if (!src.isPlaying) {
            src.Play();
        }
    }

    public void StopAudio() {
        src.Stop();
    }
}