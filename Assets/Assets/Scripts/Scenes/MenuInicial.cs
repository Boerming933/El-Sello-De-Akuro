using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MenuInicial : MonoBehaviour
{
    public Animator transitionAnimator;

    public void Jugar()
    {
        transitionAnimator.SetTrigger("FadeIn");
        StartCoroutine(ChangeScene());
    }

    public void Volver()
    {
        AudioManager.Instance.PlayMusic("WorldMusic");
        SceneManager.LoadScene(0);
    }

    public IEnumerator ChangeScene()
    {
        yield return new WaitForSeconds(1f);


        //AudioManager.Instance.PlayMusic("WorldMusic");
        SceneManager.LoadScene(1);
    }
}
