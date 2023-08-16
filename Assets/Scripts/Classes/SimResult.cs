using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimResult
{
    public float initialSpeed;
    public bool hitCourt;
    public bool hitNet;
    public float airTime;
    public Vector3 courtHitPoint;
    public Vector3[] pathPoints;
    public float[] shuttleSpeeds;

    public SimResult(float initialspeed, bool hitcourt, bool hitnet, float airtime, Vector3 courthitpoint, Vector3[] pathpoints, float[] shuttlespeeds)
    {
        initialSpeed = initialspeed;
        hitCourt = hitcourt;
        hitNet = hitnet;
        airTime = airtime;
        courtHitPoint = courthitpoint;
        pathPoints = pathpoints;
        shuttleSpeeds = shuttlespeeds;
    }
}
