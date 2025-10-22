using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChange : MonoBehaviour
{
    private Scene escenaActiva;
    public GameObject player;
    public Animator transitionAnimator;

    void Awake()
    {
        Scene escenaActiva = SceneManager.GetActiveScene();
    }

    void Update()
    {
        Scene escenaActiva = SceneManager.GetActiveScene();

        if (escenaActiva.name == "NormalGameScene")
        {
            if (Input.GetKeyDown(KeyCode.Joystick1Button9) || Input.GetKeyDown(KeyCode.L))
            {
                transitionAnimator.SetTrigger("FadeIn");
                StartCoroutine(SavePositionAndChangeScene());
            }
        }
        else if (escenaActiva.name == "SampleScene")
        {
            if (Input.GetKeyDown(KeyCode.Joystick1Button8) || Input.GetKeyDown(KeyCode.L))
            {
                transitionAnimator.SetTrigger("FadeIn");
                StartCoroutine(ChangeScene());
            }
        }
    }

    private IEnumerator SavePositionAndChangeScene()
    {
        yield return new WaitForSeconds(1f);

        if (PlayerScenePos.Instance != null && player != null)
        {
            PlayerScenePos.Instance.lastPositionBeforeSceneChange = player.transform.position;
        }

        AudioManager.Instance.PlayMusic("BattleMusic");
        SceneManager.LoadScene(0);
    }

    private IEnumerator ChangeScene()
    {
        yield return new WaitForSeconds(1f);

        
        AudioManager.Instance.PlayMusic("WorldMusic");
        SceneManager.LoadScene(1);
    }
}
