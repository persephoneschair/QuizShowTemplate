using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class AudioManager : SingletonMonoBehaviour<AudioManager>
{

    public AudioSource oneShotSource;
    public AudioSource loopingSource;

    private bool playedUnique;

    public enum OneShotClip { Impact, PodiumAnim, ClockTick, PlayerAnswer, WrongInFinal, ClockArm, Rotation, QStartAndEnd, PointTick };
    public AudioClip[] stings;

    public enum LoopClip { Titles, GameplayLoop, WinTheme, Credits };
    public AudioClip[] loops;

    #region Public Methods

    public void Play(OneShotClip oneShot, float delay = 0f)
    {
        if (delay != 0f)
            StartCoroutine(Delay(oneShot, delay));
        else
            oneShotSource.PlayOneShot(stings[(int)oneShot]);
    }

    public void PlayUnique(OneShotClip unique)
    {
        if (playedUnique)
            return;
        playedUnique = true;
        Play(unique);
        Invoke("CancelUnique", 5f);
    }

    public void StopLoop()
    {
        loopingSource.Stop();
    }

    public void Play(LoopClip loopClip, bool loop = true, float delay = 0f)
    {
        if(delay != 0f)
            StartCoroutine(Delay(loopClip, loop, delay));
        else
        {
            loopingSource.Stop();
            loopingSource.clip = loops[(int)loopClip];
            loopingSource.loop = loop;
            loopingSource.Play();
        }
    }

    public void Fade()
    {
        StartCoroutine(FadeOutLoop());
    }

    #endregion

    #region Private Methods

    private void CancelUnique()
    {
        playedUnique = false;
    }

    private IEnumerator Delay(OneShotClip oneShot, float delay)
    {
        yield return new WaitForSeconds(delay);
        Play(oneShot, 0f);
    }

    private IEnumerator Delay(LoopClip loopClip, bool loop, float delay)
    {
        yield return new WaitForSeconds(delay);
        Play(loopClip, loop);
    }

    private IEnumerator FadeOutLoop()
    {
        while (loopingSource.volume > 0)
        {
            yield return new WaitForSeconds(0.05f);
            loopingSource.volume -= 0.02f;
        }
        loopingSource.Stop();
        loopingSource.volume = 1;
    }

    #endregion
}
