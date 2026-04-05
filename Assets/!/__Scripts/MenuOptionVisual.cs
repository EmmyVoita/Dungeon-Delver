using System.Collections.Generic;
using UnityEngine;

public class MenuOptionVisual : MonoBehaviour
{
    public int optionIndex;
    public List<GameObject> visualObjects;

    void OnEnable()
    {
        MainMenuNavigator.OnSelectionChanged += HandleSelectionChanged;
    }

    void OnDisable()
    {
        MainMenuNavigator.OnSelectionChanged -= HandleSelectionChanged;
    }

    void Awake()
    {
        foreach(GameObject visualObject in visualObjects)
            visualObject.SetActive(false);
    }

    void HandleSelectionChanged(int selectedIndex)
    {
        bool active = selectedIndex == optionIndex;

        foreach(GameObject visualObject in visualObjects)
            visualObject.SetActive(active);
    }
}