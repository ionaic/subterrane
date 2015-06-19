using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class targetObject : MonoBehaviour {

    public List<NavMeshAgent> agent;

    void Start () {
        int i;
        for (i = 0; i < agent.Count; i++)
            agent[i].destination = transform.position;
    }

    void Update () {
        int i;
        for (i = 0; i < agent.Count; i++)
            agent[i].destination = transform.position;
    }
}