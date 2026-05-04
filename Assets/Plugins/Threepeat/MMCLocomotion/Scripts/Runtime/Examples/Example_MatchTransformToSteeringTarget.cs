using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Threepeat { 
public class Example_MatchTransformToSteeringTarget : MonoBehaviour
{
    public NavMeshAgent agent;
    public Transform targetObject;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (agent == null)
        {
            agent = GetComponent<NavMeshAgent>();
        }

        if (targetObject != null)
        {
            targetObject.transform.position = agent.steeringTarget;
        }
    }
}
}