using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


[RequireComponent(typeof(Camera))]
public class PlayerCamera : MonoBehaviour
{
    [SerializeField]
    // camera will follow this object
    private Transform target;

    [Header("Input Settings")] public float dragThreshold = 5f;
    public float touchSpeedFactor = 0.01f;
    public float mouseSpeedFactor = 1f;
    public float zoomStep = 5f;

    public float touchZoomSpeedFactor = 0.005f;

    [Header("Collider Settings")] public LayerMask clampLayer;
    public float colliderGap = 0.25f;


    [Header("Camera Settings")] public Vector3 localOffset;
    public Vector3 offset;
    public Vector3 rotationoffset;

    public float distance = 5.0f;
    public float xSpeed = 120.0f;
    public float ySpeed = 120.0f;
    public float yMinLimit = -20f;
    public float yMaxLimit = 80f;
    public float distanceMin = .5f;
    public float distanceMax = 15f;
    public float smoothTime = 2f;


    float rotationYAxis = 0.0f;
    float rotationXAxis = 0.0f;
    float velocityX = 0.0f;
    float velocityY = 0.0f;


    [Header("Debug")] public float deltaTouchMove = 0f;
    public bool rotateBlocked = true;
    public bool singleTouching;
    public bool hasTouch;
    public float colliderClampDistance = Mathf.Infinity;
    public bool cameraThroughWalls;


    private List<Touch> rotateTouches = new List<Touch>();
    private HashSet<int> uiFingerIds = new HashSet<int>();

    private int layerUI;
    private RaycastHit[] hits = new RaycastHit[10];

    private float lastDistance;

    void Awake()
    {
        layerUI = LayerMask.NameToLayer("UI");
    }


    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        //Do not remove this, it need to use in LateUpdate(). otherwise use on LateUpdate instead.
        var touches = FilterNonUITouches();


        if (touches.Count == 1 && touches[0].phase == TouchPhase.Began)
        {
            singleTouching = true;
            deltaTouchMove = 0f;
            hasTouch = true;
        }
        else if (Input.GetMouseButtonDown(0) && !IsPositionUIElement(Input.mousePosition))
        {
            singleTouching = true;
            deltaTouchMove = 0f;
            hasTouch = false;
        }

        if (touches.Count == 1 && touches[0].phase == TouchPhase.Ended)
        {
            singleTouching = false;
            hasTouch = false;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            singleTouching = false;
            hasTouch = false;
        }


        if (singleTouching)
        {
            if (hasTouch && touches.Count > 0)
            {
                deltaTouchMove += Mathf.Abs(touches[0].deltaPosition.x + touches[0].deltaPosition.y);
            }
            else
            {
                deltaTouchMove += Mathf.Abs(Input.GetAxis("Mouse X")) + Mathf.Abs(Input.GetAxis("Mouse Y"));
            }

            rotateBlocked = (deltaTouchMove < dragThreshold);
        }

        /*if ((touches.Count == 1 && touches[0].phase != TouchPhase.Moved) || touches.Count == 2)
        {
            rotateBlocked = true;
        }*/
    }

    void FixedUpdate()
    {
        if (target == null)
            return;

        cameraThroughWalls = TestHitColliders(target);
    }

    void LateUpdate()
    {
        if (target == null)
            return;
        //cameraThroughWalls = TestHitColliders(target);

        var touches = rotateTouches;
        //Debug.Log($"Tocuh count : {touches.Count}");


        if (singleTouching && !rotateBlocked)
        {
            float deltaX = hasTouch
                ? touches[0].deltaPosition.x * touchSpeedFactor
                : Input.GetAxis("Mouse X") * mouseSpeedFactor;
            float deltaY = hasTouch
                ? touches[0].deltaPosition.y * touchSpeedFactor
                : Input.GetAxis("Mouse Y") * mouseSpeedFactor;

            /* Calculate distance */
            //velocityX += xSpeed * deltaX* mouseSpeedFactor * distance * 0.02f;
            //velocityY += ySpeed * deltaY* mouseSpeedFactor * distance * 0.02f;

            /* Fixed speed */
            velocityX += xSpeed * deltaX * mouseSpeedFactor * 10f * 0.02f * Time.deltaTime;
            velocityY += ySpeed * deltaY * mouseSpeedFactor * 10f * 0.02f * Time.deltaTime;
        }

        var deltaZoom = 0f;
        deltaZoom = Input.GetAxis("Mouse ScrollWheel");


        if (touches.Count == 2)
        {
            Touch touchZero = touches[0];
            Touch touchOne = touches[1];

            Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

            float prevMagnitude = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float currentMagnitude = (touchZero.position - touchOne.position).magnitude;

            float difference = currentMagnitude - prevMagnitude;

            deltaZoom = difference * touchZoomSpeedFactor;
        }

        SetCameraDistanceByDelta(deltaZoom);

        /* Calculate rotation & position */
        rotationYAxis += velocityX;
        rotationXAxis -= velocityY;
        rotationXAxis = ClampAngle(rotationXAxis, yMinLimit, yMaxLimit);
        Quaternion toRotation = Quaternion.Euler(rotationXAxis, rotationYAxis, 0);
        var rotation_offset = Quaternion.Euler(rotationoffset);
        Quaternion rotation = toRotation * rotation_offset;

        Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
        Vector3 overrideOffset = offset;
        Vector3 overrideLocalOffset = localOffset;


        if (cameraThroughWalls)
        {
            negDistance = new Vector3(0.0f, 0.0f, -colliderClampDistance);
            overrideOffset.z *= 0f;
            overrideLocalOffset.z *= 0f;

            //distance = colliderClampDistance + localOffset.z + colliderGap;

            float deltaDistance = distance - this.lastDistance;
            if (deltaDistance < 0)
            {
                distance = colliderClampDistance + localOffset.z + colliderGap;
                SetCameraDistanceByDelta(deltaZoom);
                negDistance = new Vector3(0.0f, 0.0f, -colliderClampDistance + deltaDistance);
                overrideOffset = offset;
                overrideLocalOffset = localOffset;
            }
        }

        Vector3 position = rotation * negDistance + (target.position + overrideOffset);

        transform.rotation = rotation;
        transform.position = position;

        transform.Translate(overrideLocalOffset, Space.Self);
        velocityX = Mathf.Lerp(velocityX, 0, Time.deltaTime * smoothTime);
        velocityY = Mathf.Lerp(velocityY, 0, Time.deltaTime * smoothTime);


        Vector3 origin = target.position;
        origin.y += offset.y + localOffset.y;
        //Debug.DrawRay(origin, (this.transform.position - origin).normalized * distance, cameraThroughWalls ? Color.red : Color.green);

        this.lastDistance = distance;
    }


    public List<Touch> FilterNonUITouches()
    {
        rotateTouches.Clear();
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch t = Input.GetTouch(i);
            var overUI = IsPositionUIElement(t.position);


            if (t.phase == TouchPhase.Began && overUI)
            {
                uiFingerIds.Add(t.fingerId);
            }

            if (t.phase == TouchPhase.Ended)
            {
                uiFingerIds.Remove(t.fingerId);
            }

            rotateTouches.Add(t);
        }

        rotateTouches.RemoveAll((t) => { return uiFingerIds.Contains(t.fingerId); });
        return rotateTouches;
    }


    private bool TestHitCollider(Transform target)
    {
        Vector3 origin = target.position + offset + localOffset;
        int hitCount = Physics.RaycastNonAlloc(origin, origin - target.transform.position, hits, distance, clampLayer);

        if (hitCount > 0)
        {
            var h = hits[0];
            colliderClampDistance = h.distance - colliderGap;
        }
        else
        {
            colliderClampDistance = Mathf.Infinity;
        }

        return hitCount > 0;
    }

    private bool TestHitColliders(Transform target)
    {
        Vector3 origin = target.position;
        origin.y += offset.y + localOffset.y;

        Vector3 cDist = this.transform.position - origin;
        Vector3 cDir = cDist.normalized;

        RaycastHit[] hits = Physics.RaycastAll(origin, cDir, distance - localOffset.z, clampLayer);
        //Debug.DrawRay(origin, cDir * distance, hits.Length > 0 ? Color.red: Color.green);

        float minDis = Mathf.Infinity;
        if (hits.Length > 0)
        {
            for (int i = 0; i < hits.Length; i++)
            {
                var h = hits[i];


                if (h.distance < minDis)
                    minDis = h.distance;
            }

            colliderClampDistance = minDis - colliderGap;
        }
        else
        {
            colliderClampDistance = Mathf.Infinity;
        }

        return hits.Length > 0;
    }

    public void SetCameraDistanceByValue(float value)
    {
        distance = Mathf.Clamp(value, distanceMin, distanceMax);
    }

    public void SetCameraDistanceByDelta(float deltaDistance)
    {
        //distance = Mathf.Clamp(distance - deltaDistance * 5, distanceMin, Mathf.Min( distanceMax,colliderClampDistance + localOffset.z + colliderGap));
        distance = Mathf.Clamp(distance - deltaDistance * zoomStep, distanceMin, distanceMax);
    }

    public void SetCameraDistance(string distance)
    {
        this.SetCameraDistanceByDelta(float.Parse(distance));
    }


    public void SetMinCameraDistance(float min)
    {
        this.distanceMin = min;
    }

    public void SetMinCameraDistance(string min)
    {
        this.SetMinCameraDistance(float.Parse(min));
    }

    public void SetMaxCameraDistance(float max)
    {
        this.distanceMax = max;
    }


    public float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360F)
            angle += 360F;
        if (angle > 360F)
            angle -= 360F;
        return Mathf.Clamp(angle, min, max);
    }


    public bool IsPositionUIElement(Vector3 pixelCoor)
    {
        return IsPixelOverUIElement(GetEventSystemRaycastResults(pixelCoor));
    }

    private bool IsPixelOverUIElement(List<RaycastResult> eventSystemRaysastResults)
    {
        for (int index = 0; index < eventSystemRaysastResults.Count; index++)
        {
            RaycastResult curRaysastResult = eventSystemRaysastResults[index];
            if (curRaysastResult.gameObject.layer == layerUI)
                return true;
        }

        return false;
    }

    private List<RaycastResult> GetEventSystemRaycastResults(Vector3 pixelCoor)
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = pixelCoor;
        List<RaycastResult> raysastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, raysastResults);
        return raysastResults;
    }

    public Transform Target
    {
        get => this.target;
        set => this.target = value;
    }
}