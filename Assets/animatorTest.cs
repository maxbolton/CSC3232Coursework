using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class animatorTest : MonoBehaviour
{
    public Transform targetLimb;
    ConfigurableJoint CJ;
    Quaternion startRot;

    [SerializeField]
    private bool isInverse = false;

    // Start is called before the first frame update
    void Start()
    {
        CJ = GetComponent<ConfigurableJoint>();
        startRot = transform.localRotation;
    }

    // Update is called once per frame
    void Update()
    {
        if (isInverse)
        {

            CJ.targetRotation = Quaternion.Inverse(targetLimb.localRotation * startRot);
        }
        else
        {

            CJ.targetRotation = targetLimb.localRotation * startRot;
        }
    }
}
