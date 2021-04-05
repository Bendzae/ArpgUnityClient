using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{    
    [SerializeField]
    private Transform target;

    private Camera cam;
    private Vector3 offset;
    // Start is called before the first frame update
    void Start()
    {
        offset = transform.position - target.position;
        cam = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        var newPos = new Vector3(target.position.x, 0, target.position.z) + offset;
        transform.position = Vector3.Lerp(transform.position, newPos, Time.deltaTime * 5);

        if (Input.mouseScrollDelta.y < 0)
        {
            cam.orthographicSize = cam.orthographicSize + 0.5f;
        }
        if (Input.mouseScrollDelta.y > 0)
        {
            cam.orthographicSize = cam.orthographicSize - 0.5f;
        }
    }
}
