using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class controls the Door gameObject. The door starts off closed and will block the players attempts to pass. The class monitors 
/// the distance between the player and the door every frame. The ControlDoor method sets conditions that must be met in order for the door to open.
/// These conditions are distance between zombie and player and whether the player possesses a key.
/// </summary>
public class Door : MonoBehaviour {

    public GameObject door;
    public GameObject fpsPlayer1;
    public float distanceTest;

    private FPSController personController;

    /// <summary>
    /// Use this for initialization.
    /// </summary>
    void Start ()
    {
        personController = GameObject.FindObjectOfType<FPSController>();

    }

    /// <summary>
    /// Update is called once per frame.
    /// </summary>
    void Update ()
    {
        distanceTest = Vector3.Distance(door.transform.position, fpsPlayer1.transform.position);

        ControlDoor();

	}

    /// <summary>
    /// This method is used to open the Door gameObject. distanceTest is used to measure the distance between the FPSController and the 
    /// Door gameObject. When this distance is less than 3 metres and the player has the Key in their inventory slot, the OpenDoorPanel method
    /// from the FPSController class will play. This displays text on the screen telling the player how to open the door. When the player
    /// presses Q, the Door gameObject is set to not active. If the player moves away from the door, or they do not posses the Key inventory
    /// item, then the CloseDoorPanel plays and the text on screen will not display.
    /// </summary>
    void ControlDoor()
    {
        if(distanceTest < 3 && personController.KeyEnabled == true)
        {
            personController.OpenDoorPanel();
            if(Input.GetKeyDown(KeyCode.Q))
            {
                //Debug.Log("door open");
                door.SetActive(false);
            }
        }


        if(distanceTest > 3 || door.activeInHierarchy == false)
        {
            personController.CloseDoorPanel();
        }
    }

   
    
   
}
