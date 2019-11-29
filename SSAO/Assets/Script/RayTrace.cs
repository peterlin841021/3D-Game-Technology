using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayTrace : MonoBehaviour, IEnumerator
{	
    [SerializeField] private float maximumRaycastDistance = 100.0f;
    [SerializeField] private int maximumIterations = 1;
    [SerializeField] private int maxLevel = 1;
    [SerializeField] private int radiansCounts = 10;

    private float resolution = 1.0f;        
	private Texture2D renderTexture;
	private int level;
    private List<Vector3> hits = null;
    private bool isRendering = false;
    public bool drawRay = true;
    private float error = 0.0001f;
    private Material drawMTL;
    
    private float shotDistance = 0.1f;
    void Start()
    {      
        int textureWidth = (int)(Screen.width * resolution);
        int textureHeight = (int)(Screen.height * resolution);
        renderTexture = new Texture2D(textureWidth, textureHeight);
        if (drawRay)
            hits = new List<Vector3>();
        
        //根據螢幕大小產生texture貼在相機上        
        Reset();
        //
        Shader shader = Shader.Find("Hidden/Internal-Colored");
        drawMTL = new Material(shader);
        drawMTL.hideFlags = HideFlags.HideAndDontSave;        
        drawMTL.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        drawMTL.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);        
        drawMTL.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);        
        drawMTL.SetInt("_ZWrite", 0);
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
		bool beaut = false;
		int levelSize = (int)Math.Pow(2, level);
        //Color[] newColors = new Color[levelSize * levelSize];        
		for (int levelX = 0; levelX < (float)renderTexture.width / levelSize; levelX++)
        {
			for (int levelY = 0; levelY < (float)renderTexture.height / levelSize; levelY++)
            {
				if (beaut || level == maxLevel || levelX % 2 + levelY % 2 != 0)
                {
                    int x = levelX * levelSize;
                    int y = levelY * levelSize;
                    int w = (x + levelSize) >= renderTexture.width ? renderTexture.width - x : levelSize;
                    int h = (y + levelSize) >= renderTexture.height ? renderTexture.height - y : levelSize;
                    Vector3 rayPosition = beaut ? new Vector3((x + levelSize / 2) / resolution, (y + levelSize / 2) / resolution, 0) : new Vector3(x / resolution, y / resolution, 0);
                    Ray ray = GetComponent<Camera>().ScreenPointToRay(rayPosition);
                    Color color = Color.white;
                    int count = 0;
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit, maximumRaycastDistance))
                    {                        
                        Vector3 hitNormal = hit.normal;
                        
                        Vector3 cross = Vector3.Cross(hitNormal, -ray.direction).normalized;
                        Quaternion q ;
                        Vector3 newNormal;
                        RaycastHit hh;
                        for (int c = 0; c < radiansCounts; c++)
                        {
                            q = Quaternion.AngleAxis((c+1)*(360f/radiansCounts), cross);
                            newNormal = q * hitNormal;
                            ray = new Ray(hit.point, newNormal);
                            if (Physics.Raycast(ray,out hh, shotDistance))
                            {
                                count++;
                                if (drawRay)
                                {
                                    hits.Add(ray.origin);
                                    hits.Add(hh.point);
                                }
                            }
                        }
                        color.r -= (1f / radiansCounts) * count;
                        color.g = color.b = color.r;
                    }
                    //for (int i = 0; i < levelSize * levelSize; i++)
                    //{
                    //    newColors[i] = color;
                    //}
                    renderTexture.SetPixel(x, y, color);
                }
			}
		}
        
        renderTexture.Apply();
		level--;
		return level >= 0;
	}

    void OnGUI()
    {
        //顯示Rendering前後的差異
        if (!Input.GetKey(KeyCode.Space))//Difference
        {
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), renderTexture);
        }
        if (Input.GetKey(KeyCode.C))//Clear
        {
            Reset();
        }
    }

    public void Reset()
    {
        //初始化texture為黑色        
		level = maxLevel;
		Color defaultColor = Color.black;
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
			
    void FixedUpdate()//For physics
    {
        if (level >= 0 && isRendering)
        {
            MoveNext();
        }
        else if(level < 0 && isRendering)
        {
            isRendering = false;
            print("Finish!");
        }        
    }
    public void RayTracingUdpate()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            //重新Rendering
            Reset();
            isRendering = true;
        }        
        if (drawRay && hits.Count > 0 && level < 0)
        {
            for (int i = 0; i < hits.Count - 1; i += 2)
            {

                Debug.DrawLine(hits[i], hits[i + 1], Color.yellow);
            }
        }
    }    
    public void Rendering()
    {
        print("Rendering...");
        isRendering = true;
    }
    void OnPostRender()
    {
        //if (drawRay && hits.Count > 0 && level < 0)
        //{
        //    drawMTL.SetPass(0);
        //    GL.Begin(GL.LINES);
        //    for (int i = 0; i < hits.Count - 1; i += 2)
        //    {
        //        GL.Color(Color.yellow);
        //        GL.Vertex(hits[i]);// Origin
        //        GL.Vertex(hits[i + 1]);// Dest           
        //    }
        //    GL.End();
        //}
    }
}
