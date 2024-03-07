
using UnityEngine;

public class BillboardLookAt : MonoBehaviour
{


    private Camera targetCamera;

    public Camera TargetCamera { get => this.targetCamera; set => this.targetCamera = value; }

    // Start is called before the first frame update
    void Start()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

    }

    // Update is called once per frame
    void Update()
    {

        if (targetCamera == null)
            return;

        

        Vector3 camPos = targetCamera.transform.position;
 
        transform.LookAt(
            new Vector3(camPos.x, transform.position.y, camPos.z)
            );
    }



}
