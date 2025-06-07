using UnityEngine;

[DefaultExecutionOrder(-1000)]
public class HUDManager : MonoBehaviour
{
    [SerializeField] private AbilityUIBind abilityA;
    [SerializeField] private AbilityUIBind abilityB;
    [SerializeField] private AbilityUIBind abilityC;
    private PlayerChicken owner;
    public static HUDManager Instance;
    private void Awake()
    {
        
        if (Instance && Instance != this) 
        {
            Debug.Log(Instance.GetInstanceID());
            Destroy(Instance);
            return;
        }
        Instance = this;
        Debug.Log(Instance.GetInstanceID());
    }
    public void BindPlayer(PlayerChicken player)
    {
        owner = player;
        abilityA.SetTargetAbility(player.GetCluckAbility());
        abilityB.SetTargetAbility(player.GetDashAbility());
        abilityC.SetTargetAbility(player.GetJumpAbility());
    }
}
