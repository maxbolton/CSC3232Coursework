using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;

public class showEnemyCollision : MonoBehaviour
{

    [SerializeField]
    enemyAgentContoller parentAgent;

    public bool isTargeted;


    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Agents") && collision.gameObject.tag == "fightCollider") {
            //UnityEngine.Debug.Log("Hit by agent!: " + collision.collider.gameObject.name);
            //UnityEngine.Debug.Log("Velocity: " + collision.relativeVelocity.magnitude);

            if (collision.relativeVelocity.magnitude > 8f && !parentAgent.getIsIframe())
            {
                //Debug.Log(collision.collider.gameObject.name+": "+ collision.relativeVelocity.magnitude);
                parentAgent.decrementHealth(collision.relativeVelocity.magnitude);
                //UnityEngine.Debug.Log("Health remaining: "+parentAgent.getHealth());

            }

        }
    }


}
