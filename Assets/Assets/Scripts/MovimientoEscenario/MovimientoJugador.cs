using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MovimientoJugador : MonoBehaviour
{
    [SerializeField] private float speed = 5f;

    private Vector3 input;
    private Vector3 lastPosition;
    private string currentAnimState = "";

    [Header("Referencias")]
    public Zoom zoom;
    public Animator animator;
    public SpriteRenderer spriteRenderer;

    public SceneChange sceneChange;

    private bool horizontalMov = false;

    private const string ANIM_DOWN = "isDown";
    private const string ANIM_DOWN_LEFT = "isDownLeft";
    private const string ANIM_LEFT = "isLeft";
    private const string ANIM_UP_LEFT = "isUpLeft";
    private const string ANIM_UP = "isUp";

    void Start()
    {
        //if (PlayerScenePos.Instance != null)
        //{
        //    if (PlayerScenePos.Instance.lastPositionBeforeSceneChange != Vector3.zero)
        //    {
        //        transform.position = PlayerScenePos.Instance.lastPositionBeforeSceneChange;
        //    }
        //}
    }

    void Update()
    {
        float moveX = 0f;
        float moveY = 0f;

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            horizontalMov = true;
            moveX = -1f;
            spriteRenderer.flipX = false;
        }
        else if (Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.LeftArrow))
        {
            horizontalMov = false;
        }

        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            horizontalMov = true;
            moveX = 1f;
            spriteRenderer.flipX = true;
        }
        else if (Input.GetKeyUp(KeyCode.D) || Input.GetKeyUp(KeyCode.RightArrow))
        {
            horizontalMov = false;
        }

        if (Input.GetKey(KeyCode.W) && horizontalMov || Input.GetKey(KeyCode.UpArrow) && horizontalMov)
        {
            moveY = 0.5f;
        }
        else if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            moveY = 1f;
        }

        if (Input.GetKey(KeyCode.S) && horizontalMov || Input.GetKey(KeyCode.DownArrow) && horizontalMov)
        {
            moveY = -0.5f;
        }
        else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            moveY = -1f;
        }

        input = new Vector3(moveX, moveY, 0f);
        if (input.magnitude > 1f) input.Normalize();

        transform.Translate(input * speed * Time.deltaTime, Space.World);

        HandleAnimationKeys();

        if (lastPosition != transform.position)
        {
            zoom.ForceZoomOut();
            Camera.main.orthographicSize = 2.5f;
        }

        lastPosition = transform.position;
    }

    void HandleAnimationKeys()
    {
        bool up = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow);
        bool down = Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow);
        bool left = Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow);
        bool right = Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow);

        string newAnimState = "";

        if (!up && !down && !left && !right)
        {
            newAnimState = "";
        }
        else if ((down && left) || (down && right))
        {
            newAnimState = ANIM_DOWN_LEFT;
        }
        else if ((up && left) || (up && right))
        {
            newAnimState = ANIM_UP_LEFT;
        }
        else if (left || right)
        {
            newAnimState = ANIM_LEFT;
        }
        else if (down)
        {
            newAnimState = ANIM_DOWN;
        }
        else if (up)
        {
            newAnimState = ANIM_UP;
        }

        if (newAnimState != currentAnimState)
        {
            animator.SetBool(ANIM_DOWN, false);
            animator.SetBool(ANIM_DOWN_LEFT, false);
            animator.SetBool(ANIM_LEFT, false);
            animator.SetBool(ANIM_UP_LEFT, false);
            animator.SetBool(ANIM_UP, false);

            if (newAnimState != "")
            {
                animator.SetBool(newAnimState, true);
            }

            currentAnimState = newAnimState;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("TP") && sceneChange.canTP)
        {
            Debug.Log("Collision with TP detected");
            sceneChange.transitionAnimator.SetTrigger("FadeIn");
            StartCoroutine(SavePositionAndChangeScene());
        }
    }

    public IEnumerator SavePositionAndChangeScene()
    {
        yield return new WaitForSeconds(1f);

        transform.position = new Vector3(38.16f, 13.5f);

        if (PlayerScenePos.Instance != null)
        {
            PlayerScenePos.Instance.lastPositionBeforeSceneChange = transform.position;
        }

        AudioManager.Instance.PlayMusic("BattleMusic");
        
        SceneManager.LoadScene(2);
    }
}