using System.Collections.Generic;
using UnityEngine;

public class FillRecordInventory : InventoryBase
{
    private ResultsPageController _resultsPageController;
    private ResultsPageController.ResultsPage _targetExitPage;
    private List<UpgradeRecord> _upgradeRecords = new();

    public void SetVisibilityData(ResultsPageController pagesController, ResultsPageController.ResultsPage targetExitPage)
    {
        _resultsPageController = pagesController;
        _targetExitPage = targetExitPage;
    }

    protected override void Update()
    {
        if(_resultsPageController)
        {
            if(_resultsPageController.IsTransitioning)
                return;
        }

        base.Update();
    }   

    public override void Close()
    {
        if(_resultsPageController)
        {
            _resultsPageController.ShowPage(_targetExitPage, () =>
            {
                InputFocusManager.Release(this);
            });
        }

        base.Close();
    }

    public void SetRecord(List<UpgradeRecord> upgradeRecords)
    {
        _upgradeRecords = upgradeRecords;
    }

    protected override string GetDescription(UpgradeCardUI cardUI)
    {
        string prefix = $"[<color=#{UIColors.ToHex(UIColors.Yellow)}>${cardUI.Base.Cost.ToString("N0")}</color>] ";
        return prefix + cardUI.GetDescription(true);
    }
    
    protected override void FillInventory()
    {
        Clear();

        if(_upgradeRecords.Count == 0)
        {
           titleTooltip.Hide();
           return; 
        }
            
        foreach(UpgradeRecord record in _upgradeRecords)
        {
            UpgradeBase upgradeBase = upgradeDatabase.GetUpgrade(record.upgradeId);

            if(upgradeBase == null)
                return;

            int count = record.count;
            AddCard(upgradeBase, count);
        }
    }
}