using UnityEngine;

public class Movement2 : MonoBehaviour
{
    public CharacterController controller;
    private bool hasController = false;
    private Vector3 playerVelocity;
    private float gravityValue = -9.81f;
    public float speed = 10.0F;
    public float rotateSpeed = 10.0F;
    public float FB = 0;
    public float LR = 0;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    public void Move(float FB, float LR)
    {
        if (!GetComponent<Creature2>().isDead)
        {
            LR = Mathf.Clamp(LR, -1, 1);
            FB = Mathf.Clamp(FB, 0, 1);

            transform.Rotate(0, LR * rotateSpeed, 0);

            Vector3 forward = transform.TransformDirection(Vector3.forward);
            controller.SimpleMove(forward * speed * FB);
        }

        if (controller.isGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
        }
        else
        {
            playerVelocity.y += gravityValue * Time.deltaTime;
            controller.Move(playerVelocity * Time.deltaTime);
        }
    }
}