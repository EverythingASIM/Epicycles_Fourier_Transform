using asim.unity.extensions;
using asim.unity.helpers;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 1.Parse a SVG file from streaming assets, into a Vector2 path list,
/// 2.Perform IDFT on the individual X, and Y coordinates of path to obtain the results encoded with the freq, amp and phase of a sinewave
/// 3.Reconstruct sine waves using freq, amp and phase, display them as combination of Epiccycles
/// 4.Flip between Displaying Epicycles of IDFT(x) and IDFT(y) seperately, and combined
/// </summary>
public class Epicycles_Fourier_Transform : MonoBehaviour
{
    public int SamplesPerShape = 50;
    
    List<(float, float, float, float, float)> fourierX = new List<(float, float, float, float, float)>();
    List<(float, float, float, float, float)> fourierY = new List<(float, float, float, float, float)>();
    float time = 0f;

    List<Vector2> path = new List<Vector2>();
    void Awake()
    {
        UnityGameViewManager.UseCustomResolution(800, 800);
        UnityOnGUIHelper.SetFrameRate(30);
    }

    void Start()
    {
        string filename = Application.streamingAssetsPath + @"\svg.svg";
        var svgpath = VectorGraphicsHelper.GetPathFromSVGFile(filename, SamplesPerShape);

        List<float> x = new List<float>();
        List<float> y = new List<float>();

        for (int i = 0; i < svgpath.Count; i++)
        {
            x.Add(svgpath[i].x);
            y.Add(svgpath[i].y);
        }

        fourierX = Fourier.IDFT(x);
        fourierY = Fourier.IDFT(y);
    }


    Vector2 epiCycles(Vector2 startPos, float rotationOffset, List<(float re, float im, float freq, float amp, float phase)> fourier)
    {
        //from staring position, itterate though the entire list of sine waves, 
        //for each sine wave, compute the x,y offset to append to the starting positon
        //this will making the starting position crawl towards a final position
        Vector2 currentPos = startPos;
        Vector2 prevPos = currentPos;

        for (int i = 0; i < fourier.Count; i++)
        {
            prevPos = currentPos;

            var radius = fourier[i].amp;
            var freq = fourier[i].freq;
            var phase = fourier[i].phase;
            currentPos.x += radius * Mathf.Cos(freq * time + phase + rotationOffset);
            currentPos.y += radius * Mathf.Sin(freq * time + phase + rotationOffset);

            UnityOnGUIHelper.DrawDot(prevPos, radius*2, Color.green, Color.red, 1);
            UnityOnGUIHelper.DrawLine(prevPos, currentPos, Color.red, 2);
        }

        return currentPos;
    }
    Vector2 epiCycles2(Vector2 startPos, float rotationOffset,
        List<(float re, float im, float freq, float amp, float phase)> fourier,
        List<(float re, float im, float freq, float amp, float phase)> fourier2)
    {
        //from staring position, itterate though the entire list of sine waves, 
        //for each sine wave, compute the x,y offset to append to the starting positon
        //this will making the starting position crawl towards a final position
        Vector2 currentPos = startPos;
        Vector2 prevPos = currentPos;

        for (int i = 0; i < fourier.Count; i++)
        {
            prevPos = currentPos;

            var radius = fourier[i].amp;
            var freq = fourier[i].freq;
            var phase = fourier[i].phase;

            var radius2 = fourier2[i].amp;
            var freq2 = fourier2[i].freq;
            var phase2 = fourier2[i].phase;

            currentPos.x += radius * Mathf.Cos(freq * time + phase);
            currentPos.y += radius * Mathf.Sin(freq2 * time + phase);

            currentPos.x += radius2 * Mathf.Cos(freq * time + phase2 + rotationOffset);
            currentPos.y += radius2 * Mathf.Sin(freq2 * time + phase2 + rotationOffset);

            UnityOnGUIHelper.DrawDot(prevPos, radius * 2 + radius2 * 2, Color.green, Color.red, 1);
            UnityOnGUIHelper.DrawLine(prevPos, currentPos, Color.red, 2);
        }

        return currentPos;
    }

    bool DoCombined()
    {
        var vx = epiCycles(new Vector2(Screen.width / 2 + 100, 100), 0, fourierX);
        var vy = epiCycles(new Vector2(100, Screen.height / 2 + 100), Mathf.PI / 2, fourierY);
        var v = new Vector2(vx.x, vy.y);

        UnityOnGUIHelper.DrawLine(new Vector2(vx.x, vx.y), new Vector2(v.x, v.y), Color.red, 2);
        UnityOnGUIHelper.DrawLine(new Vector2(vy.x, vy.y), new Vector2(v.x, v.y), Color.red, 2);

        path.Add(v);

        Mesh mesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        List<int> indices = new List<int>();

        for (int i = 0; i < path.Count; i++)
        {
            vertices.Add(new Vector3(path[i].x, Screen.height - path[i].y));
            indices.Add(i);
        }
        mesh.SetVertices(vertices);
        mesh.SetIndices(indices, MeshTopology.LineStrip, 0);
        gameObject.GetComponent<MeshFilter>().mesh = mesh;

        float dt = (2f * Mathf.PI) / fourierX.Count;
        time += dt;

        if (time > 2 * Mathf.PI)
        {
            time = 0;
            path.Clear();
            return true;
        }
        return false;
    }
    bool DoDouble()
    {
        var v = epiCycles2(new Vector2(Screen.width / 2, Screen.height / 2), Mathf.PI/2, fourierX, fourierY);
        path.Add(v);

        Mesh mesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        List<int> indices = new List<int>();

        for (int i = 0; i < path.Count; i++)
        {
            vertices.Add(new Vector3(path[i].x, Screen.height - path[i].y));
            indices.Add(i);
        }
        mesh.SetVertices(vertices);
        mesh.SetIndices(indices, MeshTopology.LineStrip, 0);
        gameObject.GetComponent<MeshFilter>().mesh = mesh;

        float dt = (2f * Mathf.PI) / fourierX.Count;
        time += dt;

        if (time > 2 * Mathf.PI)
        {
            time = 0;
            path.Clear();
            return true;
        }
        return false;
    }



    bool flip = false;
    void OnGUI()
    {
        if (flip)
        {
            flip = !DoDouble();
        }
        else
        {
            flip = DoCombined();
        }
    }
}
