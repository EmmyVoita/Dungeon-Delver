using System.Collections;
using UnityEngine;

public class PlayerOrbitingShieldsAbility : AbilityBase
{
    [Header("Ability Settings")]
    public GameObject orbitingEffectPrefab;
    public OrbitingShield orbitingShield;
    public Transform firePoint;

    private bool awaitingLaunch = false; // ✅ new flag

    private void Start()
    {
        if (orbitingShield == null && orbitingEffectPrefab != null)
        {
            var effectInstance = Instantiate(orbitingEffectPrefab, Player.Instance.transform.position, Quaternion.identity);
            orbitingShield = effectInstance.GetComponent<OrbitingShield>();
        }
    }

    private void Update()
    {
        // Wait for launch input if shield already spawned
        if (awaitingLaunch && InputBindingManager.Instance.GetKeyDown(InputActionType.UseAbility))
        {
            orbitingShield.LaunchAllShields();
            awaitingLaunch = false; // reset
            Debug.Log("🚀 Shields launched!");
        }
    }

    public override void Activate(Quaternion rotation)
    {
        if (!orbitingShield.IsShieldActive)
        {
            orbitingShield.AddShield();
            StartCoroutine(EnableLaunchAfterDelay());
            Debug.Log("🛡 Shield spawned — press again to launch");
        }
    }

    private IEnumerator EnableLaunchAfterDelay()
    {
        awaitingLaunch = false; // temporarily disable
        yield return null;       // wait one frame
        awaitingLaunch = true;   // now it's safe
    }

}
