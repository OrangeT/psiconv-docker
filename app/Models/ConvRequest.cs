namespace OrangeTentacle.PsiConv.Models;

public class ConvDto
{
    public ConversionType ConvertTo { get; set; } = ConversionType.Markdown;

    public IFormFile? Upload { get; set; }

    public string? ConvertResponse { get; set;}
}
