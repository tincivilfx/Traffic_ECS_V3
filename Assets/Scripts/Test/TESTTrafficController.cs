using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CivilFX.TrafficV3;
using Unity.Mathematics;

public class TESTTrafficController : MonoBehaviour
{
    public TrafficPath path;
    public Transform vehicle;
    public float speed;
    public float kSlowingDistanceMeters = 45.0f;
    public float kMaxSpeedMetersPerSecond = 10.0f;

    private SplineBuilder spline;
    private float t = 0;

    private Vector3 steering;
    private Vector3 heading;
    private Vector3 velocity;
    

    // Start is called before the first frame update
    void Start()
    {
        spline = path.GetSplineBuilder();
        vehicle.position = spline.GetPointOnPath(0.0f);
        heading = spline.GetTangent(0.0f);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        t += Time.fixedDeltaTime * speed;
        if (t > 1f) {
            vehicle.position = spline.GetPointOnPath(0.0f);
            heading = spline.GetTangent(0.0f);
            t = 0;
            return;
        }

        Vector3 targetPosition = spline.GetPointOnPath(t);      
        steering = GetSteering(targetPosition, vehicle.position, velocity, 2);

        Vector3 targetHeading = math.normalizesafe(steering);
        heading = heading + (targetHeading * Time.fixedDeltaTime);
        heading = math.normalizesafe(heading);
        velocity = heading * 10f; //idealSpeed

        //set position
        vehicle.position = vehicle.position + velocity * Time.fixedDeltaTime;

        //set rotation
        var origent = Quaternion.LookRotation(heading, new Vector3(0f, 1f, 0f));
        vehicle.rotation = origent;
    }

    private Vector3 GetSteering(Vector3 target, Vector3 curr, Vector3 velocity, float speedMult)
    {
        Vector3 targetOffset = target - curr;
        float distance = math.length(targetOffset);
        float rampedSpeed = kMaxSpeedMetersPerSecond * speedMult * (distance / kSlowingDistanceMeters);
        float clippedSpeed = math.min(rampedSpeed, kMaxSpeedMetersPerSecond * speedMult);

        // Compute velocity based on target position
        Vector3 desiredVelocity = targetOffset * (clippedSpeed / distance);
        Vector3 steering = desiredVelocity - velocity;

        if (math.lengthsq(steering) < 0.5f)
            return default(Vector3);
        return steering;
    }
}
