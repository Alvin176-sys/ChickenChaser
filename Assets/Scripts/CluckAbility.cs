using AI;
using ScriptableObjects;
using UnityEngine;
using Utilities;

public class CluckAbility : AbstractAbility
{
    //We need access to some variables which may or may not exist...
    //If they don't then we need to add them manually
    [SerializeField] private ParticleSystem _cluckParticle;
    [SerializeField] private AudioClip _cluckSound;

    //An audio source allows us to play audio
    private const float AudioVolume = 0.3f;
    private AudioSource _source;

    private void Awake()
    {
        _source = GetComponentInChildren<AudioSource>();
    }

    protected override void Activate()
    {
        //play particlesystem for cluck
        _cluckParticle.Play();
        _source.pitch = Random.Range(0.8f, 1.2f);
        _source.PlayOneShot(_cluckSound, SettingsManager.currentSettings.SoundVolume * AudioVolume);
        AudioDetection.onSoundPlayed.Invoke(transform.position, 10, 20, EAudioLayer.ChickenEmergency);
    }

    public override bool CanActivate()
    {
        //must be moving, or barely
        return Owner.CurrentSpeed < 1.0f && base.CanActivate();
    }

    protected override int AbilityTriggerID()
    {
        return StaticUtilities.JumpAnimID;
    }

    protected override int AbilityBoolID()
    {
        return StaticUtilities.CluckAnimID;
    }
}