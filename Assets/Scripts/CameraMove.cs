using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMove : MonoBehaviour
{
    public float rotateSpeed = 2.0f;
    public float angleSpeed = 1.0f;
    public float zoomSpeed = 1.5f;
    public GameObject vertAngle;
    public GameObject cameraObject;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(2))
        {
            float h = rotateSpeed * Input.GetAxis("Mouse X");
            float v = angleSpeed * -Input.GetAxis("Mouse Y");
            transform.Rotate(0, h, 0);
            vertAngle.transform.Rotate(v, 0, 0);
            //mainCamera.transform.Rotate(v, 0, 0);
        }

        float z = zoomSpeed * Input.GetAxis("Mouse ScrollWheel");
        Vector3 newCameraPos = cameraObject.transform.localPosition;
        newCameraPos.z = newCameraPos.z + z;
        //newCameraPos.z = Mathf.Clamp(newCameraPos.z, -5, -15);
        cameraObject.transform.localPosition = newCameraPos;
    }
}
