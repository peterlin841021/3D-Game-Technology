using UnityEngine;
using System.Runtime.InteropServices;
using System.IO;
using System;
using System.Collections.Generic;

enum Track
{
    Outside,
    Inside,
    Sleeper
}

enum LineType
{
    Linear,
    Cardinal,
    CubicSpline
}

enum ControlPointStatus
{
    Unselected,
    Selected
}

static public class Config
{
    public static int DEFAULT_CONTROLPOINT_NUM = 4;
    public static float DEFAULT_CONTROLPOINT_DISTANCE = 100f;
    public static float DEFAULT_CONTROLPOINT_HEIGHT = 30f;
    public static int DIVIDE = 20;
    public static float SLEEPER_WIDTH = 1f;
    public static int SUPPRT_DISTANCE = 4;  
}

public class LineController : MonoBehaviour
{
    [SerializeField] private GameManager manager;
    
    private LineRenderer[] trackLineRenderer;    
    private Color[] controlPointStatusColor = new Color[2] { Color.red,Color.yellow};    
    private GameObject[] cps = new GameObject[Config.DEFAULT_CONTROLPOINT_NUM];
    private GameObject[] supports = new GameObject[Config.DEFAULT_CONTROLPOINT_NUM * Config.DIVIDE * 2 / Config.SUPPRT_DISTANCE];
    //For train
    private Vector3[] trackPositions = new Vector3[Config.DEFAULT_CONTROLPOINT_NUM * Config.DIVIDE];
    private Vector3[] trackOrientations = new Vector3[Config.DEFAULT_CONTROLPOINT_NUM * Config.DIVIDE];    
    private int trackType = 0;   
    //For controll point    
    private static int currentSelectedInstanceID = 0;
    private static int lastSelectedInstanceID = 0;
        
    public static void SetCurrentControlPointInstanceID(int id)
    {
        currentSelectedInstanceID = id;
    }

    public static void SetLastControlPointInstanceID(int id)
    {
        lastSelectedInstanceID = id;
    }

    public void SetTrackType(int type)
    {
        trackType = type;
    }

    public void SetControlPoints(GameObject[] newCps)
    {
        cps = null;
        cps = newCps;
    }

    public void SetControlPoints(int controlPointCount, Vector3[] pos, Vector3[] orients)
    {
        RemoveTrack();
        GameObject obj = null;
        cps = null;
        cps = new GameObject[controlPointCount];
        for (int i = 0; i < controlPointCount; i++)
        {
            obj = Instantiate(manager.GetObject((int)Object.Controlpoint), pos[i], new Quaternion(orients[i].x, orients[i].y, orients[i].z, 0));
            obj.name = i.ToString();
            obj.AddComponent<ControlPoint>();
            cps[i] = obj;
        }
    }

    public static int GetCurrentControlPointInstanceID()
    {
        return currentSelectedInstanceID;
    }

    public static int GetLastControlPointInstanceID()
    {
        return lastSelectedInstanceID;
    }

    public GameObject[] GetControlPoints()
    {
        return cps;
    }

    public int GetTrackType()
    {
        return trackType;
    }

    public void AddControlPoint(int id)
    {        
        int newNum = cps.Length + 1;
        int insertIdx = 0;
        int newIdx = 0;
        for (int i = 0; i < cps.Length; i++)
        {
            if (cps[i].GetInstanceID() == id)
            {
                insertIdx = i;
                break;
            }
        }
        GameObject[] newCps = new GameObject[newNum];
        Vector3 middle = insertIdx == 0 ? (cps[0].transform.position + cps[cps.Length - 1].transform.position) / 2 : (cps[insertIdx].transform.position + cps[insertIdx - 1].transform.position) / 2;
        for (int i = 0; i < cps.Length; i++)
        {
            if (i != insertIdx)
            {
                newCps[newIdx] = cps[i];
                newIdx++;
            }
            else
            {
                GameObject obj = Instantiate(manager.GetObject((int)Object.Controlpoint), middle, new Quaternion(0, 1, 0, 0));
                obj.name = newIdx.ToString();
                newCps[newIdx] = obj;
                newIdx++;
                obj.name = newIdx.ToString();
                newCps[newIdx] = cps[i];
                newIdx++;
            }
        }
        SetControlPoints(newCps);
        UpdateTrack(trackType);
    }

    public void RemoveControlPoint(int id)
    {       
        if (cps.Length > 4)
        {
            int newNum = cps.Length - 1;
            int removeIdx = 0;
            for (int i = 0; i < cps.Length; i++)
            {
                if (cps[i].GetInstanceID() == id)
                {
                    removeIdx = i;
                    break;
                }
            }
            GameObject[] newCps = new GameObject[newNum];
            for (int i = 0; i < cps.Length; i++)
            {
                if (i < removeIdx)
                {
                    newCps[i] = cps[i];
                }
                else if (i > removeIdx)
                {
                    newCps[i - 1] = cps[i];
                }
            }
            Destroy(cps[removeIdx]);
            SetLastControlPointInstanceID(0);
            SetControlPoints(newCps);
            UpdateTrack(trackType);
        }
        else
        {
            print("Control point can't less than 4!");
        }
    }

    public void RotateControlPointX(int id)
    {        
        for (int i = 0; i < cps.Length; i++)
        {
            if (cps[i].GetInstanceID() == id)
            {
                cps[i].transform.rotation *= Quaternion.Euler(30, 0, 0);
                break;
            }
        }
        SetControlPoints(cps);
        UpdateTrack(trackType);
    }

    public void RotateControlPointZ(int id)
    {        
        for (int i = 0; i < cps.Length; i++)
        {
            if (cps[i].GetInstanceID() == id)
            {
                cps[i].transform.rotation *= Quaternion.Euler(0, 0, 30);
                break;
            }
        }
        SetControlPoints(cps);
        UpdateTrack(trackType);
    }
        
    public void UpdateTrack(int type)
    {        
        //Redraw
        Vector3[][] trackPoints = CalculateTrack(cps, type);
        CalculateArcLength();
        manager.UpdatePositionAndOrientation(trackPositions,trackOrientations);
        if(trackPoints != null)
        {
            for (int i = 0; i < 3; i++)
            {
                trackLineRenderer[i].startWidth = Config.SLEEPER_WIDTH;
                trackLineRenderer[i].endWidth = Config.SLEEPER_WIDTH;
                trackLineRenderer[i].positionCount = trackPoints[i].Length;
                if (i != (int)Track.Sleeper)
                {
                    for (int j = 0; j < trackPoints[i].Length; j++)
                    {
                        trackLineRenderer[i].SetPosition(j, trackPoints[i][j]);
                    }
                }
                else
                {
                    for (int j = 0; j < trackPoints[i].Length; j += 4)
                    {
                        trackLineRenderer[i].SetPosition(j, trackPoints[i][j]);
                        trackLineRenderer[i].SetPosition(j + 1, trackPoints[i][j + 1]);
                        trackLineRenderer[i].SetPosition(j + 2, trackPoints[i][j + 3]);
                        trackLineRenderer[i].SetPosition(j + 3, trackPoints[i][j + 2]);
                    }
                    //
                    if(supports != null)
                    {
                        for (int j = 0; j < supports.Length; j++)
                        {
                            Destroy(supports[j]);
                        }
                    }
                   
                    supports = new GameObject[cps.Length * Config.DIVIDE * 2 / Config.SUPPRT_DISTANCE];
                    Vector3 v = trackPoints[i][0];
                    Vector3 sc = new Vector3(1, 20, 1); ;
                    for (int j = 0; j < supports.Length; j++)
                    {
                        supports[j] = Instantiate(manager.GetObject((int)Object.Support));
                        supports[j].transform.localScale = sc;
                        v = (trackPoints[i][j * Config.SUPPRT_DISTANCE] + trackPoints[i][j * Config.SUPPRT_DISTANCE + 1]) / 2;
                        v.y -= sc.y / 2f - 0.1f;
                        supports[j].transform.position = v;
                    }
                }
            }
        }        
    }

    public void TrackInInitialize()
    {        
        trackLineRenderer = new LineRenderer[3];
        trackLineRenderer[(int)Track.Outside] = transform.GetChild((int)Track.Outside).GetComponent<LineRenderer>();
        trackLineRenderer[(int)Track.Inside] = transform.GetChild((int)Track.Inside).GetComponent<LineRenderer>();
        trackLineRenderer[(int)Track.Sleeper] = transform.GetChild((int)Track.Sleeper).GetComponent<LineRenderer>();
        //Control points setting
        Vector3[] pos = new Vector3[Config.DEFAULT_CONTROLPOINT_NUM];
        pos[0] = new Vector3(Config.DEFAULT_CONTROLPOINT_DISTANCE, Config.DEFAULT_CONTROLPOINT_HEIGHT, Config.DEFAULT_CONTROLPOINT_DISTANCE);
        pos[1] = new Vector3(-Config.DEFAULT_CONTROLPOINT_DISTANCE, Config.DEFAULT_CONTROLPOINT_HEIGHT, Config.DEFAULT_CONTROLPOINT_DISTANCE);
        pos[2] = new Vector3(-Config.DEFAULT_CONTROLPOINT_DISTANCE, Config.DEFAULT_CONTROLPOINT_HEIGHT, -Config.DEFAULT_CONTROLPOINT_DISTANCE);
        pos[3] = new Vector3(Config.DEFAULT_CONTROLPOINT_DISTANCE, Config.DEFAULT_CONTROLPOINT_HEIGHT, -Config.DEFAULT_CONTROLPOINT_DISTANCE);
       
        GameObject tempObj = null;
        for (int i = 0; i < Config.DEFAULT_CONTROLPOINT_NUM; i++)
        {
            tempObj = Instantiate(manager.GetObject((int)Object.Controlpoint), pos[i], new Quaternion(0, 1, 0, 0));
            tempObj.name = i.ToString();
            tempObj.AddComponent<ControlPoint>();                        
            cps[i] = tempObj;
        }                
    }
        
    public void RemoveTrack()
    {       
        if(cps != null)
        {
            for (int i = 0; i < cps.Length; i++)
            {
                Destroy(cps[i]);
            }
            cps = null;//Wait GC
            for (int i = 0; i < trackLineRenderer.Length; i++)
            {
                trackLineRenderer[i].positionCount = 0;
            }
            for (int i = 0; i < supports.Length; i++)
            {
                Destroy(supports[i]);
            }
            supports = null;
        }        
    }
    
    private Vector3[][] CalculateTrack(GameObject[] cps,int type)
    {
        Vector3[][] trackPoints = null;
        if (cps != null)
        {
            trackPoints = new Vector3[3][];
            trackPositions = new Vector3[cps.Length * Config.DIVIDE];
            trackOrientations = new Vector3[cps.Length * Config.DIVIDE];

            Vector3[] tracksOutside = new Vector3[cps.Length * Config.DIVIDE];
            Vector3[] tracksInside = new Vector3[cps.Length * Config.DIVIDE];
            Vector3[] tracksSleeper = new Vector3[cps.Length * Config.DIVIDE * 2];

            float[,] cardinalMatrix = new float[4, 4];
            float[,] cubicMatrix = new float[4, 4];
            cardinalMatrix[0, 0] = -1 / 2f;
            cardinalMatrix[0, 1] = 3 / 2f;
            cardinalMatrix[0, 2] = -3 / 2f;
            cardinalMatrix[0, 3] = 1 / 2f;

            cardinalMatrix[1, 0] = 1f;
            cardinalMatrix[1, 1] = -5 / 2f;
            cardinalMatrix[1, 2] = 2f;
            cardinalMatrix[1, 3] = -1 / 2f;

            cardinalMatrix[2, 0] = -1 / 2f;
            cardinalMatrix[2, 1] = 0;
            cardinalMatrix[2, 2] = 1 / 2f;
            cardinalMatrix[2, 3] = 0;

            cardinalMatrix[3, 0] = 0;
            cardinalMatrix[3, 1] = 1;
            cardinalMatrix[3, 2] = 0;
            cardinalMatrix[3, 3] = 0;
            //////////////////////////
            cubicMatrix[0, 0] = -1 / 6f;
            cubicMatrix[0, 1] = 3 / 6f;
            cubicMatrix[0, 2] = -3 / 6f;
            cubicMatrix[0, 3] = 1 / 6f;

            cubicMatrix[1, 0] = 3 / 6f;
            cubicMatrix[1, 1] = -1f;
            cubicMatrix[1, 2] = 3 / 6f;
            cubicMatrix[1, 3] = 0;

            cubicMatrix[2, 0] = -3 / 6f;
            cubicMatrix[2, 1] = 0;
            cubicMatrix[2, 2] = 3 / 6f;
            cubicMatrix[2, 3] = 0;

            cubicMatrix[3, 0] = 1 / 6f;
            cubicMatrix[3, 1] = 4 / 6f;
            cubicMatrix[3, 2] = 1 / 6f;
            cubicMatrix[3, 3] = 0;

            Vector3[] pMatrix = new Vector3[4];
            Vector3[] oriMatrix = new Vector3[4];

            float trackWidth = 5f;           
            int toIdx = 0;
            int tiIdx = 0;
            int sIdx = 0;
            for (int i = 0; i < cps.Length; i++)
            {
                Vector3 p1 = cps[i].transform.position;
                Vector3 p2 = cps[(i + 1) % cps.Length].transform.position;
                Quaternion q1 = cps[i].transform.rotation;
                Quaternion q2 = cps[(i + 1) % cps.Length].transform.rotation;
                Vector3 or1 = new Vector3(q1.x, q1.y, q1.z);
                Vector3 or2 = new Vector3(q2.x, q2.y, q2.z);

                ////////////////////
                if (i == 0)
                {
                    pMatrix[0] = cps[cps.Length - 1].transform.position;
                    pMatrix[1] = cps[0].transform.position;
                    //
                    q1 = cps[cps.Length - 1].transform.rotation;
                    q2 = cps[0].transform.rotation;
                    //
                    oriMatrix[0] = new Vector3(q1.x, q1.y, q1.z);
                    oriMatrix[1] = new Vector3(q2.x, q2.y, q2.z);
                }
                else if (i == 1)
                {
                    pMatrix[0] = cps[0].transform.position;
                    pMatrix[1] = cps[1].transform.position;
                    //
                    q1 = cps[0].transform.rotation;
                    q2 = cps[1].transform.rotation;
                    //
                    oriMatrix[0] = new Vector3(q1.x, q1.y, q1.z);
                    oriMatrix[1] = new Vector3(q2.x, q2.y, q2.z);
                }
                else
                {
                    pMatrix[0] = cps[i - 1].transform.position;
                    pMatrix[1] = cps[i].transform.position;
                    //
                    q1 = cps[i - 1].transform.rotation;
                    q2 = cps[i].transform.rotation;
                    //
                    oriMatrix[0] = new Vector3(q1.x, q1.y, q1.z);
                    oriMatrix[1] = new Vector3(q2.x, q2.y, q2.z);
                }
                pMatrix[2] = cps[(i + 1) % cps.Length].transform.position;
                pMatrix[3] = cps[(i + 2) % cps.Length].transform.position;
                if (type == 1 || type == 2)
                {
                    q1 = cps[(i + 1) % cps.Length].transform.rotation;
                    q2 = cps[(i + 2) % cps.Length].transform.rotation;
                    oriMatrix[2] = new Vector3(q1.x, q1.y, q1.z);
                    oriMatrix[3] = new Vector3(q2.x, q2.y, q2.z);
                }
                ///////////////////////////////////
                float percent = 1f / Config.DIVIDE;
                float t = 0;
                Vector3 pt0 = (1 - t) * p1 + t * p2;
                Vector3 ort0 = (1 - t) * or1 + t * or2;

                Vector3 pt1 = (1 - t) * p1 + t * p2;
                Vector3 ort1 = (1 - t) * or1 + t * or2;

                Vector3 cross0 = new Vector3(0, 0, 0);
                Vector3 cross1 = new Vector3(0, 0, 0);

                for (int j = 0; j < Config.DIVIDE; j++)
                {
                    if (j == 0)
                    {
                        //First
                        if (type == 0)//Linear
                        {
                            pt0 = (1 - t) * p1 + t * p2;
                            ort0 = (1 - t) * or1 + t * or2;
                        }
                        else if (type == 1)//Cardinal
                        {
                            pt0 =
                                (pMatrix[0] * cardinalMatrix[0, 0] + pMatrix[1] * cardinalMatrix[0, 1] + pMatrix[2] * cardinalMatrix[0, 2] + pMatrix[3] * cardinalMatrix[0, 3]) * Mathf.Pow(t, 3) +
                                (pMatrix[0] * cardinalMatrix[1, 0] + pMatrix[1] * cardinalMatrix[1, 1] + pMatrix[2] * cardinalMatrix[1, 2] + pMatrix[3] * cardinalMatrix[1, 3]) * Mathf.Pow(t, 2) +
                                (pMatrix[0] * cardinalMatrix[2, 0] + pMatrix[1] * cardinalMatrix[2, 1] + pMatrix[2] * cardinalMatrix[2, 2] + pMatrix[3] * cardinalMatrix[2, 3]) * Mathf.Pow(t, 1) +
                                (pMatrix[0] * cardinalMatrix[3, 0] + pMatrix[1] * cardinalMatrix[3, 1] + pMatrix[2] * cardinalMatrix[3, 2] + pMatrix[3] * cardinalMatrix[3, 3]) * 1;
                            ort0 =
                                (oriMatrix[0] * cardinalMatrix[0, 0] + oriMatrix[1] * cardinalMatrix[0, 1] + oriMatrix[2] * cardinalMatrix[0, 2] + oriMatrix[3] * cardinalMatrix[0, 3]) * Mathf.Pow(t, 3) +
                                (oriMatrix[0] * cardinalMatrix[1, 0] + oriMatrix[1] * cardinalMatrix[1, 1] + oriMatrix[2] * cardinalMatrix[1, 2] + oriMatrix[3] * cardinalMatrix[1, 3]) * Mathf.Pow(t, 2) +
                                (oriMatrix[0] * cardinalMatrix[2, 0] + oriMatrix[1] * cardinalMatrix[2, 1] + oriMatrix[2] * cardinalMatrix[2, 2] + oriMatrix[3] * cardinalMatrix[2, 3]) * Mathf.Pow(t, 1) +
                                (oriMatrix[0] * cardinalMatrix[3, 0] + oriMatrix[1] * cardinalMatrix[3, 1] + oriMatrix[2] * cardinalMatrix[3, 2] + oriMatrix[3] * cardinalMatrix[3, 3]) * 1; ;
                        }
                        else if (type == 2)//Cubic
                        {
                            pt0 =
                                (pMatrix[0] * cubicMatrix[0, 0] + pMatrix[1] * cubicMatrix[0, 1] + pMatrix[2] * cubicMatrix[0, 2] + pMatrix[3] * cubicMatrix[0, 3]) * Mathf.Pow(t, 3) +
                                (pMatrix[0] * cubicMatrix[1, 0] + pMatrix[1] * cubicMatrix[1, 1] + pMatrix[2] * cubicMatrix[1, 2] + pMatrix[3] * cubicMatrix[1, 3]) * Mathf.Pow(t, 2) +
                                (pMatrix[0] * cubicMatrix[2, 0] + pMatrix[1] * cubicMatrix[2, 1] + pMatrix[2] * cubicMatrix[2, 2] + pMatrix[3] * cubicMatrix[2, 3]) * Mathf.Pow(t, 1) +
                                (pMatrix[0] * cubicMatrix[3, 0] + pMatrix[1] * cubicMatrix[3, 1] + pMatrix[2] * cubicMatrix[3, 2] + pMatrix[3] * cubicMatrix[3, 3]) * 1;
                            ort0 =
                                (oriMatrix[0] * cubicMatrix[0, 0] + oriMatrix[1] * cubicMatrix[0, 1] + oriMatrix[2] * cubicMatrix[0, 2] + oriMatrix[3] * cubicMatrix[0, 3]) * Mathf.Pow(t, 3) +
                                (oriMatrix[0] * cubicMatrix[1, 0] + oriMatrix[1] * cubicMatrix[1, 1] + oriMatrix[2] * cubicMatrix[1, 2] + oriMatrix[3] * cubicMatrix[1, 3]) * Mathf.Pow(t, 2) +
                                (oriMatrix[0] * cubicMatrix[2, 0] + oriMatrix[1] * cubicMatrix[2, 1] + oriMatrix[2] * cubicMatrix[2, 2] + oriMatrix[3] * cubicMatrix[2, 3]) * Mathf.Pow(t, 1) +
                                (oriMatrix[0] * cubicMatrix[3, 0] + oriMatrix[1] * cubicMatrix[3, 1] + oriMatrix[2] * cubicMatrix[3, 2] + oriMatrix[3] * cubicMatrix[3, 3]) * 1;
                        }
                    }
                    else
                    {
                        pt0 = pt1;
                        ort0 = ort1;
                    }
                    t += percent;

                    //Second
                    if (type == 0)//Linear
                    {
                        pt1 = (1 - t) * p1 + t * p2;
                        ort1 = (1 - t) * or1 + t * or2;
                    }
                    else if (type == 1)//Cardinal
                    {
                        pt1 =
                            (pMatrix[0] * cardinalMatrix[0, 0] + pMatrix[1] * cardinalMatrix[0, 1] + pMatrix[2] * cardinalMatrix[0, 2] + pMatrix[3] * cardinalMatrix[0, 3]) * Mathf.Pow(t, 3) +
                            (pMatrix[0] * cardinalMatrix[1, 0] + pMatrix[1] * cardinalMatrix[1, 1] + pMatrix[2] * cardinalMatrix[1, 2] + pMatrix[3] * cardinalMatrix[1, 3]) * Mathf.Pow(t, 2) +
                            (pMatrix[0] * cardinalMatrix[2, 0] + pMatrix[1] * cardinalMatrix[2, 1] + pMatrix[2] * cardinalMatrix[2, 2] + pMatrix[3] * cardinalMatrix[2, 3]) * Mathf.Pow(t, 1) +
                            (pMatrix[0] * cardinalMatrix[3, 0] + pMatrix[1] * cardinalMatrix[3, 1] + pMatrix[2] * cardinalMatrix[3, 2] + pMatrix[3] * cardinalMatrix[3, 3]) * 1;
                        ort1 =
                            (oriMatrix[0] * cardinalMatrix[0, 0] + oriMatrix[1] * cardinalMatrix[0, 1] + oriMatrix[2] * cardinalMatrix[0, 2] + oriMatrix[3] * cardinalMatrix[0, 3]) * Mathf.Pow(t, 3) +
                            (oriMatrix[0] * cardinalMatrix[1, 0] + oriMatrix[1] * cardinalMatrix[1, 1] + oriMatrix[2] * cardinalMatrix[1, 2] + oriMatrix[3] * cardinalMatrix[1, 3]) * Mathf.Pow(t, 2) +
                            (oriMatrix[0] * cardinalMatrix[2, 0] + oriMatrix[1] * cardinalMatrix[2, 1] + oriMatrix[2] * cardinalMatrix[2, 2] + oriMatrix[3] * cardinalMatrix[2, 3]) * Mathf.Pow(t, 1) +
                            (oriMatrix[0] * cardinalMatrix[3, 0] + oriMatrix[1] * cardinalMatrix[3, 1] + oriMatrix[2] * cardinalMatrix[3, 2] + oriMatrix[3] * cardinalMatrix[3, 3]) * 1; ;
                    }
                    else if (type == 2)//Cubic
                    {
                        pt1 =
                            (pMatrix[0] * cubicMatrix[0, 0] + pMatrix[1] * cubicMatrix[0, 1] + pMatrix[2] * cubicMatrix[0, 2] + pMatrix[3] * cubicMatrix[0, 3]) * Mathf.Pow(t, 3) +
                            (pMatrix[0] * cubicMatrix[1, 0] + pMatrix[1] * cubicMatrix[1, 1] + pMatrix[2] * cubicMatrix[1, 2] + pMatrix[3] * cubicMatrix[1, 3]) * Mathf.Pow(t, 2) +
                            (pMatrix[0] * cubicMatrix[2, 0] + pMatrix[1] * cubicMatrix[2, 1] + pMatrix[2] * cubicMatrix[2, 2] + pMatrix[3] * cubicMatrix[2, 3]) * Mathf.Pow(t, 1) +
                            (pMatrix[0] * cubicMatrix[3, 0] + pMatrix[1] * cubicMatrix[3, 1] + pMatrix[2] * cubicMatrix[3, 2] + pMatrix[3] * cubicMatrix[3, 3]) * 1;
                        ort1 =
                            (oriMatrix[0] * cubicMatrix[0, 0] + oriMatrix[1] * cubicMatrix[0, 1] + oriMatrix[2] * cubicMatrix[0, 2] + oriMatrix[3] * cubicMatrix[0, 3]) * Mathf.Pow(t, 3) +
                            (oriMatrix[0] * cubicMatrix[1, 0] + oriMatrix[1] * cubicMatrix[1, 1] + oriMatrix[2] * cubicMatrix[1, 2] + oriMatrix[3] * cubicMatrix[1, 3]) * Mathf.Pow(t, 2) +
                            (oriMatrix[0] * cubicMatrix[2, 0] + oriMatrix[1] * cubicMatrix[2, 1] + oriMatrix[2] * cubicMatrix[2, 2] + oriMatrix[3] * cubicMatrix[2, 3]) * Mathf.Pow(t, 1) +
                            (oriMatrix[0] * cubicMatrix[3, 0] + oriMatrix[1] * cubicMatrix[3, 1] + oriMatrix[2] * cubicMatrix[3, 2] + oriMatrix[3] * cubicMatrix[3, 3]) * 1;
                    }
                    /////////////////////
                    cross0 = (pt1 - pt0);
                    cross0 = Vector3.Cross(cross0, ort0);//Cross product
                    cross0 = cross0.normalized;

                    cross1 = (pt1 - pt0);
                    cross1 = Vector3.Cross(cross1, ort1);
                    cross1 = cross1.normalized;
                    /////////////////////
                    cross0 *= trackWidth;

                    //For train
                    trackPositions[toIdx] = pt1;
                    trackOrientations[toIdx] = ort0;
                    trackOrientations[toIdx] = cross0;
                    //
                    tracksOutside[toIdx++] = (pt1 + cross1);
                    tracksInside[tiIdx++] = (pt1 - cross0);

                    //Sleepers
                    tracksSleeper[sIdx++] = (pt1 + cross1);
                    tracksSleeper[sIdx++] = (pt1 - cross0);                    
                }                
            }
            trackPoints[0] = tracksOutside;
            trackPoints[1] = tracksInside;
            trackPoints[2] = tracksSleeper;            
        }
        return trackPoints;
    }

    private void CalculateArcLength()
    {
        List<Vector3> newPos = new List<Vector3>();
        List<Vector3> newOri = new List<Vector3>();
        
        List<Vector3> pos = new List<Vector3>(trackPositions);
        List<Vector3> ori = new List<Vector3>(trackOrientations);
        
        float minimal = Vector3.Distance(pos[0], pos[1]);
        for (int i = 1;i < pos.Count - 1;i++)
        {
            float dis = Vector3.Distance(pos[i], pos[i+1]);
            if (dis < minimal)
                minimal = dis;
        }
        //arcLength += Vector3.Distance(pos[trackPositions.Length - 1], pos[0]);        
        //float deltaPos = arcLength / totalDivide;        
        for (int i = 0; i < pos.Count - 1; i++)
        {
            float distance = Vector3.Distance(pos[i], pos[i + 1]);            
            if (distance > minimal)
            {
                int div = (int)(distance / minimal);
                Vector3 dPos = (pos[i + 1] - pos[i]) / div;
                Vector3 dOri = (ori[i + 1] - ori[i]) / div;

                for (int j = 0; j < div; j++)
                {
                    newPos.Add(pos[i] + dPos * j);
                    newOri.Add(ori[i] + dOri * j);                    
                }                
            }
            else
            {
                newPos.Add(pos[i]);               
                newOri.Add(ori[i]);                
            }            
        }
        trackPositions = null;
        trackPositions = newPos.ToArray();
        trackOrientations = null;
        trackOrientations = newOri.ToArray();

        pos.Clear();
        ori.Clear();
        newPos.Clear();
        newOri.Clear();
    }
    public void CheckControllPoint(int id)
    {
        if (id != 0)
        {
            if (id == lastSelectedInstanceID)
            {
                currentSelectedInstanceID = 0;
                lastSelectedInstanceID = 0;
                for (int i = 0; i < cps.Length; i++)
                {
                    cps[i].transform.GetChild(1).GetComponent<MeshRenderer>().material.color = controlPointStatusColor[(int)ControlPointStatus.Unselected];
                }
            }
            else
            {
                lastSelectedInstanceID = id;
                for (int i = 0; i < cps.Length; i++)
                {
                    if (cps[i].GetInstanceID() == id)
                    {
                        cps[i].transform.GetChild(1).GetComponent<MeshRenderer>().material.color = controlPointStatusColor[(int)ControlPointStatus.Selected];
                        for (int j = 0; j < cps.Length; j++)
                        {
                            if (i != j)
                            {
                                cps[j].transform.GetChild(1).GetComponent<MeshRenderer>().material.color = controlPointStatusColor[(int)ControlPointStatus.Unselected];
                            }
                        }
                    }
                }
                currentSelectedInstanceID = 0;
            }
        }
    }
    
    public void SaveTrack()
    {
        StreamWriter sw = new StreamWriter(Path.Combine(Application.dataPath, "track.txt"));
        GameObject[] controlPoints = GetControlPoints();
        int count = controlPoints.Length;
        sw.WriteLine(count);
        for (int i = 0; i < count; i++)
        {
            sw.WriteLine(controlPoints[i].transform.position.x + " " + controlPoints[i].transform.position.y + " " + controlPoints[i].transform.position.z
                + " " + controlPoints[i].transform.rotation.x + " " + controlPoints[i].transform.rotation.y + " " + controlPoints[i].transform.rotation.z);
        }
        sw.Close();
        print("Track Saved.");
    }

    public void LoadTrack()
    {
        StreamReader sr = null;
        string path = LoadTrackFilePath();
        if (path != "")
        {
            sr = new StreamReader(path, true);
            int count = Int32.Parse(sr.ReadLine());
            string str = "";
            string[] strs = null;
            Vector3[] pos = new Vector3[count];
            Vector3[] orient = new Vector3[count];
            Vector3 v = Vector3.zero;
            for (int i = 0; i < count; i++)
            {
                str = sr.ReadLine();
                strs = str.Split(' ');
                //Pos
                v.x = float.Parse(strs[0]);
                v.y = float.Parse(strs[1]);
                v.z = float.Parse(strs[2]);
                pos[i] = v;
                //Orient
                v.x = float.Parse(strs[3]);
                v.y = float.Parse(strs[4]);
                v.z = float.Parse(strs[5]);
                orient[i] = v;
            }
            SetControlPoints(count, pos, orient);
            sr.Close();
            print("Track Loaded.");
            UpdateTrack(trackType);
        }
    }

    private string LoadTrackFilePath()
    {
        FileOpenDialog dialog = new FileOpenDialog();
        dialog.structSize = Marshal.SizeOf(dialog);
        dialog.initialDir = Application.dataPath;
        //dialog.filter = "PNG\0*.png\0JPG\0*.jpg\0\0";
        dialog.filter = "Text\0*.txt";

        dialog.file = new string(new char[256]);

        dialog.maxFile = dialog.file.Length;

        dialog.fileTitle = new string(new char[64]);

        dialog.maxFileTitle = dialog.fileTitle.Length;

        dialog.initialDir = UnityEngine.Application.dataPath;

        dialog.title = "Open File Dialog";

        dialog.defExt = "Text";
        dialog.flags = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000200 | 0x00000008;

        if (DialogShow.GetOpenFileName(dialog))
        {
            return dialog.file;
        }
        else
        {
            return "";
        }
    }
}
