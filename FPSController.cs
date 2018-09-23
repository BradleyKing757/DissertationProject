using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

// Enumerations
public enum PlayerMoveStatus { NotMoving, Crouching, Walking, Running, NotGrounded, Landing }
public enum CurveControlledBobCallbackType { Horizontal, Vertical }

// Used to describe the type of callback function that need to store in curvecontrolledbob class
//variables with this type  can be used to reference a function that has a void return type and takes no paramtres
public delegate void CurveControlledBobCallback();

/// <summary>
/// Represents a single event on the timeline
/// </summary>
[System.Serializable]
public class CurveControlledBobEvent
{
    public float Time = 0.0f;   //stores time along x-axis of the curve
    public CurveControlledBobCallback Function = null;
    public CurveControlledBobCallbackType Type = CurveControlledBobCallbackType.Vertical;
}

/// <summary>
/// This class controls how the players head will bob up and down when moving and how the footsteps can be timed with the downward arch of the animation curve.
/// </summary>
[System.Serializable]
public class CurveControlledBob
{
    [SerializeField]
    //The 5 keyframes define a basic sine wave shape. (can change these in inspector)
    AnimationCurve bobcurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.5f, 1f),
                                                                    new Keyframe(1f, 0f), new Keyframe(1.5f, -1f),
                                                                    new Keyframe(2f, 0f));

    // Inspector Assigned Bob Control Variables
    [SerializeField] float horizontalMultiplier = 0.01f; //define range that allows for horiz head movement(left/right) (-1 to +1 range * this multiplier)(higher value, more bob)
    [SerializeField] float verticalMultiplier = 0.02f; //define range that allows for vertical head movement (0 to 2 range * this multiplier)
    [SerializeField] float verticaltoHorizontalSpeedRatio = 2.0f; //playhead used to step through graph for Y movement is twice as fast as playhead for horiz movement.
    [SerializeField] float baseInterval = 1.0f; //allows us to speed up or slow down the bob

    // Internals
    private float prevXPlayHead; //need to know where playheads were on previous frame and this frame and need to see if passed over event that should have happened between them
    private float prevYPlayHead;
    private float xPlayHead; //horizontal headbobbing
    private float yPlayHead; //vertical head bobbing
    private float curveEndTime; //fetch time of the last key frame
    private List<CurveControlledBobEvent> events = new List<CurveControlledBobEvent>(); //stores curvecontrolledbobevent objects



    private AudioSource m_AudioSource;




    //CharacterStats charStats = new CharacterStats();
    /// <summary>
    /// FPS Controller needs to call this from start function
    /// </summary>
    public void Initialize()
    {
        // Record time length of bob curve
        curveEndTime = bobcurve[bobcurve.length - 1].time; //gets last key frame in the array, and fetch the time of it
        xPlayHead = 0.0f;
        yPlayHead = 0.0f;
        prevXPlayHead = 0.0f;
        prevYPlayHead = 0.0f;
    }

    /// <summary>
    /// Method takes the events and sorts them into ascending order. Method is used to play an event (the footstep sound at 1.5 secs into curve)
    /// </summary>
    /// <param name="time"></param>
    /// <param name="function"></param>
    /// <param name="type"></param>
    //time falls between keyframes on curve, reference to function to be called
    public void RegisterEventCallback(float time, CurveControlledBobCallback function, CurveControlledBobCallbackType type)
    {
        CurveControlledBobEvent ccbeEvent = new CurveControlledBobEvent();
        ccbeEvent.Time = time;
        ccbeEvent.Function = function;
        ccbeEvent.Type = type;
        events.Add(ccbeEvent); //add them to events list
        events.Sort(        //store times in ascending order
            delegate (CurveControlledBobEvent t1, CurveControlledBobEvent t2)
            {
                return (t1.Time.CompareTo(t2.Time));
            }
        );
    }

    /// <summary>
    /// Takes the speed the controller is going and will return a 3d vector which describes an offset which add on to local space position of camera
    /// Method will look through events in the list of events and if there is an event between current playHead and previous Playhead then need
    /// to make sure the event method is called.
    /// </summary>
    /// <param name="speed"></param>
    /// <returns></returns>
    public Vector3 GetVectorOffset(float speed)
    {
        xPlayHead += (speed * Time.deltaTime) / baseInterval; //speed that playHead moves dependent on speed of char controller
        yPlayHead += ((speed * Time.deltaTime) / baseInterval) * verticaltoHorizontalSpeedRatio; //how Y playhead moves compared to x playhead

        //Makes sure that if reach end of curve, loops back around to start
        if (xPlayHead > curveEndTime)
        {
            xPlayHead -= curveEndTime;
        }

        if (yPlayHead > curveEndTime)
        {
            yPlayHead -= curveEndTime;
        }
            

        // Loops through events in list and fetch the event currently processing and if not null, see if its vertical or horizontal event and we process them differently.
        for (int i = 0; i < events.Count; i++)
        {
            CurveControlledBobEvent ev = events[i];
            if (ev != null)
            {
                if (ev.Type == CurveControlledBobCallbackType.Vertical)
                {
                    //we know event is between the playhead positions or when prev position is larger than current playhead position (looped round)
                    if ((prevYPlayHead < ev.Time && yPlayHead >= ev.Time) ||   
                        (prevYPlayHead > yPlayHead && (ev.Time > prevYPlayHead || ev.Time <= yPlayHead)))
                    {
                        ev.Function();
                    }
                }
                else
                {
                    if ((prevXPlayHead < ev.Time && xPlayHead >= ev.Time) ||
                        (prevXPlayHead > xPlayHead && (ev.Time > prevXPlayHead || ev.Time <= xPlayHead)))
                    {
                        ev.Function();
                    }
                }
            }
        }

        //the float of the curve at specified point in time
        float xPos = bobcurve.Evaluate(xPlayHead) * horizontalMultiplier;
        float yPos = bobcurve.Evaluate(yPlayHead) * verticalMultiplier;

        prevXPlayHead = xPlayHead; //updates the prev position of the playHeads to the new positions 
        prevYPlayHead = yPlayHead; //

        //add this vector3 into the FPS Controller c;ass to the local space position of the camera to create space bobbing
        return new Vector3(xPos, yPos, 0f);
    }
}

/// <summary>
/// This class controls the FPS characters movement status such as crouching, running, jumping, falling etc. It also controls the characters response to
/// hitting various objects colliders in the environment.
/// When the player collides with important gameobjects such as keys and weapons, the OnCollision methods in this class will open a text
/// in the UI instructing them how to pick up the object. When the player presses E, the object will be set to inactive and the associated image
/// sprite will be set to active in the inventory slot.
/// </summary>
[RequireComponent(typeof(CharacterController))] //If we drag onto object with no charactercontroller, it is forcibly added
public class FPSController : MonoBehaviour
{
    public List<AudioSource> AudioSources = new List<AudioSource>();
    private int audioToUse = 0;

    // Inspector Assigned Locomotion Settings
    [SerializeField] private float walkingSpeed = 2.0f;
    [SerializeField] private float runningSpeed = 4.5f;
    [SerializeField] private float jumpingSpeed = 7.5f;
    [SerializeField] private float crouchingSpeed = 1.0f;
    [SerializeField] private float stickingToGroundForce = 5.0f; //pushes char down, stops floating
    [SerializeField] private float gravityMultiplier = 2.5f;    //pushes player down when falling off ledges
    [SerializeField] private float runStepLengthen = 0.75f;
    [SerializeField] private CurveControlledBob headBob = new CurveControlledBob();
    [SerializeField] private GameObject flashLight = null;
    [SerializeField] private GameObject PickUpPanel;
    [SerializeField] private GameObject DoorPanel;
    [SerializeField] private Image m4Sprite;
    [SerializeField] private Image shotgunSprite;
    [SerializeField] private Image keySprite;
    [SerializeField] private Image endKeySprite;

    // Use Standard Assets Mouse Look class for mouse input -> Camera Look Control
    [SerializeField] private UnityStandardAssets.Characters.FirstPerson.MouseLook mouseLook = new UnityStandardAssets.Characters.FirstPerson.MouseLook();

    // Private internals
    private Camera _camera = null;
    private bool jumpButtonPressed = false;     //keeps track of when jump button pressed
    private Vector2 inputVector = Vector2.zero; //indirectly describes direction we want to move in
    private Vector3 moveDirection = Vector3.zero; //when mapped input vector into movement vector, store here. Pass to CharControllers move function
    private bool previouslyGrounded = false; //need to know when charatcer is on ground
    private bool isWalking = true;
    private bool isJumping = false;
    private bool isCrouching = false;
    private Vector3 localSpaceCameraPos = Vector3.zero;
    private float controllerHeight = 0.0f;
    private bool m4Enabled = false;
    private bool shotgunEnabled = false;
    private bool keyEnabled = false;
    private bool endKeyEnabled = false;
    private bool isShooting = false;

    // Timers
    private float fallingTimer = 0.0f; //incremneted by delta time each update when controller in air

    private CharacterController characterController = null;
    private PlayerMoveStatus movementStatus = PlayerMoveStatus.NotMoving;

    // Public Properties
    public PlayerMoveStatus MovementStatus
    {
        get
        {
            return movementStatus;
        }
    }
    public float walkSpeed
    {
        get
        {
            return walkingSpeed;
        }
    }
    public float runSpeed
    {
        get
        {
            return runningSpeed;
        }
    }


    protected void Start()
    {
        // component references
        characterController = GetComponent<CharacterController>();
        controllerHeight = characterController.height;

        // Get the main camera and save its local position within the FPS rig 
        _camera = Camera.main;
        localSpaceCameraPos = _camera.transform.localPosition; //every frame can generate offset frm this vector, so now rounding errors

        // Set movementStatus to not jumping and not moving
        movementStatus = PlayerMoveStatus.NotMoving;

        // Reset timers
        fallingTimer = 0.0f;

        // Setup Mouse Look Script. Uses MouseLook script thats already available.
        mouseLook.Init(transform, _camera.transform);

        // Initiate Head Bob Object
        headBob.Initialize();
        //Plays footstep sound when YplayHead passes the 1.5 second mark
        headBob.RegisterEventCallback(1.5f, PlayFootStepSound, CurveControlledBobCallbackType.Vertical);

        //Make sure the game starts wuth flashlight set to inactive
        if (flashLight)
            flashLight.SetActive(false);

        //All sprites in inventory set to inactive
        m4Sprite.enabled        =       false;
        shotgunSprite.enabled   =       false;
        keySprite.enabled       =       false;
        endKeySprite.enabled    =       false;
    }

    protected void Update()
    {
        if(Time.timeScale != 0)
        {
            // If grounded keep falling timer at 0. Else we are falling so increment timer
            if(characterController.isGrounded)
            {
                fallingTimer = 0.0f;
            }
            else
            {
                fallingTimer += Time.deltaTime;
            }

            // First checks if game is paused, if it isnt, then reads horizontal and vertical mouse movement. Either rotates char controller left/right for horizontal mouse movement
            //or rotate camera up and down for vertical mouse movement.
            if(Time.timeScale > Mathf.Epsilon)
            {
                mouseLook.LookRotation(transform, _camera.transform);
            }
                

            //if player press F, flashlight set to active
            if(Input.GetButtonDown("Flashlight"))
            {
                if(flashLight)
                    flashLight.SetActive(!flashLight.activeSelf);
            }

            //Jump button pressed then jumpButtonpressed set to true
            if(!jumpButtonPressed && !isCrouching)
            {
                jumpButtonPressed = Input.GetButtonDown("Jump");
            }
                

            //character controller height is halved when crouch button pressed
            if(Input.GetButtonDown("Crouch"))
            {
                isCrouching = !isCrouching;
                if(isCrouching == true)
                {
                    characterController.height = controllerHeight / 2.0f;
                }
                else
                {
                    characterController.height = controllerHeight;
                }
            }

            // Calculates Character Status
            if(!previouslyGrounded && characterController.isGrounded)
            {
                moveDirection.y = 0f;
                isJumping = false;
                movementStatus = PlayerMoveStatus.Landing;
            }
            else if(!characterController.isGrounded)
            {
                movementStatus = PlayerMoveStatus.NotGrounded;
            }
            else if(characterController.velocity.sqrMagnitude < 0.01f)
            { 
                movementStatus = PlayerMoveStatus.NotMoving;
            }
            else if(isCrouching)
            {
                movementStatus = PlayerMoveStatus.Crouching;
            }
            else if(isWalking)
            {
                movementStatus = PlayerMoveStatus.Walking;
            }
            
            else
                movementStatus = PlayerMoveStatus.Running;

            previouslyGrounded = characterController.isGrounded;
        }


    }

    /// <summary>
    /// Move direction vector accuratley describes the direction the player wants to move the controler even when player is not grounded
    /// it is a downward motion of player falling under gravity.
    /// </summary>
    protected void FixedUpdate()
    {
        if (Time.timeScale != 0)
        {
            // Read input from axis
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            bool waswalking = isWalking;
            isWalking = !Input.GetKey(KeyCode.LeftShift); //see if left shift is being held

            // uses quick if/else to set the speed to either walking or running speed
            float speed = isCrouching ? crouchingSpeed : isWalking ? walkingSpeed : runningSpeed;
            inputVector = new Vector2(horizontal, vertical);

            //inputVector to be multiplied by the speed and used to move player through world. If > 1 then it will move faster
            //than walkspeed or runspeed
            if (inputVector.sqrMagnitude > 1) inputVector.Normalize();

            //take inputVector, then calculate 3D vector that describes direction we want to move
            // Always move along the camera forward as it is the direction that it being aimed at
            Vector3 desiredMove = transform.forward * inputVector.y + transform.right * inputVector.x;

            // Get a normal for the surface that is being touched to move along it
            //if Spherecast returns true then standing on surface, need to project desiredmovement onto the plane
            RaycastHit hitInfo;                                                                                //amount want to cast ray down 
            if (Physics.SphereCast(transform.position, characterController.radius, Vector3.down, out hitInfo, characterController.height / 2f, 1))
                desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;
            //have a normalized vector that describes accuratley the direction player wants to move

            //now describes amount we want to move vertically and horizontally
            moveDirection.x = desiredMove.x * speed;
            moveDirection.z = desiredMove.z * speed;

            //Check if going up or down
            // If grounded
            if (characterController.isGrounded)
            {
                // Apply large down force to keep controller sticking to floor
                moveDirection.y = -stickingToGroundForce;

                // If the jump button was pressed then add speed in upwards
                // then set isJumping to true and reset jump button status
                if (jumpButtonPressed)
                {
                    moveDirection.y = jumpingSpeed;
                    jumpButtonPressed = false;
                    isJumping = true;
                }
            }
            else
            {
                // Otherwise we are not on the ground so apply standard system gravity multiplied
                // by our gravity modifier
                // If not grounded then controller is in air and falling so add current gravity force * the gravity multiplier
                moveDirection += Physics.gravity * gravityMultiplier * Time.fixedDeltaTime;
            }

            // Move the Character Controller
            characterController.Move(moveDirection * Time.fixedDeltaTime);

            // Are we moving
            //When we run the headbob wont be quite as fast
            Vector3 speedXZ = new Vector3(characterController.velocity.x, 0.0f, characterController.velocity.z); //zero out any up and down movement
            if (speedXZ.magnitude > 0.01f)
            {
                _camera.transform.localPosition = localSpaceCameraPos + headBob.GetVectorOffset(speedXZ.magnitude * (isCrouching || isWalking ? 1.0f : runStepLengthen));
            }
            else
            {
                _camera.transform.localPosition = localSpaceCameraPos;
            }
                
        }
    }

    //when crouching then the sound will not play
    void PlayFootStepSound()
    {
        if (isCrouching)
            return;

        AudioSources[audioToUse].Play();
        audioToUse = (audioToUse == 0) ? 1 : 0;
    }


   /// <summary>
   /// When the FPSController collides with an object's collider with these names, the UI text will display how to pick up the item.
   /// </summary>
   /// <param name="other"></param>
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name.Equals("M4Pickup") || other.gameObject.name.Equals("ShotgunPickup") || other.gameObject.name.Equals("HPBox")
            || other.gameObject.name.Equals("Key") || other.gameObject.name.Equals("EndKey"))
        {
            OpenPickUpPanel();
        }
    }

    /// <summary>
    /// When FPSController collides with a gameobjects collider with a specific name, and stays inside the collider, the player can press E 
    /// to pick up the item. This will disable the object and enable the objects sprite which appears in the players inventory. Upon picking 
    /// up the item, the UI text will also be set to not enabled.
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.name.Equals("HPBox"))
        {
            ClosePickUpPanel();
        }

        if (other.gameObject.name.Equals("M4Pickup"))
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                other.gameObject.SetActive(false);
                m4Sprite.enabled = true;
                m4Enabled = true;
                ClosePickUpPanel();
            }


        }

        if (other.gameObject.name.Equals("ShotgunPickup"))
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                other.gameObject.SetActive(false);
                shotgunSprite.enabled = true;
                shotgunEnabled = true;
                Debug.Log("ShotgunPickedUp");
                ClosePickUpPanel();
            }

        }

        if (other.gameObject.name.Equals("Key"))
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                other.gameObject.SetActive(false);
                keySprite.enabled = true;
                keyEnabled = true;
                Debug.Log("KeyPickedUp");
                ClosePickUpPanel();
            }
        }
        if (other.gameObject.name.Equals("EndKey"))
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                other.gameObject.SetActive(false);
                endKeySprite.enabled = true;
                endKeyEnabled = true;
                Debug.Log("EndKeyPickedUp");
                ClosePickUpPanel();
            }
        }

    }

    /// <summary>
    /// If an object has a specific name and the player leaves this objects collider then the Pickup panel will close.
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name.Equals("M4Pickup") || other.gameObject.name.Equals("ShotgunPickup") || other.gameObject.name.Equals("HPBox")
            || other.gameObject.name.Equals("Key") || other.gameObject.name.Equals("EndKey"))
        {
            ClosePickUpPanel();
        }

    }

    /// <summary>
    /// The panel in the UI will be set to active when method called. This panel tells the player how to pick up a weapon.
    /// </summary>
    public void OpenPickUpPanel()
    {
        PickUpPanel.SetActive(true);
    }

    /// <summary>
    /// The panel in the UI will be set to not active when method called. This panel tells the player how to pick up a weapon.
    /// </summary>
    public void ClosePickUpPanel()
    {
        PickUpPanel.SetActive(false);
    }

    
    /// <summary>
    /// The DoorPanel in the UI tells the player how to open a door. When this method is called the DoorPanel text is set to enabled.
    /// </summary>
    public void OpenDoorPanel()
    {
        DoorPanel.SetActive(true);
    }

    /// <summary>
    /// <summary>
    /// The DoorPanel in the UI tells the player how to open a door. When this method is called the DoorPanel text is set to not enabled.
    /// </summary>
    public void CloseDoorPanel()
    {
        DoorPanel.SetActive(false);
    }

    /// <summary>
    /// Method returns the boolean value of m4Enabled. 
    /// </summary>
    public bool M4Enabled
    {
        get
        {
            return m4Enabled;
        }
    }

    /// <summary>
    /// Method returns the boolean value of shotgunEnabled.
    /// </summary>
    public bool ShotgunEnabled
    {
        get
        {
            return shotgunEnabled;
        }
    }

    /// <summary>
    /// Method returns the boolean value of keyEnabled.
    /// </summary>
    public bool KeyEnabled
    {
        get
        {
            return keyEnabled;
        }
    }

    /// <summary>
    /// Method returns the boolean value of endKeyEnabled.
    /// </summary>
    public bool EndKeyEnabled
    {
        get
        {
            return endKeyEnabled;
        }
    }

    /// <summary>
    /// Method returns the boolean value of isShooting.
    /// </summary>
    public bool getIsShooting()
    {
        return isShooting;
    }
}
