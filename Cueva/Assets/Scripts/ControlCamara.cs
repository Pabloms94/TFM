using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlCamara : MonoBehaviour
{
    public Transform target;
    
    [System.Serializable]
    public class PositionSettings
    {
        public Vector3 targetPosOffset = new Vector3(0.0f, 1.6f, 0.0f);
        public float lookSmooth = 100.0f;
        public float distanceFromTarget = -2.0f;
        public float zoomSmooth = 100.0f;
        public float maxZoom = -0.5f;
        public float minZoom = -4.0f;
        public bool smoothFollow = true;
        public float smooth = 0.5f;

        [HideInInspector]
        public float newDistance = -1.0f;
        [HideInInspector]
        public float adjustmentDistance = -1.0f;
    }
    
    [System.Serializable]
    public class OrbitSettings
    {
        public float xRotation = -20.0f;
        public float yRotation = -180.0f;
        public float maxXRotation = 25.0f;
        public float minXRotation = -85.0f;
        public float vOrbitSmooth = 150.0f;
        public float hOrbitSmooth = 150.0f;
    }

    [System.Serializable]
    public class InputSettings
    {
        public string ORBIT_HORIZONTAL_SNAP = "OrbitHorizontalSnap";
        public string ORBIT_HORIZONTAL = "OrbitHorizontal";
        public string ORBIT_VERTICAL = "OrbitVertical";
        public string ZOOM = "Mouse ScrollWheel";
    }

    [System.Serializable]
    public class DebugSettings
    {
        public bool drawDesiredCollisionLines = true;
        public bool drawAdjustedCollisionLines = true;
    }

    public PositionSettings position = new PositionSettings();
    public OrbitSettings orbit = new OrbitSettings();
    public InputSettings input = new InputSettings();
    public DebugSettings debug = new DebugSettings();
    public CollisionHandler collision = new CollisionHandler();

    Vector3 targetPos = Vector3.zero;
    Vector3 destination = Vector3.zero;
    Vector3 adjustedDestination = Vector3.zero;
    Vector3 camVel = Vector3.zero;
    CharacterController charController;
    float vOrbitInput;
    float hOrbitInput;
    float zoomInput;
    float hOrbitSnapInput;
    
	void Start ()
    {
        SetCameraTarget(target);

        vOrbitInput = hOrbitInput = zoomInput = hOrbitSnapInput  = 0.0f;

        MoveToTarget();

        collision.Initialize(Camera.main);
        collision.UpdateCameraClipPoints(transform.position, transform.rotation, ref collision.adjustedCameraClipPoints);
        collision.UpdateCameraClipPoints(destination, transform.rotation, ref collision.desiredCameraClipPoints); 
    }
    
    void SetCameraTarget(Transform t)
    {
        target = t;
        charController = target.GetComponent<CharacterController>();
    }
    
    void GetInput()
    {
        vOrbitInput = Input.GetAxis(input.ORBIT_VERTICAL);
        hOrbitInput = Input.GetAxis(input.ORBIT_HORIZONTAL);
        hOrbitSnapInput = Input.GetAxis(input.ORBIT_HORIZONTAL_SNAP);
        zoomInput = Input.GetAxis(input.ZOOM);
    }
    
    void Update()
    {
        GetInput();
        ZoomInOnTarget();
    }
	
	void FixedUpdate ()
    {
        MoveToTarget();
        LookAtTarget();
        OrbitTarget();

        collision.UpdateCameraClipPoints(transform.position, transform.rotation, ref collision.adjustedCameraClipPoints);
        collision.UpdateCameraClipPoints(destination, transform.rotation, ref collision.desiredCameraClipPoints);

        for (int i = 0; i < 5; i++)
        {
            if (debug.drawDesiredCollisionLines)
            {
                Debug.DrawLine(targetPos, collision.desiredCameraClipPoints[i], Color.white);
            }
            if (debug.drawAdjustedCollisionLines)
            {
                Debug.DrawLine(targetPos, collision.adjustedCameraClipPoints[i], Color.green);
            }
        }

        collision.CheckColliding(targetPos);
        position.adjustmentDistance = collision.GetAdjustedDistanceWithRayFrom(targetPos);
    }

    void MoveToTarget()
    {
        targetPos = target.position + position.targetPosOffset;
        destination = Quaternion.Euler(orbit.xRotation, orbit.yRotation + target.eulerAngles.y, 0.0f) * -Vector3.forward * position.distanceFromTarget;
        destination += targetPos;

        if (collision.colliding)
        {
            adjustedDestination = Quaternion.Euler(orbit.xRotation, orbit.yRotation + target.eulerAngles.y, 0.0f) * Vector3.forward * position.adjustmentDistance;
            adjustedDestination += targetPos;

            if (position.smoothFollow)
            {
                transform.position = Vector3.SmoothDamp(transform.position, adjustedDestination, ref camVel, position.smooth);
            }
            else
                transform.position = adjustedDestination;
        }
        else
        {
            if (position.smoothFollow)
            {
                transform.position = Vector3.SmoothDamp(transform.position, destination, ref camVel, position.smooth);
            }
            else
                transform.position = destination;
        }
    }

    void LookAtTarget()
    {
        Quaternion targetRotation = Quaternion.LookRotation(targetPos - transform.position);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, position.lookSmooth * Time.deltaTime);
    }

    void OrbitTarget()
    {
        if (hOrbitSnapInput > 0)
        {
            orbit.xRotation = -30.0f;
            orbit.yRotation = -180.0f;
        }

        orbit.xRotation += -vOrbitInput * orbit.vOrbitSmooth * Time.deltaTime;
        orbit.yRotation += -hOrbitInput * orbit.hOrbitSmooth * Time.deltaTime;

        if (orbit.xRotation > orbit.maxXRotation)
        {
            orbit.xRotation = orbit.maxXRotation;
        }
        if (orbit.xRotation < orbit.minXRotation)
        {
            orbit.xRotation = orbit.minXRotation;
        }
    }

    void ZoomInOnTarget()
    {
        position.distanceFromTarget += zoomInput * position.zoomSmooth * Time.deltaTime;
        
        if (position.distanceFromTarget > position.maxZoom)
        {
            position.distanceFromTarget = position.maxZoom;
        }
        if (position.distanceFromTarget < position.minZoom)
        {
            position.distanceFromTarget = position.minZoom;
        }
    }

    [System.Serializable]
    public class CollisionHandler
    {
        public LayerMask collisionLayer;

        [HideInInspector]
        public bool colliding = false;
        [HideInInspector]
        public Vector3[] adjustedCameraClipPoints;
        [HideInInspector]
        public Vector3[] desiredCameraClipPoints;

        Camera camera;

        public void Initialize(Camera cam)
        {
            camera = cam;
            adjustedCameraClipPoints = new Vector3[5];
            desiredCameraClipPoints = new Vector3[5];
        }

        public void UpdateCameraClipPoints(Vector3 cameraPosition, Quaternion atRotation, ref Vector3[] intoArray)
        {
            if (!camera)
                return;

            //clear the contents of intoarray
            intoArray = new Vector3[5];
            float z = camera.nearClipPlane;
            float x = Mathf.Tan(camera.fieldOfView / 3.41f) * z;
            float y = x / camera.aspect;

            //top left
            intoArray[0] = (atRotation * new Vector3(-x, y, z)) + cameraPosition; //added and rotated the point relative to the camera
            //top right
            intoArray[1] = (atRotation * new Vector3(x, y, z)) + cameraPosition; //added and rotated the point relative to the camera
            //bottom left
            intoArray[2] = (atRotation * new Vector3(-x, -y, z)) + cameraPosition; //added and rotated the point relative to the camera
            //bottom right
            intoArray[3] = (atRotation * new Vector3(x, -y, z)) + cameraPosition; //added and rotated the point relative to the camera
            //camera position
            intoArray[4] = cameraPosition - camera.transform.forward;

        }

        bool CollisionDetectedAtClipPoints(Vector3[] clipPoints, Vector3 fromPosition)
        {
            for (int i = 0; i < clipPoints.Length; i++)
            {
                Ray ray = new Ray (fromPosition, clipPoints[i] - fromPosition);
                float distance = Vector3.Distance(clipPoints[i], fromPosition);
                if (Physics.Raycast(ray, distance, collisionLayer))
                {
                    return true;
                }
            }
            return false;
        }

        public float GetAdjustedDistanceWithRayFrom(Vector3 from)
        {
            float distance = -1;

            for (int i = 0; i < desiredCameraClipPoints.Length; i++)
            {
                Ray ray = new Ray (from, desiredCameraClipPoints[i] - from);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    if (distance == -1)
                        distance = hit.distance;
                    else
                    {
                        if (hit.distance < distance)
                            distance = hit.distance;
                    }
                }
            }

            if (distance == -1)
                return 0;
            else
                return distance;
        }

        public void CheckColliding(Vector3 targetPosition)
        {
            if (CollisionDetectedAtClipPoints(desiredCameraClipPoints, targetPosition))
            {
                colliding =  true;
            }
            else
            {
                colliding = false;
            }
        }
    }
}
