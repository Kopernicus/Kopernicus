using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Kopernicus
{
    namespace OnDemand
    {
        public class TGAUtils
        {
            public static Color32[] ReadImage(TGAHeader header, byte[] data)
            {
                TGAImage image = new TGAImage();
                TGAImageType imageType = header.imageType;
                if (imageType == TGAImageType.Uncompressed_TrueColor)
                {
                    return ReadTrueColorImage(header, data);
                }
                if (imageType != TGAImageType.RTE_TrueColor)
                {
                    Debug.Log("Image type of " + header.imageType.ToString() + " is not supported.");
                    return null;
                }
                return ReadRTETrueColorImage(header, data);
            }

            private static Color32[] ReadTrueColorImage(TGAHeader header, byte[] data)
            {
                MethodInfo read = typeof(TGAImage).GetMethod("ReadTrueColorImage", (BindingFlags.Instance | BindingFlags.NonPublic));
                Color32[] colors = read.Invoke(new TGAImage(), new object[] { header, data }) as Color32[];
                return colors;
            }

            private static Color32[] ReadRTETrueColorImage(TGAHeader header, byte[] data)
            {
                MethodInfo read = typeof(TGAImage).GetMethod("ReadRTETrueColorImage", (BindingFlags.Instance | BindingFlags.NonPublic));
                Color32[] colors = read.Invoke(new TGAImage(), new object[] { header, data }) as Color32[];
                return colors;
            }
        }
    }
}
