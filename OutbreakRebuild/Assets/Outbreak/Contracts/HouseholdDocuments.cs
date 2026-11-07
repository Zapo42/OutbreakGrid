using System.Runtime.Serialization;
namespace Outbreak.Contracts
{
    public enum HouseholdFurnitureKind { None, KitchenTable, Chair, Couch, Bed, Television, Radio }
    public enum HouseholdActivity { None, GoingToMeal, TableMeal, Snack, GoingToRest, Resting }
    [DataContract] public sealed class HouseholdFurnitureDocument
    {
        [DataMember(IsRequired=true)] public int x,y;
        [DataMember(IsRequired=true)] public HouseholdFurnitureKind kind;
        [DataMember(EmitDefaultValue=false)] public bool working;
    }
}
