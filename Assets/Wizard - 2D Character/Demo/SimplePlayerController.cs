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

        // Start is called before the first frame update
        void Start()
        {
            characterController = GetComponent<CharacterController>();
            anim = GetComponent<Animator>();
        }

        private void Update()
        {
            if (characterController.isGrounded)
            {
                moveDirection.y += gravity * Time.deltaTime;
            }

            Run();
        }

        public void OnPlayerMove(InputValue value)
        {
            Vector2 move = value.Get<Vector2>();
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
            if (moveDirection.y < 0)
            {
                anim.SetTrigger("attack");
            }
        }
    }
}