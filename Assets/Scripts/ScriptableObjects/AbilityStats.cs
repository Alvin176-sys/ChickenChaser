using UnityEngine;

[CreateAssetMenu(fileName = "AbilityStats", menuName = "Scriptable Objects/AbilityStats")]
public class AbilityStats : ScriptableObject
{
    [SerializeField] private Sprite _icon;
    [SerializeField] private float _cooldown;
    [SerializeField] private bool _canBeHeld;

    public Sprite Icon => _icon;
    public float Cooldown => _cooldown;
    public bool CanBeHeld => _canBeHeld;
}
