using UnityEngine;

[RequireComponent(typeof(Light))]
public class VirtualPointLight : MonoBehaviour
{
	private Light L;

	void Awake()
    {
		L = GetComponent<Light>();
	}    
	public void SetLight(float intensity, bool visible)
    {
		GetComponent<MeshRenderer>().enabled = visible;
		GetComponentInChildren<MeshRenderer>().enabled = visible;
		L.intensity = intensity;
		L.enabled = true;
	}
    public void SetLightColor(Color c)
    {
        L.color = c;
    }
	public void CloseLight()
    {
		L.enabled = false;
		GetComponent<MeshRenderer>().enabled = false;
		GetComponentInChildren<MeshRenderer>().enabled = false;
	}
    public void OpenLight()
    {
        L.enabled = true;
        GetComponent<MeshRenderer>().enabled = true;
        GetComponentInChildren<MeshRenderer>().enabled = true;
    }
    public bool GetLightStatus()
    {
        return L.enabled;
    }
}
