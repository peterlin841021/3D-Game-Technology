using UnityEngine;
using System.Runtime.InteropServices;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using System.Linq;

//class MtlFileFormat
//{
//    private string mtlName;

//    public float ns;
//    public float ni;
//    public float d;
//    public float tr;
//    public Vector3 tf;
//    public int illum;
//    public Vector3 ka;
//    public Vector3 kd;
//    public Vector3 ks;
//    public Vector3 ke;
//    public Texture2D kaMap;
//    public Texture2D kdMap;
//    public Texture2D bumpMap;

//    MtlFileFormat(string name)
//    {
//        mtlName = name;
//        tf = Vector3.zero;
//        ka = Vector3.zero;
//        kd = Vector3.zero;
//        ks = Vector3.zero;
//        ke = Vector3.zero;
//    }
//}
public class GameManager : MonoBehaviour
{
    //[SerializeField] private RayTrace tracer = null;
    [SerializeField] private RayTrace tracer = null;
    [SerializeField] private GameObject empty = null;
    //[SerializeField] private RenderTexture mirrorRT = null;
    //[SerializeField] private Camera defaultCamera = null;
    [SerializeField] private GameObject areaLight = null;
    //[SerializeField] private Shader mirrorShader = null;
    [SerializeField] private CreateCone coneGenerator = null;
    //[SerializeField] private GameObject scenery = null;

    private List<GameObject> objects = null;
    private bool worldBegin = false;
    private string[] objectType = { "point light", "spot light", "shpere", "cylinder", "cone", "plane","obj", "directional light" };
    private int currentType = -1;
    private List<Vector3> vs = new List<Vector3>();
    
    //private string[] boomProof = {"Text", "ParseButton", "RenderButton", "EventSystem","Canvas", "GameManager", "ConeGenerator" };

    void Start ()
    {
        objects = new List<GameObject>();
    }
	
	void Update ()
    {
       
    }
    private int GetObjectType(string typeName)
    {
        int idx = -1;
        for(int index = 0; index < objectType.Length; index++)
        {
            if(typeName.Trim().Equals(objectType[index]))
            {
                idx = index;
                break;
            }
        }
        return idx;
    }
    private string[] ParseAttributes(string input)
    {
        return input.Split(' ');
    }

    private GameObject ObjectGenerator(int type,string[] attributes)
    {
        GameObject obj = null;
        int posAttriIdx = -1;
        int rotateAttriIdx = -1;
        int scaleAttriIdx = -1;
        int colorAttriIdx = -1;
        int radiusAttriIdx = -1;
        int widthAttriIdx = -1;
        int heightAttriIdx = -1;
        int yMaxAttriIdx = -1;
        int yMinAttriIdx = -1;
        int mirrorAttriIdx = -1;
        int kdIdx = -1;
        int ksIdx = -1;
        int colorMapAttriIdx = -1;
        int bumpMapAttriIdx = -1;
        int includeAttriIdx = -1;
        //
        int spotAngleAttriIdx = -1;
        int rangeAttriIdx = -1;
        switch (type)
        {
            case 0://PointLight
                obj = Instantiate(empty);
                obj.name = "Point light";
                obj.AddComponent<Light>();
                Light light = obj.GetComponent<Light>();
                light.type = LightType.Point;
                for(int i = 0; i < attributes.Length;i++)
                {
                    if (attributes[i].Equals("\"color\""))
                        colorAttriIdx = i;
                    if (attributes[i].Equals("\"from\""))
                        posAttriIdx = i;
                }               
                light.color = new Color(float.Parse(attributes[colorAttriIdx + 1])/255, float.Parse(attributes[colorAttriIdx + 2])/255, float.Parse(attributes[colorAttriIdx + 3])/255);
                obj.transform.position = new Vector3(float.Parse(attributes[posAttriIdx + 1]), float.Parse(attributes[posAttriIdx + 2]), float.Parse(attributes[posAttriIdx + 3]));
                break;
            case 1://SpotLight
                obj = Instantiate(empty);
                obj.name = "Spot light";
                obj.AddComponent<Light>();
                light = obj.GetComponent<Light>();
                light.type = LightType.Spot;
                for (int i = 0; i < attributes.Length; i++)
                {
                    if (attributes[i].Equals("\"color\""))
                        colorAttriIdx = i;
                    if (attributes[i].Equals("\"from\""))
                        posAttriIdx = i;
                    if (attributes[i].Equals("Rotate"))
                        rotateAttriIdx = i;
                    if (attributes[i].Equals("Angle"))
                        spotAngleAttriIdx = i;
                    if (attributes[i].Equals("Range"))
                        rangeAttriIdx = i;
                }
                light.color = new Color(float.Parse(attributes[colorAttriIdx + 1]) / 255, float.Parse(attributes[colorAttriIdx + 2]) / 255, float.Parse(attributes[colorAttriIdx + 3]) / 255);                
                obj.transform.position = new Vector3(float.Parse(attributes[posAttriIdx + 1]), float.Parse(attributes[posAttriIdx + 2]), float.Parse(attributes[posAttriIdx + 3]));
                if (rotateAttriIdx != -1)
                {
                    obj.transform.rotation = Quaternion.AngleAxis(float.Parse(attributes[rotateAttriIdx + 1]),
                    new Vector3(float.Parse(attributes[rotateAttriIdx + 2]), float.Parse(attributes[rotateAttriIdx + 3]), float.Parse(attributes[rotateAttriIdx + 4])));
                }
                if (spotAngleAttriIdx != -1)
                {
                    light.spotAngle = float.Parse(attributes[spotAngleAttriIdx + 1]);
                }
                if (rangeAttriIdx != -1)
                {
                    light.range = float.Parse(attributes[rangeAttriIdx + 1]);
                }
                break;
            case 2://Sphere
                obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                for (int i = 0; i < attributes.Length; i++)
                {
                    if (attributes[i].Equals("\"radius\""))
                        radiusAttriIdx = i;
                    if (attributes[i].Equals("Translate"))
                        posAttriIdx = i;
                    if (attributes[i].Equals("\"Kd\""))
                        kdIdx = i;
                    if (attributes[i].Equals("\"Ks\""))
                        ksIdx = i;
                    if (attributes[i].Equals("\"mirror\""))
                        mirrorAttriIdx = i;
                }
                if(kdIdx != -1 && ksIdx != -1)
                {
                    Material mtl = new Material(Shader.Find("NSS"));
                    mtl.SetColor("_Color", new Color(float.Parse(attributes[kdIdx + 1]), float.Parse(attributes[kdIdx + 2]), float.Parse(attributes[kdIdx + 2])));
                    mtl.SetColor("_SpecularColor", new Color(float.Parse(attributes[ksIdx + 1]), float.Parse(attributes[ksIdx + 2]), float.Parse(attributes[ksIdx + 2])));
                    obj.GetComponent<MeshRenderer>().material = mtl;
                    //
                    obj.AddComponent<RenderInfo>();
                    obj.GetComponent<RenderInfo>().reflectiveCoefficient = 0.1f;
                }
                else if (mirrorAttriIdx != -1)//ReflectiveCoefficient = 1
                {
                    obj.AddComponent<RenderInfo>();
                    obj.GetComponent<RenderInfo>().reflectiveCoefficient = 1;
                    //Material mtl = new Material(mirrorShader);
                    //mtl.mainTexture = mirrorRT;                    
                    //obj.GetComponent<MeshRenderer>().material = mtl;
                }
                //
                Destroy(obj.GetComponent<SphereCollider>());
                obj.AddComponent<MeshCollider>();
                //
                obj.transform.localScale = new Vector3(float.Parse(attributes[radiusAttriIdx + 1]), float.Parse(attributes[radiusAttriIdx + 1]), float.Parse(attributes[radiusAttriIdx + 1]));
                obj.transform.position = new Vector3(float.Parse(attributes[posAttriIdx + 1]), float.Parse(attributes[posAttriIdx + 2]), float.Parse(attributes[posAttriIdx + 3]));
                break;
            case 3://Cylinder
                obj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                for (int i = 0; i < attributes.Length; i++)
                {
                    if (attributes[i].Equals("\"radius\""))
                        radiusAttriIdx = i;
                    if (attributes[i].Equals("Translate"))
                        posAttriIdx = i;
                    if (attributes[i].Equals("\"Kd\""))
                        kdIdx = i;
                    if (attributes[i].Equals("\"Ks\""))
                        ksIdx = i;
                    if (attributes[i].Equals("\"mirror\""))
                        mirrorAttriIdx = i;
                    if (attributes[i].Equals("Rotate"))
                        rotateAttriIdx = i;
                    if (attributes[i].Equals("\"ymax\""))
                        yMaxAttriIdx = i;
                    if (attributes[i].Equals("\"ymin\""))
                        yMinAttriIdx = i;
                }
                if (kdIdx != -1 && ksIdx != -1)
                {
                    Material mtl = new Material(Shader.Find("NSS"));
                    mtl.SetColor("_Color", new Color(float.Parse(attributes[kdIdx + 1]), float.Parse(attributes[kdIdx + 2]), float.Parse(attributes[kdIdx + 2])));
                    mtl.SetColor("_SpecularColor", new Color(float.Parse(attributes[ksIdx + 1]), float.Parse(attributes[ksIdx + 2]), float.Parse(attributes[ksIdx + 2])));
                    obj.GetComponent<MeshRenderer>().material = mtl;
                    //
                    obj.AddComponent<RenderInfo>();
                    obj.GetComponent<RenderInfo>().reflectiveCoefficient = 0.1f;
                }
                else if (mirrorAttriIdx != -1)
                {
                    //Material mtl = new Material(mirrorShader);
                    //mtl.mainTexture = mirrorRT;
                    //obj.GetComponent<MeshRenderer>().material = mtl;
                    obj.AddComponent<RenderInfo>();
                    obj.GetComponent<RenderInfo>().reflectiveCoefficient = 1;
                }
                //
                Destroy(obj.GetComponent<CapsuleCollider>());
                obj.AddComponent<MeshCollider>();
                //
                obj.transform.localScale = new Vector3(float.Parse(attributes[radiusAttriIdx + 1]),
                    (Mathf.Abs(float.Parse(attributes[yMinAttriIdx + 1]) - float.Parse(attributes[yMaxAttriIdx + 1])) / 2),
                    float.Parse(attributes[radiusAttriIdx + 1]));
                obj.transform.position = new Vector3(float.Parse(attributes[posAttriIdx + 1]), float.Parse(attributes[posAttriIdx + 2]), float.Parse(attributes[posAttriIdx + 3]));
                obj.transform.rotation = Quaternion.AngleAxis(float.Parse(attributes[rotateAttriIdx + 1]),
                    new Vector3(float.Parse(attributes[rotateAttriIdx + 2]), float.Parse(attributes[rotateAttriIdx + 3]),float.Parse(attributes[rotateAttriIdx + 4])));
                break;
            case 4://Cone
                for (int i = 0; i < attributes.Length; i++)
                {
                    if (attributes[i].Equals("\"radius\""))
                        radiusAttriIdx = i;
                    if (attributes[i].Equals("Translate"))
                        posAttriIdx = i;
                    if (attributes[i].Equals("\"Kd\""))
                        kdIdx = i;
                    if (attributes[i].Equals("\"Ks\""))
                        ksIdx = i;
                    if (attributes[i].Equals("\"mirror\""))
                        mirrorAttriIdx = i;
                    if (attributes[i].Equals("Rotate"))
                        rotateAttriIdx = i;
                    if (attributes[i].Equals("\"height\""))
                        heightAttriIdx = i;
                }
                obj = coneGenerator.GenerateCone();
                obj.transform.localScale = new Vector3(float.Parse(attributes[radiusAttriIdx + 1]), float.Parse(attributes[heightAttriIdx + 1]), float.Parse(attributes[radiusAttriIdx + 1]));
                obj.transform.position = new Vector3(float.Parse(attributes[posAttriIdx + 1]), float.Parse(attributes[posAttriIdx + 2]), float.Parse(attributes[posAttriIdx + 3]));
                if (rotateAttriIdx != -1)
                {
                    obj.transform.rotation = Quaternion.AngleAxis(float.Parse(attributes[rotateAttriIdx + 1]),new Vector3(float.Parse(attributes[rotateAttriIdx + 2]), float.Parse(attributes[rotateAttriIdx + 3]), float.Parse(attributes[rotateAttriIdx + 4])));
                }                    
                if (kdIdx != -1 && ksIdx != -1)
                {
                    Material mtl = new Material(Shader.Find("NSS"));
                    mtl.SetColor("_Color", new Color(float.Parse(attributes[kdIdx + 1]), float.Parse(attributes[kdIdx + 2]), float.Parse(attributes[kdIdx + 2])));
                    mtl.SetColor("_SpecularColor", new Color(float.Parse(attributes[ksIdx + 1]), float.Parse(attributes[ksIdx + 2]), float.Parse(attributes[ksIdx + 2])));
                    obj.GetComponent<MeshRenderer>().material = mtl;
                    //
                    obj.AddComponent<RenderInfo>();
                    obj.GetComponent<RenderInfo>().reflectiveCoefficient = 0.1f;
                }
                else if (mirrorAttriIdx != -1)
                {
                    obj.AddComponent<RenderInfo>();
                    obj.GetComponent<RenderInfo>().reflectiveCoefficient = 1;                    
                }
                break;
            case 5://Plane
                obj = GameObject.CreatePrimitive(PrimitiveType.Plane);
                for (int i = 0; i < attributes.Length; i++)
                {                   
                    if (attributes[i].Equals("Translate"))
                        posAttriIdx = i;
                    if (attributes[i].Equals("\"Kd\""))
                        kdIdx = i;
                    if (attributes[i].Equals("\"Ks\""))
                        ksIdx = i;
                    if (attributes[i].Equals("\"mirror\""))
                        mirrorAttriIdx = i;
                    if (attributes[i].Equals("Rotate"))
                        rotateAttriIdx = i;
                    if (attributes[i].Equals("\"width\""))
                        widthAttriIdx = i;
                    if (attributes[i].Equals("\"height\""))
                        heightAttriIdx = i;
                    if (attributes[i].Equals("\"colormap\""))
                        colorMapAttriIdx = i;
                    if (attributes[i].Equals("\"bumpmap\""))
                        bumpMapAttriIdx = i;
                }
                if (kdIdx != -1 && ksIdx != -1)
                {
                    Material mtl = new Material(Shader.Find("NSS"));
                    mtl.SetColor("_Color", new Color(float.Parse(attributes[kdIdx + 1]), float.Parse(attributes[kdIdx + 2]), float.Parse(attributes[kdIdx + 2])));
                    mtl.SetColor("_SpecularColor", new Color(float.Parse(attributes[ksIdx + 1]), float.Parse(attributes[ksIdx + 2]), float.Parse(attributes[ksIdx + 2])));
                    obj.GetComponent<MeshRenderer>().material = mtl;
                    //
                    obj.AddComponent<RenderInfo>();
                    obj.GetComponent<RenderInfo>().reflectiveCoefficient = 0.1f;
                }
                else if (mirrorAttriIdx != -1)
                {
                    obj.AddComponent<RenderInfo>();
                    obj.GetComponent<RenderInfo>().reflectiveCoefficient = 1;                   
                }
                else if (colorMapAttriIdx != -1)
                {
                    Material mtl = new Material(Shader.Find("NSS"));
                    Texture2D texture = new Texture2D(512, 512);
                    byte[] img = System.IO.File.ReadAllBytes(Application.dataPath + "/image/" + attributes[colorMapAttriIdx + 1]);
                    texture.LoadImage(img);
                    mtl.SetTexture("_MainTex", texture);
                    
                    if(bumpMapAttriIdx != -1)
                    {                        
                        texture = new Texture2D(512, 512);
                        img = System.IO.File.ReadAllBytes(Application.dataPath + "/image/" + attributes[bumpMapAttriIdx + 1]);
                        texture.LoadImage(img);
                        mtl.SetTexture("_NormalTex", texture);
                    }
                    obj.GetComponent<MeshRenderer>().material = mtl;
                    //
                    obj.AddComponent<RenderInfo>();
                    obj.GetComponent<RenderInfo>().reflectiveCoefficient = 0.1f;                   
                }
                obj.transform.localScale = new Vector3(float.Parse(attributes[widthAttriIdx + 1]),1, float.Parse(attributes[heightAttriIdx + 1]));
                obj.transform.position = new Vector3(float.Parse(attributes[posAttriIdx + 1]), float.Parse(attributes[posAttriIdx + 2]), float.Parse(attributes[posAttriIdx + 3]));                
                if(rotateAttriIdx != -1)
                {
                    obj.transform.rotation = Quaternion.AngleAxis(float.Parse(attributes[rotateAttriIdx + 1]),new Vector3(float.Parse(attributes[rotateAttriIdx + 2]), float.Parse(attributes[rotateAttriIdx + 3]), float.Parse(attributes[rotateAttriIdx + 4])));                    
                }
                
                break;
            case 6://Obj
                for (int i = 0; i < attributes.Length; i++)
                {
                    if (attributes[i].Equals("Include"))
                        includeAttriIdx = i;                                        
                }
                string name = attributes[includeAttriIdx + 1].Substring(1, attributes[includeAttriIdx + 1].Length -6);
                obj = Instantiate((GameObject)AssetDatabase.LoadAssetAtPath("Assets/prefabs/"+ attributes[includeAttriIdx + 1].Substring(1, attributes[includeAttriIdx + 1].Length-2), typeof(GameObject)));                
                obj.name = name;                               
                for (int i = 0; i < attributes.Length; i++)
                {
                    if (attributes[i].Equals("Translate"))
                        posAttriIdx = i;
                    if (attributes[i].Equals("Rotate"))
                        rotateAttriIdx = i;
                    if (attributes[i].Equals("Scale"))
                        scaleAttriIdx = i;
                }                
                obj.transform.localScale = new Vector3(float.Parse(attributes[scaleAttriIdx + 1]), float.Parse(attributes[scaleAttriIdx + 2]), float.Parse(attributes[scaleAttriIdx + 3]));
                obj.transform.position = new Vector3(float.Parse(attributes[posAttriIdx + 1]), float.Parse(attributes[posAttriIdx + 2]), float.Parse(attributes[posAttriIdx + 3]));
                if (rotateAttriIdx != -1)
                {
                    obj.transform.rotation = Quaternion.AngleAxis(float.Parse(attributes[rotateAttriIdx + 1]), new Vector3(float.Parse(attributes[rotateAttriIdx + 2]), float.Parse(attributes[rotateAttriIdx + 3]), float.Parse(attributes[rotateAttriIdx + 4])));
                }
                Material[] mtls = GenerateMaterials(Application.streamingAssetsPath + "/" + name + ".mtl");               
                obj.transform.gameObject.GetComponent<MeshRenderer>().materials = mtls;
                obj.transform.gameObject.AddComponent<MeshCollider>();
                obj.transform.gameObject.GetComponent<MeshCollider>().sharedMesh = obj.gameObject.GetComponent<MeshFilter>().sharedMesh;
                obj.AddComponent<RenderInfo>();                                
                //tracer.SetComplexMesh(obj.GetComponent<MeshFilter>().mesh, obj.GetComponent<MeshRenderer>().materials);
                break;
            case 7://DirectionalLight
                obj = Instantiate(empty);
                obj.name = "Directional light";
                obj.AddComponent<Light>();
                light = obj.GetComponent<Light>();
                light.type = LightType.Directional;
                for (int i = 0; i < attributes.Length; i++)
                {
                    if (attributes[i].Equals("\"color\""))
                        colorAttriIdx = i;                  
                    if (attributes[i].Equals("Rotate"))
                        rotateAttriIdx = i;                               
                }
                light.color = new Color(float.Parse(attributes[colorAttriIdx + 1]) / 255, float.Parse(attributes[colorAttriIdx + 2]) / 255, float.Parse(attributes[colorAttriIdx + 3]) / 255);
                //obj.transform.position = new Vector3(float.Parse(attributes[posAttriIdx + 1]), float.Parse(attributes[posAttriIdx + 2]), float.Parse(attributes[posAttriIdx + 3]));
                if (rotateAttriIdx != -1)
                {
                    obj.transform.rotation = Quaternion.AngleAxis(float.Parse(attributes[rotateAttriIdx + 1]),
                    new Vector3(float.Parse(attributes[rotateAttriIdx + 2]), float.Parse(attributes[rotateAttriIdx + 3]), float.Parse(attributes[rotateAttriIdx + 4])));
                }                
                break;
        }
        return obj;
    }

    private Material[] GenerateMaterials(string mtlFilePath)
    {
        List<Material> mtls = new List<Material>();
        StreamReader sr = new StreamReader(mtlFilePath, true);
        string buffer = "";

        while (!sr.EndOfStream)
        {
            buffer = sr.ReadLine();
            Texture2D texture = null;
            byte[] img = null;

            if (buffer.Split(' ')[0].Equals("newmtl"))
            {
                Material mtl = new Material(Shader.Find("NSS"));
                buffer = sr.ReadLine();
                while (/*!buffer.Split(' ')[0].Equals("newmtl") && */!sr.EndOfStream)
                {

                    string[] strs = buffer.Split(' ');
                    switch (strs[0].Trim())
                    {
                        case "Ns":
                            mtl.SetFloat("_Shiness", float.Parse(strs[1]));
                            break;
                        case "Ni":
                            mtl.SetFloat("_RefractiveCoefficient", float.Parse(strs[1]));
                            break;
                        case "d":
                            mtl.SetFloat("_Alpha", float.Parse(strs[1]));
                            break;
                        case "Tr":
                        case "Tf":
                        case "illum":
                            break;
                        case "Ka":
                            break;
                        case "Kd":
                            mtl.SetColor("_Color", new Color(float.Parse(strs[1]), float.Parse(strs[2]), float.Parse(strs[3]), 1));
                            break;
                        case "Ks":
                            mtl.SetColor("_SpecularColor", new Color(float.Parse(strs[1]), float.Parse(strs[2]), float.Parse(strs[3]), 1));
                            break;
                        case "Ke":
                            mtl.SetColor("_Emission", new Color(float.Parse(strs[1]), float.Parse(strs[2]), float.Parse(strs[3]), 1));
                            break;
                        case "map_Ka":
                            texture = new Texture2D(512, 512);
                            img = System.IO.File.ReadAllBytes(Application.dataPath + "/image/" + strs[1]);
                            texture.LoadImage(img);
                            mtl.SetTexture("_MainTex", texture);
                            break;
                        case "map_Kd":
                            texture = new Texture2D(512, 512);
                            img = System.IO.File.ReadAllBytes(Application.dataPath + "/image/" + strs[1]);
                            texture.LoadImage(img);
                            mtl.SetTexture("_KAMAD", texture);
                            break;
                        case "map_bump":
                            texture = new Texture2D(512, 512);
                            img = System.IO.File.ReadAllBytes(Application.dataPath + "/image/" + strs[1]);
                            texture.LoadImage(img);
                            mtl.SetTexture("_NormalTex", texture);
                            break;
                    }
                    buffer = sr.ReadLine();
                    if (buffer.Length == 0) break;
                }
                mtls.Add(mtl);
            }
        }
        return mtls.ToArray();
    }

    public class MTLFace
    {
        public string mtlName;
        public List<int> fv;
        public List<int> fvt;
        public MTLFace(string n)
        {
            mtlName = n;
            fv = new List<int>();
            fvt = new List<int>();
        }        
    }
    private Mesh ObjToMesh(string objFilePath)
    {
        Mesh mesh = new Mesh();
        string[] lines = File.ReadAllLines(objFilePath);
        List<Vector3> v = new List<Vector3>();
        List<Vector2> vt = new List<Vector2>();
        List<Vector3> vn = new List<Vector3>();
        List<MTLFace> f = new List<MTLFace>();
        List<int> tempFV = new List<int>();
        List<int> tempFVT = new List<int>();
        int index = 0;
        
        foreach (string line in lines)
        {
            if (line == "" || line.StartsWith("#"))
                continue;

            string[] token = line.Split(' ');
            switch (token[0])
            {
                case ("o"):                    
                    break;
                case ("mtllib"):                    
                    break;
                case ("usemtl"):                    
                    bool exist = f.Exists(s => s.mtlName == token[1].Trim());                    
                    if (exist)
                    {
                        MTLFace m = f.Single(s => s.mtlName == token[1].Trim());
                        m.fv.AddRange(tempFV.ToArray());
                        m.fvt.AddRange(tempFVT.ToArray());
                        tempFV.Clear();
                        tempFVT.Clear();
                    }
                    else
                    {
                        if(f.Count > 0)
                        {
                            f[f.Count - 1].fv.AddRange(tempFV.ToArray());
                            f[f.Count - 1].fvt.AddRange(tempFVT.ToArray());
                            tempFV.Clear();
                            tempFVT.Clear();
                        }                                                
                        f.Add(new MTLFace(token[1].Trim()));
                    }
                    break;
                case ("v"):
                    v.Add(new Vector3(float.Parse(token[1]), float.Parse(token[2]), float.Parse(token[3])));
                    break;
                case ("vn"):
                    vn.Add(new Vector3(float.Parse(token[1]), float.Parse(token[2]), float.Parse(token[3])));
                    break;
                case ("vt"):
                    vt.Add(new Vector2(float.Parse(token[1]), float.Parse(token[2])));
                    break;
                case ("f"):
                    if (token.Length > 5)
                    {
                        string[] s1 = token[1].Split('/');                        
                        string[] s3 = token[3].Split('/');
                        string[] s4 = token[4].Split('/');
                        tempFV.Add(int.Parse(s1[0]));
                        tempFV.Add(int.Parse(s3[0]));
                        tempFV.Add(int.Parse(s4[0]));
                        tempFVT.Add(int.Parse(s1[1]));
                        tempFVT.Add(int.Parse(s3[1]));
                        tempFVT.Add(int.Parse(s4[1]));
                    }
                    else
                    {
                        string[] s1 = token[1].Split('/');
                        string[] s2 = token[2].Split('/');
                        string[] s3 = token[3].Split('/');
                        tempFV.Add(int.Parse(s1[0]));
                        tempFV.Add(int.Parse(s2[0]));
                        tempFV.Add(int.Parse(s3[0]));
                        tempFVT.Add(int.Parse(s1[1]));
                        tempFVT.Add(int.Parse(s2[1]));
                        tempFVT.Add(int.Parse(s3[1]));
                    }                   
                    index++;
                    break;
            }
        }        
       
        //
        mesh.subMeshCount = f.Count;
        mesh.name = objFilePath.Substring(objFilePath.LastIndexOf('/') + 1, objFilePath.Length - 5 - objFilePath.LastIndexOf('/'));
        
        List<Vector3> realVertices = new List<Vector3>();
        List <Vector2> realUVs = new List<Vector2>();

        for (int subIdx = 0; subIdx < f.Count; subIdx++)
        {            
            for(int i = 0;i < f[subIdx].fv.Count/3;i += 3)
            {
                realVertices.Add(v[f[subIdx].fv[i + 0]]);
                realVertices.Add(v[f[subIdx].fv[i + 1]]);
                realVertices.Add(v[f[subIdx].fv[i + 2]]);
                realUVs.Add(vt[f[subIdx].fvt[i + 0]]);
                realUVs.Add(vt[f[subIdx].fvt[i + 1]]);
                realUVs.Add(vt[f[subIdx].fvt[i + 2]]);
            }
        }
        mesh.vertices = realVertices.ToArray();
        mesh.uv = realUVs.ToArray();        
        for (int subIdx = 0; subIdx < f.Count; subIdx++)
        {
            mesh.SetTriangles(f[subIdx].fv.ToArray(), subIdx);        
        }
        realVertices.Clear();
        realUVs.Clear();
        v.Clear();
        vt.Clear();
        vn.Clear();
        f.Clear();
        return mesh;
    }

    private int[][] GetSubMeshInfo(string objFilePath)
    {        
        int[][] arrays = null;
        //Read obj
        List<MTLFace> f = new List<MTLFace>();
        List<int> tempFV = new List<int>();
        string[] lines = File.ReadAllLines(objFilePath);
        foreach (string line in lines)
        {
            if (line == "" || line.StartsWith("#"))
                continue;

            string[] token = line.Split(' ');
            switch (token[0])
            {
                case "v":
                    vs.Add(new Vector3(float.Parse(token[1]), float.Parse(token[2]), float.Parse(token[3])));
                    break;
                case "vt":
                    vs.Add(new Vector2(float.Parse(token[1]), float.Parse(token[2])));
                    break;
                case ("usemtl"):
                    bool exist = f.Exists(s => s.mtlName == token[1].Trim());
                    if (exist)
                    {
                        MTLFace m = f.Single(s => s.mtlName == token[1].Trim());
                        m.fv.AddRange(tempFV.ToArray());                                            
                        tempFV.Clear();
                    }
                    else
                    {
                        if (f.Count > 0)
                        {
                            f[f.Count - 1].fv.AddRange(tempFV.ToArray());
                            tempFV.Clear();
                        }
                        f.Add(new MTLFace(token[1].Trim()));
                    }
                break;
                case ("f"):
                    string[] s1 = token[1].Split('/');
                    string[] s2 = token[2].Split('/');
                    string[] s3 = token[3].Split('/');
                    if (token.Length == 6)
                    {
                        string[] s4 = token[4].Split('/');
                        tempFV.Add(int.Parse(s1[0]));
                        tempFV.Add(int.Parse(s2[0]));
                        tempFV.Add(int.Parse(s3[0]));
                        //
                        tempFV.Add(int.Parse(s3[0]));
                        tempFV.Add(int.Parse(s4[0]));
                        tempFV.Add(int.Parse(s1[0]));
                    }
                    else
                    {
                        tempFV.Add(int.Parse(s1[0]));
                        tempFV.Add(int.Parse(s2[0]));
                        tempFV.Add(int.Parse(s3[0]));
                    }                                           
                break;
            }
        }
        //
        arrays = new int[f.Count][];
        for (int idx = 0;idx < f.Count; idx++)
        {
            arrays[idx] = f[idx].fv.ToArray();
        }        
        f.Clear();
        tempFV.Clear();        
        return arrays;
    }

    public void ParseScene()
    {        
        FileOpenDialog dialog = new FileOpenDialog();
        dialog.structSize = Marshal.SizeOf(dialog);
        dialog.initialDir = Application.dataPath;        
        dialog.filter = "CPBRT\0*.cpbrt";
        dialog.file = new string(new char[256]);
        dialog.maxFile = dialog.file.Length;
        dialog.fileTitle = new string(new char[64]);
        dialog.maxFileTitle = dialog.fileTitle.Length;
        dialog.initialDir = UnityEngine.Application.dataPath;
        dialog.title = "Open File Dialog";
        dialog.defExt = "Cpbrt";
        dialog.flags = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000200 | 0x00000008;

        if (DialogShow.GetOpenFileName(dialog))
        {
            Boom();
            StreamReader sr = new StreamReader(dialog.file,true);
            string buffer = "";
            while (!sr.EndOfStream)
            {
                buffer = sr.ReadLine();                
                //Camera
                if (buffer.Length > 6 && buffer.Substring(0,6).Equals("LookAt") && !worldBegin)
                {                    
                    string[] splits = buffer.Split(' ');
                    objects.Add(Instantiate(empty));
                    objects[0].name = "SceneCamera";
                    if(!objects[0].GetComponent<Camera>())
                        objects[0].AddComponent<Camera>();
                    //
                    objects[0].AddComponent<RayTrace>();
                    //tracer = objects[0].GetComponent<RayTrace>();
                    tracer = objects[0].GetComponent<RayTrace>();
                    //
                    Camera camera = objects[0].GetComponent<Camera>();                    
                    camera.transform.position = new Vector3(float.Parse(splits[1]), float.Parse(splits[2]), float.Parse(splits[3]));
                    camera.transform.up = new Vector3(float.Parse(splits[7]), float.Parse(splits[8]), float.Parse(splits[9]));
                    //Direction
                    Vector3 target = new Vector3(float.Parse(splits[4]), float.Parse(splits[5]), float.Parse(splits[6]));
                    Vector3 dir = (target - camera.transform.position).normalized;                    
                    camera.transform.forward = dir;
                    //defaultCamera.gameObject.transform.position = camera.transform.position;
                    //defaultCamera.gameObject.transform.rotation = camera.transform.rotation;
                    //defaultCamera.targetTexture = mirrorRT;
                }
                else if (buffer.Length > 6 && buffer.Substring(0, 6).Equals("Camera") && !worldBegin)
                {
                    if(objects[0] != null)
                    {
                        Camera camera = objects[0].GetComponent<Camera>();
                        string[] splits = buffer.Split(' ');
                        if (splits[0].Equals("perspective"))
                            camera.orthographic = true;
                        else
                            camera.orthographic = false;
                        //Fov                        
                        camera.fieldOfView = float.Parse(splits[3]);//  
                        camera.clearFlags = CameraClearFlags.SolidColor;
                        //defaultCamera.fieldOfView = camera.fieldOfView;
                    }
                }
                if (!worldBegin && buffer.Equals("WorldBegin"))
                {
                    worldBegin = true;                    
                }
                else if (worldBegin && buffer.Equals("WorldEnd"))
                {
                    worldBegin = false;                    
                }
                //Parse world
                if (worldBegin)
                {
                    if(buffer.Trim().Length > 0 && buffer.Trim().Substring(0,1).Equals("#"))
                    {
                        currentType = GetObjectType(buffer.Trim().Substring(1, buffer.Trim().Length - 1));                        
                    }                    
                    if(currentType != -1)
                    {
                        buffer = sr.ReadLine();//AttributeBegin
                        buffer = sr.ReadLine();
                        string input = null;
                        while (!buffer.Trim().Equals("AttributeEnd"))
                        {
                            if(input == null)
                                input =  buffer.Trim();
                            else
                                input += " "+ buffer.Trim();

                            buffer = sr.ReadLine();
                        }                        
                        string[] attr = ParseAttributes(input);                        
                        objects.Add(ObjectGenerator(currentType, attr));
                        currentType = -1;
                    }
                }
            }
            sr.Close();
            //Find lights
            tracer.FindLights(objects.ToArray());
        }        
    }

    public void Rendering()
    {
        if (tracer != null)
            tracer.Render();
    }
    private void Boom()
    {
        //List<string> objName = new List<string>(boomProof);
        //foreach (GameObject o in Object.FindObjectsOfType<GameObject>())
        //{
        //    if (!objName.Exists(name => name == o.name))
        //        Destroy(o);
        //}
        foreach (GameObject obj in objects)
        {
            Destroy(obj);
        }
        objects.Clear();
    }
}

