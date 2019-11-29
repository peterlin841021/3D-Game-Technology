using System.Collections.Generic;
using UnityEngine;

public static class Boundary
{
    public class Edge
    {
        public int v1;
        public int v2;
        public int triangleIndex;
        public Edge(int aV1, int aV2, int aIndex)
        {
            v1 = aV1;
            v2 = aV2;
            triangleIndex = aIndex;
        }
    }
    //Compare edge
    public class EdgeComparator : IEqualityComparer<Edge>
    {
        public bool Equals(Edge e1, Edge e2)
        {
            if ((e1.v1 == e2.v1) && (e1.v2 == e2.v2)) return true;
            else if ((e1.v2 == e2.v1) && (e1.v1 == e2.v2)) return true;
            else return false;
        }

        public int GetHashCode(Edge e)
        {
            return e.ToString().GetHashCode();
        }
    }
    public static List<Edge> GetEdges(int[] triIdx, Vector3[] vertices)
    {
        List<Edge> result = new List<Edge>(triIdx.Length * 3);

        for (int i = 0; i < triIdx.Length / 3; i++)
        {
            int v1 = triIdx[i * 3 + 0];
            int v2 = triIdx[i * 3 + 1];
            int v3 = triIdx[i * 3 + 2];
            Edge e1 = new Edge(v1, v2, i);
            Edge e2 = new Edge(v2, v3, i);
            Edge e3 = new Edge(v3, v1, i);
            result.Add(e1);
            result.Add(e2);
            result.Add(e3);
        }
        return result;
    }

    public static List<Edge> FindBoundary(this List<Edge> aEdges)
    {
        List<Edge> result = new List<Edge>(aEdges);
        for (int i = result.Count - 1; i > 0; i--)
        {
            for (int j = i - 1; j >= 0; j--)
            {
                if ((result[i].triangleIndex != result[j].triangleIndex) && ((result[i].v1 == result[j].v2 && result[i].v2 == result[j].v1) || (result[i].v1 == result[j].v1 && result[i].v2 == result[j].v2)))
                {
                    result.RemoveAt(i);
                    result.RemoveAt(j);
                    i--;
                    break;
                }
            }
        }
        return result;
    }

    public static List<Edge> SortEdges(this List<Edge> aEdges)
    {
        List<Edge> result = new List<Edge>(aEdges);
        for (int i = 0; i < result.Count - 2; i++)
        {
            Edge E = result[i];
            for (int n = i + 1; n < result.Count; n++)
            {
                Edge a = result[n];
                if (E.v2 == a.v1)
                {
                    if (n == i + 1)
                        break;
                    result[n] = result[i + 1];
                    result[i + 1] = a;
                    break;
                }
            }
        }
        return result;
    }

    //Change vertex index  
    public static List<int> RemoveDuplicatePoints(List<Vector3> meshPoints, int[] triangles)
    {
        List<int> idx = new List<int>(triangles);
        for (int i = 0; i < idx.Count; i++)
        {
            for (int j = i + 1; j < idx.Count; j++)
            {
                if (idx[i] != idx[j] && meshPoints[idx[i]].Equals(meshPoints[idx[j]]))
                {
                    int index = (idx[j] < idx[i]) ? idx[j] : idx[i];

                    for (int k =0; k < idx.Count; k++)
                    {
                        if (idx[k] == idx[j])
                        {
                            idx[k] = index;
                        }
                    }
                }
            }
        }
        return idx;
    }
    //Change vertex index  
    public static List<int> NewRemoveDuplicatePoints(List<Vector3> meshPoints, int[] triangles)
    {
        List<int> idx = new List<int>(triangles);
        for (int i = 0; i < idx.Count; i++)
        {
            for (int j = i + 1; j < idx.Count; j++)
            {
                if (idx[i] != idx[j] && meshPoints[idx[i]].Equals(meshPoints[idx[j]]))
                {
                    int index = (idx[j] < idx[i]) ? idx[j] : idx[i];

                    for (int k = j + 1; k < idx.Count; k++)
                    {
                        if (idx[k] == idx[j])
                        {
                            idx[k] = index;
                        }
                    }
                }
            }
        }
        return idx;
    }

}
