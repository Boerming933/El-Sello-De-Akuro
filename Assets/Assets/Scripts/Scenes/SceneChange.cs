using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChange : MonoBehaviour
{
    private Scene escenaActiva;

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
                SceneManager.LoadScene(0);
            }
        }
        else if (escenaActiva.name == "SampleScene")
        {
            if (Input.GetKeyDown(KeyCode.Joystick1Button8) || Input.GetKeyDown(KeyCode.L))
            {
                SceneManager.LoadScene(1);
            }
        }
    }
}
