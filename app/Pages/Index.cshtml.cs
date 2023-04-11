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
    public ConvRequest? ConvRequest { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        foreach(var file in ConvRequest.Upload)
        {
            if (file.Length > 0) {

                // Copy file to local file system
                string sourceFilePath = Path.GetTempFileName();
                string targetFilePath = Path.GetTempFileName();
                using (Stream fileStream = new FileStream(sourceFilePath, FileMode.Create)) {
                    await file.CopyToAsync(fileStream);
                }

                // Exec pandoc, create file and convert
                var processStartInfo = new ProcessStartInfo();
                processStartInfo.CreateNoWindow = true;
                // processStartInfo.RedirectStandardOutput = true;
                // processStartInfo.RedirectStandardInput = true;
                processStartInfo.UseShellExecute = false;
                processStartInfo.Arguments = $" -T html5 -o {targetFilePath} {sourceFilePath}";
                processStartInfo.FileName = "/opt/psiconv/program/psiconv/psiconv";

                var process = new Process();
                process.StartInfo = processStartInfo;
                process.Start();
                await process.WaitForExitAsync();

                // Take the output file and return
                var outputBytes = await System.IO.File.ReadAllBytesAsync(targetFilePath);

                // Delete the temp files
                System.IO.File.Delete(sourceFilePath);
                System.IO.File.Delete(targetFilePath);

                return File(outputBytes, "text/html", "target.html");
            }
        }

        return Page();
   }
}
