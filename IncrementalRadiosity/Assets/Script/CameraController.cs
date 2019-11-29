using UnityEngine;

enum CameraType
{
    World
}

public class CameraController : MonoBehaviour
{
    [SerializeField] private Camera[] cameras = new Camera[1];
    [SerializeField] private float cameraMoveSpeed = 0.05f;
    [SerializeField] private float cameraRotateAngle = 25f;
    private Vector3 originPos = Vector3.zero;
    private Quaternion originRotation;

    void Start()
    {
        originPos = cameras[0].transform.position;
        originRotation = cameras[0].transform.rotation;
    }
    public void CameraControl()
    {
        //WASD
        if (Input.GetKey(KeyCode.W))
        {
            cameras[0].transform.Translate(new Vector3(0, 0, cameraMoveSpeed * 25 * Time.deltaTime));
        }
        if (Input.GetKey(KeyCode.S))
        {
            cameras[0].transform.Translate(new Vector3(0, 0, -1 * cameraMoveSpeed * 25 * Time.deltaTime));
        }
        if (Input.GetKey(KeyCode.A))
        {
            cameras[0].transform.Translate(new Vector3(-1 * cameraMoveSpeed * 1, 0, 0 * Time.deltaTime));
        }
        if (Input.GetKey(KeyCode.D))
        {
            cameras[0].transform.Translate(new Vector3(cameraMoveSpeed * 1, 0, 0 * Time.deltaTime));
        }
        //↑↓←→
        //Translate in +Y axis
        if (Input.GetKey(KeyCode.UpArrow))
        {
            cameras[0].transform.Translate(new Vector3(0, cameraMoveSpeed * 1, 0 * Time.deltaTime));
        }
        //Translate in -Y axis
        if (Input.GetKey(KeyCode.DownArrow))
        {
            cameras[0].transform.Translate(new Vector3(0, -cameraMoveSpeed * 1, 0 * Time.deltaTime));
        }
        //Rotate in -Y axis
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            cameras[0].transform.Rotate(new Vector3(0, -cameraMoveSpeed * cameraRotateAngle, 0 * Time.deltaTime));
        }
        //Rotate in +Y axis
        if (Input.GetKey(KeyCode.RightArrow))
        {
            cameras[0].transform.Rotate(new Vector3(0, cameraMoveSpeed * cameraRotateAngle, 0 * Time.deltaTime));
        }
        //Reset
        if (Input.GetKey(KeyCode.R))
        {
            ResetCamera();
        }
        
    }

    private void ResetCamera()
    {
        cameras[0].transform.position = originPos;
        cameras[0].transform.rotation = originRotation;
    }
}
