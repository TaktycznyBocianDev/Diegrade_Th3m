using System;
using UnityEditor;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {

    //Public variables
    [Header("Assingables")]
    public Transform playerCam;
    public Transform orientation;

    [Space(10)]
    [Header("Movement")]
    public float moveSpeed = 4500;
    public float maxSpeed = 20;
    public bool grounded;
    public float counterMovement = 0.175f;
    private float threshold = 0.01f;
    public float maxSlopeAngle = 35f;

    [Space(10)]
    [Header("Jumping")]
    public float jumpForce = 550f;
    private bool readyToJump = true;
    private float jumpCooldown = 0.25f;

    [Space(10)]
    [Header("Crouch & Slide")]
    public float slideForce = 400;
    public float slideCounterMovement = 0.2f;
    [Range(0, 1)]
    public float crouchScaleModifier;
    private Vector3 crouchScale;
    private Vector3 playerScale;

    [Space(10)]
    [Header("Wallrun")]
    public float wallrunForce, maxWallrunTime, maxWallrunSpeed;
    public float maxWallRunCameraTilt, wallRunCameraTilt;
    bool iswallRight;
    bool iswallLeft;
    bool isWallrunning;


    [Space(10)] 
    [Header("Layers")]
    public LayerMask whatIsGround;
    public LayerMask whatIsWall;

    //Private variables

    //Assing in Awake()
    private Rigidbody rb;

    //Rotation and look
    private float xRotation;
    private float sensitivity = 50f;
    private float sensMultiplier = 1f;
    //Input

    float x, y;
    bool jumping, sprinting, crouching; 
    //Sliding

    private Vector3 normalVector = Vector3.up;
    private Vector3 wallNormalVector;



    void Awake() {
        rb = GetComponent<Rigidbody>();
        crouchScale = new Vector3(1, crouchScaleModifier, 1);
    }
    
    void Start() {
        playerScale =  transform.localScale;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        TakeInput();
        Look();
        CheckForWall();
        WallRunInput();
    }

    private void FixedUpdate() {
        Movement();
    }

    private void WallRunInput()
    {
        //Initiate wallrun from player input
        if (Input.GetKey(KeyCode.D) && iswallRight) StartWallRun();
        if (Input.GetKey(KeyCode.A) && iswallLeft) StartWallRun();


    }

    private void StartWallRun()
    {
        rb.useGravity = false;
        isWallrunning = true;

        //check if you are not at max speed 
        if (rb.velocity.magnitude <= maxWallrunSpeed)
        {
            //Player run
            rb.AddForce(orientation.forward * wallrunForce * Time.deltaTime);

            //Player sticks to wall
            if (iswallRight) rb.AddForce(orientation.right * wallrunForce / 5 * Time.deltaTime);
            if (iswallLeft)  rb.AddForce(-orientation.right * wallrunForce / 5 * Time.deltaTime);
        }
    }

    private void StopWallRun()
    {
        rb.useGravity = true;
        isWallrunning = false;
    }


    private void CheckForWall()
    {
        iswallRight = Physics.Raycast(transform.position, orientation.right, 1f, whatIsWall);
        iswallLeft = Physics.Raycast(transform.position, -orientation.right, 1f, whatIsWall);

        //leave run -> no more wall!
        if (!iswallLeft && !iswallRight) StopWallRun();

    }


    /// <summary>
    /// Find user input.  TO DO: Input own class 
    /// </summary>
    private void TakeInput() {
        x = Input.GetAxisRaw("Horizontal");
        y = Input.GetAxisRaw("Vertical");
        jumping = Input.GetButton("Jump");
        crouching = Input.GetKey(KeyCode.LeftControl);
      
        //Crouching
        if (Input.GetKeyDown(KeyCode.LeftControl))
            StartCrouch();
        if (Input.GetKeyUp(KeyCode.LeftControl))
            StopCrouch();
    }

    private void StartCrouch() {
        transform.localScale = crouchScale;
        transform.position = new Vector3(transform.position.x, transform.position.y - 0.5f, transform.position.z);
        if (rb.velocity.magnitude > 0.5f) {
            if (grounded) {
                rb.AddForce(orientation.transform.forward * slideForce);
            }
        }
    }

    private void StopCrouch() {
        transform.localScale = playerScale;
        transform.position = new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z);
    }

    private void Movement() {
        //Extra gravity
        rb.AddForce(Vector3.down * Time.deltaTime * 10);
        
        //Find actual velocity relative to where player is looking
        Vector2 mag = FindVelRelativeToLook();
        float xMag = mag.x, yMag = mag.y;

        //Counteract sliding and sloppy movement
        CounterMovement(x, y, mag);
        
        //If holding jump && ready to jump, then jump
        if (readyToJump && jumping) Jump();

        //Set max speed
        float maxSpeed = this.maxSpeed;
        
        //If sliding down a ramp, add force down so player stays grounded and also builds speed
        if (crouching && grounded && readyToJump) {
            rb.AddForce(Vector3.down * Time.deltaTime * 3000);
            return;
        }
        
        //If speed is larger than maxspeed, cancel out the input so you don't go over max speed
        if (x > 0 && xMag > maxSpeed) x = 0;
        if (x < 0 && xMag < -maxSpeed) x = 0;
        if (y > 0 && yMag > maxSpeed) y = 0;
        if (y < 0 && yMag < -maxSpeed) y = 0;

        //Some multipliers
        float multiplier = 1f, multiplierV = 1f;
        
        // Movement in air
        if (!grounded) {
            multiplier = 1f;
            multiplierV = 1f;
        }
        
        // Movement while sliding
        if (grounded && crouching) multiplierV = 0f;

        //Apply forces to move player
        rb.AddForce(orientation.transform.forward * y * moveSpeed * Time.deltaTime * multiplier * multiplierV);
        rb.AddForce(orientation.transform.right * x * moveSpeed * Time.deltaTime * multiplier);
    }

    private void Jump() {
        if (grounded && readyToJump) {
            readyToJump = false;

            //Add jump forces
            rb.AddForce(Vector2.up * jumpForce * 1.5f);
            rb.AddForce(normalVector * jumpForce * 0.5f);
            
            //If jumping while falling, reset y velocity.
            Vector3 vel = rb.velocity;
            if (rb.velocity.y < 0.5f)
                rb.velocity = new Vector3(vel.x, 0, vel.z);
            else if (rb.velocity.y > 0) 
                rb.velocity = new Vector3(vel.x, vel.y / 2, vel.z);
            
            Invoke(nameof(ResetJump), jumpCooldown);
        }

        //WallJump
        if (isWallrunning)
        {
            readyToJump = false;

            //normal jump
            if (iswallLeft && !Input.GetKey(KeyCode.D) || iswallRight && !Input.GetKey(KeyCode.A))
            {
                rb.AddForce(Vector2.up * jumpForce * 3.2f);
                rb.AddForce(normalVector * jumpForce * 0.5f);
            }

            //side jump
            if (iswallRight || iswallRight && Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.A)) rb.AddForce(orientation.up * jumpForce);
            if (iswallRight && Input.GetKey(KeyCode.A)) rb.AddForce(-orientation.right * jumpForce * 2f);
            if(iswallLeft && Input.GetKey(KeyCode.D)) rb.AddForce(orientation.right * jumpForce * 2f);

            //Little push :)
            rb.AddForce(orientation.forward * jumpForce);

            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }
    
    private void ResetJump() {
        readyToJump = true;
    }
    
    private float desiredX;
    private void Look() {
        float mouseX = Input.GetAxis("Mouse X") * sensitivity * Time.fixedDeltaTime * sensMultiplier;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity * Time.fixedDeltaTime * sensMultiplier;

        //Find current look rotation
        Vector3 rot = playerCam.transform.localRotation.eulerAngles;
        desiredX = rot.y + mouseX;
        
        //Rotate, and also make sure we dont over- or under-rotate.
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        //Perform the rotations
        playerCam.transform.localRotation = Quaternion.Euler(xRotation, desiredX, wallRunCameraTilt);
        orientation.transform.localRotation = Quaternion.Euler(0, desiredX, 0);

        if (Math.Abs(wallRunCameraTilt) < maxWallRunCameraTilt && isWallrunning && iswallRight)
        {
            wallRunCameraTilt += Time.deltaTime * maxWallRunCameraTilt * 2;
        }

        if (Math.Abs(wallRunCameraTilt) < maxWallRunCameraTilt && isWallrunning && iswallLeft)
        {
            wallRunCameraTilt -= Time.deltaTime * maxWallRunCameraTilt * 2;
        }


        //Undo camera tilt
        if (wallRunCameraTilt > 0 && !iswallRight && !iswallLeft)
        {
            wallRunCameraTilt -= Time.deltaTime * maxWallRunCameraTilt * 2;
        }
        if (wallRunCameraTilt < 0 && !iswallRight && !iswallLeft)
        {
            wallRunCameraTilt += Time.deltaTime * maxWallRunCameraTilt * 2;
        }
    }

    private void CounterMovement(float x, float y, Vector2 mag) {
        if (!grounded || jumping) return;

        //Slow down sliding
        if (crouching) {
            rb.AddForce(moveSpeed * Time.deltaTime * -rb.velocity.normalized * slideCounterMovement);
            return;
        }

        //Counter movement
        if (Math.Abs(mag.x) > threshold && Math.Abs(x) < 0.05f || (mag.x < -threshold && x > 0) || (mag.x > threshold && x < 0)) {
            rb.AddForce(moveSpeed * orientation.transform.right * Time.deltaTime * -mag.x * counterMovement);
        }
        if (Math.Abs(mag.y) > threshold && Math.Abs(y) < 0.05f || (mag.y < -threshold && y > 0) || (mag.y > threshold && y < 0)) {
            rb.AddForce(moveSpeed * orientation.transform.forward * Time.deltaTime * -mag.y * counterMovement);
        }
        
        //Limit diagonal running. This will also cause a full stop if sliding fast and un-crouching, so not optimal.
        if (Mathf.Sqrt((Mathf.Pow(rb.velocity.x, 2) + Mathf.Pow(rb.velocity.z, 2))) > maxSpeed) {
            float fallspeed = rb.velocity.y;
            Vector3 n = rb.velocity.normalized * maxSpeed;
            rb.velocity = new Vector3(n.x, fallspeed, n.z);
        }
    }

    /// <summary>
    /// Find the velocity relative to where the player is looking
    /// Useful for vectors calculations regarding movement and limiting movement
    /// </summary>
    /// <returns></returns>
    public Vector2 FindVelRelativeToLook() {
        float lookAngle = orientation.transform.eulerAngles.y;
        float moveAngle = Mathf.Atan2(rb.velocity.x, rb.velocity.z) * Mathf.Rad2Deg;

        float u = Mathf.DeltaAngle(lookAngle, moveAngle);
        float v = 90 - u;

        float magnitue = rb.velocity.magnitude;
        float yMag = magnitue * Mathf.Cos(u * Mathf.Deg2Rad);
        float xMag = magnitue * Mathf.Cos(v * Mathf.Deg2Rad);
        
        return new Vector2(xMag, yMag);
    }

    private bool IsFloor(Vector3 v) {
        float angle = Vector3.Angle(Vector3.up, v);
        return angle < maxSlopeAngle;
    }

    private bool cancellingGrounded;
    
    /// <summary>
    /// Handle ground detection
    /// </summary>
    private void OnCollisionStay(Collision other) {
        //Make sure we are only checking for walkable layers
        int layer = other.gameObject.layer;
        if (whatIsGround != (whatIsGround | (1 << layer))) return;

        //Iterate through every collision in a physics update
        for (int i = 0; i < other.contactCount; i++) {
            Vector3 normal = other.contacts[i].normal;
            //FLOOR
            if (IsFloor(normal)) {
                grounded = true;
                cancellingGrounded = false;
                normalVector = normal;
                CancelInvoke(nameof(StopGrounded));
            }
        }

        //Invoke ground/wall cancel, since we can't check normals with CollisionExit
        float delay = 3f;
        if (!cancellingGrounded) {
            cancellingGrounded = true;
            Invoke(nameof(StopGrounded), Time.deltaTime * delay);
        }
    }

    private void StopGrounded() {
        grounded = false;
    }
    
}
