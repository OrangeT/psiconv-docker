using System.Diagnostics;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OrangeTentacle.PsiConv.Models;

namespace OrangeTentacle.PsiConv.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(ILogger<IndexModel> logger)
    {
        _logger = logger;
    }

    [BindProperty]
    public ConvDto ConvDto { get; set; } = new ConvDto();

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (ConvDto?.Upload != null && ConvDto.Upload.Length > 0)
        {
            var file = ConvDto.Upload;
            // Copy file to local file system
            string sourceFilePath = Path.GetTempFileName();
            string targetFilePath = Path.GetTempFileName();
            using (Stream fileStream = new FileStream(sourceFilePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            var psiConvType = ConvDto.ConvertTo switch
            {
                ConversionType.Text => "ascii",
                _ => "html5"
            };

            // Exec pandoc, create file and convert
            var processStartInfo = new ProcessStartInfo();
            processStartInfo.CreateNoWindow = true;
            // processStartInfo.RedirectStandardOutput = true;
            // processStartInfo.RedirectStandardInput = true;
            processStartInfo.UseShellExecute = false;
            processStartInfo.Arguments = $" -T {psiConvType} -o {targetFilePath} {sourceFilePath}";
            processStartInfo.FileName = "/opt/psiconv/program/psiconv/psiconv";

            var process = new Process();
            process.StartInfo = processStartInfo;
            process.Start();
            await process.WaitForExitAsync();

            // Perform markdown conversion
            if (ConvDto.ConvertTo == ConversionType.Markdown)
            {
                System.IO.File.Copy(targetFilePath, sourceFilePath, true); // Reset files

                // Exec pandoc, create file and convert
                processStartInfo = new ProcessStartInfo();
                processStartInfo.CreateNoWindow = true;
                // processStartInfo.RedirectStandardOutput = true;
                // processStartInfo.RedirectStandardInput = true;
                processStartInfo.UseShellExecute = false;
                processStartInfo.Arguments = $"-f html -t markdown_strict -o {targetFilePath} {sourceFilePath}";
                processStartInfo.FileName = "/usr/bin/pandoc";

                process = new Process();
                process.StartInfo = processStartInfo;
                process.Start();
                await process.WaitForExitAsync();
            }

            // Take the output file and return
            var outputText = await System.IO.File.ReadAllTextAsync(targetFilePath);
            ConvDto.ConvertResponse = outputText;

            // Delete the temp files
            System.IO.File.Delete(sourceFilePath);
            System.IO.File.Delete(targetFilePath);
            return Page();
        }

        return Page();
    }
}
