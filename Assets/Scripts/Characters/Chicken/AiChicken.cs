using AI;
using Interfaces;
using ScriptableObjects;
using UnityEngine;
using UnityEngine.AI;
using Utilities;

public class AiChicken : Chicken, IDetector
{
    private FaceTarget facetarget;
    private AudioDetection audiodetection;
    private NavMeshAgent agent;
    [SerializeField] private HearStats activehearing;
    protected override void Awake()
    {
        base.Awake();
        facetarget = GetComponent<FaceTarget>();
        agent = GetComponent<NavMeshAgent>();
        audiodetection = GetComponent<AudioDetection>();
        agent.speed = stats.MaxSpeed;
        agent.acceleration = stats.Speed;
        agent.SetDestination(Vector3.zero);
    }

    private void OnEnable()
    {
        facetarget.enabled = false;
        agent.enabled = true;
        audiodetection.SetStats(activehearing);
    }
    public override void OnCaptured()
    {
        throw new System.NotImplementedException();
    }

    public override void OnEscaped(Vector3 position)
    {
        throw new System.NotImplementedException();
    }

    public override void OnFreedFromCage()
    {
        throw new System.NotImplementedException();
    }

    protected override void HandleMovement()
    {
        CurrentSpeed = Mathf.Max(0, agent.remainingDistance - agent.stoppingDistance, 0.2f);
        AnimatorController.SetFloat(StaticUtilities.MoveSpeedAnimID, CurrentSpeed);
    }

    public void AddDetection(Vector3 location, float detection, EDetectionType type)
    {
        if (!enabled || detection < 1)
            return;
        Debug.Log($"I'm moving towards {location}");
        agent.SetDestination(location);
        AnimatorController.SetBool(StaticUtilities.CluckAnimID, false);
    }
}
