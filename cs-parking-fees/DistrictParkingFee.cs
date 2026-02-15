using Colossal.Serialization.Entities;
using Unity.Entities;

namespace ParkingFeeControl
{
    /// <summary>
    /// ECS component that stores the parking fee for a district.
    /// Attached directly to district entities and serialized with the save file
    /// via the game's built-in ISerializable framework.
    /// Entity references are automatically remapped on save/load, so the fee
    /// persists correctly across sessions without relying on district names.
    /// Safe to remove the mod: the game silently ignores unknown components on load.
    /// </summary>
    public struct DistrictParkingFee : IComponentData, IQueryTypeParameter, ISerializable
    {
        public int m_Fee;

        public DistrictParkingFee(int fee)
        {
            m_Fee = fee;
        }

        public void Serialize<TWriter>(TWriter writer) where TWriter : IWriter
        {
            writer.Write(m_Fee);
        }

        public void Deserialize<TReader>(TReader reader) where TReader : IReader
        {
            reader.Read(out m_Fee);
        }
    }
}
