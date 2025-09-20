using UnityEngine;
using UnityEngine.InputSystem;

namespace ClearSky
{
    public class SimplePlayerController : MonoBehaviour
    {
        public float movePower = 10f;
        public float gravity = -9.81f;

        private CharacterController characterController;
        private Animator anim;
        private int direction = 1;

        Vector3 moveDirection;
        GameInputHandler gameInputHandler;
        PlayerHarvestController playerHarvestController;

        void OnEnable()
        {
            gameInputHandler = FindObjectOfType<GameInputHandler>();
            Debug.Assert(gameInputHandler != null, "GameInputHandler not found in the scene.");
            gameInputHandler.OnPlayerMoveRequested += OnPlayerMove;
        }

        void Start()
        {
            playerHarvestController = GetComponent<PlayerHarvestController>();
            characterController = GetComponent<CharacterController>();
            anim = GetComponent<Animator>();

            // UI 켜지면 멈춤
            GameManager.Instance.onUIOn += StopMove;

            // 수확 모드 들어가면 멈춤
            playerHarvestController.onEnterHarvestMode += StopMove;
            playerHarvestController.onEnterHarvestMode += Attack;

        }
        
        void OnDisable()
        {
            gameInputHandler.OnPlayerMoveRequested -= OnPlayerMove;
            GameManager.Instance.onUIOn -= StopMove;
            playerHarvestController.onEnterHarvestMode -= StopMove;
            playerHarvestController.onEnterHarvestMode -= Attack;
        }

        private void Update()
        {
            if (characterController.isGrounded)
            {
                moveDirection.y += gravity * Time.deltaTime;
            }

            Run();
        }

        public void OnPlayerMove(Vector2 move)
        {
            moveDirection.x = move.x;
            moveDirection.z = move.y;
        }


        void Run()
        {
            anim.SetBool("isRun", false);


            if (moveDirection.x < 0)
            {
                direction = -1;

                transform.localScale = new Vector3(direction, 1, 1);
                if (!anim.GetBool("isJump"))
                    anim.SetBool("isRun", true);

            }
            if (moveDirection.x > 0)
            {
                direction = 1;

                transform.localScale = new Vector3(direction, 1, 1);
                if (!anim.GetBool("isJump"))
                    anim.SetBool("isRun", true);

            }
            if (moveDirection.z != 0)
            {
                anim.SetBool("isRun", true);
            }
            characterController.Move(moveDirection * movePower * Time.deltaTime);
           
        }

        void StopMove()
        {
            moveDirection = Vector3.zero;
            anim.SetBool("isRun", false);
        }
        // void Jump()
        // {
        //     if (moveVelocity.y > 0
        //     && !anim.GetBool("isJump"))
        //     {
        //         isJumping = true;
        //         anim.SetBool("isJump", true);
        //     }
        //     if (!isJumping)
        //     {
        //         return;
        //     }

        //     rb.velocity = Vector2.zero;

        //     Vector2 jumpVelocity = new Vector2(0, jumpPower);
        //     rb.AddForce(jumpVelocity, ForceMode2D.Impulse);

        //     isJumping = false;
        // }
        void Attack()
        {
            anim.SetTrigger("attack");
        }
    }
}