using System.Collections;
using UnityEngine;
using Utilities;

[RequireComponent(typeof(Rigidbody))]
public class DashAbility : AbstractAbility
{
    [Header("Dash")]
    [SerializeField] private float _dashDistance;
    [SerializeField] private float _dashDuration;

    [Header("Effects")]
    [SerializeField] private ParticleSystem _feathers;

    private Rigidbody _rb;
    private bool _canDash = true;
    private float _radius;

    private const int NumSamples = 10;
    private static readonly Vector3 MaxVerical = new Vector3(0, 1, 0);

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        SphereCollider c = GetComponentInChildren<SphereCollider>();
        _radius = c.radius * c.transform.lossyScale.x;
    }

    private IEnumerator ActivateAbility(Vector3 direction)
    {
        //Do not dash upwards.
        direction = new Vector3(direction.x, 0, direction.z) * _dashDistance;
        _canDash = false;

        //Find the highest point on a slope
        Vector3 origin = transform.position;
        float yMax = 0;
        Vector3 start = origin + MaxVerical;
        for (int i = 0; i < NumSamples; ++i)
        {
            float percent = (float)i / NumSamples;
            if (Physics.Raycast(start + direction * percent, Vector3.down,
                    out RaycastHit targetHit,
                    _dashDistance, StaticUtilities.VisibilityLayer))
            {
                Debug.DrawRay(start + direction * percent, Vector3.down * _dashDistance, (targetHit.point.y > yMax) ? Color.green : Color.red, 3);
                if (targetHit.point.y > yMax) yMax = targetHit.point.y;
            }
            else
            {
                Debug.DrawRay(start + direction * percent, Vector3.down * _dashDistance, Color.red, 3);
            }
        }

        // Make sure going upwards actually makes sense by subtracting the goal Y from the current y. Then add back the radius, so we have the true end point.
        float d = yMax - origin.y + _radius * 2;
        //If it was negative, or 0 just go forward. else, set the target D
        if (d > 0) direction.y = d;

        direction = direction.normalized;

        // Face the opposite direction of ourselves.
        _feathers.transform.forward = -direction;
        _feathers.Play();

        Vector3 endPoint =
            Physics.SphereCast(origin, _radius, direction, out RaycastHit hit, _dashDistance, StaticUtilities.VisibilityLayer)
                ? hit.point + hit.normal * (_radius * 2)
                : direction * _dashDistance + transform.position;
        Debug.DrawLine(origin, endPoint, Color.magenta, 5);
        float curTime = 0;
        //Lock the rigidbody physics
        _rb.isKinematic = true;
        while (curTime < _dashDuration)
        {
            curTime += Time.deltaTime;
            transform.position = Vector3.Lerp(transform.position, endPoint, curTime / _dashDuration);
            yield return null;
        }
        transform.position = Vector3.Lerp(transform.position, endPoint, 1);
        //Unlock the rigidbody physics
        _rb.isKinematic = false;
        _feathers.Stop();
        _canDash = true;

    }


    public override bool CanActivate()
    {
        //Partially redundant, but you can have an additional cooldown this way.
        return _canDash && base.CanActivate();
    }

    protected override void Activate()
    {
        StartCoroutine(ActivateAbility(Owner.GetLookDirection()));
    }


    private void OnDrawGizmosSelected()
    {
        SphereCollider c = GetComponentInChildren<SphereCollider>();
        _radius = c.radius * c.transform.lossyScale.x;

        Gizmos.color = Color.yellow;
        GizmosExtras.DrawWireSphereCast(transform.position, transform.forward, _dashDistance, _radius);
    }

    public override void ForceCancelAbility()
    {
        base.ForceCancelAbility();
        _feathers.Stop();
    }

    protected override int AbilityTriggerID()
    {
        return StaticUtilities.DashAnimID;
    }
}