using Svg;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollectionData.Util
{
    public class SVGParser
    {
        public static Size MaximumSize { get; set; }

        public static Bitmap GetBitmapFromSVG(string filePath)
        {
            SvgDocument document = GetSvgDocument(filePath);

            return document.Draw();
        }

        public static SvgDocument GetSvgDocument(string filePath)
        {
            SvgDocument document = SvgDocument.Open(filePath);

            return AdjustSize(document);
        }

        public static SvgDocument AdjustSize(SvgDocument document)
        {
            document.Width = MaximumSize.Width;
            document.Height = MaximumSize.Height;

            return document;
        }
    }
}
