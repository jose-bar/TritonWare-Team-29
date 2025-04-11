using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OneSoundEffects : MonoBehaviour
{
    public AudioSource src1, src2, src3;
    public AudioClip sfx1, sfx2, sfx3, sfx4;

    public void PlayJumpAudio() {
        src1.clip = sfx1;
        src1.Play();
    }

    public void PlayBumpAudio() {
        src2.clip = sfx2;
        src2.Play();
    }

    public void PlayCrouchAudio() {
        src3.clip = sfx3;
        src3.Play();
    }

    public void PlayUncrouchAudio() {
        src3.clip = sfx4;
        if (!src3.isPlaying) {
            src3.Play();
        }
    }

    public void StopAudio1() {
        src1.Stop();
    }

    public void StopAudio2() {
        src2.Stop();
    }

    public void StopAudio3() {
        src3.Stop();
    }
}