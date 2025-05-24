using UnityEngine;

public class AiChicken : Chicken
{
    protected override void Awake()
    {
        base.Awake();
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
        throw new System.NotImplementedException();
    }
}
