using UnityEngine;

public class GoldenArrowDiceBuff : UpgradeEffectBase
{
    public int goldenArrowCount = 10;
    private int arrowsAffected = 0;
    bool IsExpired => arrowsAffected >= goldenArrowCount;

    public override void Apply(Player target)
    {
        base.Apply(target);
        arrowsAffected = 0;
        //ArrowEffectManager.Instance.RegisterEffect(this);
        Debug.Log("✨ Golden Arrow Buff active!");
    }

    public void ApplyToArrow(ArrowBase arrow)
    {
        if (arrowsAffected >= goldenArrowCount)
        {
            // remove self when limit reached
            //ArrowEffectManager.Instance.UnregisterEffect(this);
            Destroy(this.gameObject);
            return;
        }

        // example: tint the arrow gold and maybe boost its score value
        arrow.SetGolden();   // you can implement this on ArrowBase
        arrowsAffected++;
    }

    public override void Remove()
    {
        //ArrowEffectManager.Instance.UnregisterEffect(this);
        Debug.Log("💔 Golden Arrow Buff expired.");
        base.Remove();
    }
}
