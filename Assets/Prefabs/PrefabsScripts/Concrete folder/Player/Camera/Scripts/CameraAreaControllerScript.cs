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
        if(transform.position.x > 6000f) { CorrectPosition(new Vector3(2000f,transform.position.y,transform.position.z));}
        if(transform.position.x < 300f) { CorrectPosition(new Vector3(500f, transform.position.y, transform.position.z)); }
        if(transform.position.z > 6000f) { CorrectPosition(new Vector3(transform.position.x, transform.position.y, 4000f)); }
        if(transform.position.z < 1000f) { CorrectPosition(new Vector3(transform.position.x, transform.position.y, 2500f)); } 
    }
    private void CorrectPosition(Vector3 positionNeed)
    {
        transform.position = positionNeed;
    }
}
