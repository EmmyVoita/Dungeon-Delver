
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Linq;



[CustomPropertyDrawer(typeof(SortingLayerPicker))]
public class SortingLayerPickerDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var idProp = property.FindPropertyRelative("layerID");

        var layers = SortingLayer.layers;
        var names = layers.Select(l => l.name).ToArray();

        int currentIndex = Mathf.Max(0,
            System.Array.FindIndex(layers, l => l.id == idProp.intValue)
        );

        int newIndex = EditorGUI.Popup(position, label.text, currentIndex, names);

        idProp.intValue = layers[newIndex].id;
    }
}
#endif
