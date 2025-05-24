using System.Collections;
using TMPro;
using UnityEngine;

public abstract class AbstractAbility : MonoBehaviour
{
    //every ability should have the following:
    [SerializeField] private Sprite _icon;
    [SerializeField] private float _cooldown;
    [SerializeField] private bool _canBeHeld;

    protected Chicken Owner; //who controls the chicken

    protected Animator AnimatorController; //need to know what animations to control

    //track our state variables
    private bool _isReady = true;
    private bool _isBeingHeld;
    private float _currentCooldownTime;

    public Sprite GetIcon()
    {
        return _icon;
    }

    public float GetCooldownPercent()
    {
        return _currentCooldownTime / _cooldown;
    }

    private bool IsTriggerAnimation()
    {
        return AbilityTriggerID() != 0;
    }

    private bool IsBooleanAnimation()
    {
        return AbilityBoolID() != 0;
    }

    private void Start()
    {
        //use start because we don't own these components
        Owner = GetComponentInParent<Chicken>();
        AnimatorController = GetComponentInChildren<Animator>();
    }

    private IEnumerator BeginCooldown()
    {
        do
        {
            //wait until we can activate
            yield return new WaitUntil(CanActivate);

            //If we've let go of the key in question, then we should leave the loop
            if (!_isBeingHeld) yield break;

            //activate and animate
            Activate();
            if (IsTriggerAnimation()) AnimatorController.SetTrigger(AbilityTriggerID());

            //refresh the cooldown
            _currentCooldownTime = 0;
            _isReady = false;

            //this will loop indefinitely until the cooldown reaches its time
            while (_currentCooldownTime < _cooldown)
            {
                _currentCooldownTime += Time.deltaTime;
                //wait until next frame
                yield return null;
            }

            //one cooldown has reached max, mark as ready, and set the currentTime to cooldownTimer so that it's 100%
            _currentCooldownTime = _cooldown;
            _isReady = true;
        }
        while (_isBeingHeld && _canBeHeld); //boolean that triggers if the player continues to hold keys for future commands?

        StopUsingAbility();
    }

    //accessibility functions
    public void StartUsingAbility()
    {
        //flags using ability when function is called
        _isBeingHeld = true;

        if (_isReady) StartCoroutine(BeginCooldown());

        //changes animation if bool ID is anything but 0
        if (IsBooleanAnimation()) AnimatorController.SetBool(AbilityBoolID(), true);
    }

    public void StopUsingAbility()
    {
        _isBeingHeld = false;
        if (IsBooleanAnimation()) AnimatorController.SetBool(AbilityBoolID(), false);
    }

    //sets the condition for being able to activate the ability
    public virtual bool CanActivate()
    {
        return _isReady;
    }

    //end the action immediately
    public virtual void ForceCancelAbility()
    {
        _currentCooldownTime = _cooldown;
        _isReady = true;
        StopAllCoroutines();
        StopUsingAbility();
    }

    protected virtual int AbilityBoolID()
    {
        return 0;
    }

    /// <summary>
    /// </summary>
    /// <returns>the hash for the string used to play the animation</returns>
    protected virtual int AbilityTriggerID()
    {
        return 0;
    }

    protected abstract void Activate();
}
