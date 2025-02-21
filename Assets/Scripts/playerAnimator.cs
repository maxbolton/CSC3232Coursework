using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class playerAnimator : MonoBehaviour
{

    private Dictionary<ConfigurableJoint, Transform> linkedObjects = new Dictionary<ConfigurableJoint, Transform>();
    private Dictionary<ConfigurableJoint, Quaternion> initialRotations = new Dictionary<ConfigurableJoint, Quaternion>();
    public GameObject animatedAgent;

    [SerializeField]
    private bool printJoints;



    // Start is called before the first frame update
    void Start()
    {
        
        // Call the function to collect the joints from the children of this GameObject
        CollectConfigurableJoints(transform);

        // Set up your joints and record initial rotations
        foreach (var kvp in linkedObjects)
        {
            ConfigurableJoint joint = kvp.Key;
            Transform linkedTransform = kvp.Value;

            if (joint != null && linkedTransform != null)
            {
                // Record the initial rotation
                initialRotations[joint] = linkedTransform.rotation;
            }
        }
    }




    // Update is called once per frame
    void Update()
    {
        foreach (var kvp in linkedObjects)
        {
            ConfigurableJoint joint = kvp.Key;
            Transform linkedTransform = kvp.Value;

            if (joint != null && linkedTransform != null)
            {
                joint.targetRotation = linkedTransform.rotation * initialRotations[joint];
                joint.targetPosition = linkedTransform.position;
             
            }
        }
    }

    void CollectConfigurableJoints(Transform parent)
    {
        // Loop through the children of the parent transform
        foreach (Transform child in parent)
        {
            // Check if the child has a ConfigurableJoint component
            ConfigurableJoint joint = child.GetComponent<ConfigurableJoint>();

            if (joint != null && (joint.name != "Hips"))
            {
              
                // If a ConfigurableJoint component is found, find the corresponding animatedJoint
                Transform animatedJoint = FindAnimatedJoint(animatedAgent.transform, joint.name);

                if (animatedJoint != null)
                {
                    if (printJoints) {
                        //UnityEngine.Debug.Log("joint: " + joint.name + ", Linked: " + animatedJoint);
                    }


                    // Add the joint and animatedJoint to the linkedObjects dictionary
                    linkedObjects[joint] = animatedJoint;

                }
                else
                {
                    //UnityEngine.Debug.LogWarning("AnimatedJoint not found for joint: "+ joint.name);
                }
                
                
            }

            // Recursively call the function on the child to check its children
            CollectConfigurableJoints(child);
        }
    }

    // Helper method to find the animatedJoint by name in the hierarchy
    Transform FindAnimatedJoint(Transform parent, string jointName)
    {
        Transform animatedJoint = parent.Find(jointName);

        if (animatedJoint == null)
        {
            // If not found in the immediate children, search recursively in all children
            foreach (Transform child in parent)
            {
                animatedJoint = FindAnimatedJoint(child, jointName);

                if (animatedJoint != null)
                {
                    break;
                }
            }
        }

        return animatedJoint;
    }



}
