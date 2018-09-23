using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// WeaponSway adds extra movement to the guns position when the player moves the camera around to simulate how a gun sways in real life. The class
/// also sets a speed to how fast the weapon should sway.
/// </summary>
public class WeaponSway : MonoBehaviour
{

    public float amount; //how much weapon sways left/right
    public float maxAmount;
    public float smoothAmount; //how fats weapon should sway

    private Vector3 initialPosition;

    private void Start()
    {
        initialPosition = transform.localPosition; //local position respects its parents position
    }

    /// <summary>
    /// Sways the weapon when moving weapon around.
    /// </summary>
    void LateUpdate()
    {
        //inverts right to left, left to right
        float movementX = -Input.GetAxis("Mouse X") * amount;
        float movementY = -Input.GetAxis("Mouse Y") * amount;
        movementX = Mathf.Clamp(movementX, -maxAmount, maxAmount);  //limit movementX based on maxAmount
        movementY = Mathf.Clamp(movementY, -maxAmount, maxAmount);  //limit movementY based on maxAmount

        Vector3 finalPosition = new Vector3(movementX, movementY, 0);
        transform.localPosition = Vector3.Lerp(transform.localPosition, finalPosition + initialPosition, Time.deltaTime * smoothAmount);    //lerp = from a to b at what speed


    }
}
