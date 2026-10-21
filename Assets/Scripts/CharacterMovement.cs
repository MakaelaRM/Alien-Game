// This is from the Unity Docs
// https://docs.unity3d.com/6000.3/Documentation/ScriptReference/CharacterController.Move.html

//using UnityEngine;
//using UnityEngine.InputSystem;

//public class CharacterMovement : MonoBehaviour
//{
//    private float playerSpeed = 4f;
//    private float sprintMultiplier = 1.8f;
//    private float jumpHeight = 0.35f;
//    private float gravityValue = -3f;
//    private float turnSpeed = 10f;

//    public CharacterController controller;
//    private Vector3 playerVelocity;
//    private bool groundedPlayer;

//    [Header("Input Actions")]
//    public InputActionReference moveAction;
//    public InputActionReference jumpAction;
//    public InputActionReference sprintAction;

//    private void OnEnable()
//    {
//        moveAction.action.Enable();
//        jumpAction.action.Enable();
//        sprintAction.action.Enable();
//    }

//    private void OnDisable()
//    {
//        moveAction.action.Disable();
//        jumpAction.action.Disable();
//        sprintAction.action.Disable();
//    }

//    void Update()
//    {
//        groundedPlayer = controller.isGrounded;

//        if (groundedPlayer)
//        {
//            // Slight downward velocity to keep grounded stable
//            if (playerVelocity.y < -2f)
//                playerVelocity.y = -2f;
//        }

//        // Read input
//        Vector2 input = moveAction.action.ReadValue<Vector2>();
//        Vector3 move = new Vector3(input.x, 0, input.y);
//        move = Vector3.ClampMagnitude(move, 1f);

//        if (move != Vector3.zero)
//        {
//            Quaternion targetRotation = Quaternion.LookRotation(move);
//            controller.transform.rotation = Quaternion.Slerp(
//                controller.transform.rotation,
//                targetRotation,
//                turnSpeed * Time.deltaTime);
//        }

//        // Jump using WasPressedThisFrame()
//        if (groundedPlayer && jumpAction.action.WasPressedThisFrame())
//        {
//            playerVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravityValue);
//        }

//        // Apply gravity
//        playerVelocity.y += gravityValue * Time.deltaTime;

//        // Sprint while the button is held
//        bool isSprinting = sprintAction.action.IsPressed();
//        float currentSpeed = isSprinting ? playerSpeed * sprintMultiplier : playerSpeed;


//        // Move
//        Vector3 finalMove = move * currentSpeed + Vector3.up * playerVelocity.y;
//        controller.Move(finalMove * Time.deltaTime);
//    }
//}


using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterMovement : MonoBehaviour
{
    private float playerSpeed = 4f;
    private float sprintMultiplier = 1.8f;
    private float jumpHeight = 0.35f;
    private float gravityValue = -3f;
    private float turnSpeed = 10f;

    public CharacterController controller;
    public GameObject inventoryUI;

    private Vector3 playerVelocity;
    private bool groundedPlayer;
    private bool inventoryOpen;

    [Header("Input Actions")]
    public InputActionReference moveAction;
    public InputActionReference jumpAction;
    public InputActionReference sprintAction;

    private void Start()
    {
        // Make sure the inventory starts closed.
        inventoryUI.SetActive(false);
    }

    private void OnEnable()
    {
        moveAction.action.Enable();
        jumpAction.action.Enable();
        sprintAction.action.Enable();
    }

    private void OnDisable()
    {
        moveAction.action.Disable();
        jumpAction.action.Disable();
        sprintAction.action.Disable();
    }

    void Update()
    {
        // Toggle inventory when I is pressed.
        if (Keyboard.current != null &&
            Keyboard.current.iKey.wasPressedThisFrame)
        {
            inventoryOpen = !inventoryOpen;
            inventoryUI.SetActive(inventoryOpen);
        }

        // Stop all player movement while inventory is open.
        if (inventoryOpen)
        {
            return;
        }

        groundedPlayer = controller.isGrounded;

        if (groundedPlayer)
        {
            if (playerVelocity.y < -2f)
                playerVelocity.y = -2f;
        }

        Vector2 input = moveAction.action.ReadValue<Vector2>();
        Vector3 move = new Vector3(input.x, 0, input.y);
        move = Vector3.ClampMagnitude(move, 1f);

        if (move != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(move);

            controller.transform.rotation = Quaternion.Slerp(
                controller.transform.rotation,
                targetRotation,
                turnSpeed * Time.deltaTime);
        }

        if (groundedPlayer &&
            jumpAction.action.WasPressedThisFrame())
        {
            playerVelocity.y = Mathf.Sqrt(
                jumpHeight * -2f * gravityValue);
        }

        playerVelocity.y += gravityValue * Time.deltaTime;

        bool isSprinting = sprintAction.action.IsPressed();

        float currentSpeed = isSprinting
            ? playerSpeed * sprintMultiplier
            : playerSpeed;

        Vector3 finalMove =
            move * currentSpeed + Vector3.up * playerVelocity.y;

        controller.Move(finalMove * Time.deltaTime);
    }
}
