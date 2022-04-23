using System.Collections.Generic;
using System.IO;
using Unity.VectorGraphics;
using UnityEngine;

/// <summary>
/// Helper class of Unity's Vector Graphic package
/// </summary>
public class VectorGraphicsHelper
{
    public static List<Vector2> GetPathFromSVGFile(string Filename, int SamplesPerShape = 50)
    {
        using (StreamReader reader = File.OpenText(Filename))
        {
            return GetPathFromSceneNode(SVGParser.ImportSVG(reader).Scene.Root, SamplesPerShape);
        }
    }

    public static List<Vector2> GetPathFromSceneNode(SceneNode Node, int SamplesPerShape = 50)
    {
        List<Vector2> path = new List<Vector2>();
        if (Node.Children != null && Node.Children.Count > 0)
        {
            for (int i = 0; i < Node.Children.Count; i++)
            {
                path.AddRange(GetPathFromSceneNode(Node.Children[i], SamplesPerShape));
            }
        }
        else
        {
            List<Shape> shapes = Node.Shapes;
            foreach (Shape shape in shapes)
            {
                for (int i = 0; i < shape.Contours.Length; i++)
                {
                    var segments = shape.Contours[i].Segments;
                    float maxtime = segments.Length - 1;
                    float deltatime = maxtime / SamplesPerShape;

                    float time = 0;
                    for (int j = 0; j < SamplesPerShape; j++)
                    {
                        int index = Mathf.FloorToInt(time);
                        float t = time - index;

                        var p0 = segments[index].P0;
                        var p1 = segments[index].P1;
                        var p2 = segments[index].P2;
                        var p3 = segments[index + 1].P0;

                        path.Add(InterpolateQuadraticBezier(p0, p1, p2, p3, t));

                        time += deltatime;
                    }
                }
            }
        }

        return path;
    }
    public static Vector2 InterpolateQuadraticBezier(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
    {
        Vector2 point =
            (1 - t) * (1 - t) * (1 - t) * p0 +
            3 * (1 - t) * (1 - t) * t * p1 +
            3 * (1 - t) * t * t * p2 +
            t * t * t * p3;

        return point;
    }
}
