using DigitalPrinting.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System;
using System.Text;
using iText.Layout;
using iText.IO.Image;
using iText.Kernel.Pdf;
using iText.Layout.Element;
using iText.Kernel.Geom;

namespace DigitalPrinting.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MultiplePrintingController : ControllerBase
    {
        private readonly string LocalImagePath = @"C:\Users\umara\Pictures\Saved Pictures";
        private readonly string LocalPdfPath = @"C:\Users\umara\Desktop\AKSA Tasks\BackEndC#\DigitalPrinting";


        //[HttpPost("GeneratePdf")]
        //  public async Task<IActionResult> GeneratePdf([FromBody] MultipleImagesRequest request)
        //  {
        //      try
        //      {
        //          List<string> pdfFileNames = new List<string>();

        //          foreach (var imageRequest in request.Images)
        //          {
        //              Bitmap image;
        //              string localImagePath = System.IO.Path.Combine(LocalImagePath, System.IO.Path.GetFileName(imageRequest.ImageUrlOrLocalPath));

        //              // Check if the image exists locally, otherwise download it
        //              if (System.IO.File.Exists(localImagePath))
        //              {
        //                  using (var stream = new FileStream(localImagePath, FileMode.Open, FileAccess.Read))
        //                  {
        //                      image = new Bitmap(stream);
        //                  }
        //              }
        //              else
        //              {
        //                  using (var httpClient = new HttpClient())
        //                  {
        //                      var imageStream = await httpClient.GetStreamAsync(imageRequest.ImageUrlOrLocalPath);
        //                      image = new Bitmap(imageStream);

        //                      // Save the image locally
        //                      image.Save(localImagePath);
        //                  }
        //              }

        //              foreach (var annotation in imageRequest.TextAnnotations)
        //              {
        //                  using (var graphics = Graphics.FromImage(image))
        //                  {
        //                      if (!IsValidFont(annotation.FontName) || annotation.FontSize <= 0)
        //                      {
        //                          return BadRequest("Font name or size is incorrect.");
        //                      }
        //                      var font = new System.Drawing.Font(annotation.FontName, annotation.FontSize);

        //                      var brush = new SolidBrush(Color.Black); // You can specify the text color

        //                      float maxLineWidth = image.Width;

        //                      // Split the text into multiple lines to fit within the maximum width
        //                      var lines = WrapTextWithinImageWidth(annotation.Text, font, maxLineWidth);

        //                      // Draw each line of text
        //                      float yOffset = annotation.Y;
        //                      foreach (var line in lines)
        //                      {
        //                          graphics.DrawString(line, font, brush, annotation.X, yOffset);
        //                          yOffset += font.GetHeight();
        //                      }
        //                  }
        //              }

        //              // Save the modified image
        //              var modifiedImageFileName = $"{Guid.NewGuid()}_modified.png";
        //              var modifiedImagePath = System.IO.Path.Combine(LocalImagePath, modifiedImageFileName);
        //              image.Save(modifiedImagePath);

        //              pdfFileNames.Add(modifiedImagePath);
        //          }

        //          // Create a PDF with the modified images
        //          var pdfFileName = $"{Guid.NewGuid()}_modified.pdf";
        //          var pdfFilePath = System.IO.Path.Combine(LocalPdfPath, pdfFileName);
        //          CreatePdfFromImages(pdfFileNames, pdfFilePath);

        //          var pdfUrl = Url.Action("GetPdf", new { pdfName = pdfFileName });

        //          // Get the base URL (excluding the action)
        //          var baseUrl = Url.ActionContext.HttpContext.Request.Scheme + "://" + Url.ActionContext.HttpContext.Request.Host;

        //          // Combine the base URL and PDF URL
        //          var fullPdfUrl = baseUrl + pdfUrl;

        //          string logFilePath = @"C:\Users\umara\Desktop\AKSA Tasks\BackEndC#\DigitalPrinting\log.txt";
        //          string logEntry = $"PDF URL: {fullPdfUrl}, Timestamp: {DateTime.UtcNow}";

        //          // Append the log entry to the text file
        //          System.IO.File.AppendAllText(logFilePath, logEntry + Environment.NewLine);

        //          // Return the PDF URL in a JSON response
        //          return Ok(new { PdfUrl = fullPdfUrl });
        //      }
        //      catch (Exception ex)
        //      {
        //          return BadRequest($"Error: {ex.Message}");
        //      }
        //  }


        [HttpPost("GeneratePdf")]
        public async Task<IActionResult> GeneratePdf([FromBody] MultipleImagesRequest request)
        {
            try
            {
                List<string> pdfFileNames = new List<string>();

                foreach (var imageRequest in request.Images)
                {
                    Bitmap image;
                    string localImagePath = System.IO.Path.Combine(LocalImagePath, System.IO.Path.GetFileName(imageRequest.ImageUrlOrLocalPath));

                    // Check if the image exists locally, otherwise download it
                    if (System.IO.File.Exists(localImagePath))
                    {
                        using (var stream = new FileStream(localImagePath, FileMode.Open, FileAccess.Read))
                        {
                            image = new Bitmap(stream);
                        }
                    }
                    else
                    {
                        using (var httpClient = new HttpClient())
                        {
                            var imageStream = await httpClient.GetStreamAsync(imageRequest.ImageUrlOrLocalPath);
                            image = new Bitmap(imageStream);

                            // Save the image locally
                            image.Save(localImagePath);
                        }
                    }

                    foreach (var annotation in imageRequest.TextAnnotations)
                    {
                        using (var graphics = Graphics.FromImage(image))
                        {
                            if (!IsValidFont(annotation.FontName) || annotation.FontSize <= 0)
                            {
                                return BadRequest("Font name or size is incorrect.");
                            }
                            var font = new System.Drawing.Font(annotation.FontName, annotation.FontSize);

                            var brush = new SolidBrush(Color.Black); // You can specify the text color

                            float maxLineWidth = image.Width;

                            // Split the text into multiple lines to fit within the maximum width
                            var lines = WrapTextWithinImageWidth(annotation.Text, font, maxLineWidth);

                            // Draw each line of text
                            float yOffset = annotation.Y;
                            foreach (var line in lines)
                            {
                                graphics.DrawString(line, font, brush, annotation.X, yOffset);
                                yOffset += font.GetHeight();
                            }
                        }
                    }

                    // Save the modified image
                    var modifiedImageFileName = $"{Guid.NewGuid()}_modified.png";
                    var modifiedImagePath = System.IO.Path.Combine(LocalImagePath, modifiedImageFileName);
                    image.Save(modifiedImagePath);

                    pdfFileNames.Add(modifiedImagePath);
                }

                // Create a PDF with the modified images
               // var pdfFileName = $"{Guid.NewGuid()}_modified.pdf";
                var pdfFileName = request.PdfFileName;

                var pdfFilePath = System.IO.Path.Combine(LocalPdfPath, pdfFileName);
                if (System.IO.File.Exists(pdfFilePath))
                {
                    return BadRequest($"A PDF file with the name '{pdfFileName}' already exists. Please choose a different name. A PDF file with the name 'my_custom3_pdf.pdf' already exists. Please choose a different name. A PDF file with the name 'my_custom3_pdf.pdf' already exists. Please choose a different name.A PDF file with the name 'my_custom3_pdf.pdf' already exists. Please choose a different name.A PDF file with the name 'my_custom3_pdf.pdf' already exists. Please choose a different name.A PDF file with the name 'my_custom3_pdf.pdf' already exists. Please choose a different name.A PDF file with the name 'my_custom3_pdf.pdf' already exists. Please choose a different name.A PDF file with the name 'my_custom3_pdf.pdf' already exists. Please choose a different name.A PDF file with the name 'my_custom3_pdf.pdf' already exists. Please choose a different name.A PDF file with the name 'my_custom3_pdf.pdf' already exists. Please choose a different name.A PDF file with the name 'my_custom3_pdf.pdf' already exists. Please choose a different name.A PDF file with the name 'my_custom3_pdf.pdf' already exists. Please choose a different name.A PDF file with the name 'my_custom3_pdf.pdf' already exists. Please choose a different name.A PDF file with the name 'my_custom3_pdf.pdf' already exists. Please choose a different name.");
                }

                if (request.Flag)
                {
                    CreatePdfWithSingleImagePerPage(pdfFileNames, pdfFilePath);
                }
                else
                {
                    CreatePdfFromImages(pdfFileNames, pdfFilePath);
                }

                var pdfUrl = Url.Action("GetPdf", new { pdfName = pdfFileName });

                // Get the base URL (excluding the action)
                var baseUrl = Url.ActionContext.HttpContext.Request.Scheme + "://" + Url.ActionContext.HttpContext.Request.Host;

                // Combine the base URL and PDF URL
                var fullPdfUrl = baseUrl + pdfUrl;

                //string logFilePath = @"C:\Users\umara\Desktop\AKSA Tasks\BackEndC#\DigitalPrinting\log.txt";
                //string logEntry = $"PDF URL: {fullPdfUrl}, Timestamp: {DateTime.UtcNow}";

                //// Append the log entry to the text file
                //System.IO.File.AppendAllText(logFilePath, logEntry + Environment.NewLine);

                // Return the PDF URL in a JSON response
                return Ok(new { PdfUrl = fullPdfUrl });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }


        [HttpGet("GetPdf")]
        public IActionResult GetPdf(string pdfName)
        {
            try
            {
                // Assuming LocalPdfPath is the directory where your PDF files are stored
                string pdfFilePath = System.IO.Path.Combine(LocalPdfPath, pdfName);

                if (!System.IO.File.Exists(pdfFilePath))
                {
                    // Handle the case where the PDF file does not exist
                    return NotFound();
                }

                // Read the PDF file as a byte array
                byte[] pdfBytes = System.IO.File.ReadAllBytes(pdfFilePath);

                // Return the PDF as a file response
                return File(pdfBytes, "application/pdf", pdfName);
            }
            catch (Exception ex)
            {
                // Handle any exceptions that may occur during PDF retrieval
                return BadRequest($"Error: {ex.Message}");
            }
        }


        private float GetWordWidth(string word, System.Drawing.Font font)
        {
            using (Bitmap dummyImage = new Bitmap(1, 1))
            using (Graphics dummyGraphics = Graphics.FromImage(dummyImage))
            {
                return dummyGraphics.MeasureString(word, font).Width;
            }
        }

     

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
                        lines.Add(currentLine.ToString());
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
                    lines.Add(currentLine.ToString());
                }

                return lines;
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
                    // If adding the character would exceed the max line width,
                    // break to the next line and reset the line width
                    break;
                }
            }

            return result.ToString();
        }

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
        public void CreatePdfFromImages(List<string> imagePaths, string pdfPath)
        {
            if (imagePaths == null || imagePaths.Count == 0)
            {
                throw new ArgumentNullException("imagePaths", "The list of image paths is null or empty.");
            }

            using (var fs = new FileStream(pdfPath, FileMode.Create))
            {
                // Create a new PDF document
                PdfDocument pdfDoc = new PdfDocument(new PdfWriter(fs));
                PageSize pageSize = PageSize.A4;
                Document document = new Document(pdfDoc, pageSize);

                foreach (var imagePath in imagePaths)
                {
                    // Load the image
                    iText.Layout.Element.Image image = new iText.Layout.Element.Image(ImageDataFactory.Create(imagePath));

                    // Set the image scale to fit the page
                    image.SetAutoScale(true);

                    // Calculate the height of the image on the page
                    float imageHeight = image.GetImageHeight() * (pageSize.GetWidth() / image.GetImageWidth());

                    if (document.GetRenderer().GetCurrentArea().GetBBox().GetHeight() < imageHeight)
                    {
                        // Start a new page if the image does not fit
                        document.Add(new AreaBreak());
                    }

                    // Add the image to the document
                    document.Add(image);
                }

                // Close the document after processing all images
                document.Close();
            }
        }

        public void CreatePdfWithSingleImagePerPage(List<string> imagePaths, string pdfPath)
        {
            if (imagePaths == null || imagePaths.Count == 0)
            {
                throw new ArgumentNullException("imagePaths", "The list of image paths is null or empty.");
            }

            using (var fs = new FileStream(pdfPath, FileMode.Create))
            {
                // Create a new PDF document
                PdfDocument pdfDoc = new PdfDocument(new PdfWriter(fs));
                PageSize pageSize = PageSize.A4;
                Document document = new Document(pdfDoc, pageSize);

                foreach (var imagePath in imagePaths)
                {
                    // Load the image
                    iText.Layout.Element.Image image = new iText.Layout.Element.Image(ImageDataFactory.Create(imagePath));

                    // Set the image scale to fit the page
                    image.SetAutoScale(true);

                    // Add the image to the document
                    document.Add(image);

                    // Start a new page for the next image (ensures one image per page)
                    document.Add(new AreaBreak());
                }

                // Close the document after processing all images
                document.Close();
            }
        }


        //public void CreatePdfFromImages(List<string> imagePaths, string pdfPath)
        //{
        //    if (imagePaths == null || imagePaths.Count == 0)
        //    {
        //        throw new ArgumentNullException("imagePaths", "The list of image paths is null or empty.");
        //    }

        //    using (var fs = new FileStream(pdfPath, FileMode.Create))
        //    {
        //        // Create a new PDF document
        //        PdfDocument pdfDoc = new PdfDocument(new PdfWriter(fs));
        //        PageSize pageSize = PageSize.A4;

        //        // Initialize variables to track the current page height
        //        float currentPageHeight = 0;
        //        Document document = null;

        //        foreach (var imagePath in imagePaths)
        //        {
        //            // Load the image
        //            iText.Layout.Element.Image image = new iText.Layout.Element.Image(ImageDataFactory.Create(imagePath));

        //            // Set the image scale to fit the page
        //            image.SetAutoScale(true);

        //            // Calculate the height of the image on the page
        //            float imageHeight = image.GetImageHeight() * (pageSize.GetWidth() / image.GetImageWidth());

        //            // Check if the image should be placed on a new page
        //            if (currentPageHeight + imageHeight > pageSize.GetHeight())
        //            {
        //                // Start a new page with the same page size
        //                document = new Document(pdfDoc, pageSize);
        //                currentPageHeight = 0;
        //            }

        //            // If document is null, create a new one
        //            if (document == null)
        //            {
        //                document = new Document(pdfDoc, pageSize);
        //            }

        //            // Add the image to the document
        //            document.Add(image);

        //            // Update the current page height
        //            currentPageHeight += imageHeight;
        //        }

        //        // Close the document after processing all images
        //        document.Close();
        //    }
        //}
    }
}
