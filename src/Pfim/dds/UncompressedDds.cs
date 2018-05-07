﻿using System;
using System.IO;

namespace Pfim
{
    /// <summary>
    /// A DirectDraw Surface that is not compressed.  
    /// Thus what is in the input stream gets directly translated to the image buffer.
    /// </summary>
    public class UncompressedDds : Dds
    {
        private readonly uint? _bitsPerPixel;
        private readonly bool? _rgbSwapped;
        private ImageFormat _format;

        internal UncompressedDds(DdsHeader header, uint bitsPerPixel, bool rgbSwapped) : base(header)
        {
            _bitsPerPixel = bitsPerPixel;
            _rgbSwapped = rgbSwapped;
        }

        internal UncompressedDds(DdsHeader header) : base(header)
        {
            
        }

        public override int BitsPerPixel => ImageInfo().Depth;

        public override ImageFormat Format => _format;

        public override bool Compressed => false;
        public override void Decompress()
        {
        }

        protected override void Decode(Stream stream, PfimConfig config)
        {
            Data = DataDecode(stream, config);
        }

        /// <summary>Determine image info from header</summary>
        public DdsLoadInfo ImageInfo()
        {
            bool rgbSwapped = _rgbSwapped ?? Header.PixelFormat.RBitMask < Header.PixelFormat.GBitMask;

            switch (_bitsPerPixel ?? Header.PixelFormat.RGBBitCount)
            {
                case 8:
                    return new DdsLoadInfo(false, rgbSwapped, true, 1, 1, 8, ImageFormat.Rgb8);
                case 16:
                    ImageFormat format = SixteenBitImageFormat();
                    return new DdsLoadInfo(false, rgbSwapped, false, 1, 2, 16, format);
                case 24:
                    return new DdsLoadInfo(false, rgbSwapped, false, 1, 3, 24, ImageFormat.Rgb24);
                case 32:
                    return new DdsLoadInfo(false, rgbSwapped, false, 1, 4, 32, ImageFormat.Rgba32);
                default:
                    throw new Exception($"Unrecognized rgb bit count: {Header.PixelFormat.RGBBitCount}");
            }
        }

        private ImageFormat SixteenBitImageFormat()
        {
            var pf = Header.PixelFormat;

            if (pf.ABitMask == 0xF000 && pf.RBitMask == 0xF00 && pf.GBitMask == 0xF0 && pf.BBitMask == 0xF)
            {
                return ImageFormat.Rgba16;
            }

            if (pf.PixelFormatFlags.HasFlag(DdsPixelFormatFlags.AlphaPixels))
            {
                return ImageFormat.R5g5b5a1;
            }

            return pf.GBitMask == 0x7e0 ? ImageFormat.R5g6b5 : ImageFormat.R5g5b5;
        }

        /// <summary>Decode data into raw rgb format</summary>
        private byte[] DataDecode(Stream str, PfimConfig config)
        {
            var imageInfo = ImageInfo();
            _format = imageInfo.Format;

            byte[] data = new byte[Dds.CalcSize(imageInfo, Header)];

            if (str is MemoryStream s && s.TryGetBuffer(out var arr))
            {
                Buffer.BlockCopy(arr.Array, (int)s.Position, data, 0, data.Length);
            }
            else
            {
                Util.Fill(str, data, config.BufferSize);
            }

            // Swap the R and B channels
            if (imageInfo.Swap)
            {
                switch (imageInfo.Format)
                {
                    case ImageFormat.Rgba32:
                        for (int i = 0; i < data.Length; i += 4)
                        {
                            byte temp = data[i];
                            data[i] = data[i + 2];
                            data[i + 2] = temp;
                        }
                        break;
                    case ImageFormat.Rgba16:
                        for (int i = 0; i < data.Length; i += 2)
                        {
                            byte temp = (byte) (data[i] & 0xF);
                            data[i] = (byte) ((data[i] & 0xF0) + (data[i + 1] & 0XF));
                            data[i + 1] = (byte) ((data[i + 1] & 0xF0) + temp);
                        }
                        break;
                    default:
                        throw new Exception($"Do not know how to swap {imageInfo.Format}");
                }
            }

            return data;
        }
    }
}
