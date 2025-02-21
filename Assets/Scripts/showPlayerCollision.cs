using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;

public class showPlayerCollision : MonoBehaviour
{
    
    [SerializeField]
    gameContoller parentAgent;

    public bool isTargeted;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy") && collision.gameObject.tag == "fightCollider") {
            

            // check if hit was intentional and agent is not blocking
            if (collision.relativeVelocity.magnitude > 8f && !parentAgent.getIsIframe())
            {
                // decrease health accordingly
                parentAgent.incrementPlayerHealth(-collision.relativeVelocity.magnitude);

            }

        }
    }


}
