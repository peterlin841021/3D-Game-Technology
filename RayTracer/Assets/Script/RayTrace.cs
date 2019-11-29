using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class RayTrace : MonoBehaviour, IEnumerator
{	
    private float maximumRaycastDistance = 100.0f;
    [SerializeField]  private int maximumIterations = 4;
    [SerializeField]  private int maxLevel = 5;
    [SerializeField]  private bool subMesh = false;
    //private RenderInfo ri = null;
    [SerializeField] private bool jitter = false;
    private float resolution = 1.0f;   
    private Light[] lights;//Directional,Point,Spot lights
	private Texture2D renderTexture;
	private int level;
    //private List<Vector3> hits = null;
    private bool isRendering = false;
    //public bool drawRay = false;
    private float error = 0.0001f;
    //private static int sceneNum = 1;
    //private Mesh complexMesh = null;
    private List<List<int>> triangles;
    //private Material[] materials = null;
    /// <summary>
    /// Find lights in scene!
    /// </summary>
    /// 
    public void FindLights(GameObject[] objs)
    {
        //lights = FindObjectsOfType(typeof(Light)) as Light[];

        List<Light> ls = new List<Light>();
        foreach (GameObject obj in objs)
            if (obj.GetComponent<Light>())
                ls.Add(obj.GetComponent<Light>());
        //ls.RemoveRange(1, ls.Count - 1);
        foreach (Light l in ls)
            print(l.name);
        lights = ls.ToArray();
        ls.Clear();
    }
    
    void Start()
    {        
        int textureWidth = (int)(Screen.width * resolution);
        int textureHeight = (int)(Screen.height * resolution);
        renderTexture = new Texture2D(textureWidth, textureHeight);        
        //根據螢幕大小產生texture貼在相機上        
        Reset();
    }
				
    public object Current//For iteration
    {
        get
        {
            return level;
        }
    }

    public bool MoveNext()//For iteration
    {		
		int levelSize = (int)Math.Pow(2, level);
		Color[] newColors = new Color[levelSize * levelSize];
        for (int levelX = 0; levelX < (float)renderTexture.width / levelSize; levelX++)
        {
            for (int levelY = 0; levelY < (float)renderTexture.height / levelSize; levelY++)
            {
                if (jitter || level == maxLevel || levelX % 2 + levelY % 2 != 0)
                {
                    int x = levelX * levelSize;
                    int y = levelY * levelSize;
                    //int w = (x + levelSize) >= renderTexture.width ? renderTexture.width - x : levelSize;
                    //int h = (y + levelSize) >= renderTexture.height ? renderTexture.height - y : levelSize;
                    //
                    System.Random r = new System.Random();

                    //Vector3 rayPosition = jitter ? new Vector3((x + levelSize / 2) / resolution, (y + levelSize / 2) / resolution, 0) : new Vector3(x / resolution, y / resolution, 0);
                    Vector3 rayPosition = jitter ? new Vector3((x + (float)(r.NextDouble() * 2 -1) * levelSize / 2) / resolution, (y + +(float)(r.NextDouble() * 2 - 1) * levelSize / 2) / resolution, 0) : new Vector3(x / resolution, y / resolution, 0);
                    Ray ray = GetComponent<Camera>().ScreenPointToRay(rayPosition);
                    Color color = ComputeColor(ray, Color.black);
                    for (int i = 0; i < levelSize * levelSize; i++)
                    {
                        newColors[i] = color;
                    }
                    renderTexture.SetPixel(x, y, color);
                }
            }
        }
        renderTexture.Apply();
		level--;
		return level >= 0;
	}

	public void Reset()
    {
        //初始化texture為黑色
		level = maxLevel;
		Color defaultColor = Color.white;
		defaultColor.a = 0;
		for (int x = 0; x < renderTexture.width; x++)
        {
			for (int y = 0; y < renderTexture.height; y++)
            {
				renderTexture.SetPixel(x, y, defaultColor);
			}
		}        
        renderTexture.Apply();
	}
	
	private Color ComputeColor(Ray ray, Color positionColor, int currentIteration = 0)
    {
		if (currentIteration < maximumIterations)
        {
			RaycastHit hit;//沒有Collider永遠打不到
			if (Physics.Raycast(ray, out hit, maximumRaycastDistance))
            {				
				Material objectMaterial = null;
				Vector3 hitNormal = hit.normal;
                MeshFilter mf = hit.collider.gameObject.GetComponent<MeshFilter>();
                if (mf && subMesh)
                {
                    int[] triangles;
                    Mesh mesh = mf.sharedMesh;
                    Vector3[] normals = mesh.normals;
                    triangles = mesh.triangles;
                    Vector3 n0 = normals[triangles[hit.triangleIndex * 3 + 0]];
                    Vector3 n1 = normals[triangles[hit.triangleIndex * 3 + 1]];
                    Vector3 n2 = normals[triangles[hit.triangleIndex * 3 + 2]];
                    Vector3 baryCenter = hit.barycentricCoordinate;
                    hitNormal = n0 * baryCenter.x + n1 * baryCenter.y + n2 * baryCenter.z;
                    hitNormal.Normalize();
                    hitNormal = hit.transform.TransformDirection(hitNormal);

                    //if (hit.collider.gameObject.GetComponent<MeshRenderer>().materials.Length > 1)//Render多個material
                    //{
                    //    int triangleIdx = hit.triangleIndex;
                    //    int lookupIdx1 = mesh.triangles[triangleIdx * 3 + 0];
                    //    int lookupIdx2 = mesh.triangles[triangleIdx * 3 + 1];
                    //    int lookupIdx3 = mesh.triangles[triangleIdx * 3 + 2];

                    //    for (int i = 0; i < mesh.subMeshCount && objectMaterial == null; i++)
                    //    {
                    //        triangles = mesh.GetTriangles(i);
                    //        for (int j = 0; j < triangles.Length; j++)
                    //        {
                    //            if (triangles[j] == lookupIdx1 && triangles[j + 1] == lookupIdx2 && triangles[j + 2] == lookupIdx3)
                    //            {
                    //                objectMaterial = hit.collider.gameObject.GetComponent<Renderer>().materials[i];
                    //                break;
                    //            }
                    //        }
                    //    }
                    //}
                    objectMaterial = hit.collider.gameObject.GetComponent<MeshRenderer>().material;
                }
                RenderInfo objectInfo = hit.collider.gameObject.GetComponent<RenderInfo>();

                if (objectMaterial == null)
                {
                    //if (mf.mesh.subMeshCount > 1)
                    //{
                    //    //int triangleIdx = hit.triangleIndex;
                    //    //for (int i = 0; i < mf.mesh.subMeshCount; i++)
                    //    //{
                    //    //    if (triangles[i].Exists(v => v == triangleIdx))
                    //    //    {
                    //    //        objectMaterial = materials[i];
                    //    //        break;
                    //    //    }
                    //    //}
                    //    objectMaterial = hit.collider.gameObject.GetComponent<MeshRenderer>().material;
                    //}
                    //else
                        objectMaterial = hit.collider.gameObject.GetComponent<MeshRenderer>().material;
                }
                
                if (objectMaterial.mainTexture)
                {
                    Texture2D tex = (Texture2D)objectMaterial.mainTexture;                    
                    positionColor += tex.GetPixelBilinear(hit.textureCoord.x, hit.textureCoord.y);
                    tex = (Texture2D)objectMaterial.GetTexture("_NormalTex");
                    if(tex != null)
                    {
                        Color nc = tex.GetPixelBilinear(hit.textureCoord.x, hit.textureCoord.y);
                        hitNormal = new Vector3(nc.r, nc.g, nc.b);
                    }                    
                }
                else
                {                    
                    positionColor += objectMaterial.GetColor("_Color");
                }

                //Vector3 hitPosition = hit.point + hit.normal * error;
                Vector3 hitPosition = hit.point + hitNormal * error;
                if (objectInfo != null)
                {
                    Color[] lightColors = HandleLights(objectInfo, hitPosition, hitNormal, ray.direction);
                    positionColor = lightColors[0] + lightColors[1] * positionColor + lightColors[2];
                    if (objectInfo.reflectiveCoefficient > 0f)//處理反射
                    {
                        float reflect = 2.0f * Vector3.Dot(ray.direction, hitNormal);
                        Ray newRay = new Ray(hitPosition, ray.direction - reflect * hitNormal);
                        positionColor = positionColor * (1 - objectInfo.reflectiveCoefficient) + objectInfo.reflectiveCoefficient * ComputeColor(newRay, positionColor, ++currentIteration);
                    }

                    if (objectInfo.transparentCoefficient > 0f)//處理透明度
                    {
                        Ray newRay = new Ray(hit.point - hitNormal * error, ray.direction);
                        positionColor = positionColor * (1 - objectInfo.transparentCoefficient) + objectInfo.transparentCoefficient * ComputeColor(newRay, positionColor, ++currentIteration);
                    }
                }                
            }
		}
		return positionColor;
	}
	
	private Color[] HandleLights(RenderInfo objectInfo, Vector3 rayHitPosition, Vector3 surfaceNormal, Vector3 rayDirection)
    {
		Color[] lightColors = new Color[3];
		lightColors[0] = RenderSettings.ambientLight;
		lightColors[1] = lightColors[2] = Color.black;//Diffuse & Specular
        for (int i = 0; i < lights.Length; i++)
        {
			if (lights[i].enabled)
            {
				Vector2 intensitys = LightTrace(objectInfo, lights[i], rayHitPosition, surfaceNormal, rayDirection);
				lightColors[1] += lights[i].color * intensitys[0];
				lightColors[2] += lights[i].color * intensitys[1];
			}
		}
		return lightColors;
	}

	//計算燈光貢獻度
	private Vector2 LightTrace(RenderInfo objectInfo, Light light, Vector3 rayHitPosition, Vector3 surfaceNormal, Vector3 rayDirection)
    {
		if (light.type == LightType.Directional)
        {
			Vector2 intensitys = Vector2.zero;
			Vector3 lightDirection = -light.transform.forward;
			
			float dotDirectionNormal = Vector3.Dot(lightDirection, surfaceNormal);
			if (dotDirectionNormal > 0 /*&& !Physics.Raycast(rayHitPosition, lightDirection, maximumRaycastDistance)*/)
            {
				intensitys[0] = objectInfo.lambertCoefficient * dotDirectionNormal;
				intensitys[1] = CalculateSpecular(objectInfo, dotDirectionNormal, rayDirection, surfaceNormal, rayHitPosition, light);
			}
			return light.intensity * intensitys;
		}
		else if (light.type == LightType.Spot || light.type == LightType.Point)
        {
			Vector2 intensitys = Vector2.zero;
			Vector3 lightDirection = (light.transform.position - rayHitPosition).normalized;
			float dotDirectionNormal = Vector3.Dot(lightDirection, surfaceNormal);
			float lightDistance = Vector3.Distance(rayHitPosition, light.transform.position);
            
			if (lightDistance < light.range && dotDirectionNormal > 0f)
            {
				float dotDirectionLight = Vector3.Dot(lightDirection, -light.transform.forward);				
				if ((dotDirectionLight > Mathf.Cos(light.spotAngle * Mathf.Deg2Rad) || light.type == LightType.Point) /*&& !Physics.Raycast(rayHitPosition, lightDirection, lightDistance)*/)
                {
					intensitys[0] = objectInfo.lambertCoefficient * dotDirectionNormal;
					intensitys[1] = CalculateSpecular(objectInfo, dotDirectionNormal, rayDirection, surfaceNormal, rayHitPosition, light);
				}
			}
			return light.intensity * intensitys;
		}
		return Vector2.zero;
	}

	private float CalculateSpecular(RenderInfo objectInfo, float dotDirectionNormal, Vector3 rayDirection, Vector3 surfaceNormal, Vector3 hitPosition, Light light)
    {
		float lightContribution = 0;
		if (objectInfo.reflectiveCoefficient > 0)
        {
			if (objectInfo.phongCoefficient > 0)
            {
				lightContribution += Phong(objectInfo, light, rayDirection, surfaceNormal, hitPosition);
			}
			if (objectInfo.blinnPhongCoefficient > 0)//反射角 > 90，提高鏡面貢獻
            {
				lightContribution += BlinnPhong(objectInfo, light, rayDirection, surfaceNormal);
			}
		}
		return lightContribution;
	}

    //計算Phong reflection
    private float Phong(RenderInfo objectInfo, Light light, Vector3 rayDirection, Vector3 hitSurfaceNormal, Vector3 hitPosition)
    {
		Vector3 lightDirection = light.type == LightType.Directional ? -light.transform.forward : Vector3.Normalize(light.transform.position - hitPosition);
		float reflect = 2.0f * Vector3.Dot(rayDirection, hitSurfaceNormal);
		Vector3 phongDirection = rayDirection - reflect * hitSurfaceNormal;
		float phongTerm = Mathf.Max(Vector3.Dot(phongDirection, lightDirection), 0f);
		phongTerm = objectInfo.reflectiveCoefficient * Mathf.Pow(phongTerm, objectInfo.phongPower) * objectInfo.phongCoefficient;
		return phongTerm;
	}

    //計算Blinn-Phong reflection
    private float BlinnPhong(RenderInfo objectInfo, Light light, Vector3 rayDirection, Vector3 hitSurfaceNormal)
    {
		Vector3 blinnDirection = -light.transform.forward - rayDirection;
		float temp = Mathf.Sqrt(Vector3.Dot(blinnDirection, blinnDirection));
		if (temp > 0f)
        {
			blinnDirection = (1f / temp) * blinnDirection;//Half vector
			float blinnTerm = Mathf.Max(Vector3.Dot(blinnDirection, hitSurfaceNormal), 0f);
			blinnTerm = objectInfo.reflectiveCoefficient * Mathf.Pow(blinnTerm, objectInfo.blinnPhongPower) * objectInfo.blinnPhongCoefficient;
			return blinnTerm;
		}
		return 0f;
	}

    void OnGUI()
    {        
        //顯示Rendering前後的差異
        if (!Input.GetKey(KeyCode.D))//Difference
        {
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), renderTexture);            
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            //重新Rendering       
            Reset();
            isRendering = false;
        }        
    }    

    void FixedUpdate()//For physics
    {
        if (level >= 0 && isRendering)
        {
            MoveNext();
        }
    }

    public void Render()
    {
        isRendering = true;
    }   
    //void Reflective(float value)
    //{
    //    ri.reflectiveCoefficient = value;
    //}
    //void Transparency(float value)
    //{
    //    ri.transparentCoefficient = value;
    //}
    //IEnumerator LoadYourAsyncScene()
    //{
    //    string sceneName = "Cornell";
    //    switch (sceneNum)
    //    {
    //        case 1:
    //            sceneName = "Cornell";
    //            break;
    //        case 2:
    //            sceneName = "Killeroo";
    //            break;
    //        case 3:
    //            sceneName = "Sibenik";
    //            break;
    //    }
    //    Debug.Log(sceneName);
    //    AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
    //    asyncLoad.allowSceneActivation = false;
    //    while (!asyncLoad.isDone)
    //    {
            
    //        if (asyncLoad.progress >= 0.9f)                
    //            asyncLoad.allowSceneActivation = true;
    //        yield return null;
    //    }
    //}
}
