using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// EndDoor is the class used to control whether the final door. The class measures the distance between the player and the door and if its less
/// than 3 meters and the player owns a key, the player sees a text telling them how to open the door. If the player presses Q then the door 
/// will open and they can enter.
/// </summary>
public class EndDoor : MonoBehaviour
{
    public GameObject endDoor;
    public GameObject fpsPlayer1;
    public float distanceTest2;
    public bool isLevelComplete = false;

    private FPSController personController;

    /// <summary>
    /// Finds the gameObject with the FPSController script attached to it.
    /// </summary>
    void Start()
    {
        personController = GameObject.FindObjectOfType<FPSController>();

    }

    /// <summary>
    /// Monitors the distance between the player and the endDoors position.
    /// EndControlDoor is called every frame.
    /// </summary>
    void Update()
    {
        distanceTest2 = Vector3.Distance(endDoor.transform.position, fpsPlayer1.transform.position);
        EndControlDoor();
    }


    /// <summary>
    /// This method is used to open the EndDoor gameObject. distanceTest2 is used to measure the distance between the FPSController and the 
    /// EndDoor gameObject. When this distance is less than 3 metres and the player has the EndKey in their inventory slot, the OpenDoorPanel method
    /// from the FPSController class will play. This displays text on the screen telling the player how to open the door. When the player
    /// presses Q, the EndDoor gameObject is set to not active. If the player moves away from the door, or they do not posses the EndKey inventory
    /// item, then the CloseDoorPanel plays and the text on screen will not display.
    /// </summary>
    void EndControlDoor()
    {
        if (distanceTest2 < 3 && personController.EndKeyEnabled == true)
        {
            personController.OpenDoorPanel();
            if (Input.GetKeyDown(KeyCode.Q))
            {
                isLevelComplete = true;
                //Debug.Log("end door open");
                endDoor.SetActive(false);
            }
        }
        if (distanceTest2 > 3 || endDoor.activeInHierarchy == false)
        {
            personController.CloseDoorPanel();
        }
    }


}
