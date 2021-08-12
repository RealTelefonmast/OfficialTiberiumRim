using System.Collections.Generic;
using UnityEngine;

namespace TiberiumRim
{
    public static class Geometry
    {
        //Where is p in relation to a-b
        // < 0 -> to the right
        // = 0 -> on the line
        // > 0 -> to the left
        public static float IsAPointLeftOfVectorOrOnTheLine(Vector2 a, Vector2 b, Vector2 p)
        {
            float determinant = (a.x - p.x) * (b.y - p.y) - (a.y - p.y) * (b.x - p.x);

            return determinant;
        }

        //Clamp list indices
        //Will even work if index is larger/smaller than listSize, so can loop multiple times
        public static int ClampListIndex(int index, int listSize)
        {
            index = ((index % listSize) + listSize) % listSize;

            return index;
        }

        //Is a triangle in 2d space oriented clockwise or counter-clockwise
        //https://math.stackexchange.com/questions/1324179/how-to-tell-if-3-connected-points-are-connected-clockwise-or-counter-clockwise
        //https://en.wikipedia.org/wiki/Curve_orientation
        public static bool IsTriangleOrientedClockwise(Vector2 p1, Vector2 p2, Vector2 p3)
        {
            bool isClockWise = true;

            float determinant = p1.x * p2.y + p3.x * p1.y + p2.x * p3.y - p1.x * p3.y - p3.x * p2.y - p2.x * p1.y;

            if (determinant > 0f)
            {
                isClockWise = false;
            }

            return isClockWise;
        }

        //From http://totologic.blogspot.se/2014/01/accurate-point-in-triangle-test.html
        //p is the testpoint, and the other points are corners in the triangle
        public static bool IsPointInTriangle(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p)
        {
            bool isWithinTriangle = false;

            //Based on Barycentric coordinates
            float denominator = ((p2.y - p3.y) * (p1.x - p3.x) + (p3.x - p2.x) * (p1.y - p3.y));

            float a = ((p2.y - p3.y) * (p.x - p3.x) + (p3.x - p2.x) * (p.y - p3.y)) / denominator;
            float b = ((p3.y - p1.y) * (p.x - p3.x) + (p1.x - p3.x) * (p.y - p3.y)) / denominator;
            float c = 1 - a - b;

            //The point is within the triangle or on the border if 0 <= a <= 1 and 0 <= b <= 1 and 0 <= c <= 1
            //if (a >= 0f && a <= 1f && b >= 0f && b <= 1f && c >= 0f && c <= 1f)
            //{
            //    isWithinTriangle = true;
            //}

            //The point is within the triangle
            if (a > 0f && a < 1f && b > 0f && b < 1f && c > 0f && c < 1f)
            {
                isWithinTriangle = true;
            }

            return isWithinTriangle;
        }

        //http://thirdpartyninjas.com/blog/2008/10/07/line-segment-intersection/
        public static bool AreLinesIntersecting(Vector2 l1_p1, Vector2 l1_p2, Vector2 l2_p1, Vector2 l2_p2, bool shouldIncludeEndPoints)
        {
            bool isIntersecting = false;

            float denominator = (l2_p2.y - l2_p1.y) * (l1_p2.x - l1_p1.x) - (l2_p2.x - l2_p1.x) * (l1_p2.y - l1_p1.y);

            //Make sure the denominator is > 0, if not the lines are parallel
            if (denominator != 0f)
            {
                float u_a = ((l2_p2.x - l2_p1.x) * (l1_p1.y - l2_p1.y) - (l2_p2.y - l2_p1.y) * (l1_p1.x - l2_p1.x)) / denominator;
                float u_b = ((l1_p2.x - l1_p1.x) * (l1_p1.y - l2_p1.y) - (l1_p2.y - l1_p1.y) * (l1_p1.x - l2_p1.x)) / denominator;

                //Are the line segments intersecting if the end points are the same
                if (shouldIncludeEndPoints)
                {
                    //Is intersecting if u_a and u_b are between 0 and 1 or exactly 0 or 1
                    if (u_a >= 0f && u_a <= 1f && u_b >= 0f && u_b <= 1f)
                    {
                        isIntersecting = true;
                    }
                }
                else
                {
                    //Is intersecting if u_a and u_b are between 0 and 1
                    if (u_a > 0f && u_a < 1f && u_b > 0f && u_b < 1f)
                    {
                        isIntersecting = true;
                    }
                }

            }

            return isIntersecting;
        }

        //Whats the coordinate of an intersection point between two lines in 2d space if we know they are intersecting
        //http://thirdpartyninjas.com/blog/2008/10/07/line-segment-intersection/
        public static Vector2 GetLineLineIntersectionPoint(Vector2 l1_p1, Vector2 l1_p2, Vector2 l2_p1, Vector2 l2_p2)
        {
            float denominator = (l2_p2.y - l2_p1.y) * (l1_p2.x - l1_p1.x) - (l2_p2.x - l2_p1.x) * (l1_p2.y - l1_p1.y);

            float u_a = ((l2_p2.x - l2_p1.x) * (l1_p1.y - l2_p1.y) - (l2_p2.y - l2_p1.y) * (l1_p1.x - l2_p1.x)) / denominator;

            Vector2 intersectionPoint = l1_p1 + u_a * (l1_p2 - l1_p1);

            return intersectionPoint;
        }

        //Is a point to the left, to the right, or on a plane
        //https://gamedevelopment.tutsplus.com/tutorials/understanding-sutherland-hodgman-clipping-for-physics-engines--gamedev-11917
        //Notice that the plane normal doesnt have to be normalized
        public static float DistanceFromPointToPlane(Vector3 planeNormal, Vector3 planePos, Vector3 pointPos)
        {
            //Positive distance denotes that the point p is on the front side of the plane 
            //Negative means it's on the back side
            float distance = Vector3.Dot(planeNormal, pointPos - planePos);

            return distance;
        }

        //Get the coordinate if we know a ray-plane is intersecting
        public static Vector3 GetRayPlaneIntersectionCoordinate(Vector3 planePos, Vector3 planeNormal, Vector3 rayStart, Vector3 rayDir)
        {
            float denominator = Vector3.Dot(-planeNormal, rayDir);

            Vector3 vecBetween = planePos - rayStart;

            float t = Vector3.Dot(vecBetween, -planeNormal) / denominator;

            Vector3 intersectionPoint = rayStart + rayDir * t;

            return intersectionPoint;
        }

        //Is a line-plane intersecting?
        public static bool AreLinePlaneIntersecting(Vector3 planeNormal, Vector3 planePos, Vector3 linePos1, Vector3 linePos2)
        {
            bool areIntersecting = false;

            Vector3 lineDir = (linePos1 - linePos2).normalized;

            float denominator = Vector3.Dot(-planeNormal, lineDir);

            //No intersection if the line and plane are parallell
            if (denominator > 0.000001f || denominator < -0.000001f)
            {
                Vector3 vecBetween = planePos - linePos1;

                float t = Vector3.Dot(vecBetween, -planeNormal) / denominator;

                Vector3 intersectionPoint = linePos1 + lineDir * t;

                if (IsPointBetweenPoints(linePos1, linePos2, intersectionPoint))
                {
                    areIntersecting = true;
                }
            }

            return areIntersecting;
        }

        //Is a point c between point a and b (we assume all 3 are on the same line)
        public static bool IsPointBetweenPoints(Vector3 a, Vector3 b, Vector3 c)
        {
            bool isBetween = false;

            //Entire line segment
            Vector3 ab = b - a;
            //The intersection and the first point
            Vector3 ac = c - a;

            //Need to check 2 things: 
            //1. If the vectors are pointing in the same direction = if the dot product is positive
            //2. If the length of the vector between the intersection and the first point is smaller than the entire line
            if (Vector3.Dot(ab, ac) > 0f && ab.sqrMagnitude >= ac.sqrMagnitude)
            {
                isBetween = true;
            }

            return isBetween;
        }

        //We know a line plane is intersecting and now we want the coordinate of intersection
        public static Vector3 GetLinePlaneIntersectionCoordinate(Vector3 planeNormal, Vector3 planePos, Vector3 linePos1, Vector3 linePos2)
        {
            Vector3 vecBetween = planePos - linePos1;

            Vector3 lineDir = (linePos1 - linePos2).normalized;

            float denominator = Vector3.Dot(-planeNormal, lineDir);

            float t = Vector3.Dot(vecBetween, -planeNormal) / denominator;

            Vector3 intersectionPoint = linePos1 + lineDir * t;

            return intersectionPoint;
        }

        //The list describing the polygon has to be sorted either clockwise or counter-clockwise because we have to identify its edges
        public static bool IsPointInPolygon(List<Vector2> polygonPoints, Vector2 point)
        {
            //Step 1. Find a point outside of the polygon
            //Pick a point with a x position larger than the polygons max x position, which is always outside
            Vector2 maxXPosVertex = polygonPoints[0];

            for (int i = 1; i < polygonPoints.Count; i++)
            {
                if (polygonPoints[i].x > maxXPosVertex.x)
                {
                    maxXPosVertex = polygonPoints[i];
                }
            }

            //The point should be outside so just pick a number to make it outside
            Vector2 pointOutside = maxXPosVertex + new Vector2(10f, 0f);

            //Step 2. Create an edge between the point we want to test with the point thats outside
            Vector2 l1_p1 = point;
            Vector2 l1_p2 = pointOutside;

            //Step 3. Find out how many edges of the polygon this edge is intersecting
            int numberOfIntersections = 0;

            for (int i = 0; i < polygonPoints.Count; i++)
            {
                //Line 2
                Vector2 l2_p1 = polygonPoints[i];

                int iPlusOne = ClampListIndex(i + 1, polygonPoints.Count);

                Vector2 l2_p2 = polygonPoints[iPlusOne];

                //Are the lines intersecting?
                if (AreLinesIntersecting(l1_p1, l1_p2, l2_p1, l2_p2, true))
                {
                    numberOfIntersections += 1;
                }
            }

            //Step 4. Is the point inside or outside?
            bool isInside = true;

            //The point is outside the polygon if number of intersections is even or 0
            if (numberOfIntersections == 0 || numberOfIntersections % 2 == 0)
            {
                isInside = false;
            }

            return isInside;
        }
    }
}
