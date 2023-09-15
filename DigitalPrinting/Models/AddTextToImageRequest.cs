using System.Collections.Generic;

namespace DigitalPrinting.Models
{
    public class AddTextToImageRequest
    {
        public string ImageUrlOrLocalPath { get; set; }
        public List<TextAnnotation> TextAnnotations { get; set; }
    }

    public class TextAnnotation
    {
        public int X { get; set; }
        public int Y { get; set; }
        public string Text { get; set; }
        public string FontName { get; set; }
        public int FontSize { get; set; }
    }

    public class ModifiedImageResponse
    {
        public string ModifiedImageUrl { get; set; }
    }
    public class ImagePathSettings
    {
        public string LocalImagePath { get; set; }
    }

    public class MultipleImagesRequest
    {
        public List<ImageWithAnnotations> Images { get; set; }
        public bool Flag { get; set; }

        public string PdfFileName { get; set; }
    }

    public class ImageWithAnnotations
    {
        public string ImageUrlOrLocalPath { get; set; }
        public List<TextAnnotation> TextAnnotations { get; set; }
    }
}
