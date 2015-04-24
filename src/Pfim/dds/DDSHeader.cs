﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
namespace Pfim
{
    enum CompressionAlgorithm : uint
    {
        None = 0,
        D3DFMT_DXT1 = 827611204,  //  DXT1 compression texture format "1TXD" converted from hex to num
        D3DFMT_DXT2 = 844388420,
        D3DFMT_DXT3 = 861165636,
        D3DFMT_DXT4 = 877942852,
        D3DFMT_DXT5 = 894720068
    }

    [Flags]
    enum DDSFlags : uint
    {
        /// <summary>
        /// Required in every .dds file.
        /// </summary>
        Caps = 0x1,

        /// <summary>
        /// Required in every .dds file.
        /// </summary>
        Height = 0x2,

        /// <summary>
        /// Required in every .dds file.
        /// </summary>
        Width = 0x4,

        /// <summary>
        /// Required when pitch is provided for an uncompressed texture.
        /// </summary>
        Pitch = 0x8,

        /// <summary>
        /// Required in every .dds file.
        /// </summary>
        PixelFormat = 0x1000,

        /// <summary>
        /// Required in a mipmapped texture.
        /// </summary>
        MipMapCount = 0x20000,

        /// <summary>
        /// Required when pitch is provided for a compressed texture.
        /// </summary>
        LinearSize = 0x80000,

        /// <summary>
        /// Required in a depth texture.
        /// </summary>
        Depth = 0x800000
    }
    struct DDSPixelFormat
    {
        /// <summary>
        /// Structure size; set to 32 (bytes).
        /// </summary>
        public uint Size;

        /// <summary>
        /// Values which indicate what type of data is in the surface. 
        /// </summary>
        public uint Flags;

        /// <summary>
        /// Four-character codes for specifying compressed or custom formats. 
        /// Possible values include: DXT1, DXT2, DXT3, DXT4, or DXT5. 
        /// A FourCC of DX10 indicates the prescense of the DDS_HEADER_DXT10 extended header, 
        /// and the dxgiFormat member of that structure indicates the true format. When using a four-character code, 
        /// dwFlags must include DDPF_FOURCC.
        /// </summary>
        public CompressionAlgorithm FourCC;

        /// <summary>
        /// Number of bits in an RGB (possibly including alpha) format.
        /// Valid when dwFlags includes DDPF_RGB, DDPF_LUMINANCE, or DDPF_YUV.
        /// </summary>
        public uint RGBBitCount;

        /// <summary>
        /// Red (or lumiannce or Y) mask for reading color data.
        /// For instance, given the A8R8G8B8 format, the red mask would be 0x00ff0000.
        /// </summary>
        public uint RBitMask;

        /// <summary>
        /// Green (or U) mask for reading color data.
        /// For instance, given the A8R8G8B8 format, the green mask would be 0x0000ff00.
        /// </summary>
        public uint GBitMask;

        /// <summary>
        /// Blue (or V) mask for reading color data.
        /// For instance, given the A8R8G8B8 format, the blue mask would be 0x000000ff.
        /// </summary>
        public uint BBitMask;

        /// <summary>
        /// Alpha mask for reading alpha data. 
        /// dwFlags must include DDPF_ALPHAPIXELS or DDPF_ALPHA. 
        /// For instance, given the A8R8G8B8 format, the alpha mask would be 0xff000000.
        /// </summary>
        public uint ABitMask;
    }
    class DDSHeader
    {
        /// <summary>
        /// Size of a Direct Draw Header in number of bytes.  This does not include the magic number
        /// </summary>
        public const int SIZE = 124;

        /// <summary>
        /// The magic number is the 4 bytes that starts off every Direct Draw Surface file.
        /// </summary>
        const uint DDS_MAGIC = 542327876;

        DDSPixelFormat pixelFormat;
        public DDSHeader(FileStream fsStream)
        {
            headerInit(fsStream);
        }
        private unsafe void headerInit(FileStream fsStream)
        {
            byte[] buffer = new byte[SIZE + 4];
            Reserved1 = new uint[11];
            int bufferSize, workingSize;
            bufferSize = workingSize = fsStream.Read(buffer, 0, SIZE + 4);

            fixed (byte* bufferPtr = buffer)
            {
                uint* workingBufferPtr = (uint*)bufferPtr;
                if (*workingBufferPtr++ != DDS_MAGIC)
                    throw new ApplicationException("Not a valid DDS");
                if ((Size = *workingBufferPtr++) != SIZE)
                    throw new ApplicationException("Not a valid header size");
                Flags = (DDSFlags)(*workingBufferPtr++);
                Height = *workingBufferPtr++;
                Width = *workingBufferPtr++;
                PitchOrLinearSize = *workingBufferPtr++;
                Depth = *workingBufferPtr++;
                MipMapCout = *workingBufferPtr++;
                fixed (uint* reservedPtr = Reserved1)
                {
                    uint* workingReservedPtr = reservedPtr;
                    for (int i = 0; i < 11; i++)
                        *workingReservedPtr++ = *workingBufferPtr++;
                }

                pixelFormat.Size = *workingBufferPtr++;
                pixelFormat.Flags = *workingBufferPtr++;
                pixelFormat.FourCC = (CompressionAlgorithm)(*workingBufferPtr++);
                pixelFormat.RGBBitCount = *workingBufferPtr++;
                pixelFormat.RBitMask = *workingBufferPtr++;
                pixelFormat.GBitMask = *workingBufferPtr++;
                pixelFormat.BBitMask = *workingBufferPtr++;
                pixelFormat.ABitMask = *workingBufferPtr++;

                Caps = *workingBufferPtr++;
                Caps2 = *workingBufferPtr++;
                Caps3 = *workingBufferPtr++;
                Caps4 = *workingBufferPtr++;
                Reserved2 = *workingBufferPtr++;
            }
        }

        /// <summary>
        /// Size of structure. This member must be set to 124.
        /// </summary>
        public uint Size { get; private set; }

        /// <summary>
        /// Flags to indicate which members contain valid data. 
        /// </summary>
        DDSFlags Flags { get;  set; }

        /// <summary>
        /// Surface height in pixels
        /// </summary>
        public uint Height { get; private set; }

        /// <summary>
        /// Surface width in pixels
        /// </summary>
        public uint Width { get; private set; }

        /// <summary>
        /// The pitch or number of bytes per scan line in an uncompressed texture.
        /// The total number of bytes in the top level texture for a compressed texture.
        /// </summary>
        public uint PitchOrLinearSize { get; private set; }

        /// <summary>
        /// Depth of a volume texture (in pixels), otherwise unused. 
        /// </summary>
        public uint Depth { get; private set; }

        /// <summary>
        /// Number of mipmap levels, otherwise unused.
        /// </summary>
        public uint MipMapCout { get; private set; }

        /// <summary>
        /// Unused
        /// </summary>
        public uint[] Reserved1 { get; private set; }

        /// <summary>
        /// The pixel format 
        /// </summary>
        public DDSPixelFormat PixelFormat { get { return pixelFormat ;} }

        /// <summary>
        /// Specifies the complexity of the surfaces stored.
        /// </summary>
        public uint Caps { get; private set; }

        /// <summary>
        /// Additional detail about the surfaces stored.
        /// </summary>
        public uint Caps2 { get; private set; }

        /// <summary>
        /// Unused
        /// </summary>
        public uint Caps3 { get; private set; }

        /// <summary>
        /// Unused
        /// </summary>
        public uint Caps4 { get; private set; }

        /// <summary>
        /// Unused
        /// </summary>
        public uint Reserved2 { get; private set; }
        public bool IsCompressed { get; private set; }
    }
}