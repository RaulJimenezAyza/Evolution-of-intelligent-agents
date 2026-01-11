
using UnityEngine;

public class Movement : MonoBehaviour
{
    public CharacterController controller;
    private Vector3 playerVelocity;
    private float gravityValue = -9.81f;
    public float speed = 10.0F;
    public float rotateSpeed = 10.0F;

    private bool isEnabled = true;
    public bool IsEnabled { get => isEnabled; set => isEnabled = value; }


    private void Awake()
    {
        if (controller == null)
            controller = GetComponent<CharacterController>();
    }

    public void Move(float forwardBackward, float leftRight)
    {
        if (!isEnabled) return;

        // Clamp values
        leftRight = Mathf.Clamp(leftRight, -1, 1);
        forwardBackward = Mathf.Clamp(forwardBackward, 0, 1);

        // Rotate around y-axis
        transform.Rotate(0, leftRight * rotateSpeed, 0);

        // Move forward/backward
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        controller.SimpleMove(forward * speed * forwardBackward);

        // Handle gravity
        ApplyGravity();
    }
    private void ApplyGravity()
    {
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

    private void OnEnable()
    {
        isEnabled = true;
    }

    private void OnDisable()
    {
        isEnabled = false;
    }
}
