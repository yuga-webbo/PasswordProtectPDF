using iTextSharp.text.pdf;
using iTextSharp.text.pdf.qrcode;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Asn1.X509;
using PasswordProtectionPDF.Models;
using System.IO;
using System.IO.Compression;
using System.Net.Http.Headers;
using System.Net.Mime;
using static iTextSharp.text.pdf.events.IndexEvents;

namespace PasswordProtectionPDF.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        public static IConfiguration _configuration;
        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }
        /// <summary>
        /// Updload  Files
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("UploadPdfFiles")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)] //[ModelBinder(BinderType = typeof(JsonModelBinder))] ClientPhotosDto clientphotos
        public async Task<IActionResult> UpdloadFilesForPasswordRemove(IFormFileCollection pdf)
        {
            var tempFile = Path.GetTempFileName();
            List<byte[]> byteList = new List<byte[]>();
            List<string> fileNameList = new List<string>();
            for (var i=0;i<pdf.Count;i++)
            {
                var memoryStreamInput = new MemoryStream();
                await pdf[i].CopyToAsync(memoryStreamInput);
                using (MemoryStream memoryStreamOutput = new MemoryStream())
                {
                    string pdfFilePassword = "Ghy678";
                    memoryStreamInput.Position = 0;
                    PdfReader pdfReader = new PdfReader(memoryStreamInput);
                    PdfReader.unethicalreading = true;
                    PdfEncryptor.Encrypt(pdfReader, memoryStreamOutput, true,pdfFilePassword,pdfFilePassword,PdfWriter.AllowScreenReaders);
                    byte[] pdfBytes = memoryStreamOutput.ToArray();
                    byteList.Add(pdfBytes);
                    fileNameList.Add(pdf[i].FileName);
                }
            }
            using (var zipFile = System.IO.File.Create(tempFile))
            using (var zipArchive = new ZipArchive(zipFile, ZipArchiveMode.Create))
            {
                var i = 0;
                foreach (var file in byteList)
                {
                    var entry = zipArchive.CreateEntry(fileNameList[i], CompressionLevel.Fastest);
                    using (var entryStream = entry.Open())
                    {
                        MemoryStream memoryStream = new MemoryStream(file);
                        await memoryStream.CopyToAsync(entryStream);
                        i = i+1;
                    }
                }

            }
            var stream = new FileStream(tempFile, FileMode.Open);
            return File(stream, "application/zip", "dataZip.zip");
        }
        /// <summary>
        /// Updload Single File
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("UploadPdfFile")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(HttpResponseMessage))]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity, Type = typeof(CommonResponseDto))] //[ModelBinder(BinderType = typeof(JsonModelBinder))] ClientPhotosDto clientphotos
        public async Task<IActionResult> UpdloadFileForPasswordRemove(IFormFile pdf)
        {
                var memoryStreamInput = new MemoryStream();
                await pdf.CopyToAsync(memoryStreamInput);

                using (MemoryStream memoryStreamOutput = new MemoryStream())
                {
                    string pdfFilePassword = "Ghy678";
                    memoryStreamInput.Position = 0;
                    PdfReader pdfReader = new PdfReader(memoryStreamInput);
                    PdfReader.unethicalreading = true;
                    PdfEncryptor.Encrypt(pdfReader, memoryStreamOutput, true, pdfFilePassword, pdfFilePassword, PdfWriter.AllowScreenReaders);
                    byte[] pdfBytes = memoryStreamOutput.ToArray();
                    Stream stream = new MemoryStream(pdfBytes);
                    // Set the response headers
                    var contentDisposition = new ContentDisposition
                    {
                        FileName = Path.GetFileNameWithoutExtension(pdf.FileName) +"_pssprotected"+".pdf", // Specify the file name for the client
                        Inline = false // Set to false if you want to force a download
                    };

                    Response.Headers.Add("Content-Disposition", contentDisposition.ToString());
                    return new FileStreamResult(stream, "application/pdf");
                }
        }
    }
}