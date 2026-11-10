using System;
using System.Collections.Generic;
using Godot;

namespace Simulation.Util;


public static class TrackGenerator
{
    public static event Action<Rect2> TrackGenerated;
    private static Random _random = new();

    public readonly struct PhysicsTrackData(List<(Vector2 inPoint, Vector2 outPoint)> checkpoints, List<Vector2> innerWallPoints, List<Vector2> outerWallPoints)
    {
        public List<(Vector2 inPoint, Vector2 outPoint)> Checkpoints { get; } = checkpoints;
        public List<Vector2> InnerWallPoints { get; } = innerWallPoints;
        public List<Vector2> OuterWallPoints { get; } = outerWallPoints;
    }

    public static PhysicsTrackData CreatePhysicsTrack(Curve2D trackCurve, float trackWidth = 150f)
    {
        Vector2[] bakedPoints = trackCurve.GetBakedPoints(); 

        List<Vector2> outerWallPoints = [];
        List<Vector2> innerWallPoints = [];
        List<(Vector2 inPoint, Vector2 outPoint)> checkpointPositions = [];

        for (int i = 0; i < bakedPoints.Length; i++)
        {
            Vector2 current = bakedPoints[i];
            Vector2 next = bakedPoints[(i + 1) % bakedPoints.Length];
            Vector2 prev = bakedPoints[(i - 1 + bakedPoints.Length) % bakedPoints.Length];
            Vector2 forward = (next - current).Normalized();
            Vector2 normal = new(-forward.Y, forward.X);

            Vector2 dirToPrev = (prev - current).Normalized();
            Vector2 dirToNext = (next - current).Normalized();
            float dotProduct = dirToPrev.Dot(dirToNext);

            float currentWidth = trackWidth;

            if (dotProduct > -0.7f)
            {
                currentWidth = trackWidth * (0.5f + (1f + dotProduct) * 0.2f);
            }
            

            Vector2 outerPoint = current + normal * (currentWidth / 2f);
            Vector2 innerPoint = current - normal * (currentWidth / 2f);

            outerWallPoints.Add(outerPoint);
            innerWallPoints.Add(innerPoint);

            if (i % 15 == 0 && i > 0 && i < bakedPoints.Length - 15*10) 
            {
                checkpointPositions.Add((innerPoint, outerPoint)); 
            }
        }
        return new(checkpointPositions, innerWallPoints, outerWallPoints);
    }

    public static Curve2D GenerateTrack(Vector2 center, int numPoints = 12, float minRadius = 300f, float maxRadius = 700f, float smoothness = 0.4f)
    {
        Curve2D curve = new();
        Vector2[] basePoints = new Vector2[numPoints];

        float angleStep = Mathf.Tau / numPoints;
        for (var i = 0; i < numPoints; i++)
        {
            float angle = i * angleStep;
            angle += (_random.NextSingle() * 2 - 1) * (angleStep * 0.25f); // Random Noise

            float radius = minRadius + (maxRadius - minRadius) * _random.NextSingle();
            basePoints[i] = center + radius * Vector2.FromAngle(angle);
        }

        for (var i = 0; i < numPoints; i++)
        {
            AddPoint(curve, basePoints, i, numPoints, smoothness);
        }

        if (numPoints > 0)
        {
            AddPoint(curve, basePoints, 0, numPoints, smoothness);
        }
        TrackGenerated?.Invoke(GetRect(curve));
        return curve;
    }

    private static Rect2 GetRect(Curve2D c)
    {
        Vector2 pos = Vector2.Zero;
        Vector2 end = Vector2.Zero;
        foreach (var p in c.GetBakedPoints())
        {
            if(p.X - 150 < pos.X) pos.X = p.X - 150;
            if(p.Y - 150 < pos.Y) pos.Y = p.Y - 150;

            if(p.X + 150 > end.X) end.X = p.X + 150;
            if(p.Y + 150 > end.Y) end.Y = p.Y + 150;
        }
        Vector2 size = (end - pos).Abs();
        return new(pos, size);
    }

    private static void AddPoint(Curve2D c, Vector2[] basePoints, int currentIdx, int numPoints, float smoothness)
    {
        Vector2 pCurrent = basePoints[currentIdx];
        Vector2 pPrevious = basePoints[(currentIdx - 1 + numPoints) % numPoints];
        Vector2 pNext = basePoints[(currentIdx + 1) % numPoints];
        Vector2 tangent = pPrevious.DirectionTo(pNext);

        float distToPrevious = pCurrent.DistanceTo(pPrevious);
        float distToNext = pCurrent.DistanceTo(pNext);

        Vector2 handleIn = -tangent * distToPrevious * smoothness;
        Vector2 handleOut = tangent * distToNext * smoothness;
        c.AddPoint(pCurrent, handleIn, handleOut);
    }
}
