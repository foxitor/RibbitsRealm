using System.Collections; using UnityEngine;

public class PlayerController : MonoBehaviour {
    [Header("<References>")]
    public PlayerModel model;
    [Header("<Strengths & Speed>")]
    public float WalkSpeed = 2.6f; 
    public float RunSpeed = 3f;
    public float CrouchSpeed = 1.6f;
    float CurrentSpeed = 0f; 
    [Space]
    public float JumpHeight = 5f;
    float mouseSensitivity = 1f;
    float gravity = -14f;
    [Header("<Inventory>")]
    public ItemStack[] HotbarItems;
    [HideInInspector]
    public int SelectedSlot;
    [HideInInspector]
    public ItemStack SelectedItem;

    CharacterController CharControll;
    Camera Cam;

    Vector2 moveInput; 
    Vector2 lookInput;
    float rotationX = 0f;
    Vector3 velocity; 

    [HideInInspector]
    public bool atGround, runState, crouchState;

    void Start() {
        Cam = Camera.main;
        CharControll = GetComponent<CharacterController>();
    }

    void Update() {
        CurrentSpeed = WalkSpeed;
        moveInput.x = Input.GetAxis("Horizontal");
        moveInput.y = Input.GetAxis("Vertical");

        lookInput.x = Input.GetAxis("Mouse X");
        lookInput.y = Input.GetAxis("Mouse Y");

        if (Input.GetKey(KeyCode.Space)) { Jump(); }

        HandleMovement(); HandleMouseLook();
        Cursor.lockState = CursorLockMode.Locked;
        CharControll.Move(velocity * Time.deltaTime);

        for (int i = 1; i <= 9; i++) {
            if (Input.GetKeyDown(KeyCode.Alpha0 + i)) {
                SelectedSlot = i-1;
                SelectedItem = HotbarItems[SelectedSlot];
            }
        }
        if (Input.GetMouseButtonDown(0)) {
            model.punch();
        }
        if (Input.GetMouseButtonDown(1)) {
            model.interact();
        }
    }
    void FixedUpdate() {
        atGround = CharControll.isGrounded;
        if(!atGround) {
            velocity.y += gravity * Time.deltaTime;
            if (velocity.y > 10) { velocity.y = 10; }
        } else { velocity.y = 0; }
    }
    void Jump() { 
        if (atGround) { velocity.y = JumpHeight; } 
    }

    void HandleMovement() {
        if (Input.GetKey(KeyCode.LeftControl)) { runState = true; } else { runState = false; }
        if (Input.GetKey(KeyCode.LeftShift)) { crouchState = true; } else { crouchState = false; }

        float FinalSpeed = WalkSpeed;
        if (runState) {  if (!crouchState) { FinalSpeed = RunSpeed; } }
        else if (crouchState) { if (!runState) { FinalSpeed = CrouchSpeed; } }

        CurrentSpeed = FinalSpeed;
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y; 
        CharControll.Move(move * CurrentSpeed * Time.deltaTime);
    }
    void HandleMouseLook() {
        transform.Rotate(Vector3.up * lookInput.x * mouseSensitivity);

        rotationX -= lookInput.y * mouseSensitivity;
        rotationX = Mathf.Clamp(rotationX, -90f, 90f);
        Cam.transform.localEulerAngles = new Vector3(rotationX, 0, 0);
    }
}
