using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
[DefaultExecutionOrder(-1000)]
public class HUDManager : MonoBehaviour
{
    [Header("Interactables")] 
    [SerializeField] private AbilityUIBind abilityA;
    [SerializeField] private AbilityUIBind abilityB;
    [SerializeField] private AbilityUIBind abilityC;
    private PlayerChicken owner;
    [Header("Hud")]
    [SerializeField] private Transform trappedparent;
    [SerializeField] private Transform freedparent;
    [SerializeField] private Sprite trappedimage;
    [SerializeField] private Sprite freedimage;
    [SerializeField] private Image chickenimageprefab;
    private Dictionary<AiChicken, Image> hudChickens = new();
    public static HUDManager Instance { get; private set; }
    

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
    #region RegisteringChickens
    public void BindPlayer(PlayerChicken player)

    {
        owner = player;
        abilityA.SetTargetAbility(player.GetCluckAbility());
        abilityB.SetTargetAbility(player.GetDashAbility());
        abilityC.SetTargetAbility(player.GetJumpAbility());

    }

    public void RegisterChicken(AiChicken Chicken) 
    {
       Image clone = Instantiate(chickenimageprefab);
        hudChickens.Add(Chicken, clone);
        Chicken.OnCaught += () => CaughtChicken(clone);
        Chicken.OnFreed += () => FreeChicken(clone);
        CaughtChicken(clone);
    }
    public void DeRegisterChicken(AiChicken Chicken) 
    {
        Destroy(hudChickens[Chicken]);
        hudChickens.Remove(Chicken);
    }
    private void CaughtChicken(Image Img)
    {
        Img.transform.SetParent(trappedparent, false);
        Img.sprite = trappedimage;
        
    }
    private void FreeChicken(Image Img)
    {
        Img.transform.SetParent (freedparent, false);
        Img.sprite = freedimage;
    }
    #endregion

}
