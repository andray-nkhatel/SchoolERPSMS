using System.Collections.Generic;
using System.IO;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using PdfSharpPdfDocument = PdfSharp.Pdf.PdfDocument;
using PdfSharpPdfReader = PdfSharp.Pdf.IO.PdfReader;

namespace BluebirdCore.Services
{
    public interface IPdfMergeService
    {
        byte[] MergePdfs(IEnumerable<byte[]> pdfFiles);
        Stream MergePdfChunk(IEnumerable<Stream> pdfStreams);
    }

    public class PdfMergeService : IPdfMergeService
    {
        public byte[] MergePdfs(IEnumerable<byte[]> pdfFiles)
        {
            using (var outputDocument = new PdfSharpPdfDocument())
            {
                foreach (var pdfBytes in pdfFiles)
                {
                    using (var inputStream = new MemoryStream(pdfBytes))
                    {
                        using (var inputDocument = PdfSharpPdfReader.Open(inputStream, PdfSharp.Pdf.IO.PdfDocumentOpenMode.Import))
                        {
                            for (int idx = 0; idx < inputDocument.PageCount; idx++)
                            {
                                outputDocument.AddPage(inputDocument.Pages[idx]);
                            }
                        }
                    }
                }

                using (var outputStream = new MemoryStream())
                {
                    outputDocument.Save(outputStream, false);
                    return outputStream.ToArray();
                }
            }
        }

        public Stream MergePdfChunk(IEnumerable<Stream> pdfStreams)
        {
            var outputDocument = new PdfSharpPdfDocument();
            foreach (var pdfStream in pdfStreams)
            {
                using (var inputDocument = PdfSharpPdfReader.Open(pdfStream, PdfSharp.Pdf.IO.PdfDocumentOpenMode.Import))
                {
                    for (int idx = 0; idx < inputDocument.PageCount; idx++)
                    {
                        outputDocument.AddPage(inputDocument.Pages[idx]);
                    }
                }
            }

            var outputMemoryStream = new MemoryStream();
            outputDocument.Save(outputMemoryStream, false);
            outputMemoryStream.Position = 0; // Reset stream position for reading
            return outputMemoryStream;
        }
    }
}