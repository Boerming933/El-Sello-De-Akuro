using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChange : MonoBehaviour
{
    public Scene escenaActiva;
    public GameObject player;
    public Animator transitionAnimator;

    public bool canTP = true;

    void Awake()
    {
        Scene escenaActiva = SceneManager.GetActiveScene();
    }

    void Update()
    {
        Scene escenaActiva = SceneManager.GetActiveScene();

        if (escenaActiva.name == "SampleScene")
        {
            if (Input.GetKeyDown(KeyCode.Joystick1Button8) || Input.GetKeyDown(KeyCode.L))
            {
                transitionAnimator.SetTrigger("FadeIn");
                StartCoroutine(ChangeScene());
            }
        }
    }
    
    public IEnumerator ChangeScene()
    {
        yield return new WaitForSeconds(1f);
        
        AudioManager.Instance.PlayMusic("WorldMusic");
        SceneManager.LoadScene(2);
    }
}
