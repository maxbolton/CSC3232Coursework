using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;

public class enemyAgentContoller : MonoBehaviour
{
    private float agentHealth;
    private float punchForce = 5f;
    private bool iframe;
    private float speed = 7f;
    private float dampingFactor = 0.98f;

    private float dt = 0;

    private float timer = 0;
    [SerializeField]
    private float actionDelay = 1;


    [SerializeField]
    private Rigidbody agentHips;
    [SerializeField]
    private Rigidbody agentHead;
    [SerializeField]
    private Rigidbody agentLeftForeArm;
    [SerializeField]
    private Rigidbody agentRightForeArm;


    [SerializeField]
    private GameObject playerHips;

    [SerializeField]
    private GameObject navPlane;

    [SerializeField]
    private GameObject animatedAgent;
    private Animator animator;

    [SerializeField]
    private enemyId enemyId;

    // current position of players hips
    private Vector3 playerLocation;
    // measurement of distance to player
    private Vector3 distanceToPlayer;
    // player location normalized to magnitude of 1
    private Vector3 normalizedPlayerVector;


    // the global coordinate of the agents target
    private Vector3 agentTargetLocation;
    // can be described as the vector to apply force in the direction of the target player or a random location in retreat
    private Vector3 agentTargetVector;
    // the target immediately in front of the agent
    private Vector3 agentDestination;
    // agents current location
    private Vector3 agentPosition;

    private Rigidbody[] rigidBodies;
    private ConfigurableJoint[] cJoints;

    [SerializeField]
    private gameContoller gameContoller;

    enum agentState
    {
        inactive,
        travelling,
        fighting
    }

    enum fightState
    {
        attack,
        block,
        none
    }

    enum travelState
    {
        walk,
        retreat,
        none
    }

    agentState state = agentState.travelling;
    fightState fState = fightState.none;
    travelState tState = travelState.walk;

    // Start is called before the first frame update
    void Start()
    {
        animator = animatedAgent.GetComponent<Animator>();
        navPlane = GameObject.Find("navPlane");
        playerHips = GameObject.Find("PlayerHips");

        rigidBodies = getEveryRBody();
        cJoints = getEveryCJoint();

        gameContoller = GameObject.Find("gameController").GetComponent<gameContoller>();
        agentHealth = 20f;
        enemyId.setIsActive(true);

    }

    public float getHealth()
    {
        return agentHealth;
    }

    public void decrementHealth(float velocity)
    {
        agentHealth -= velocity;
    }

    void Update()
    {
        Debug.Log("id: " + enemyId.getId() + ", health:" + agentHealth);
        if (iframe)
        {
        }
        dt = Time.deltaTime;
        timer += dt;


        updatePlayerLocations();

        if (agentHealth <= 0f)
        {
            state = agentState.inactive;

            flop(cJoints);
            animator.SetBool("walking", false);
            enemyId.setIsActive(false);
        }
        else if (agentHealth < 10f || gameContoller.getPlayerHealth() <= 0)
        {
            state = agentState.travelling;
            fState = fightState.none;
            tState = travelState.retreat;
        }
        else if (distanceToPlayer.magnitude < 2.5f)
        {
            if (enemyId.getTargeted() == true)
            {
                fState = fightState.block;
                tState = travelState.none;
                state = agentState.fighting;
            }
            else
            {
                tState = travelState.none;
                fState = fightState.attack;
                state = agentState.fighting;
            }
        }
        else
        {
            state = agentState.travelling;
            tState = travelState.walk;
            fState = fightState.none;
        }


        // Heirarchical State Machine
        if (state == agentState.inactive)
        {
        }
        else if (state == agentState.fighting)
        {
            if (fState == fightState.attack)
            {
                System.Random random = new System.Random();

                int randomAttack = random.Next(1, 4);
                switch (randomAttack)
                {
                    case 1:
                        leftJab();
                        break;
                    case 2:
                        rightJab();
                        break;
                    case 3:
                        headbutt();
                        break;
                    default:
                        Debug.Log("Invalid choice");
                        break;
                }
            }
            else if (fState == fightState.block)
            {
            block();
            }
            animator.SetBool("walking", false);
        }
        else if (state == agentState.travelling)
        {
            if (tState == travelState.retreat)
            {

                //Debug.Log("target: " + agentTargetLocation + ", Current: " + agentPosition);

                // if agent has reached target dest
                if ((agentPosition - agentTargetLocation).magnitude < 4f || agentTargetLocation == new Vector3(0f, 0f, 0f))
                {
                    Debug.Log("called rand");
                    agentTargetLocation = randomDest();
                }

                // constantly re-calc targetVec in case of collision, sliding etc
                agentTargetVector = (agentTargetLocation - agentPosition).normalized;
            }
            else if (tState == travelState.walk)
            {
                agentTargetVector = normalizedPlayerVector;
            }


            // agent will always be moving towards a target within travel state
            applyMovement(agentTargetVector);
            animator.SetBool("walking", true);

        }

        showDestLine();



           
        
    }

    // Pick a random destination on the plane for an enemy to retreat to.
    private Vector3 randomDest()
    {
        Vector3 navPlaneSize = navPlane.GetComponent<BoxCollider>().size;

        float x = UnityEngine.Random.Range(-navPlaneSize.x / 2f, navPlaneSize.x / 2f);
        float z = UnityEngine.Random.Range(-navPlaneSize.z / 2f, navPlaneSize.z / 2f);
        Vector3 localRandomPosition = new Vector3(x, 0f, z);
        Vector3 globalRandomPosition = navPlane.transform.TransformPoint(localRandomPosition);



        return globalRandomPosition;
    }

    private void rotateAgent(Vector3 directionToTarget)
    {
        float rotationSpeed = 2.5f;

        // Create a rotation that faces the direction of the target
        Quaternion targetRotation = Quaternion.Inverse(Quaternion.LookRotation(directionToTarget));
        Quaternion aimedRotation = Quaternion.Slerp(agentHips.GetComponent<ConfigurableJoint>().targetRotation, targetRotation, Time.deltaTime * rotationSpeed);

        agentHips.GetComponent<ConfigurableJoint>().targetRotation = aimedRotation;

    }
  
    private void updatePlayerLocations()
    {
        
        playerLocation = playerHips.transform.position;
        // Calculate the direction from the agent to the player
        distanceToPlayer = playerLocation - agentHips.transform.position;
        distanceToPlayer.y = 0f;
        normalizedPlayerVector = distanceToPlayer.normalized;
        agentPosition = agentHips.transform.position;
    }

    private void applyMovement(Vector3 target)
    {
        agentHips.AddForce((target) * speed);
        rotateAgent(target);
        applyDamping(rigidBodies, target);
    }
    private void leftJab()
    {
        if (timer > actionDelay)
        {
            timer = 0;
            agentLeftForeArm.AddForce(agentDestination * punchForce, ForceMode.Impulse);
        }
    }
    private void rightJab()
    {
        if (timer > actionDelay)
        {
            timer = 0;
            agentRightForeArm.AddForce(agentDestination * punchForce, ForceMode.Impulse);
        }
    }
    private void headbutt()
    {
        if (timer > actionDelay)
        {
            timer = 0;
            agentHead.AddForce(agentDestination * (punchForce / 2), ForceMode.Impulse);
        }
    }

    private void block()
    {
        iframe = true;
    }

    private Rigidbody[] getEveryRBody()
    {
        Rigidbody[] rigidbodies = GetComponentsInChildren<Rigidbody>();
        return rigidbodies;
    }

    private ConfigurableJoint[] getEveryCJoint()
    {
        ConfigurableJoint[] CJoints = GetComponentsInChildren<ConfigurableJoint>();
        return CJoints;
    }
    private void showDestLine()
    {
        // Draw the destination line from player head
        agentDestination = agentHead.position + agentHead.transform.forward * 10f;
    }
    private void applyDamping(Rigidbody[] rigidbodies, Vector3 agentTarget)
    {
        foreach (Rigidbody body in rigidBodies)
        {
            body.AddForce(agentTarget.x * dampingFactor, 0f, agentTarget.z * dampingFactor);
        }
    }

    private void flop(ConfigurableJoint[] CJoints)
    {
        foreach (ConfigurableJoint joint in CJoints)
        {
            JointDrive xDrive = joint.slerpDrive;
            xDrive.positionSpring = 0f;
            joint.slerpDrive = xDrive;

            JointDrive yzDrive = joint.angularYZDrive;
            yzDrive.positionSpring = 0f;
            joint.angularYZDrive = yzDrive;
        }
    }

    private void OnDrawGizmosSelected()
    {
        float radius = 1f;

            Gizmos.color = Color.green;

            Gizmos.DrawSphere(agentTargetLocation, radius);
        

    }

    public bool getIsIframe()
    {
        return iframe;
    }

}