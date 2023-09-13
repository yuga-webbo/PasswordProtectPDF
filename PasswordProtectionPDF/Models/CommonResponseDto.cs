namespace PasswordProtectionPDF.Models
{
    public class CommonResponseDto
    {
            public string Message { get; set; }
            public bool Status { get; set; }
            public int MaxId { get; set; }
            public string MaxNo { get; set; }
            public int StatusCode { get; set; }
    }
}
