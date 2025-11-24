using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using System.Collections;

public class Cutscene : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public Animator transitionAnimator;

    void Start()
    {
        videoPlayer.loopPointReached += OnLoopPointReached;

        videoPlayer.Play();
    }

    void OnLoopPointReached(VideoPlayer vp)
    {
        transitionAnimator.SetTrigger("FadeIn");
        StartCoroutine(ChangeScene());
    }

    public IEnumerator ChangeScene()
    {
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene(1);
    }
}
