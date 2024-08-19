using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class FetchAgent : Agent
{
    public Transform Target;
    public Transform Finish;
    public Material GreenMaterial;
    public Material BlackMaterial;
    private bool grabbed = false;
    private bool finished = false;
    public override void OnEpisodeBegin()
    {
        grabbed = false;
        finished = false;
        Target.GetComponent<MeshRenderer>().material = GreenMaterial;
        // reset de positie en orientatie als de agent gevallen is
        if (this.transform.localPosition.y < 0)
        {
            this.transform.localPosition = new Vector3(0, 0.5f, 0);
            this.transform.localRotation = Quaternion.identity;
        }

        // verplaats de target naar een nieuwe willekeurige locatie
        Target.localPosition = new Vector3(Random.value * 8 - 4, 0.5f, Random.value * 8 - 4);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Agent posities
        sensor.AddObservation(this.transform.localPosition);
    }

    public float speedMultiplier = 0.5f;
    public float rotationMultiplier = 5;
    public override void OnActionReceived(ActionBuffers actions)
    {
        // Acties, size = 2
        Vector3 controlSignal = Vector3.zero;
        controlSignal.z = actions.ContinuousActions[0];
        transform.Translate(controlSignal * speedMultiplier);
        transform.Rotate(0.0f, rotationMultiplier * actions.ContinuousActions[1], 0.0f);

        // Beloningen
        float distanceToTarget = Vector3.Distance(this.transform.localPosition, Target.localPosition);
        float distanceToFinish = Vector3.Distance(this.transform.localPosition, Finish.localPosition);

        // target bereikt
        if (distanceToTarget < 1.42f && grabbed == false)
        {
            AddReward(0.50f);
            Target.GetComponent<MeshRenderer>().material = BlackMaterial;
            grabbed = true;
        }

        if (finished && grabbed)
        {
            AddReward(0.50f);
            EndEpisode();
        }
        // Van het platform gevallen?
        else if (this.transform.localPosition.y < 0)
        {
            EndEpisode();
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Vertical");
        continuousActionsOut[1] = Input.GetAxis("Horizontal");
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name == "Finish")
        {
            finished = true;
            Debug.Log("Finished");
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.name == "Finish")
        {
            finished = false;
            Debug.Log("Finish exited");
        }
    }
}
