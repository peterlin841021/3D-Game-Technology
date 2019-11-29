using UnityEngine;
using UnityEngine.UI;

public class ButtonController : MonoBehaviour
{
    [SerializeField] private GameManager manager;          
    private const int STAND_INDEX = 0;

    void Start()
    {
        //Get all buttons
        int buttonCounts = manager.GetObject((int)Object.Canvas).transform.GetChild(0).transform.childCount;
        GameObject layout = manager.GetObject((int)Object.Canvas).transform.GetChild(0).gameObject;        
        for (int i = 0;i < buttonCounts;i++)
        {
            GameObject currenObj = layout.transform.GetChild(i).gameObject;  
            if(currenObj.GetComponent<Button>())
                currenObj.GetComponent<Button>().onClick.AddListener(delegate { ChangeView(currenObj.GetComponentInChildren<Text>().text);});
        }        
    }

    void ChangeView(string view)
    {        
        switch (view)
        {
            case "Top":
                manager.GetCameraController().SetCamera((int)CameraType.World, false);
                manager.GetCameraController().SetCamera((int)CameraType.Top, true);
                manager.GetCameraController().SetCamera((int)CameraType.Train, false);
                break;
            case "World":
                manager.GetCameraController().SetCamera((int)CameraType.World, true);
                manager.GetCameraController().SetCamera((int)CameraType.Top, false);
                manager.GetCameraController().SetCamera((int)CameraType.Train, false);                                
                break;
            case "Train":
                manager.GetCameraController().SetCamera((int)CameraType.World, false);
                manager.GetCameraController().SetCamera((int)CameraType.Top, false);
                manager.GetCameraController().SetCamera((int)CameraType.Train, true);
                break;
            case "Add":                
                if (LineController.GetLastControlPointInstanceID() != 0)
                {                    
                    manager.GetLineController().AddControlPoint(LineController.GetLastControlPointInstanceID());
                }
                break;
            case "Remove":
                if (LineController.GetLastControlPointInstanceID() != 0)
                {
                    manager.GetLineController().RemoveControlPoint(LineController.GetLastControlPointInstanceID());
                }
                break;
            case "RotateX":
                if (LineController.GetLastControlPointInstanceID() != 0)
                {
                    manager.GetLineController().RotateControlPointX(LineController.GetLastControlPointInstanceID());
                }
                break;
            case "RotateZ":
                if (LineController.GetLastControlPointInstanceID() != 0)
                {
                    manager.GetLineController().RotateControlPointZ(LineController.GetLastControlPointInstanceID());
                }
                break;
            case "Linear":
                manager.GetLineController().SetTrackType(0);
                manager.GetLineController().UpdateTrack(manager.GetLineController().GetTrackType());
                break;
            case "Cardinal Spline":
                manager.GetLineController().SetTrackType(1);
                manager.GetLineController().UpdateTrack(manager.GetLineController().GetTrackType());
                break;
            case "B Spline":
                manager.GetLineController().SetTrackType(2);
                manager.GetLineController().UpdateTrack(manager.GetLineController().GetTrackType());
                break;            
            case "GrayScale":
                if(manager.GetMaterialController().GetSpecialEffectMaterial((int)MaterialTarget.Miku).GetInt("_EffectType") != 1)
                    manager.GetMaterialController().GetSpecialEffectMaterial((int)MaterialTarget.Miku).SetInt("_EffectType", 1);
                else
                    manager.GetMaterialController().GetSpecialEffectMaterial((int)MaterialTarget.Miku).SetInt("_EffectType", 0);                               
                break;
            case "Invert":
                if (manager.GetMaterialController().GetSpecialEffectMaterial((int)MaterialTarget.Miku).GetInt("_EffectType") != 2)
                    manager.GetMaterialController().GetSpecialEffectMaterial((int)MaterialTarget.Miku).SetInt("_EffectType", 2);
                else
                    manager.GetMaterialController().GetSpecialEffectMaterial((int)MaterialTarget.Miku).SetInt("_EffectType", 0);                
                break;
            case "Save track":
                manager.GetLineController().SaveTrack();
                break;
            case "Load track":
                manager.GetLineController().LoadTrack();
                break;
        }
    }    
}
