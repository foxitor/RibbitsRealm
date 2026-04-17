using System.Collections; using System.Collections.Generic; using UnityEngine; using UnityEngine.UI;

public class FirstPersonController : MonoBehaviour {
    Rigidbody rb;

    [Header("Camera Movement Variables")]
    public Camera playerCamera;

    public float fov = 70f;
    public bool invertCamera = false;
    public bool cameraCanMove = true;
    public float mouseSensitivity = 2f;
    public float maxLookAngle = 50f;

    // Crosshair
    public bool lockCursor = true;
    public bool crosshair = true;
    public Sprite crosshairImage;

    // Internal Variables
    float yaw = 0.0f;
    float pitch = 0.0f;
    Image crosshairObject;

    [Header("Camera Zoom Variables")]
    public bool enableZoom = true;
    public bool holdToZoom = false;
    public KeyCode zoomKey = KeyCode.Mouse1;
    public float zoomFOV = 30f;
    public float zoomStepTime = 5f;

    // Internal Variables
    bool isZoomed = false;

    [Header("Movement Variables")]
    public bool playerCanMove = true;
    public float walkSpeed = 5f;
    public float maxVelocityChange = 10f;

    // Internal Variables
    bool isWalking = false;

    [Header("Sprint")]
    public KeyCode sprintKey = KeyCode.LeftControl;
    public float sprintSpeed = 7f;
    public float sprintDuration = 5f;
    public float sprintCooldown = .5f;
    public float sprintFOV = 5f;
    public float sprintFOVStepTime = 10f;

    // Internal Variables
    CanvasGroup sprintBarCG;
    bool isSprinting = false;

    [Header("Jump")]
    public bool enableJump = true;
    public KeyCode jumpKey = KeyCode.Space;
    public float jumpPower = 5f;
    bool jumpOneShot;

    // Internal Variables
    bool isGrounded = false;

    [Header("Crouch")]
    public bool enableCrouch = true;
    public bool holdToCrouch = true;
    public KeyCode crouchKey = KeyCode.LeftShift;
    public float crouchHeight = .75f;
    public float speedReduction = .5f;

    // Internal Variables
    bool isCrouched = false;
    Vector3 originalScale;

    [Header("Head Bob")]

    public bool enableHeadBob = true;
    public Transform joint;
    public float bobSpeed = 10f;
    public Vector3 bobAmount = new Vector3(0f, .01f, .01f);

    // Internal Variables
    Vector3 jointOriginalPos;
    float timer = 0;

    void Awake() {
        rb = GetComponent<Rigidbody>();

        crosshairObject = GetComponentInChildren<Image>();

        // Set internal variables
        playerCamera.fieldOfView = fov;
        originalScale = transform.localScale;
        jointOriginalPos = joint.localPosition;
    }

    void Start() {
        if (lockCursor) { Cursor.lockState = CursorLockMode.Locked; }

        if (crosshair) {
            crosshairObject.sprite = crosshairImage;
        }
        else { crosshairObject.gameObject.SetActive(false); }
    }

    float camRotation;

    void Update() {
        //CheckGround();
        #region Camera

        // Control camera movement
        if (cameraCanMove) {
            yaw = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * mouseSensitivity;

            if (!invertCamera) {
                pitch -= mouseSensitivity * Input.GetAxis("Mouse Y");
            } else {
                // Inverted Y
                pitch += mouseSensitivity * Input.GetAxis("Mouse Y");
            }

            // Clamp pitch between lookAngle
            pitch = Mathf.Clamp(pitch, -maxLookAngle, maxLookAngle);

            transform.localEulerAngles = new Vector3(0, yaw, 0);
            playerCamera.transform.localEulerAngles = new Vector3(pitch, 0, 0);
        }

        #region Camera Zoom

        if (enableZoom) {
            // Changes isZoomed when key is pressed
            // Behavior for toogle zoom
            if (Input.GetKeyDown(zoomKey) && !holdToZoom && !isSprinting) {
                if (!isZoomed) { isZoomed = true; }
                else { isZoomed = false; }
            }

            // Changes isZoomed when key is pressed
            // Behavior for hold to zoom
            if (holdToZoom && !isSprinting) {
                if(Input.GetKeyDown(zoomKey)) { isZoomed = true; }
                else if(Input.GetKeyUp(zoomKey)) { isZoomed = false; }
            }

            // Lerps camera.fieldOfView to allow for a smooth transistion
            if (isZoomed) {
                playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, zoomFOV, zoomStepTime * Time.deltaTime);
            }
            else if (!isZoomed && !isSprinting) {
                playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, fov, zoomStepTime * Time.deltaTime);
            }
        }

        #endregion
        #endregion

        #region Sprint

        if (isSprinting) {
            isZoomed = false;
            playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, fov + sprintFOV, sprintFOVStepTime * Time.deltaTime);
        }

        #endregion

        #region Crouch

        if (enableCrouch) {
            if (Input.GetKeyDown(crouchKey) && !holdToCrouch) { Crouch(); }
            
            if (Input.GetKeyDown(crouchKey) && holdToCrouch) {
                isCrouched = false;
                Crouch();
            }
            else if (Input.GetKeyUp(crouchKey) && holdToCrouch) {
                isCrouched = true;
                Crouch();
            }
        }
        // Gets input and calls jump method
        if (Input.GetKey(jumpKey) && !jumpOneShot) {
            //CheckGround();
            if (isGrounded) { jumpOneShot = true; Jump(); }
        }
        #endregion
    }

    void FixedUpdate() {
        #region Movement
        if (playerCanMove) {
            // Calculate how fast we should be moving
            Vector3 targetVelocity = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

            // Checks if player is walking and isGrounded
            // Will allow head bob
            if (targetVelocity.x != 0 || targetVelocity.z != 0 && isGrounded) { isWalking = true; }
            else { isWalking = false; }

            // All movement calculations shile sprint is active
            if (Input.GetKey(sprintKey)) {
                targetVelocity = transform.TransformDirection(targetVelocity) * sprintSpeed;

                // Apply a force that attempts to reach our target velocity
                Vector3 velocity = rb.velocity;
                Vector3 velocityChange = (targetVelocity - velocity);
                velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
                velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
                velocityChange.y = 0;

                // Player is only moving when valocity change != 0
                // Makes sure fov change only happens during movement
                if (velocityChange.x != 0 || velocityChange.z != 0) {
                    isSprinting = true;

                    if (isCrouched) { Crouch(); }
                }

                rb.AddForce(velocityChange, ForceMode.VelocityChange);
            }/* All movement calculations while walking */ else {
                isSprinting = false;

                targetVelocity = transform.TransformDirection(targetVelocity) * walkSpeed;

                // Apply a force that attempts to reach our target velocity
                Vector3 velocity = rb.velocity;
                Vector3 velocityChange = (targetVelocity - velocity);
                velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
                velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
                velocityChange.y = 0;

                rb.AddForce(velocityChange, ForceMode.VelocityChange);
            }
            
        }
        if (enableHeadBob) { HeadBob(); }
        #endregion
    }

    // Sets isGrounded based on a raycast sent straigth down from the player object
    //void CheckGround() {
        //Vector3 origin = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        //Vector3 direction = transform.TransformDirection(Vector3.down);
        //float distance = 0.5f;
        //float sphereRadius = 0.5f;
        //Debug.DrawRay(origin, direction * distance, Color.blue);
        //if (Physics.Raycast(origin, direction, out RaycastHit hit, distance)) {
        //    isGrounded = true;
        //} else {
        //    if (rb.velocity.y < 0.001) {
        //        isGrounded = true;
       //     } else {
        //        isGrounded = false;
        //    }
        //}
    //}
    void OnCollisionEnter(Collision coll) {
        isGrounded = true;
    }
    void OnCollisionExit(Collision coll) {
        isGrounded = false;
    }

    void Jump() {
        // Adds force to the player rigidbody to jump
        rb.AddForce(0f, jumpPower, 0f, ForceMode.Impulse);
        jumpOneShot = false;
        isGrounded = false;
        // When crouched and using toggle system, will uncrouch for a jump
        if (isCrouched && !holdToCrouch) {
            Crouch();
        }
    }

    void Crouch() {
        // Stands player up to full height
        // Brings walkSpeed back up to original speed
        if(isCrouched) {
            transform.localScale = new Vector3(originalScale.x, originalScale.y, originalScale.z);
            walkSpeed /= speedReduction;

            isCrouched = false;
        } /* Crouches player down to set height Reduces walkSpeed*/ else {
            transform.localScale = new Vector3(originalScale.x, crouchHeight, originalScale.z);
            walkSpeed *= speedReduction;

            isCrouched = true;
        }
    }

    void HeadBob() {
        if (isWalking) {
            // Calculates HeadBob speed during sprint
            if(isSprinting)
            {
                timer += Time.deltaTime * (bobSpeed + sprintSpeed);
            }
            // Calculates HeadBob speed during crouched movement
            else if (isCrouched)
            {
                timer += Time.deltaTime * (bobSpeed * speedReduction);
            }
            // Calculates HeadBob speed during walking
            else
            {
                timer += Time.deltaTime * bobSpeed;
            }
            // Applies HeadBob movement
            joint.localPosition = new Vector3(jointOriginalPos.x + Mathf.Sin(timer) * bobAmount.x, jointOriginalPos.y + Mathf.Sin(timer) * bobAmount.y, jointOriginalPos.z + Mathf.Sin(timer) * bobAmount.z);
        } else {
            // Resets when play stops moving
            timer = 0;
            joint.localPosition = new Vector3(Mathf.Lerp(joint.localPosition.x, jointOriginalPos.x, Time.deltaTime * bobSpeed), Mathf.Lerp(joint.localPosition.y, jointOriginalPos.y, Time.deltaTime * bobSpeed), Mathf.Lerp(joint.localPosition.z, jointOriginalPos.z, Time.deltaTime * bobSpeed));
        }
    }
}
