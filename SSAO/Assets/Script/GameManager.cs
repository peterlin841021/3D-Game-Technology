using UnityEngine;
using UnityEngine.UI;

enum Object
{
    Miku,
    Bunny,
    Sibenik
}

public class GameManager : MonoBehaviour
{
    [SerializeField] private CameraController  cameraController = null;
	[SerializeField] private ContourController contourController = null;
    [SerializeField] private SuggestiveContourController suggestiveContourController = null;
    [SerializeField] private SSAOController ssaoController = null;
    [SerializeField] private RayTrace raytracer = null;
    [SerializeField] private Light sun = null;
    [SerializeField] private Transform bunnyTransform = null;
    [SerializeField] private GameObject[] objects = null;
    [SerializeField] private Slider[] AOSliders = new Slider[3];
    [SerializeField] private Slider[] ContourSliders = new Slider[4];
    [SerializeField] private Slider[] SuggestiveContourSliders = new Slider[4];
    
    public void ShowBunny()
    {
        if (!objects[(int)Object.Bunny].activeSelf)
        {
            objects[(int)Object.Bunny].SetActive(true);
            objects[(int)Object.Miku].SetActive(false);
            objects[(int)Object.Sibenik].SetActive(false);
        }
        else
        {
            objects[(int)Object.Bunny].SetActive(false);
            objects[(int)Object.Miku].SetActive(true);
            objects[(int)Object.Sibenik].SetActive(true);
        }
    }

    public void LightSetting()
    {
        sun.gameObject.SetActive(!sun.gameObject.activeSelf);
    }

    public void SSAOSetting()
    {
        if (!ssaoController.GetShowAO() && !ssaoController.enabled)
        {
            ssaoController.enabled = true;
            ssaoController.SetShowAO(false);
        }
        else if (!ssaoController.GetShowAO() && !ssaoController.GetShowSC() && ssaoController.enabled)
        {
            ssaoController.SetShowAO(true);
        }
        else if (ssaoController.GetShowAO() && !ssaoController.GetShowSC() && ssaoController.enabled)
        {
            ssaoController.SetShowSC(true);           
        }
        else
        {
            ssaoController.enabled = false;
            ssaoController.SetShowAO(false);
            ssaoController.SetShowSC(false);
        }
    }

    public void ContourSetting()
    {        
        contourController.enabled = !contourController.enabled;
    }

    public void DoRayTrace()
    {
        raytracer.Rendering();
    }

    void Update ()
    {        
        bunnyTransform.rotation *= Quaternion.Euler(0, 1f, 0);
        cameraController.CameraControl();
        contourController.ContourUpdate();
        raytracer.RayTracingUdpate();        
               
        if(objects[(int)Object.Bunny].activeSelf)
        {
            

            if(!SuggestiveContourSliders[0].gameObject.activeSelf)
            {
                for (int i = 0; i < AOSliders.Length; i++)
                    AOSliders[i].gameObject.SetActive(false);
                for (int i = 0; i < ContourSliders.Length; i++)
                    ContourSliders[i].gameObject.SetActive(false);
                for (int i = 0; i < SuggestiveContourSliders.Length; i++)
                    SuggestiveContourSliders[i].gameObject.SetActive(true);
            }
            suggestiveContourController.SetMTLProperty(SC_Property._FZ.ToString(), SuggestiveContourSliders[0].value);
            suggestiveContourController.SetMTLProperty(SC_Property._C_Limit.ToString(), SuggestiveContourSliders[1].value);
            suggestiveContourController.SetMTLProperty(SC_Property._SC_Limit.ToString(), SuggestiveContourSliders[2].value);
            suggestiveContourController.SetMTLProperty(SC_Property._DWKR_Limit.ToString(), SuggestiveContourSliders[3].value);
        }
        else
        {
            if (!AOSliders[0].gameObject.activeSelf)
            {
                for (int i = 0; i < AOSliders.Length; i++)
                    AOSliders[i].gameObject.SetActive(true);
                for (int i = 0; i < ContourSliders.Length; i++)
                    ContourSliders[i].gameObject.SetActive(true);
                for (int i = 0; i < SuggestiveContourSliders.Length; i++)
                    SuggestiveContourSliders[i].gameObject.SetActive(false);
            }            
            ssaoController.SetAOStrength(AOSliders[0].value);
            ssaoController.SetSampleCount((int)AOSliders[1].value);
            ssaoController.SetSampleRadius(AOSliders[2].value);

            contourController.SetLowerThreshold(ContourSliders[0].value);
            contourController.SetUpperThreshold(ContourSliders[1].value);
            contourController.SetColorSensitivity(ContourSliders[2].value);
            contourController.SetDepthSensitivity(ContourSliders[3].value);
        }
    }
}
