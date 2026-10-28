using System.Runtime.Serialization;
namespace Outbreak.Contracts
{
    [DataContract] public sealed class BuildingRoomDocument
    {
        // One stable floor anchor identifies a physical room; roles remain readable authoring identities.
        [DataMember(IsRequired=true)] public string id, role;
        [DataMember(IsRequired=true)] public int x,y;
    }
    [DataContract] public sealed class BuildingRoomsDocument
    {
        [DataMember(IsRequired=true)] public string id;
        [DataMember(IsRequired=true)] public BuildingRoomDocument[] rooms;
    }
}
