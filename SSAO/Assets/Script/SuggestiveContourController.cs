using UnityEngine;

enum SC_Property
{
    _FZ,
    _C_Limit,
    _SC_Limit,
    _DWKR_Limit
}

public class SuggestiveContourController : MonoBehaviour
{
    [SerializeField] private Material scMTL = null;    
    
    private void PropertyInitialize()
    {
        float[] values = {60f,100f,100f,1f};
        for(int i = 0; i < values.Length; i++)
        {
            SetMTLProperty(((SC_Property)i).ToString(), values[i]);            
        }
    }

    public void SetMTLProperty(string name,float value)
    {
        scMTL.SetFloat(name, value);
    }
}
