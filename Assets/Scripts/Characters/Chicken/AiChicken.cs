using AI;
using Interfaces;
using ScriptableObjects;
using System.Collections;
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
        PlayerChicken.OnPlayerCaught += AiChickenGather;
        PlayerChicken.OnPlayerEscape += AiChickenFollow;
    }

    private void OnEnable()
    {
        facetarget.enabled = false;
        agent.enabled = true;
        audiodetection.SetStats(activehearing);
        BodyCollider.enabled = true;
    }
    private void OnDisable()
    {
        facetarget.enabled = true;
        agent.ResetPath();
        agent.enabled = false;
        BodyCollider.enabled = false;
    }
    public override void OnCaptured()
    {
        enabled = false;
        agent.enabled = false;
    }

    public override void OnEscaped(Vector3 position)
    {
        Visibility = 0;
        AiChickenGather(position);
        StartCoroutine(CheckForEscaped());
    }

    public override void OnFreedFromCage()
    {
        enabled = true;
        agent.enabled = true;
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
    public void AiChickenGather(Vector3 location)
    {
        agent.SetDestination(location);
    }
    public void AiChickenFollow(Vector3 location)
    { 
        agent.SetDestination(location);
    }
    private IEnumerator CheckForEscaped()
    { 
        WaitUntil target = new WaitUntil(()=> agent.hasPath && agent.remainingDistance <= agent.stoppingDistance);
        yield return target;
        Destroy(gameObject);
    }
}
