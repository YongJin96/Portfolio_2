using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    #region Variables

    private Vector3 OriginRightDoor;
    private Vector3 OriginLeftDoor;

    public GameObject OpenRightDoor;
    public GameObject OpenLeftDoor;

    public GameObject CloseRightDoor;
    public GameObject CloseLeftDoor;

    public bool IsOpen = false;
    public float Speed;

    #endregion

    #region Init

    private void Start()
    {
        OriginRightDoor = OpenRightDoor.transform.position;
        OriginLeftDoor = OpenLeftDoor.transform.position;
    }

    private void Update()
    {
        OpenDoor();
        CloseDoor();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            IsOpen = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            IsOpen = false;
        }
    }

    #endregion

    #region Function

    private void OpenDoor()
    {
        if (IsOpen == true)
        {
            OpenRightDoor.transform.position = Vector3.Lerp(OpenRightDoor.transform.position, CloseRightDoor.transform.position, Time.deltaTime * Speed);
            OpenLeftDoor.transform.position = Vector3.Lerp(OpenLeftDoor.transform.position, CloseLeftDoor.transform.position, Time.deltaTime * Speed);
        }
    }

    private void CloseDoor()
    {
        if (IsOpen == false)
        {
            OpenRightDoor.transform.position = Vector3.Lerp(OpenRightDoor.transform.position, OriginRightDoor, Time.deltaTime * Speed);
            OpenLeftDoor.transform.position = Vector3.Lerp(OpenLeftDoor.transform.position, OriginLeftDoor, Time.deltaTime * Speed);
        }
    }

    #endregion
}
