using UnityEngine;
using UnityEngine.UI;

public class PanelController : MonoBehaviour
{
	[SerializeField] private Radiosity radiosity;
    [SerializeField] private Canvas uiCanvas;
    [SerializeField] private Text FPSText;
    [SerializeField] private Text VPLText;
    [SerializeField] private Text VPLCountText;
    [SerializeField] private Text SpotAngleText;
    private float deltaTime = 0.0f;

    public void SetVPLVisibility(bool visible)
    {
        VPLText.text = visible ? "Show VPLS" : "Hide VPLS";
        radiosity.SetVPLVisibility(visible);
    }
    public void SetVPLCount(float count)
    {
        VPLCountText.text = "VPL Count:" + (int)count;
        radiosity.SetVPLCounts((int)count);
    }
    public void SetSpotLightAngle(float angle)
    {
        float a = radiosity.GetFlushLight().spotAngle;
        SpotAngleText.text = "Spot Angle:" + a;
        //
        radiosity.SetSpotAngle(angle);
    }
    void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        FPSText.text = "FPS: " + Mathf.Floor(1 / deltaTime);

        if (Input.GetKeyDown(KeyCode.Escape))//開關控制面板
        {
            uiCanvas.enabled = !uiCanvas.enabled;
        }
    }
}
