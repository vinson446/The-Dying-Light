using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCam : MonoBehaviour
{
    public Transform cameraPos;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        UpdateCameraPos();
    }

    void UpdateCameraPos()
    {
        // camera obj can get wonky if attached to a rigidbody gameobj
        // this fixes the potential problem -> set camera holder pos = camera pos gameobj on the player
        transform.position = cameraPos.position;
    }
}
