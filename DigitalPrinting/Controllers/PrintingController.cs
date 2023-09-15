using Microsoft.Extensions.Options;
using DigitalPrinting.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PdfSharp.Drawing;
//using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text;
//using iTextSharp.text.pdf;
using System.Drawing.Text;
using System.Drawing.Imaging;

using PdfSharp.Pdf;

namespace DigitalPrinting.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PrintingController : ControllerBase
    {
        private readonly string LocalImagePath;

        public PrintingController(IOptions<ImagePathSettings> imagePathSettings)
        {
            LocalImagePath = imagePathSettings.Value.LocalImagePath;
            if (string.IsNullOrEmpty(LocalImagePath))
            {
                throw new ArgumentNullException(nameof(LocalImagePath), "LocalImagePath is not configured.");
            }
        }


        //[HttpPost("GeneratePdf")]
        //public async Task<IActionResult> GeneratePdf([FromBody] MultipleImagesRequest request)
        //{
        //    try
        //    {
        //        Create a PDF document to store the modified images
        //       var pdfDocument = new PdfSharp.Pdf.PdfDocument();
        //        pdfDocument.Info.Title = "Your Title";
        //        pdfDocument.Info.Creator = "Your Creator";
        //        pdfDocument.Info.Author = "Your Author";

        //        foreach (var imageWithAnnotations in request.Images)
        //        {
        //            Bitmap image;

        //            Check if the image path is a URL or a local file path
        //            if (Uri.IsWellFormedUriString(imageWithAnnotations.ImageUrlOrLocalPath, UriKind.Absolute))
        //            {
        //                using (var httpClient = new HttpClient())
        //                {
        //                    var imageStream = await httpClient.GetStreamAsync(imageWithAnnotations.ImageUrlOrLocalPath);
        //                    image = new Bitmap(imageStream);
        //                }
        //            }
        //            else
        //            {
        //                Load the image directly from the local file path
        //               image = new Bitmap(imageWithAnnotations.ImageUrlOrLocalPath);
        //            }

        //            foreach (var annotation in imageWithAnnotations.TextAnnotations)
        //            {
        //                using (var graphics = Graphics.FromImage(image))
        //                {
        //                    if (!IsValidFont(annotation.FontName) || annotation.FontSize <= 0)
        //                    {
        //                        return BadRequest("Font name or size is incorrect.");
        //                    }
        //                    var font = new System.Drawing.Font(annotation.FontName, annotation.FontSize);

        //                    var brush = new SolidBrush(Color.Black); // You can specify the text color

        //                    float maxLineWidth = image.Width;

        //                    Split the text into multiple lines to fit within the maximum width
        //                    var lines = WrapTextWithinImageWidth(annotation.Text, font, maxLineWidth);

        //                    Draw each line of text
        //                    float yOffset = annotation.Y;
        //                    foreach (var line in lines)
        //                    {
        //                        graphics.DrawString(line, font, brush, annotation.X, yOffset);
        //                        yOffset += font.GetHeight();
        //                    }
        //                }
        //            }

        //            Create a new PDF page for each image

        //           var pdfPage = pdfDocument.AddPage();
        //            var gfx = XGraphics.FromPdfPage(pdfPage);

        //            Convert the Bitmap image to XImage
        //            XImage pdfImage;
        //            using (var stream = new MemoryStream())
        //            {
        //                image.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
        //                pdfImage = XImage.FromStream(stream);
        //            }

        //            Define the position and size of the image on the page
        //           var xRect = new XRect(0, 0, pdfPage.Width, pdfPage.Height);

        //            try
        //            {
        //                Draw the image on the page
        //                gfx.DrawImage(pdfImage, xRect);
        //            }
        //            catch (Exception ex)
        //            {
        //                Handle the exception(e.g., log it or take appropriate action)
        //                Console.WriteLine($"Error drawing image on PDF: {ex.Message}");
        //            }
        //        }

        //        Save the PDF document with UTF-8 encoding
        //       var pdfFileName = $"{Guid.NewGuid()}_modified.pdf";
        //        var pdfFilePath = Path.Combine(LocalImagePath, pdfFileName);
        //        pdfDocument.Save(pdfFilePath);

        //        Get the URL for the generated PDF

        //       var pdfUrl = Url.Action("GetModifiedPdf", new { pdfName = pdfFileName });
        //        var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}";
        //        var fullPdfUrl = baseUrl + pdfUrl;

        //        string logFilePath = @"C:\Users\umara\Desktop\AKSA Tasks\BackEndC#\DigitalPrinting\log.txt";
        //        string logEntry = $"Modified PDF URL: {fullPdfUrl}, Timestamp: {DateTime.UtcNow}";

        //        Append the log entry to the text file
        //        System.IO.File.AppendAllText(logFilePath, logEntry + Environment.NewLine);

        //        Return the URL to the generated PDF in a JSON response
        //        return Ok(new { ModifiedPdfUrl = fullPdfUrl });
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest($"Error: {ex.Message}");
        //    }
        //}





        private List<string> WrapTextWithinImageWidth(string text, System.Drawing.Font font, float maxLineWidth)
        {
            List<string> lines = new List<string>();
            string[] words = text.Split(' ');

            StringBuilder currentLine = new StringBuilder();
            float currentLineWidth = 0;

            foreach (var word in words)
            {
                float wordWidth = GetWordWidth(word, font);

                if (currentLineWidth + wordWidth <= maxLineWidth)
                {
                    // Add the word to the current line
                    if (currentLine.Length > 0)
                    {
                        currentLine.Append(' ');
                    }
                    currentLine.Append(word);
                    currentLineWidth += wordWidth;
                }
                else
                {
                    // Start a new line if adding the word exceeds the max line width
                    lines.Add(currentLine.ToString().Trim()); // Trim to remove extra spaces
                    currentLine.Clear();
                    currentLine.Append(word);
                    currentLineWidth = wordWidth;

                    // Check if the new word itself is longer than the image width
                    if (wordWidth > maxLineWidth)
                    {
                        // If a single word is longer than the max line width, split it
                        var splitWord = SplitWordToFit(word, font, maxLineWidth);
                        lines.Add(splitWord);
                        currentLine.Clear();
                        currentLineWidth = 0;
                    }
                }
            }

            // Add the last line
            if (currentLine.Length > 0)
            {
                lines.Add(currentLine.ToString().Trim()); // Trim to remove extra spaces
            }

            return lines;
        }

        private float GetWordWidth(string word, System.Drawing.Font font)
        {
            float totalWidth = 0;

            foreach (char c in word)
            {
                totalWidth += GetCharacterWidth(c, font);
            }

            return totalWidth;
        }

        private string SplitWordToFit(string word, System.Drawing.Font font, float maxLineWidth)
        {
            StringBuilder result = new StringBuilder();
            float currentLineWidth = 0;

            foreach (char c in word)
            {
                float charWidth = GetCharacterWidth(c, font);

                if (currentLineWidth + charWidth <= maxLineWidth)
                {
                    result.Append(c);
                    currentLineWidth += charWidth;
                }
                else
                {
                    // If adding the character would exceed the max line width, break to the next line
                    result.AppendLine();
                    result.Append(c);
                    currentLineWidth = charWidth;
                }
            }

            return result.ToString().Trim();
        }




        //[HttpPost("DoPrint")]
        //public async Task<IActionResult> DoPrint([FromBody] AddTextToImageRequest request)
        //{
        //    try
        //    {
        //        Bitmap image;
        //        string localImagePath = Path.Combine(LocalImagePath, Path.GetFileName(request.ImageUrlOrLocalPath));

        //        // Check if the image exists locally, otherwise download it
        //        if (System.IO.File.Exists(localImagePath))
        //        {
        //            using (var stream = new FileStream(localImagePath, FileMode.Open, FileAccess.Read))
        //            {
        //                image = new Bitmap(stream);
        //            }
        //        }
        //        else
        //        {
        //            using (var httpClient = new HttpClient())
        //            {
        //                var imageStream = await httpClient.GetStreamAsync(request.ImageUrlOrLocalPath);
        //                image = new Bitmap(imageStream);

        //                // Save the image locally
        //                image.Save(localImagePath);
        //            }
        //        }

        //        foreach (var annotation in request.TextAnnotations)
        //        {
        //            using (var graphics = Graphics.FromImage(image))
        //            {
        //                if (!IsValidFont(annotation.FontName) || annotation.FontSize <= 0)
        //                {
        //                    return BadRequest("Font name or size is incorrect.");
        //                }
        //                var font = new System.Drawing.Font(annotation.FontName, annotation.FontSize);

        //                var brush = new SolidBrush(Color.Black); // You can specify the text color

        //                float maxLineWidth = image.Width;

        //                // Split the text into multiple lines to fit within the maximum width
        //                var lines = WrapTextWithinImageWidth(annotation.Text, font, maxLineWidth);

        //                // Draw each line of text
        //                float yOffset = annotation.Y;
        //                foreach (var line in lines)
        //                {
        //                    graphics.DrawString(line, font, brush, annotation.X, yOffset);
        //                    yOffset += font.GetHeight();
        //                }
        //            }
        //        }

        //        // Save the modified image and get its URL
        //        var modifiedImageFileName = $"{Guid.NewGuid()}_modified.png";
        //        var modifiedImagePath = Path.Combine(LocalImagePath, modifiedImageFileName);
        //        image.Save(modifiedImagePath);

        //        var modifiedImageUrl = Url.Action("GetModifiedImage", new { imageName = modifiedImageFileName });

        //        // Get the base URL (excluding the action)
        //        var baseUrl = Url.ActionContext.HttpContext.Request.Scheme + "://" + Url.ActionContext.HttpContext.Request.Host;

        //        // Combine the base URL and modified image URL
        //        var fullModifiedImageUrl = baseUrl + modifiedImageUrl;

        //        string logFilePath = @"C:\Users\umara\Desktop\AKSA Tasks\BackEndC#\DigitalPrinting\log.txt";
        //        string logEntry = $"Modified Image URL: {fullModifiedImageUrl}, Timestamp: {DateTime.UtcNow}";

        //        // Append the log entry to the text file
        //        System.IO.File.AppendAllText(logFilePath, logEntry + Environment.NewLine);

        //        // Return the URL in a JSON response
        //        return Ok(new { ModifiedImageUrl = fullModifiedImageUrl });
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest($"Error: {ex.Message}");
        //    }
        //}



        //private float GetWordWidth(string word, System.Drawing.Font font)
        //{
        //    using (Bitmap dummyImage = new Bitmap(1, 1))
        //    using (Graphics dummyGraphics = Graphics.FromImage(dummyImage))
        //    {
        //        return dummyGraphics.MeasureString(word, font).Width;
        //    }
        //}

        //private List<string> WrapTextWithinImageWidth(string text, System.Drawing.Font font, float maxLineWidth)
        //{
        //    List<string> lines = new List<string>();
        //    string[] words = text.Split(' ');

        //    StringBuilder currentLine = new StringBuilder();
        //    float currentLineWidth = 0;

        //    foreach (var word in words)
        //    {
        //        float wordWidth = GetWordWidth(word, font);

        //        if (currentLineWidth + wordWidth <= maxLineWidth)
        //        {
        //            // Add the word to the current line
        //            if (currentLine.Length > 0)
        //            {
        //                currentLine.Append(' ');
        //            }
        //            currentLine.Append(word);
        //            currentLineWidth += wordWidth;
        //        }
        //        else
        //        {
        //            // Start a new line if adding the word exceeds the max line width
        //            lines.Add(currentLine.ToString());
        //            currentLine.Clear();
        //            currentLine.Append(word);
        //            currentLineWidth = wordWidth;

        //            // Check if the new word itself is longer than the image width
        //            if (wordWidth > maxLineWidth)
        //            {
        //                // If a single word is longer than the max line width, split it
        //                var splitWord = SplitWordToFit(word, font, maxLineWidth);
        //                lines.Add(splitWord);
        //                currentLine.Clear();
        //                currentLineWidth = 0;
        //            }
        //        }
        //    }

        //    // Add the last line
        //    if (currentLine.Length > 0)
        //    {
        //        lines.Add(currentLine.ToString());
        //    }

        //    return lines;
        //}

        //private string SplitWordToFit(string word, System.Drawing.Font font, float maxLineWidth)
        //{
        //    StringBuilder result = new StringBuilder();
        //    float currentLineWidth = 0;

        //    foreach (char c in word)
        //    {
        //        float charWidth = GetCharacterWidth(c, font);

        //        if (currentLineWidth + charWidth <= maxLineWidth)
        //        {
        //            result.Append(c);
        //            currentLineWidth += charWidth;
        //        }
        //        else
        //        {
        //            // If adding the character would exceed the max line width,
        //            // break to the next line and reset the line width
        //            break;
        //        }
        //    }

        //    return result.ToString();
        //}

        private float GetCharacterWidth(char c, System.Drawing.Font font)
        {
            using (Bitmap dummyImage = new Bitmap(1, 1))
            using (Graphics dummyGraphics = Graphics.FromImage(dummyImage))
            {
                return dummyGraphics.MeasureString(c.ToString(), font).Width;
            }
        }

        private bool IsValidFont(string fontName)
        {
            return !string.IsNullOrWhiteSpace(fontName);
        }

        [HttpGet("DoPrint/{imageName}")]
        public IActionResult GetModifiedImage(string imageName)
        {
            var imagePath = Path.Combine(LocalImagePath, imageName);

            if (System.IO.File.Exists(imagePath))
            {
                // Return the file as a physical file
                return PhysicalFile(imagePath, "image/png"); // Adjust the content type as needed
            }
            else
            {
                return NotFound(); // Handle the case where the file doesn't exist
            }
        }
       

    }
}
