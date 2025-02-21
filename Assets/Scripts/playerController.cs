using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;



public class playerController : MonoBehaviour
{
    
    private float speed = 50f;
    private float jumpForce = 100f;
    private float punchForce = 5f;
    private float dampingFactor = 0.9f;

    [SerializeField]
    private Rigidbody hips;
    [SerializeField]
    private Rigidbody legLeft;
    [SerializeField]
    private Rigidbody legRight;
    [SerializeField]
    private Rigidbody head;
    [SerializeField]
    private Rigidbody leftForeArm;
    [SerializeField]
    private Rigidbody leftArm;
    [SerializeField]
    private Rigidbody rightArm;
    [SerializeField]
    private Rigidbody rightForeArm;
    private bool isGrounded;
    [SerializeField]
    private LayerMask agentLayer;
    private LayerMask nonAgentLayer;
    [SerializeField]
    private LayerMask enemyLayer;
    private Vector3 destination;

    private bool hasAppliedForce;
    private float dt;

    private float timer = 0;

    private enemyId lastEnemy;

    [SerializeField]
    private float actionDelay;

    private Rigidbody[] rigidBodies;

    [SerializeField]
    private GameObject animatedAgent;

    Animator animator;

    [SerializeField]
    gameContoller gameContr;


    // Start is called before the first frame update
    void Start()
    {
        animator = animatedAgent.GetComponent<Animator>();

        LayerMask nonAgentLayer = ~agentLayer;

        rigidBodies = getEveryRBody();

        hasAppliedForce = false;
        

    }

    // Update is called once per physics frame
    void Update()
    {
        float dt = Time.deltaTime;
        timer += dt;

        if (timer > actionDelay && timer < actionDelay + 0.05f)
        {
            hasAppliedForce = false;
        }
        //UnityEngine.Debug.Log("Timer: " + timer);

        // Get user inputs
        float normal = Input.GetAxisRaw("Horizontal");
        float strafe = Input.GetAxisRaw("Vertical");

        // Generate direction vector based on input
        Vector3 direction = new Vector3(normal, 0f, strafe).normalized;



        // Grounded Check !!! Fix layer mask at another time !!!
        bool leftStand = !legLeft.IsSleeping() && Physics.Raycast(legLeft.position, Vector3.down, 1f);
        bool rightStand = !legRight.IsSleeping() && Physics.Raycast(legRight.position, Vector3.down, 1f);
        if (leftStand || rightStand)
        {
            isGrounded = true;
            //UnityEngine.Debug.Log("Grounded!");
        }
        else
        {
            isGrounded = false;
            //UnityEngine.Debug.Log("!NOT!Grounded!");
        }

        animator.SetBool("blocking", false);

        // If direction input is detected and player not blocking
        if (direction.magnitude >= 0.1f && !gameContr.getIsIframe())
        {
            if (isGrounded)
            {
                //move character in direction of input
                hips.AddForce(direction.x * speed, 0f, direction.z * speed);

                //rotate hips in direction of movement
                hips.GetComponent<ConfigurableJoint>().targetRotation = Quaternion.Inverse(Quaternion.LookRotation(direction));

                // tell animator
                animator.SetBool("walking", true);
            }
            else
            {
                //move character in direction of input
                hips.AddForce(direction.x * speed / 3, 0f, direction.z * speed / 3);

                //rotate hips in direction of movement
                hips.GetComponent<ConfigurableJoint>().targetRotation = Quaternion.Inverse(Quaternion.LookRotation(direction));


                animator.SetBool("walking", false);
            }
        }
        else
        {
            foreach (Rigidbody body in rigidBodies)
            {
                body.AddForce(direction.x * dampingFactor, 0f, direction.z * dampingFactor);
            }
            animator.SetBool("walking", false);
        }


        // Set Animation
        animator.SetBool("grounded", isGrounded);

        // Only apply forces if grounded and action delay has passed and not in iframe
        if (isGrounded && !hasAppliedForce && !gameContr.getIsIframe())
        {

            // Jump
            if (Input.GetKeyDown("space"))
            {
                UnityEngine.Debug.Log("'space' Press");
                jump();
                hasAppliedForce = true;

            }

            // left jab
            if (Input.GetMouseButtonDown(0))
            {
                leftJab();
                hasAppliedForce = true;
                UnityEngine.Debug.Log("Left Jab");
            }

            
            // right jab
            if (Input.GetMouseButtonDown(1))
            {
                rightJab();
                UnityEngine.Debug.Log("Right Jab");
                hasAppliedForce = true;
            }
            
            // headbutt
            if (Input.GetKeyDown(KeyCode.X))
            {
                headbutt();
                hasAppliedForce = true;
                UnityEngine.Debug.Log("'X' Press");
            }


            // Block
            if (Input.GetKeyDown(KeyCode.Q))
            {
                block();
            }
            
        }
        if (Input.GetKeyUp(KeyCode.Q))
        {
            gameContr.setIframe(false);
            animator.SetBool("blocking", false);
            UnityEngine.Debug.Log("'Q' released");
        }

        // reset action delay if action delay is true
        if (hasAppliedForce)
        {
            if (Input.GetKeyUp("space")) { resetAppliedForce(); }
            if (Input.GetMouseButtonUp(0)) { resetAppliedForce(); }
            if (Input.GetMouseButtonUp(1)) { resetAppliedForce(); }
            if (Input.GetKeyUp(KeyCode.X))
            {
                resetAppliedForce();
                UnityEngine.Debug.Log("'X' released");
            }
            
        }

        destination = head.position + head.transform.forward * 5f;
        Ray rayHitEnemy = new Ray(head.transform.position, head.transform.forward);
        RaycastHit rayEnemyData;


        if (Physics.Raycast(rayHitEnemy, out rayEnemyData, 10f, enemyLayer))
        {
            if (rayEnemyData.transform.gameObject.layer == 7)
            {

                // enemy detected
                enemyId enemyId = rayEnemyData.transform.gameObject.GetComponentInParent<enemyId>();

                if (enemyId != null)
                {
                    if (lastEnemy != null)// valid last enemy
                    {
                        if (lastEnemy.getId() != enemyId.getId())//last enemy id is not same as current
                        {
                            lastEnemy.setTargeted(false);
                            enemyId.setTargeted(true);
                        }
                    }
                    else
                    {
                        enemyId.setTargeted(true);
                        lastEnemy = enemyId;
                    }
                }
                else
                {
                    UnityEngine.Debug.LogError("enemyId component not found on the detected enemy.");
                }
            }

        }


    }
    private void leftJab()
    {
        leftForeArm.AddForce(destination * punchForce, ForceMode.Impulse);
    }
    private void rightJab()
    {
        rightForeArm.AddForce(destination * punchForce, ForceMode.Impulse);
    }
    private void headbutt()
    {
        head.AddForce(destination * punchForce, ForceMode.Impulse);
    }
    private void block()
    {
        gameContr.setIframe(true);
        animator.SetBool("blocking", true);
        UnityEngine.Debug.Log("'Q' Press");
    }
    private void jump()
    {
        hips.AddForce(0, jumpForce, 0, ForceMode.Impulse);
    }
    private Rigidbody[] getEveryRBody()
    {
        Rigidbody[] rigidbodies = GetComponentsInChildren<Rigidbody>();
        return rigidbodies;
    }

    //reset delay timer when button lifted and timer has passed delay threshold
    private void resetAppliedForce()
    {
        if (timer > actionDelay+0.05f) {
            timer = 0;
        }
    }
   

    private void powerUpBomb()
    {
        float maxForce = 50f;
        float upwardForce = 20f;
        float radius = 15f;

        RaycastHit[] hits = Physics.SphereCastAll(transform.position, radius, Vector3.up, Mathf.Infinity, enemyLayer);
        foreach (RaycastHit hit in hits)
        {
            // Check if the hit object is not the same as the one with the script
            if (hit.collider.gameObject != gameObject)
            {
                // Check if the hit object has a Rigidbody component
                Rigidbody hitRigidbody = hit.collider.gameObject.GetComponent<Rigidbody>();

                if (hitRigidbody != null)
                {
                    // Calculate force based on the distance from the origin
                    float distance = Vector3.Distance(transform.position, hit.collider.transform.position);
                    float force = Mathf.Lerp(maxForce, 0f, distance / radius); // Adjust force based on distance

                    // Apply force towards the detected object
                    Vector3 forceDirection = hit.collider.transform.position - transform.position;
                    hitRigidbody.AddForce(forceDirection.normalized * force, ForceMode.Impulse);

                    // Apply upward force
                    hitRigidbody.AddForce(Vector3.up * upwardForce, ForceMode.Impulse);

                    UnityEngine.Debug.Log(hit.collider.gameObject.name + " is inside the radius. Applied force: " + force);
                }
                else
                {
                    UnityEngine.Debug.LogWarning(hit.collider.gameObject.name + " does not have a Rigidbody component.");
                }
            }
        }

    }

    private ConfigurableJoint[] getEveryCJoint()
    {
        ConfigurableJoint[] CJoints = GetComponentsInChildren<ConfigurableJoint>();
        return CJoints;
    }

    public void flop()
    {
        foreach (ConfigurableJoint joint in getEveryCJoint())
        {
            JointDrive xDrive = joint.slerpDrive;
            xDrive.positionSpring = 0f;
            joint.slerpDrive = xDrive;

            JointDrive yzDrive = joint.angularYZDrive;
            yzDrive.positionSpring = 0f;
            joint.angularYZDrive = yzDrive;
        }
    }

    public void resetFlop()
    {
        foreach (ConfigurableJoint joint in getEveryCJoint())
        {
            if (joint.name != "LeftArm" || joint.name != "RightArm" || joint.name != "LeftForeArm" || joint.name != "RightForeArm")
            {
                JointDrive xDrive = joint.slerpDrive;
                xDrive.positionSpring = 7500f;
                joint.slerpDrive = xDrive;

                JointDrive yzDrive = joint.angularYZDrive;
                yzDrive.positionSpring = 300f;
                joint.angularYZDrive = yzDrive;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("PowerUp"))
        {
            if (other.gameObject.tag == "bomb")
            {
                GameObject powerUp = other.gameObject;
                UnityEngine.Debug.Log("Colliding: " + powerUp);
                powerUpBomb();
                powerUp.SetActive(false);
            }
            else if (other.gameObject.tag == "healthPack")
            {
                gameContr.incrementPlayerHealth(150);
                Destroy(other.gameObject);
            }
            
        }
    }

}


 


