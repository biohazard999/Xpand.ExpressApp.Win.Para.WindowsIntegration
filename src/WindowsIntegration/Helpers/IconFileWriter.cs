using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Xpand.ExpressApp.Win.Para.WindowsIntegration.Helpers
{
    /// <summary>Saves a collection of images to an icon file. Only PNG images are supported.</summary>
    /// <remarks><para>
    /// This is my first implementation of an icon file writer. It just does the basics; and doesn't do any validation
    /// that the images you add to the collection before trying to save are actually PNG files (but will convert other
    /// images anyway).</para>
    /// <para>
    /// Because this will convert all images to be saved to PNG format (32-bit with alpha channel), this <i>should not
    /// be used to save different pixel format images of the same resolution to the same icon file</i>. For example,
    /// if you try to save two 32x32 images to the same icon file, one of which is 32BPP and one is 8BPP, only one of
    /// them will be saved. (whichever one is first) This may not be the better quality image.</para></remarks>
    public class IconFileWriter
    {
        #region Fields

        private List<Image> images = new List<Image>();

        #endregion Fields

        #region Properties

        public IList<Image> Images
        {
            get { return images; }
        }

        #endregion Properties

        #region Methods

        #region Public Methods

        public void Save(string path)
        {
            using (var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                SaveToStream(fileStream);
            }
        }

        #endregion Public Methods

        #region Private Methods

        private void SaveToStream(Stream stream)
        {
            using (var writer = new BinaryWriter(stream, Encoding.UTF8))
            {
                // Remove any duplicated resolutions. Done first because this may change the image count.
                ValidateImages();

                var imageData = new Dictionary<IconEntry, byte[]>();

                // Write header
                new IconHeader
                {
                    Reserved = 0,
                    Type = 1,
                    Count = Convert.ToInt16(images.Count)
                }.Save(writer);

                // The offset of the first icon.
                var offset = Marshal.SizeOf(typeof(IconHeader)) + images.Count * Marshal.SizeOf(typeof(IconEntry));

                // Write all the icon entries
                for (var i = 0; i < images.Count; i++)
                {
                    var image = images[i] as Bitmap;

                    var data = image.ToArray(); // This extension method saves an Image to a png-format byte array.

                    var entry = new IconEntry
                    {
                        Width = image.Width < 256 ? Convert.ToByte(image.Width) : (byte)0,
                        Height = image.Height < 256 ? Convert.ToByte(image.Height) : (byte)0,
                        ColorCount = 0,
                        Reserved = 0,
                        Planes = 1,
                        BitCount = 32,
                        BytesInRes = data.Length,
                        ImageOffset = offset
                    };

                    imageData[entry] = data;
                    entry.Save(writer);

                    offset += data.Length;
                }

                // Write the Icons.
                foreach (var kvp in imageData)
                {
                    writer.Seek(kvp.Key.ImageOffset, SeekOrigin.Begin);
                    writer.Write(kvp.Value);
                }
            }
        }

        private void ValidateImages()
        {
            var contained = new List<int>();

            var validatedImages = new List<Image>();

            // Make sure there are not multiple images of the same resolution
            for (var i = 0; i < images.Count; i++)
            {
                var image = images[i] as Bitmap;

                /* Images larger than 256x256 will create invalid
                 * icons, so resize any image that's too large. */
                if (image.Width > 256 || image.Height > 256)
                {
                    image = image.Resize(256, 256) as Bitmap;
                }

                if (!contained.Contains(image.Width))
                {
                    contained.Add(image.Width);
                    validatedImages.Add(image);
                }
            }

            images.Clear();
            images.AddRange(validatedImages);
        }

        #endregion Private Methods

        #endregion Methods
    }
}
