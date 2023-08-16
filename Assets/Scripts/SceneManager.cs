using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SceneManager : MonoBehaviour
{
    public enum Mode
    {
        Feather,
        Plastic,
        ShortestSmash,
        FeatherVsPlastic
    }

    public enum ShuttleType
    {
        Feather,
        Plastic
    }

    public GameObject hitMarker;
    public GameObject shuttleHitHeightMarker;
    public GameObject initialTargetMarker;
    public GameObject courtHitMarker;
    public GameObject trajectoryHoverMarker;

    public LineRenderer shuttleHeightLine;
    public LineRenderer shuttleFlightStraightLine;
    public LineRenderer featherShuttleTrajectory;
    public LineRenderer plasticShuttleTrajectory;

    public Slider heightSlider;
    public Slider speedSlider;
    public Slider angleSlider;
    public Slider horizontalAngleSlider;

    public Text heightText;
    public Text speedText;
    public Text angleText;
    public Text horizontalAngleText;
    public Text timeOfFlightText;

    public ToggleGroup modeToggleGroup;

    public GameObject shuttleSpeedTooltip;
    public Text shuttleSpeedTooltipText;

    public Canvas uiParentCanvas;

    Vector3 courtBasePoint;
    Vector3 shuttleHitPoint;

    public float[] plasticShuttleSpeeds;
    public Vector3[] plasticTrajectoryPoints;
    public float[] featherShuttleSpeeds;
    public Vector3[] featherTrajectoryPoints;

    public Mode simMode;

    public GameObject aboutPanel;

    Gradient invalidPathGradient;
    Gradient validPathGradient;

    // Start is called before the first frame update
    void Start()
    {
        heightText.text = heightSlider.value.ToString("F2") + " meters";
        speedText.text = speedSlider.value.ToString("F1") + " m/s";
        angleText.text = angleSlider.value.ToString("F1") + "°";
        simMode = Mode.Plastic;

        invalidPathGradient = new Gradient();
        GradientColorKey[] colorKey = new GradientColorKey[1];
        GradientAlphaKey[] alphaKey = new GradientAlphaKey[1];
        colorKey[0].color = Color.red;
        colorKey[0].time = 0.0f;
        alphaKey[0].alpha = 1.0f;
        alphaKey[0].time = 0.0f;
        invalidPathGradient.SetKeys(colorKey, alphaKey);
        invalidPathGradient.mode = GradientMode.Blend;

        validPathGradient = new Gradient();
        colorKey = new GradientColorKey[1];
        alphaKey = new GradientAlphaKey[1];
        colorKey[0].color = Color.red;
        colorKey[0].time = 0.0f;
        alphaKey[0].alpha = 1.0f;
        alphaKey[0].time = 0.0f;
        validPathGradient.SetKeys(colorKey, alphaKey);
        validPathGradient.mode = GradientMode.Fixed;
    }

    // Update is called once per frame
    void Update()
    {
        // Left-mouse button click, get position on court plane
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("Right mouse click");
            // this creates a horizontal plane passing through this object's center
            Plane plane = new Plane(Vector3.up, transform.position);
            // create a ray from the mousePosition
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            // plane.Raycast returns the distance from the ray start to the hit point
            float distance;
            if (plane.Raycast(ray, out distance))
            {
                // some point of the plane was hit - get its coordinates
                Vector3 hitPoint = ray.GetPoint(distance);
                if (hitPoint.x <= 6.7 && hitPoint.x >= -6.7 && hitPoint.z <= 3.05 && hitPoint.z >= -3.05)
                {
                    // Within court bounds
                    Debug.Log("Raycast hit: " + hitPoint);
                    hitMarker.transform.position = hitPoint;
                    courtBasePoint = hitPoint;

                    UpdateHitMarker();
                    UpdateScene();
                }
                else
                {
                    // Out of court bounds
                    Debug.Log("Raycast out of court bounds");
                }
            }
        }

        // Right-mouse button click, raycast to plane and detect hits
        if (Input.GetMouseButtonDown(1))
        {
            Debug.Log("Left mouse click");

            RaycastHit raycastHit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out raycastHit))
            {
                if (raycastHit.transform != null)
                {
                    Debug.Log("Hit target: " + raycastHit.transform.gameObject.name + " with coords: " + raycastHit.point);
                    Debug.DrawLine(ray.origin, raycastHit.point, Color.red, 10f);
                }
            }
        }
    }

    public void UpdateScene()
    {
        switch (simMode)
        {
            case Mode.Plastic:
                UpdateHitMarker();
                courtHitMarker.SetActive(false);
                plasticShuttleTrajectory.gameObject.SetActive(true);
                HandleTrajectorySimResults(8.29f, ShuttleType.Plastic, plasticShuttleTrajectory, true, false, Color.red);
                break;
            case Mode.Feather:
                UpdateHitMarker();
                courtHitMarker.SetActive(false);
                featherShuttleTrajectory.gameObject.SetActive(true);
                HandleTrajectorySimResults(7.98f, ShuttleType.Feather, featherShuttleTrajectory, true, false, Color.red);
                break;
            case Mode.FeatherVsPlastic:
                UpdateHitMarker();
                courtHitMarker.SetActive(false);
                featherShuttleTrajectory.gameObject.SetActive(true);
                plasticShuttleTrajectory.gameObject.SetActive(true);
                //HandleTrajectorySimResults(8.29f, ShuttleType.Plastic, plasticShuttleTrajectory, false, true, Color.red);
                HandleTrajectorySimResults(8.29f, ShuttleType.Plastic, plasticShuttleTrajectory, false, true, Color.red);
                //HandleTrajectorySimResults(7.98f, featherShuttleTrajectory, false, true, Color.green);
                //HandleTrajectorySimResults(6.86f, featherShuttleTrajectory, false, true, Color.green)
                HandleTrajectorySimResults(7.98f, ShuttleType.Feather, featherShuttleTrajectory, false, true, Color.green);
                break;
            case Mode.ShortestSmash:
                break;
            default:
                break;
        }
    }

    public void HandleTrajectorySimResults(float shuttleVt, ShuttleType shuttleType, LineRenderer lineRenderer, bool speedGradient, bool fixedColor, Color colour)
    {
        SimResult result = SimulateShuttleTrajectory(shuttleVt, speedSlider.value, angleSlider.value, 0, heightSlider.value, shuttleHitPoint);
        Debug.Log("Got sim result - pathPoints: " + result.pathPoints.Length + ", speedPoints: " + result.shuttleSpeeds.Length);

        // Set the width to a large value so the baked mesh + collider are wider, so is easier to hover the line
        lineRenderer.startWidth = 0.25f;
        lineRenderer.endWidth = 0.25f;

        lineRenderer.useWorldSpace = true;
        lineRenderer.positionCount = result.pathPoints.Length;
        lineRenderer.SetPositions(result.pathPoints);

        MeshCollider meshCollider = lineRenderer.gameObject.GetComponent<MeshCollider>();
        Mesh mesh = new Mesh();
        lineRenderer.BakeMesh(mesh, true);
        meshCollider.sharedMesh = mesh;

        float[] shuttleSpeeds;

        if (shuttleType == ShuttleType.Feather)
        {
            featherShuttleSpeeds = result.shuttleSpeeds;
            featherTrajectoryPoints = result.pathPoints;
            shuttleSpeeds = featherShuttleSpeeds;
        }
        else
        {
            plasticShuttleSpeeds = result.shuttleSpeeds;
            plasticTrajectoryPoints = result.pathPoints;
            shuttleSpeeds = plasticShuttleSpeeds;
        }

        // Set the actual width to an aesthetically pleasing value after mesh baking
        lineRenderer.startWidth = 0.04f;
        lineRenderer.endWidth = 0.04f;

        if (speedGradient)
        {
            lineRenderer.colorGradient = GenerateGradient(shuttleSpeeds, 5, result.initialSpeed);
        }
        
        if (result.hitCourt & !result.hitNet)
        {
            //shuttleTrajectory.startColor = Color.green;
            //shuttleTrajectory.endColor = Color.green;
            courtHitMarker.transform.position = result.courtHitPoint;
            courtHitMarker.SetActive(true);            
            Debug.Log("Color at 25%:" + lineRenderer.colorGradient.Evaluate(0.25f));
            if (!speedGradient)
            {
                lineRenderer.colorGradient = validPathGradient;
            }
        }
        else
        {
            lineRenderer.startColor = Color.red;
            lineRenderer.endColor = Color.red;
            lineRenderer.colorGradient = invalidPathGradient;
        }

        if (fixedColor)
        {
            lineRenderer.startColor = colour;
            lineRenderer.endColor = colour;
        }

        timeOfFlightText.text = "Time of Flight: " + result.airTime.ToString("F2") + " s";
    }

    public Gradient GenerateGradient(float[] shuttleSpeeds, float minSpeed, float maxSpeed)
    {
        Gradient newGradient = new Gradient();

        // Determine the range of speed values for colour grading
        float speedRange = maxSpeed - minSpeed;

        // To get the percentage of max speed, we divide a given speed by speedRange. 

        // We want to sample shuttleSpeeds 8 times at equal points, so divide count by 8
        int speedSampleRate = (int)Mathf.Floor((shuttleSpeeds.Length - 1) / 8);
        Debug.Log("Speed sample rate: " + speedSampleRate);

        GradientColorKey[] colorKey = new GradientColorKey[8];
        GradientAlphaKey[] alphaKey = new GradientAlphaKey[8];

        for (int i = 0; i < 8; i++)
        {
            // gives us a value of 1 for a speed close to max, or 0 for close to min
            float speedPercentageOfMax = shuttleSpeeds[(i + 1) * speedSampleRate] / speedRange;
            float red;
            float green;

            red = Mathf.Clamp01(1 - ((speedPercentageOfMax - 0.5f) / 0.5f));
            green = (speedPercentageOfMax / 0.5f);

            colorKey[i].color = new Color(red, green, 0);
            colorKey[i].time = 0.125f * (i + 1);
            alphaKey[i].alpha = 1.0f;
            alphaKey[i].time = 0.125f * (i + 1);
            Debug.Log("Speed% at " + (12.5 * (i + 1)) + "%: " + speedPercentageOfMax + ", color: " + colorKey[i].color);
        }

        newGradient.SetKeys(colorKey, alphaKey);
        newGradient.mode = GradientMode.Blend;
        Debug.Log("Color at 25%:" + newGradient.Evaluate(0.25f));
        return newGradient;
    }

    public void UpdateHitMarker()
    {
        shuttleHitPoint = new Vector3(courtBasePoint.x, courtBasePoint.y + heightSlider.value, courtBasePoint.z);
        shuttleHitHeightMarker.transform.position = shuttleHitPoint;

        shuttleHeightLine.startColor = Color.blue;
        shuttleHeightLine.endColor = Color.blue;
        shuttleHeightLine.startWidth = 0.02f;
        shuttleHeightLine.endWidth = 0.02f;
        shuttleHeightLine.positionCount = 2;
        shuttleHeightLine.useWorldSpace = true;

        shuttleHeightLine.SetPosition(0, courtBasePoint);
        shuttleHeightLine.SetPosition(1, shuttleHitPoint);
    }

    /// <summary>
    /// Intended as a method for automatically finding the steepest possible smash, doesn't currently function with the 'true' shuttle trajectory calculation - instead uses a straight line.
    /// </summary>
    public void SimulateSmashes()
    {
        // First simulate a straight smash, with targetpoint calculated as in the x-plane.
        Vector3 initialTargetPoint;
        int sideMultiplier; // so we can affect targeting based on the side of the court, set to either 1 or -1

        if (courtBasePoint.x >= 0)
        {
            sideMultiplier = -1;
        }
        else
        {
            sideMultiplier = 1;
        }
        initialTargetPoint = new Vector3(sideMultiplier * 0.1f, 0, courtBasePoint.z);
        initialTargetMarker.transform.position = initialTargetPoint;

        Debug.DrawLine(shuttleHitPoint, initialTargetPoint, Color.red, 3f);
        RaycastHit raycastHit;
        if (Physics.Linecast(shuttleHitPoint, initialTargetPoint, out raycastHit))
        {
            if (raycastHit.transform != null)
            {
                Debug.Log("Hit target: " + raycastHit.transform.gameObject.name + " with coords: " + raycastHit.point);
            }
        }

        Vector3 targetPoint = new Vector3(initialTargetPoint.x + (0.01f * sideMultiplier), initialTargetPoint.y, initialTargetPoint.z);
        int count = 0;

        // while casting a line between the shuttle hit point and the target point hits an object... 
        //  ...expecting it to hit the net/netblocking object first, then when it hits the court or a line we break
        while (Physics.Linecast(shuttleHitPoint, targetPoint, out raycastHit))
        {
            count++;
            if (raycastHit.transform != null)
            {
                if (raycastHit.transform.gameObject.tag == "Court")
                {
                    // We've hit over the net, so break the while loop
                    Debug.DrawLine(shuttleHitPoint, targetPoint, Color.blue, 8f);
                    courtHitMarker.transform.position = targetPoint;

                    shuttleFlightStraightLine.gameObject.SetActive(true);
                    courtHitMarker.gameObject.SetActive(true);

                    shuttleFlightStraightLine.startColor = Color.blue;
                    shuttleFlightStraightLine.endColor = Color.blue;
                    shuttleFlightStraightLine.startWidth = 0.02f;
                    shuttleFlightStraightLine.endWidth = 0.02f;
                    shuttleFlightStraightLine.positionCount = 2;
                    shuttleFlightStraightLine.useWorldSpace = true;

                    shuttleFlightStraightLine.SetPosition(0, shuttleHitPoint);
                    shuttleFlightStraightLine.SetPosition(1, targetPoint);
                    break;
                }
                else
                {
                    // We're still hitting the net, so increment x position of target point by 10mm
                    targetPoint.x = targetPoint.x + (0.01f * sideMultiplier);
                    //Debug.DrawLine(shuttleHitPoint, targetPoint, Color.red, 2f);
                }
            }
            if (Mathf.Abs(targetPoint.x) >= 6.7)
            {
                shuttleFlightStraightLine.gameObject.SetActive(false);
                courtHitMarker.gameObject.SetActive(false);
                break;
            }
        }
        Debug.Log("Performed " + count + " while loops while testing rays");

        //SimulateShuttleTrajectory(speedSlider.value, angleSlider.value, 0, heightSlider.value, shuttleHitPoint);
    }

    /// <summary>
    /// Simulates a shuttle trajectory given initial conditions.
    /// </summary>
    /// <param name="initialSpeed">Initial speed of the shuttle (m/s)</param>
    /// <param name="verticalAngle">Upwards/Downwards angle of the shuttle initial velocity (deg)</param>
    /// <param name="horizontalAngle">Left/Right angle of the shuttle initial velocity (deg)</param>
    /// <param name="initialHeight">Initial height of the shuttle (m)</param>
    /// <returns></returns>
    public SimResult SimulateShuttleTrajectory(float shuttleVt, float initialSpeed, float verticalAngle, float horizontalAngle, float initialHeight, Vector3 shuttleHitPosition)
    {
        int sideMultiplier; // so we can affect targeting based on the side of the court, set to either 1 or -1
        if (shuttleHitPosition.x >= 0)
        {
            sideMultiplier = -1;
        }
        else
        {
            sideMultiplier = 1;
        }

        //float terminalVelocity = 6.86f;
        float terminalVelocity = shuttleVt;
        float gravitationalAcceleration = 9.81f;

        // We want to generate an array of (x,y) values which correspond to the shuttle trajectory, and map them to world coordinates (x,y,z).
        // For the first step, we will use time steps to generate coords based on the below equations, so that we don't get weird saturation when the shuttle path is steep.

        float verticalAngleInRads = verticalAngle * Mathf.Deg2Rad;

        float Vxi = initialSpeed * Mathf.Cos(verticalAngleInRads); // Initial horizontal velocity
        float Vyi = initialSpeed * Mathf.Sin(verticalAngleInRads); // Initial vertical velocity

        Debug.Log("Horizontal Speed: " + Vxi + ", Vertical Speed: " + Vyi);

        List<float> xCoords = new List<float>();
        List<float> yCoords = new List<float>();

        List<Vector3> coords = new List<Vector3>();
        List<float> speeds = new List<float>();
        float airTime = 0;

        speeds.Add(initialSpeed); // The first data point in speeds should be the initial speed.

        for (float t = 0; t < 10; t = t + 0.01f)
        {
            float terminalVelocitySquared = Mathf.Pow(terminalVelocity, 2);

            float logClauseX = (Vxi * gravitationalAcceleration * t + terminalVelocitySquared) / terminalVelocitySquared;
            float x = (terminalVelocitySquared / gravitationalAcceleration) * Mathf.Log(logClauseX);
            x = x * sideMultiplier;
            x = x + shuttleHitPosition.x;

            float logClauseY = (Mathf.Sin(((gravitationalAcceleration * t) / terminalVelocity) + Mathf.Atan(terminalVelocity / Vyi))) / Mathf.Sin(Mathf.Atan(terminalVelocity / Vyi));
            float y = (terminalVelocitySquared / gravitationalAcceleration) * Mathf.Log(logClauseY);
            y = y + initialHeight;

            Vector3 coord = new Vector3(x, y, shuttleHitPosition.z);
            coords.Add(coord);

            if (t > 0) // ie, if we're not in the first iteration
            {
                float distance = Vector3.Distance(coords[coords.Count - 1], coords[coords.Count - 2]);
                float speed = distance / 0.01f; // deltaTime is the difference between the current time step, and the last one, ie. 0.01s
                speeds.Add(speed);
            }

            if (y < 0)
            {
                Debug.Log("Path point has crossed y<0 point at " + x + ". Last point: " + coords[coords.Count - 1] + ", secondtolast: " + coords[coords.Count - 2]);
                airTime = t;
                break;
            }
        }
        Debug.Log("Path computation complete, " + coords.Count + " points found.");

        // We can determine whether the shuttle hits the court by linecasting from penultimate to last point in the path. We also get the hitPoint.
        Vector3 courtHitPoint = new Vector3();
        bool hitCourt = false;
        Vector3 secondToLastPos = new Vector3(coords[coords.Count - 2].x, coords[coords.Count - 2].y, coords[coords.Count - 2].z);
        Vector3 lastPos = new Vector3(coords[coords.Count - 1].x, coords[coords.Count - 1].y, coords[coords.Count - 1].z);
        RaycastHit linecastHit;
        if (Physics.Linecast(secondToLastPos, lastPos, out linecastHit))
        {
            if (linecastHit.transform != null)
            {
                if (linecastHit.transform.gameObject.tag == "Court")
                {
                    // We hit a court object! 
                    hitCourt = true;
                    courtHitPoint = linecastHit.point;
                }
            }
        }
        Debug.Log("Shuttle has hit court: " + hitCourt);

        // We should now have a list of coordinates that define the shuttle path.
        bool hitNet = false;
        int lastPointIndex = coords.Count;
        for (int i = 0; i < (coords.Count - 1); i++)
        {
            Vector3 currentPosition = new Vector3(coords[i].x, coords[i].y, coords[i].z);
            Vector3 nextPosition = new Vector3(coords[i + 1].x, coords[i + 1].y, coords[i + 1].z);
            Debug.DrawLine(currentPosition, nextPosition, Color.green, 2f);
                
            if (Physics.Linecast(currentPosition, nextPosition, out linecastHit))
            {
                if (linecastHit.transform != null)
                {
                    // We hit a net object between the current position and the next, so the shuttle path is invalid (wouldn't cross the net)
                    if (linecastHit.transform.gameObject.tag == "Net")
                    {
                        hitNet = true;
                        lastPointIndex = i;
                        break;
                    }
                }
            }
        }
        Debug.Log("Shuttle has hit net: " + hitNet + ", at coords[" + lastPointIndex + "], with coords.Count = " + coords.Count + ", and speeds.Count: " + speeds.Count);

        // We need to turn points list into an array. If we hit the net, it needs to be shorter.
        float[] shuttleSpeeds = new float[lastPointIndex];
        Vector3[] pathPoints = new Vector3[lastPointIndex];
        for (int j = 0; j < lastPointIndex; j++)
        {
            pathPoints[j] = coords[j];
            shuttleSpeeds[j] = speeds[j];
        }

        SimResult result = new SimResult(initialSpeed, hitCourt, hitNet, airTime, courtHitPoint, pathPoints, shuttleSpeeds);

        return result;
    }

    public void SetShuttleSpeedTooltip(bool active, ShuttleType shuttleType)
    {
        shuttleSpeedTooltip.SetActive(active);
        if (active)
        {
            Vector3 mousePos = Input.mousePosition;
            Vector2 mousePosInScreen;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(uiParentCanvas.transform as RectTransform, mousePos, uiParentCanvas.worldCamera, out mousePosInScreen);

            mousePosInScreen.y = mousePosInScreen.y + 20;
            shuttleSpeedTooltip.transform.localPosition = mousePosInScreen;

            RaycastHit raycastHit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out raycastHit))
            {
                if (raycastHit.transform != null && raycastHit.transform.gameObject.tag == "Trajectory")
                {
                    Vector3[] trajectoryPoints;
                    float[] shuttleSpeeds;
                    string type;
                    if (shuttleType == ShuttleType.Feather)
                    {
                        trajectoryPoints = featherTrajectoryPoints;
                        shuttleSpeeds = featherShuttleSpeeds;
                        type = "Feather: ";
                    }
                    else
                    {
                        trajectoryPoints = plasticTrajectoryPoints;
                        shuttleSpeeds = plasticShuttleSpeeds;
                        type = "Plastic: ";
                    }

                    Debug.Log("Hit target: " + raycastHit.transform.gameObject.name + " with coords: " + raycastHit.point);
                    Debug.DrawLine(ray.origin, raycastHit.point, Color.red, 2f);
                    float smallestDistance = 100;
                    int closestIndex = 0;
                    for (int i = 0; i < trajectoryPoints.Length; i++)
                    {
                        // Find closest point in trajectory points array
                        float distance = Vector3.Distance(raycastHit.point, trajectoryPoints[i]);
                        //Debug.Log("distance at index " + i + ": " + );
                        if (distance < smallestDistance)
                        {
                            // current trajectory point is closer than any previous ones
                            smallestDistance = distance;
                            closestIndex = i;
                        }
                    }
                    trajectoryHoverMarker.transform.localPosition = trajectoryPoints[closestIndex];
                    shuttleSpeedTooltipText.text = type + shuttleSpeeds[closestIndex].ToString("F1") + " m/s";
                }
            }
        }
    }

    public void HeightSliderChange()
    {
        //Debug.Log("Slider value: " + heightSlider.value.ToString("F2"));
        heightText.text = heightSlider.value.ToString("F2") + " meters";
        UpdateScene();
    }

    public void SpeedSliderChange()
    {
        //Debug.Log("Slider value: " + speedSlider.value.ToString("F2"));
        speedText.text = speedSlider.value.ToString("F1") + " m/s";
        UpdateScene();
    }

    public void AngleSliderChange()
    {
        Debug.Log("Slider value: " + angleSlider.value.ToString("F1"));
        angleText.text = angleSlider.value.ToString("F1") + "°";
        UpdateScene();
    }

    public void HorizontalAngleSliderChange()
    {
        Debug.Log("Slider value: " + horizontalAngleSlider.value.ToString("F1"));
        horizontalAngleText.text = horizontalAngleSlider.value.ToString("F1") + "°";
        UpdateScene();
    }

    public void ExitButtonClick()
    {
        Application.Quit();
    }

    public void modeToggleChange()
    {
        Toggle activeModeToggle = modeToggleGroup.GetFirstActiveToggle();
        if (activeModeToggle.name == "PlasticShuttleToggle")
        {
            simMode = Mode.Plastic;
        }
        else if (activeModeToggle.name == "FeatherShuttleToggle")
        {
            simMode = Mode.Feather;
        }
        else if (activeModeToggle.name == "FeatherVsPlasticToggle")
        {
            simMode = Mode.FeatherVsPlastic;
        }
        Debug.Log("mode changed to " + simMode);
        featherShuttleTrajectory.gameObject.SetActive(false);
        plasticShuttleTrajectory.gameObject.SetActive(false);
    }

    public void openAboutPanel()
    {
        aboutPanel.SetActive(true);
    }

    public void closeAboutPanel()
    {
        aboutPanel.SetActive(false);
    }
}
