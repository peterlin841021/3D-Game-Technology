using System.Collections.Generic;
using UnityEngine;


public class Radiosity : MonoBehaviour
{
    [SerializeField] private CameraController cameraController;
    [SerializeField] private GameObject VPL;
    [SerializeField] private RenderTexture shadowMap = null;
    //[SerializeField] private VPLPool pool;
    //[SerializeField] private GameObject plane;
    [SerializeField] private Light flashlight = null;
    [SerializeField] private Light pointlight = null;
    private bool VPLVisibility = true;       	    
    private Vector3[] vs;
    private Transform tp;
    //
    private VirtualPointLight[] VPLs = null;
    private static int VPLCount = 7;
    private static float vpllightsum = 0.5f;
    //
    private Texture2D temp;
    List<VirtualPointLight> VPLPool;
    List<VirtualPointLight> invalidVPLS;

    public void SettingSpotLight()
    {
        //flashlight.gameObject.SetActive(!flashlight.gameObject.activeSelf);
        flashlight.transform.parent.gameObject.SetActive(!flashlight.transform.parent.gameObject.activeSelf);
        pointlight.transform.parent.gameObject.SetActive(false);
    }
    public void SettingPointLight()
    {        
        flashlight.transform.parent.gameObject.SetActive(false);
        pointlight.transform.parent.gameObject.SetActive(!pointlight.transform.parent.gameObject.activeSelf);
    }
    void Awake()
    {
        temp = new Texture2D(512, 512, TextureFormat.RGB24, false);
        VPLPool = new List<VirtualPointLight>();
        invalidVPLS = new List<VirtualPointLight>();
    }
    private void VPLPoolInitialization()
    {
        VPLs = new VirtualPointLight[VPLCount];
        for(int i = 0 ; i < VPLCount;i++)
        {            
            GameObject vplObj = Instantiate(VPL);
            vplObj.AddComponent<VirtualPointLight>();
            VPLs[i] = vplObj.GetComponent<VirtualPointLight>();
            VPLs[i].CloseLight();
        }
    }

    private Vector3[] GenerateProjectDirection(int count, Light spot)
    {
        Vector3[] directions = new Vector3[count];
        for(int i = 0; i < count; i++)
        {
            directions[i] = RandomSpotLightCirclePoint(spot);
        }
        //Vector2 randomXY = Random.insideUnitCircle.normalized;
        ////Vector3 randomPoint;
        ////do
        ////{
        ////    randomPoint = Vector3.Cross(Random.insideUnitCircle, transform.up);
        ////} while (randomPoint == Vector3.zero);

        ////Debug.DrawLine(center, randomPoint, Color.blue);
        //RaycastHit hit;
        //Vector3 v = Vector3.zero;
        //if (Physics.Raycast(center, Vector3.forward, out hit, 100f))
        //{
        //    v = hit.point - center;
        //    v.x += randomXY.x;
        //    v.y += randomXY.y;
        //    Debug.DrawRay(center, q * (v).normalized * 10, Color.blue);
        //}
        ////Vector3 dir = forward;        
        ////dir.y = randomXY.x;
        ////dir.x = randomXY.y;
        ////Vector3 start = Vector3.zero;        
        return directions;
    }

    Vector3 RandomSpotLightCirclePoint(Light spot)
    {
        float radius = Mathf.Tan(Mathf.Deg2Rad * spot.spotAngle / 2) * spot.range;
        Vector2 circle = Random.insideUnitCircle * radius;
        Vector3 target = spot.transform.position + spot.transform.forward * spot.range + spot.transform.rotation * new Vector3(circle.x, circle.y);
        return target;
    }
   
    public void SetVPLVisibility(bool visibility)
    {
        VPLVisibility = visibility;
    }

    public void SetVPLCounts(int count)
    {
        ReGenerateVPLS(VPLCount, count);
    }

    public void SetSpotAngle(float angle)
    {
        flashlight.spotAngle = angle;
    }

    public Light GetFlushLight()
    {
        return flashlight;
    }

    private void ReGenerateVPLS(int oldVplCount,int newVplCount)
    {
        for (int i = 0; i < oldVplCount; i++)
        {
            Destroy(VPLs[i].gameObject);
        }        
        VPLCount = newVplCount;
        VPLPoolInitialization();
    }

	void Start()
    {
        VPLPoolInitialization();      
    }
	private List<VirtualPointLight> CheckVPLS(bool checkClose, List<VirtualPointLight> vpls,int type)//Out of spot range
    {
        //bool valid = true;
        //if (Vector3.Dot(normal, viewDir) == 0f)
        //    valid = false;
        //Rect rectReadPicture = new Rect(0, 0, 512, 512);
        //RenderTexture.active = shadowMap;
        ////Texture2D tex = new Texture2D(512, 512, TextureFormat.RGB24, false);
        //temp.ReadPixels(rectReadPicture, 0,0);
        //temp.Apply();
        //if (temp.GetPixelBilinear(uv.x,uv.y) == Color.black)
        //{
        //    valid = false;
        //}
        //RenderTexture.active = null;
        Light l;
        if (type == 0)
        {
            l = flashlight;
            if (!checkClose)
            {
                RaycastHit hit;
                for (int i = 0; i < VPLCount; i++)
                {
                    if (Mathf.Acos(Vector3.Dot(l.transform.forward, Vector3.Normalize(VPLs[i].transform.position - l.transform.position))) > l.spotAngle / 2 * Mathf.Deg2Rad ||
                        Physics.Raycast(new Ray(l.transform.position, VPLs[i].transform.position - l.transform.position), out hit) && hit.point != VPLs[i].transform.position)
                    {
                        VPLPool.Add(VPLs[i]);
                        //VPLs[i].CloseLight();
                        VPLPool[VPLPool.Count - 1].CloseLight();
                    }
                    else
                    {
                        invalidVPLS.Add(VPLs[i]);
                    }
                }
            }
        }
        else
        {
            l = pointlight;
            if (!checkClose)
            {                
                for (int i = 0; i < VPLCount; i++)
                {
                    VPLPool.Add(VPLs[i]);
                    VPLPool[VPLPool.Count - 1].CloseLight();                    
                }
            }
        }

        //if (!checkClose)
        //{
        //    RaycastHit hit;
        //    for (int i = 0; i < VPLCount; i++)
        //    {                
        //        if (Mathf.Acos(Vector3.Dot(l.transform.forward, Vector3.Normalize(VPLs[i].transform.position - l.transform.position))) > l.spotAngle / 2 * Mathf.Deg2Rad ||
        //            Physics.Raycast(new Ray(l.transform.position, VPLs[i].transform.position - l.transform.position), out hit) && hit.point != VPLs[i].transform.position)
        //        {
        //            VPLPool.Add(VPLs[i]);
        //            //VPLs[i].CloseLight();
        //            VPLPool[VPLPool.Count - 1].CloseLight();
        //        }
        //        else
        //        {
        //            invalidVPLS.Add(VPLs[i]);
        //        }
        //    }
        //}
        //else
        //{
        //    //RaycastHit hit;
        //    //for (int i = 0; i < vpls.Count; i++)
        //    //{
        //    //    if (Mathf.Acos(Vector3.Dot(flashlight.transform.forward, Vector3.Normalize(vpls[i].transform.position - flashlight.transform.position))) > flashlight.spotAngle / 2 * Mathf.Deg2Rad ||
        //    //        Physics.Raycast(new Ray(flashlight.transform.position, vpls[i].transform.position - flashlight.transform.position), out hit) && hit.point != vpls[i].transform.position)
        //    //    {
        //    //        vpls.RemoveAt(i);
        //    //        i--;
        //    //        VPLPool.Add(vpls[i]);
        //    //        VPLPool[VPLPool.Count - 1].CloseLight();
        //    //    }                
        //    //}
        //}
        ////if(invalidVPLS.Count > 0)
        ////{
        ////    FillVPLS(invalidVPLS);
        ////}
        return invalidVPLS;
    }
    private void ShotVPL()
    {
        for (int i = 0; i < VPLCount; i++)
        {
            RaycastHit hit;

            if (Mathf.Acos(Vector3.Dot(transform.forward, Vector3.Normalize(VPLs[i].transform.position - transform.position))) > flashlight.spotAngle / 2 * Mathf.Deg2Rad ||
                Physics.Raycast(new Ray(transform.position, VPLs[i].transform.position - transform.position), out hit) && hit.point != VPLs[i].transform.position)
            {
                VPLs[i].GetComponent<VirtualPointLight>().CloseLight();
            }
        }

        //float angleScale = Mathf.Tan(light.spotAngle / 2 * Mathf.Deg2Rad);
        for (int i = 0; i < VPLCount; i++)
        {
            RaycastHit hit;
            //Vector3 randomXYZ = RandomXY(vs);
            //隨機產生放VPL的位置
            //Debug.DrawLine(new Vector3(10,10,10), new Vector3(randomXY.x, randomXY.y, 0),Color.yellow);                                                 
            Vector3 start = flashlight.transform.position;
            Vector3[] dirs = GenerateProjectDirection(VPLCount, flashlight);
            Ray ray = new Ray(start, dirs[i]);
            if (Physics.Raycast(ray, out hit))//要加上Mesh collider
            {
                Material hitMaterial = null;
                Vector3 hitNormal = hit.normal;
                //Remove vpls
                //print(CheckVPLStatus(hitNormal, flashlight.transform.parent.rotation * flashlight.transform.forward, hit.textureCoord));
                //int iter = 0;
                //while (!CheckVPLStatus(hitNormal, flashlight.transform.parent.rotation * flashlight.transform.forward, hit.textureCoord) && iter < 5)
                //{
                //    dirs = GenerateProjectDirection(VPLCount, flashlight);
                //    for (int j = 0; j < VPLCount; j++)
                //    {
                //        ray = new Ray(start, dirs[j]);
                //        if (Physics.Raycast(ray, out hit))
                //        {
                //            hitNormal = hit.normal;
                //            iter++;
                //            break;
                //        }
                //    }
                //}                
                VPLs[i].transform.position = hit.point;
                VPLs[i].transform.LookAt(hit.point + hit.normal);
                //
                Mesh mesh = null;
                if (hit.collider.gameObject.GetComponent<SkinnedMeshRenderer>())
                {
                    mesh = hit.collider.gameObject.GetComponent<SkinnedMeshRenderer>().sharedMesh;
                }
                else if (hit.collider.gameObject.GetComponent<MeshFilter>() && hit.triangleIndex != -1)
                {
                    mesh = hit.collider.gameObject.GetComponent<MeshFilter>().sharedMesh;
                }

                if (mesh != null)
                {
                    int[] triangles;
                    Vector3[] normals = mesh.normals;
                    triangles = mesh.triangles;
                    //Vector3 n0 = normals[triangles[hit.triangleIndex * 3 + 0]];
                    //Vector3 n1 = normals[triangles[hit.triangleIndex * 3 + 1]];
                    //Vector3 n2 = normals[triangles[hit.triangleIndex * 3 + 2]];
                    //Vector3 baryCenter = hit.barycentricCoordinate;

                    if (hit.collider.gameObject.GetComponent<Renderer>().materials.Length != 1)
                    {
                        int triangleIdx = hit.triangleIndex;
                        int lookupIdx1 = mesh.triangles[triangleIdx * 3 + 0];
                        int lookupIdx2 = mesh.triangles[triangleIdx * 3 + 1];
                        int lookupIdx3 = mesh.triangles[triangleIdx * 3 + 2];
                        //List<int> tri = null;
                        for (int subMeshIdx = 0; subMeshIdx < mesh.subMeshCount && hitMaterial == null; subMeshIdx++)
                        {
                            triangles = mesh.GetTriangles(subMeshIdx);
                            //tri = new List<int>(mesh.GetTriangles(subMeshIdx));
                            //if (tri.Exists(v => v == lookupIdx1))
                            //{
                            //    hitMaterial = hit.collider.gameObject.GetComponent<Renderer>().materials[subMeshIdx];
                            //    break;
                            //}
                            //tri.Clear();

                            for (int j = 0; j < triangles.Length; j += 3)
                            {
                                if (triangles[j] == lookupIdx1 && triangles[j + 1] == lookupIdx2 && triangles[j + 2] == lookupIdx3)
                                {
                                    hitMaterial = hit.collider.gameObject.GetComponent<Renderer>().materials[subMeshIdx];
                                    break;
                                }
                            }
                        }
                    }
                }

                if (hitMaterial == null)
                {
                    hitMaterial = hit.collider.gameObject.GetComponent<Renderer>().material;
                }
                if (hitMaterial.mainTexture)
                {
                    Texture2D mainTexture = hitMaterial.mainTexture as Texture2D;
                    VPLs[i].GetComponent<VirtualPointLight>().SetLightColor(mainTexture.GetPixelBilinear(hit.textureCoord.x, hit.textureCoord.y));
                }
                else
                {
                    VPLs[i].GetComponent<VirtualPointLight>().SetLightColor(hitMaterial.color);
                }
            }
        }
        // Computing Intensities for VPLs total = 0.5        
        //pool.SetLightSum(pool.GetLightSum() / pool.GetVPLCounts(), VPLVisibility);
        SetVPLightSum(1f, VPLVisibility);
    }

    private void FillVPLS(List<VirtualPointLight> invalidVPLS)
    {
        //int invaldCount = invalidVPLS.Count;
        //while (invaldCount != 0)
        //{
        //    RandomPut(invalidVPLS,0);
        //    invaldCount = CheckVPLS(true, invalidVPLS).Count;
        //}
    }
    Vector2 RandomSample()
    {
        Vector2 vec = new Vector2();
        do
        {
            vec.x = Random.value * 2 - 1;
            vec.y = Random.value * 2 - 1;
        } while (Vector2.Distance(vec, Vector2.zero) > 1);
        return vec;
    }
    Vector3 RandowSampleSphere(float radius)
    {
        return Random.insideUnitSphere * radius;
    }
    private void RandomPut(List<VirtualPointLight> vpls,int type)
    {        
        RaycastHit hit;
        for (int i = 0; i < vpls.Count; i++)
        {
            Vector2 sampleVector = Vector2.zero;
            Ray ray;
            if(type == 0)
            {
                sampleVector = RandomSample();
                ray = new Ray(flashlight.transform.position, flashlight.transform.forward + (flashlight.transform.right * sampleVector.x + flashlight.transform.up * sampleVector.y));
            }
            else
            {
                ray = new Ray(flashlight.transform.position, RandowSampleSphere(pointlight.range));
                //Debug.DrawRay(flashlight.transform.position, RandowSampleSphere(pointlight.range));
                //ray = new Ray(flashlight.transform.position, flashlight.transform.forward + (flashlight.transform.right * sampleVector.x + flashlight.transform.up * sampleVector.y));
            }

            if (Physics.Raycast(ray, out hit))
            {
                vpls[i].transform.position = hit.point;
                vpls[i].transform.LookAt(hit.point + hit.normal);
                Material hitMaterial = null;
                Vector3 hitNormal = hit.normal;

                Mesh mesh = null;
                
                if (hit.collider.gameObject.GetComponent<MeshFilter>() && hit.triangleIndex != -1)
                {
                    mesh = hit.collider.gameObject.GetComponent<MeshFilter>().sharedMesh;
                }
                if (mesh != null)//
                {
                    int[] triangles;
                    Vector3[] normals = mesh.normals;
                    triangles = mesh.triangles;
                    Vector3 n0 = normals[triangles[hit.triangleIndex * 3 + 0]];
                    Vector3 n1 = normals[triangles[hit.triangleIndex * 3 + 1]];
                    Vector3 n2 = normals[triangles[hit.triangleIndex * 3 + 2]];
                    Vector3 baryCenter = hit.barycentricCoordinate;

                    if (hit.collider.gameObject.GetComponent<Renderer>().materials.Length != 1)
                    {
                        int triangleIdx = hit.triangleIndex;
                        int lookupIdx1 = mesh.triangles[triangleIdx * 3 + 0];
                        int lookupIdx2 = mesh.triangles[triangleIdx * 3 + 1];
                        int lookupIdx3 = mesh.triangles[triangleIdx * 3 + 2];
                        for (int fi = 0; fi < mesh.subMeshCount && hitMaterial == null; fi++)
                        {
                            triangles = mesh.GetTriangles(fi);
                            for (int j = 0; j < triangles.Length; j += 3)
                            {
                                if (triangles[j] == lookupIdx1 && triangles[j + 1] == lookupIdx2 && triangles[j + 2] == lookupIdx3)
                                {
                                    hitMaterial = hit.collider.gameObject.GetComponent<Renderer>().materials[fi];
                                    break;
                                }
                            }
                        }
                    }
                }

                if (hitMaterial == null)
                {
                    hitMaterial = hit.collider.gameObject.GetComponent<Renderer>().material;
                }
                if (hitMaterial.mainTexture)
                {
                    Texture2D mainTexture = hitMaterial.mainTexture as Texture2D;
                    vpls[i].SetLightColor(mainTexture.GetPixelBilinear(hit.textureCoord.x, hit.textureCoord.y));
                }
                else
                {
                    vpls[i].SetLightColor(hitMaterial.color);
                }
            }            
        }
    }
    private void SpotLightShot()
    {
        List<VirtualPointLight> temp =  CheckVPLS(false, VPLPool,0);
       
        RandomPut(VPLPool,0);
        SetVPLightSum(0.5f, VPLVisibility);
    }

    private void PointLightShot()
    {
        List<VirtualPointLight> temp = CheckVPLS(false, VPLPool,1);
        RandomPut(VPLPool, 1);
        SetVPLightSum(0.5f, VPLVisibility);
    }
	void Update()
    {
        cameraController.CameraControl();
        if (flashlight.transform.parent.gameObject.activeSelf)
        {
            SpotLightShot();
        }
        if (pointlight.transform.parent.gameObject.activeSelf)
        {
            PointLightShot();
        }
        VPLPool.Clear();
        invalidVPLS.Clear();
    }
    private void SetVPLightSum(float totalIntensity,bool visibility)
    {
        float intensity = totalIntensity / VPLCount;
        for (int i = 0; i < VPLCount; i++)
            VPLs[i].SetLight(intensity, visibility);
    }
}

