namespace OrangeTentacle.PsiConv.Models;

public class ConvRequest
{
    public bool AsText { get; set; }
    public bool AsHtml { get; set; }
    public bool AsMarkdown { get; set; }

    public IList<IFormFile> Upload { get; set; }
}