using UnityEngine;
using UnityEngine.UI;

enum Object
{
    Controlpoint,
    Support,
    Train,
    Canvas,
    MikuStand,
    Earth
}

public class GameManager : MonoBehaviour
{
    [SerializeField] private ButtonController buttonController;
    [SerializeField] private CameraController cameraController;
    [SerializeField] private LineController lineController;
    [SerializeField] private MaterialController materialController;
    [SerializeField] private GameObject[] objects = new GameObject[6];
    [SerializeField] private Slider speedSlider;
    private float timer1;
    private float timer2;
    private int trackPositionIndex = 0;
    private Vector3[] positions;
    private Vector3[] orientations;
    private float radius = 100f;
    private float angle = 0f;
    void Start ()
    {
        timer1 = Time.time;
        ObjectInitialize();
        cameraController.CameraInitialize();
        materialController.MaterialSetting();
        lineController.TrackInInitialize();
        lineController.UpdateTrack(lineController.GetTrackType());
    }
		
	void Update ()
    {        
        cameraController.CameraUpdate();
        lineController.CheckControllPoint(LineController.GetCurrentControlPointInstanceID());
        if(LineController.GetLastControlPointInstanceID() != 0)
            lineController.UpdateTrack(lineController.GetTrackType());
        if (Time.time - timer1 > 5f/(speedSlider.value * 4f))
        {
            timer1 = Time.time;
            trackPositionIndex++;
            if (trackPositionIndex > positions.Length - 1)
                trackPositionIndex = 0;           
        }
        if(Time.time - timer2 > 0.2)
        {
            timer2 = Time.time;
            angle += 0.2f;
            if (angle >= 360f)
                angle = 0;
        }
        if(positions != null && positions.Length > 0)
        {            
            PutTrain(
                positions[trackPositionIndex % positions.Length], 
                positions[(trackPositionIndex + 1) % positions.Length] - positions[trackPositionIndex % positions.Length],
                orientations[trackPositionIndex % orientations.Length]);
        }
        //
        objects[(int)Object.MikuStand].transform.rotation = Quaternion.Euler(0, angle * 50, 0);
        objects[(int)Object.Earth].transform.position = new Vector3(objects[(int)Object.MikuStand].transform.position.x + radius * Mathf.Cos(angle), 150, objects[(int)Object.MikuStand].transform.position.z + radius * Mathf.Sin(angle));
    }

    private void ObjectInitialize()
    {
        objects[(int)Object.Train] = Instantiate(objects[(int)Object.Train]);
    }

    public ButtonController GetButtonController()
    {
        return buttonController;
    }

    public CameraController GetCameraController()
    {
        return cameraController;
    }

    public LineController GetLineController()
    {
        return lineController;
    }

    public MaterialController GetMaterialController()
    {
        return materialController;
    }

    public GameObject GetObject(int idx)
    {
        return objects[idx];
    }

    private void PutTrain(Vector3 pos, Vector3 front, Vector3 ori)
    {
        pos.y += 5f;
        objects[(int)Object.Train].transform.position = pos;
        objects[(int)Object.Train].transform.up = ori;
        objects[(int)Object.Train].transform.rotation = Quaternion.LookRotation(front, objects[(int)Object.Train].transform.up);
    }

    public void UpdatePositionAndOrientation(Vector3[] pos,Vector3[] ori)
    {
        positions = pos;
        orientations = ori;       
    }
}
