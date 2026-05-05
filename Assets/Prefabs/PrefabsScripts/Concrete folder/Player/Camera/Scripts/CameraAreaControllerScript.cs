using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraAreaControllerScript : MonoBehaviour
{
    private void Update()
    {
        CheckArea();
    }
    private void CheckArea()
    {
        if(transform.position.x > 6000f) { CorrectPosition(new Vector3(6000f,transform.position.y,transform.position.z));}
        if(transform.position.x < 300f) { CorrectPosition(new Vector3(300f, transform.position.y, transform.position.z)); }
        if(transform.position.z > 6000f) { CorrectPosition(new Vector3(transform.position.x, transform.position.y, 6000f)); }
        if(transform.position.z < 1000f) { CorrectPosition(new Vector3(transform.position.x, transform.position.y, 1000f)); } 
    }
    private void CorrectPosition(Vector3 positionNeed)
    {
        transform.position = positionNeed;
    }
}
