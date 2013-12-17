using System.IO;

namespace Xpand.ExpressApp.Win.Para.WindowsIntegration.Helpers
{
    internal struct IconEntry
    {
        /// <summary>This constructor should be called by an IconFileReader.
        /// The IconFileWriter uses a constructor that passes in all values
        /// explicitly.</summary>
        public IconEntry(BinaryReader reader)
            : this()
        {
            Width = reader.ReadByte();
            Height = reader.ReadByte();
            ColorCount = reader.ReadByte();
            Reserved = reader.ReadByte();
            Planes = reader.ReadInt16();
            BitCount = reader.ReadInt16();
            BytesInRes = reader.ReadInt32();
            ImageOffset = reader.ReadInt32();
        }

        public byte Width { get; set; }

        public byte Height { get; set; }

        public byte ColorCount { get; set; }

        public byte Reserved { get; set; }

        public short Planes { get; set; }

        public short BitCount { get; set; }

        public int BytesInRes { get; set; }

        public int ImageOffset { get; set; }

        public void Save(BinaryWriter writer)
        {
            writer.Write(Width);
            writer.Write(Height);
            writer.Write(ColorCount);
            writer.Write(Reserved);
            writer.Write(Planes);
            writer.Write(BitCount);
            writer.Write(BytesInRes);
            writer.Write(ImageOffset);
        }
    }
}