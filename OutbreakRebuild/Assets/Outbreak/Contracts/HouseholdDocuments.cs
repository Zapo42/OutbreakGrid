using System.Runtime.Serialization;
namespace Outbreak.Contracts
{
    public enum HouseholdFurnitureKind { None, KitchenTable, Chair, Couch, Bed, Television, Radio, ComfyChair }
    public enum HouseholdActivity { None, GoingToMeal, TableMeal, Snack, GoingToRest, Resting }
    [DataContract] public sealed class HouseholdFurnitureDocument
    {
        [DataMember(IsRequired=true)] public int x,y;
        [DataMember(IsRequired=true)] public HouseholdFurnitureKind kind;
        [DataMember(EmitDefaultValue=false)] public bool working;
        // Omitted width preserves the original single-cell furniture contract. Only couches may span two cells.
        [DataMember(EmitDefaultValue=false)] public int width;
    }
}
