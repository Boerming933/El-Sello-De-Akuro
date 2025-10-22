using UnityEngine;

public class PlayerScenePos : MonoBehaviour
{
    public static PlayerScenePos Instance;

    public Vector3 lastPositionBeforeSceneChange;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
