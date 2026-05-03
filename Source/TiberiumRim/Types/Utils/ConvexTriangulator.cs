using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TR;

/* Vertex Triangulation thanks to https://www.habrador.com/tutorials/math/10-triangulation/
 *
 */
public class Vertex
{
    //The outgoing halfedge (a halfedge that starts at this vertex)
    //Doesnt matter which edge we connect to it
    public HalfEdge halfEdge;
    public bool isConvex;
    public bool isEar;

    //Properties this vertex may have
    //Reflex is concave
    public bool isReflex;
    public Vertex nextVertex;
    public Vector3 position;

    //The previous and next vertex this vertex is attached to
    public Vertex prevVertex;

    //Which triangle is this vertex a part of?
    public Triangle triangle;

    public Vertex(Vector3 position)
    {
        this.position = position;
    }

    //Get 2d pos of this vertex
    public Vector2 GetPos2D_XZ()
    {
        var pos_2d_xz = new Vector2(position.x, position.z);

        return pos_2d_xz;
    }
}

public class HalfEdge
{
    //The next edge
    public HalfEdge nextEdge;

    //The edge going in the opposite direction
    public HalfEdge oppositeEdge;

    //The previous
    public HalfEdge prevEdge;

    //The face this edge is a part of
    public Triangle t;

    //The vertex the edge points to
    public Vertex v;

    //This structure assumes we have a vertex class with a reference to a half edge going from that vertex
    //and a face (triangle) class with a reference to a half edge which is a part of this face 
    public HalfEdge(Vertex v)
    {
        this.v = v;
    }
}

public class Triangle
{
    //If we are using the half edge mesh structure, we just need one half edge
    public HalfEdge halfEdge;

    //Corners
    public Vertex v1;
    public Vertex v2;
    public Vertex v3;

    public Triangle(Vertex v1, Vertex v2, Vertex v3)
    {
        this.v1 = v1;
        this.v2 = v2;
        this.v3 = v3;
    }

    public Triangle(Vector3 v1, Vector3 v2, Vector3 v3)
    {
        this.v1 = new Vertex(v1);
        this.v2 = new Vertex(v2);
        this.v3 = new Vertex(v3);
    }

    public Triangle(HalfEdge halfEdge)
    {
        this.halfEdge = halfEdge;
    }

    //Change orientation of triangle from cw -> ccw or ccw -> cw
    public void ChangeOrientation()
    {
        var temp = v1;

        v1 = v2;

        v2 = temp;
    }
}

public static class ConvexTriangulator
{
    public static List<Triangle> TriangulateConvexPolygon(List<Vertex> convexHullpoints)
    {
        var triangles = new List<Triangle>();

        for (var i = 2; i < convexHullpoints.Count; i++)
        {
            var a = convexHullpoints[0];
            var b = convexHullpoints[i - 1];
            var c = convexHullpoints[i];

            triangles.Add(new Triangle(a, b, c));
        }

        return triangles;
    }

    public static List<Vertex> GetConvexHull(List<Vertex> points)
    {
        //If we have just 3 points, then they are the convex hull, so return those
        if (points.Count == 3)
            //These might not be ccw, and they may also be colinear
            return points;

        //If fewer points, then we cant create a convex hull
        if (points.Count < 3) return null;


        //The list with points on the convex hull
        var convexHull = new List<Vertex>();

        //Step 1. Find the vertex with the smallest x coordinate
        //If several have the same x coordinate, find the one with the smallest z
        var startVertex = points[0];

        var startPos = startVertex.position;

        for (var i = 1; i < points.Count; i++)
        {
            var testPos = points[i].position;

            //Because of precision issues, we use Mathf.Approximately to test if the x positions are the same
            if (testPos.x < startPos.x || (Mathf.Approximately(testPos.x, startPos.x) && testPos.z < startPos.z))
            {
                startVertex = points[i];

                startPos = startVertex.position;
            }
        }

        //This vertex is always on the convex hull
        convexHull.Add(startVertex);

        points.Remove(startVertex);


        //Step 2. Loop to generate the convex hull
        var currentPoint = convexHull[0];

        //Store colinear points here - better to create this list once than each loop
        var colinearPoints = new List<Vertex>();

        var counter = 0;

        while (true)
        {
            //After 2 iterations we have to add the start position again so we can terminate the algorithm
            //Cant use convexhull.count because of colinear points, so we need a counter
            if (counter == 2) points.Add(convexHull[0]);

            //Pick next point randomly
            var nextPoint = points[TRandom.Range(0, points.Count)];

            //To 2d space so we can see if a point is to the left is the vector ab
            var a = currentPoint.GetPos2D_XZ();

            var b = nextPoint.GetPos2D_XZ();

            //Test if there's a point to the right of ab, if so then it's the new b
            for (var i = 0; i < points.Count; i++)
            {
                //Dont test the point we picked randomly
                if (points[i].Equals(nextPoint)) continue;

                var c = points[i].GetPos2D_XZ();

                //Where is c in relation to a-b
                // < 0 -> to the right
                // = 0 -> on the line
                // > 0 -> to the left
                var relation = Geometry.IsAPointLeftOfVectorOrOnTheLine(a, b, c);

                //Colinear points
                //Cant use exactly 0 because of floating point precision issues
                //This accuracy is smallest possible, if smaller points will be missed if we are testing with a plane
                var accuracy = 0.00001f;

                if (relation < accuracy && relation > -accuracy)
                {
                    colinearPoints.Add(points[i]);
                }
                //To the right = better point, so pick it as next point on the convex hull
                else if (relation < 0f)
                {
                    nextPoint = points[i];

                    b = nextPoint.GetPos2D_XZ();

                    //Clear colinear points
                    colinearPoints.Clear();
                }
                //To the left = worse point so do nothing
            }


            //If we have colinear points
            if (colinearPoints.Count > 0)
            {
                colinearPoints.Add(nextPoint);

                //Sort this list, so we can add the colinear points in correct order
                colinearPoints = colinearPoints.OrderBy(n => Vector3.SqrMagnitude(n.position - currentPoint.position))
                    .ToList();

                convexHull.AddRange(colinearPoints);

                currentPoint = colinearPoints[colinearPoints.Count - 1];

                //Remove the points that are now on the convex hull
                for (var i = 0; i < colinearPoints.Count; i++) points.Remove(colinearPoints[i]);

                colinearPoints.Clear();
            }
            else
            {
                convexHull.Add(nextPoint);

                points.Remove(nextPoint);

                currentPoint = nextPoint;
            }

            //Have we found the first point on the hull? If so we have completed the hull
            if (currentPoint.Equals(convexHull[0]))
            {
                //Then remove it because it is the same as the first point, and we want a convex hull with no duplicates
                convexHull.RemoveAt(convexHull.Count - 1);

                break;
            }

            counter += 1;
        }

        return convexHull;
    }
}

public static class ConcaveTriangulator
{
}