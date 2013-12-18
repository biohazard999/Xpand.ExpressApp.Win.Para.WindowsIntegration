using System.IO;

namespace Xpand.ExpressApp.Win.Para.WindowsIntegration.Helpers
{
    internal struct IconHeader
    {
        public IconHeader(BinaryReader reader)
            : this()
        {
            Reserved = reader.ReadInt16();
            Type = reader.ReadInt16();
            Count = reader.ReadInt16();
        }

        public short Reserved { get; set; }

        public short Type { get; set; }

        public short Count { get; set; }

        public void Save(BinaryWriter writer)
        {
            writer.Write(Reserved);
            writer.Write(Type);
            writer.Write(Count);
        }
    }
}
