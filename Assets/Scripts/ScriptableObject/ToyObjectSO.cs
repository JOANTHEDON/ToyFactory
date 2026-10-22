using UnityEngine;
[CreateAssetMenu()]
public class ToyObjectSO : ScriptableObject {
    public Transform prefab;
    public Sprite sprite;
    public string objectName;

    /// <summary>Display name with a few legacy typos cleaned up ("Wod" -> "Wood", "Airoplane" -> "Airplane").</summary>
    public static string GetDisplayName(ToyObjectSO so) {
        if (so == null) return "";
        string name = !string.IsNullOrEmpty(so.objectName) ? so.objectName : so.name;
        switch (name) {
            case "Wod": return "Wood";
            case "Airoplane": return "Airplane";
            case "CuttedWood": return "Cut Wood";
            case "MeltedMetal": return "Melted Metal";
            case "MeltedPlastic": return "Melted Plastic";
            case "WoodDino": return "Wood Dino";
            case "WoodDoll": return "Wood Doll";
            case "BoyDoll": return "Boy Doll";
            case "MetalHolder": return "Metal Picker";
            case "PlasticHolder": return "Plastic Holder";
        }
        return name;
    }
}
