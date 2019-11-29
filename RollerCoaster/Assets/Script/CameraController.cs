using UnityEngine;

enum CameraType
{
    World,
    Top,
    Train,
    Render
}
public class CameraController : MonoBehaviour
{
    /***********************************************************Flying camera***********************************************************/
    //[SerializeField] private GameObject[] standObjs;
    //[SerializeField] private GameObject earthObj;
    //[SerializeField] private Transform cameraTranform;
    //public float mainSpeed = 100.0f;
    //public float shiftAdd = 250.0f;
    //public float maxShift = 1000.0f;
    //public float camSens = 0.25f;
    //private Vector3 lastMouse = new Vector3(255, 255, 255);
    //private float totalRun = 1.0f;
    //private float rotateAngle = 0.5f;
    //private float earthAngle = 0f;
    //private float radius = 500;
    //private float timer = 0;
    ////WASD
    //private Vector3 GetBaseInput()
    //{
    //    Vector3 p_Velocity = new Vector3();
    //    if (Input.GetKey(KeyCode.W))
    //    {
    //        p_Velocity += new Vector3(0, 0, 1);
    //    }
    //    if (Input.GetKey(KeyCode.S))
    //    {
    //        p_Velocity += new Vector3(0, 0, -1);
    //    }
    //    if (Input.GetKey(KeyCode.A))
    //    {
    //        p_Velocity += new Vector3(-1, 0, 0);
    //    }
    //    if (Input.GetKey(KeyCode.D))
    //    {
    //        p_Velocity += new Vector3(1, 0, 0);
    //    }
    //    return p_Velocity;
    //}

    //void Update()
    //{
    //    //Press space to navigate
    //    if(Input.GetKey(KeyCode.Space))
    //    {
    //        lastMouse = Input.mousePosition - lastMouse;
    //        lastMouse = new Vector3(-lastMouse.y * camSens, lastMouse.x * camSens, 0);
    //        lastMouse = new Vector3(cameraTranform.eulerAngles.x + lastMouse.x, cameraTranform.eulerAngles.y + lastMouse.y, 0);
    //        cameraTranform.eulerAngles = lastMouse;
    //        lastMouse = Input.mousePosition;
    //        Vector3 p = GetBaseInput();
    //        if (Input.GetKey(KeyCode.LeftShift))
    //        {
    //            totalRun += Time.deltaTime;
    //            p = p * totalRun * shiftAdd;
    //            p.x = Mathf.Clamp(p.x, -maxShift, maxShift);
    //            p.y = Mathf.Clamp(p.y, -maxShift, maxShift);
    //            p.z = Mathf.Clamp(p.z, -maxShift, maxShift);
    //        }
    //        else
    //        {
    //            totalRun = Mathf.Clamp(totalRun * 0.5f, 1f, 1000f);
    //            p = p * mainSpeed;
    //        }
    //        p = p * Time.deltaTime;
    //        Vector3 newPosition = cameraTranform.position;
    //        if (Input.GetKey(KeyCode.Space))
    //        {
    //            cameraTranform.Translate(p);
    //            newPosition.x = cameraTranform.position.x;
    //            newPosition.z = cameraTranform.position.z;
    //            cameraTranform.position = newPosition;
    //        }
    //        else
    //        {
    //            cameraTranform.Translate(p);
    //        }
    //    }
    //    //
    //    standObjs[0].transform.rotation *= Quaternion.Euler(0,rotateAngle,0);
    //    if(Time.time - timer > 0.2)
    //    {
    //        timer = Time.time;
    //        earthAngle += 0.2f;
    //        if (earthAngle > 360) earthAngle = 0;
    //    }
    //    earthObj.transform.position = new Vector3(standObjs[0].transform.position.x + radius * Mathf.Cos(earthAngle), 150, standObjs[0].transform.position.z + radius * Mathf.Sin(earthAngle));
    //}
    /***********************************************************Flying camera***********************************************************/
    [SerializeField] private GameManager manager;
    [SerializeField] private Camera[] cameras = new Camera[3];
    [SerializeField] private float cameraMoveSpeed = 0.05f;
    [SerializeField] private float cameraRotateAngle = 25f;
    [SerializeField] private Vector3 origin = Vector3.zero;
    public void CameraUpdate()
    {
        CameraControl();
    }

    public void CameraInitialize()
    {
        SetCamera((int)CameraType.World,true);
        SetCamera((int)CameraType.Top, false);        
        cameras[(int)CameraType.Train] = manager.GetObject((int)Object.Train).transform.GetChild(0).GetComponent<Camera>();
        SetCamera((int)CameraType.Train, false);
    }

    private void CameraControl()
    {
        //WASD
        if (Input.GetKey(KeyCode.W))
        {
            cameras[0].transform.Translate(new Vector3(0, 0, cameraMoveSpeed * 2500 * Time.deltaTime));
        }        
        if (Input.GetKey(KeyCode.S))
        {
            cameras[0].transform.Translate(new Vector3(0, 0, -1 * cameraMoveSpeed * 2500 * Time.deltaTime));
        }       
        if (Input.GetKey(KeyCode.A))
        {
            cameras[0].transform.Translate(new Vector3(-1 * cameraMoveSpeed * 100, 0, 0 * Time.deltaTime));
        }        
        if (Input.GetKey(KeyCode.D))
        {
            cameras[0].transform.Translate(new Vector3(cameraMoveSpeed * 100, 0, 0 * Time.deltaTime));
        }
        //↑↓←→
        //Translate in +Y axis
        if (Input.GetKey(KeyCode.UpArrow))
        {
            cameras[0].transform.Translate(new Vector3(0, cameraMoveSpeed * 100, 0 * Time.deltaTime));
        }
        //Translate in -Y axis
        if (Input.GetKey(KeyCode.DownArrow))
        {
            cameras[0].transform.Translate(new Vector3(0, -cameraMoveSpeed * 100, 0 * Time.deltaTime));
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

    public void SetCamera(int idx, bool status)
    {
        cameras[idx].gameObject.SetActive(status);
        if (idx == (int)CameraType.Render)
        {
            cameras[idx].depthTextureMode = DepthTextureMode.Depth;
        }
    }
    public Camera GetCamera(int idx)
    {
        return cameras[idx];
    }
    private void ResetCamera()
    {
        cameras[0].transform.position = origin;
    }
}
