using MLAgents;
using UnityEngine;

public class CarAgent : Agent
{
    public int sensorLength = 15;

    Dot_Truck_Controller truckController;
    Rigidbody truckRigidboddy;
    CarAcademy carAcademy;
    public Transform restartTransform;
    Vector3 lastPos;
    RaycastHit hit;

    public Vector3 forward;
    public Vector3 right;
    public Vector3 left;
    public Vector3 rightMiddle;
    public Vector3 leftMiddle;

    const float maxSpeed = 6;

    public float timeFromStart;
    public float lastCollisionTime;
    public float lastAgentActionTime;

    void Awake()
    {
        truckController = gameObject.GetComponent<Dot_Truck_Controller>();
        carAcademy = FindObjectOfType<CarAcademy>();
        truckRigidboddy = gameObject.GetComponent<Rigidbody>();
    }

    private void Update()
    {
        timeFromStart += Time.deltaTime;
        if (timeFromStart > 4 && truckRigidboddy.velocity.magnitude < 0.0025f)
        {
            if (Time.time - lastCollisionTime > 1)
            {
                Done();
                lastCollisionTime = Time.time;
            }
        }
    }

    public override void InitializeAgent()
    {
        timeFromStart = 0;
        lastCollisionTime = Time.time;
        lastPos = transform.position;
        lastAgentActionTime = Time.time;
    }

    public override void CollectObservations()
    {
        AddVectorObs(GetSensor(left));
        AddVectorObs(GetSensor(forward));
        AddVectorObs(GetSensor(right));
        AddVectorObs(GetSensor(leftMiddle));
        AddVectorObs(GetSensor(rightMiddle));
        AddVectorObs(truckRigidboddy.velocity.magnitude/maxSpeed);
    }


    public override void AgentAction(float[] vectorAction)
    {
        truckController.agentMotor = vectorAction[0];
        truckController.agentSteering = vectorAction[1];
        float time = (Time.time - lastAgentActionTime) + 1;
        AddReward(Vector3.Distance(transform.position, lastPos) / time);
        lastAgentActionTime = Time.time;
        lastPos = transform.position;
    }


    public override void AgentReset()
    {
        transform.position = restartTransform.position;
        transform.rotation = restartTransform.rotation;
        truckRigidboddy.velocity = Vector3.zero;
        truckRigidboddy.angularVelocity = Vector3.zero;
        lastPos = transform.position;
        lastAgentActionTime = Time.time;
        timeFromStart = 0;
        lastCollisionTime = Time.time;
    }


    float GetSensor(Vector3 direction)
    {
        Vector3 fwd = transform.TransformDirection(direction);
        if (Physics.Raycast(transform.position, fwd, out hit))
        {
            if (hit.distance < sensorLength)
            {
                Debug.DrawRay(transform.position + (transform.forward * 2), fwd * sensorLength, Color.red, 0, true);
                return 1f - hit.distance / sensorLength;
            }
            else
            {
                Debug.DrawRay(transform.position + (transform.forward * 2), fwd * sensorLength, Color.green, 0, true);
            }
        }
        else
            Debug.DrawRay(transform.position + (transform.forward * 2), fwd * sensorLength, Color.green, 0, true);
        return 0;
    }


    void OnCollisionEnter(Collision col)
    {
        if(Time.time - lastCollisionTime > 1)
        {
            Done();
            lastCollisionTime = Time.time;
        }
    }


    public override float[] Heuristic()
    {
         if (Input.GetKey(KeyCode.W))
         {
             return new float[] { 1f, 0f };
         }
         if (Input.GetKey(KeyCode.D))
         {
             return new float[] { 1, 1f };
         }
         if (Input.GetKey(KeyCode.A))
         {
             return new float[] { 1, -1f };
         }
         if (Input.GetKey(KeyCode.S))
         {
             return new float[] { -1, 0f };
         }
        return new float[] { 0, 0.0f };
    }
}
