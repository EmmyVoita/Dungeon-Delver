using UnityEngine;

public class PlaceGoalShieldAbility : AbilityBase
{
     [Header("Ability Settings")]
    public GameObject goalPrefab;


    private void Start()
    {

    }

    private void Update()
    {

    }

    public override void Activate(Quaternion rotation)
    {
        GameObject goal = Instantiate(goalPrefab, Player.Instance.transform.position, rotation);
    }
}
