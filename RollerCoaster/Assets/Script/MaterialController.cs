using UnityEngine;

enum MaterialTarget
{
    Contorl_Point_Bottom,
    Contorl_Point_Top,
    Earth,
    Miku,
    Sleeper,
    Track,
    Train,
    Water
}

public class MaterialController : MonoBehaviour
{
    [SerializeField] private GameManager manager;
    [SerializeField] private Material[] objMTLS;
    [SerializeField] private Shader effectShader;
    public void MaterialSetting()
    {
        objMTLS[(int)MaterialTarget.Contorl_Point_Bottom].color = Color.red;
        objMTLS[(int)MaterialTarget.Contorl_Point_Top].color = Color.white;
        objMTLS[(int)MaterialTarget.Miku].shader = effectShader;
        objMTLS[(int)MaterialTarget.Sleeper].color = Color.white;
        objMTLS[(int)MaterialTarget.Track].color = Color.black;
        objMTLS[(int)MaterialTarget.Train].color = Color.blue;
        RenderTexture rt = manager.GetCameraController().GetCamera((int)CameraType.Render).targetTexture;
        objMTLS[(int)MaterialTarget.Water].SetTexture("_ReflectTex", rt);        
    }
    public Material GetSpecialEffectMaterial(int idx)
    {
        return objMTLS[idx];
    }
}
