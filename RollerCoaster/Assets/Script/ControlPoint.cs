
using UnityEngine;

public class ControlPoint : MonoBehaviour
{
    private Vector3 screenPoint;
    private Vector3 offset;
    private GameObject objInstance;

    public ControlPoint()
    {
        
    }
   
    void OnMouseDown()
    {
        screenPoint = Camera.allCameras[0].WorldToScreenPoint(gameObject.transform.position);
        offset = gameObject.transform.position - Camera.allCameras[0].ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));
        LineController.SetCurrentControlPointInstanceID(gameObject.GetInstanceID());
    }

    void OnMouseDrag()
    {
        Vector3 cursorPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);
        Vector3 cursorPosition = Camera.allCameras[0].ScreenToWorldPoint(cursorPoint) + offset;
        transform.position = cursorPosition;        
    }    
}