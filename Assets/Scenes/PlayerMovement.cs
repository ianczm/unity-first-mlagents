using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;

public class PlayerMovement : Agent {

    public Rigidbody rb;
    public float initialForce = 2000f;
    public float maxSpeed = 20f;
    public float sidewardsForce = 500f;
    public float hitRewardLoss = 20f;
    public float maxSpawnSidewardsDeviation = 6f;
    public float spawnDelay = 3f;

    private const int STRAIGHT = 0;
    private const int LEFT = 1;
    private const int RIGHT = 2;

    private bool isMoving = true;
    private Vector3 startingPosition;
    private Quaternion startingRotation;

    // Test
    void FixedUpdate() {
        if (rb.velocity.z < maxSpeed && isMoving) {
            rb.AddForce(0, 0, initialForce * Time.fixedDeltaTime);
        }
    }

    public override void Initialize() {
        startingPosition = transform.position;
        startingRotation = transform.rotation;
    }

    public override void OnEpisodeBegin() {
        Vector3 newPosition = startingPosition + Random.Range(-maxSpawnSidewardsDeviation, maxSpawnSidewardsDeviation) * Vector3.right;
        transform.SetPositionAndRotation(newPosition, startingRotation);
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        isMoving = true;
    }

    public override void Heuristic(in ActionBuffers actionsOut) {

        ActionSegment<int> discreteActions = actionsOut.DiscreteActions;

        if (Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D)) {
            discreteActions[0] = LEFT;
        } else if (!Input.GetKey(KeyCode.A) && Input.GetKey(KeyCode.D)) {
            discreteActions[0] = RIGHT;
        } else {
            discreteActions[0] = STRAIGHT;
        }
        
    }

    public override void OnActionReceived(ActionBuffers actions) {

        ActionSegment<int> discreteActions = actions.DiscreteActions;

        if (discreteActions[0] == LEFT && rb.velocity.x > -maxSpeed && isMoving) {
            rb.AddForce(sidewardsForce * Time.fixedDeltaTime * Vector3.left, ForceMode.VelocityChange);
        } else if (discreteActions[0] == RIGHT && rb.velocity.x < maxSpeed && isMoving) {
            rb.AddForce(sidewardsForce * Time.fixedDeltaTime * Vector3.right, ForceMode.VelocityChange);
        }

    }

    public void Update() {
        Debug.Log(GetCumulativeReward());
        if (isMoving) {
            AddReward(rb.velocity.z * Time.deltaTime);
        }
    }

    public void OnCollisionEnter(Collision collision) {
        isMoving = false;
        if (collision.collider.tag == "Obstacle" || collision.collider.tag == "EdgeGuard") {
            AddReward(-hitRewardLoss);
        } else if (collision.collider.tag == "Finish") {
            AddReward(hitRewardLoss);
        }
        Invoke("EndEpisode", spawnDelay);
    }

}
