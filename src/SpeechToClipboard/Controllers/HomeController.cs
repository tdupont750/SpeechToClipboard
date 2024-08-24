using Microsoft.AspNetCore.Mvc;
using SpeechToClipboard.Services;

namespace SpeechToClipboard.Controllers;

[Controller, Route("")]
public class HomeController : Controller
{
    private readonly IFindReplaceService _findReplaceService;

    public HomeController(IFindReplaceService findReplaceService) => _findReplaceService = findReplaceService;

    [HttpGet]
    public IActionResult Get()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Post([FromForm] string text, [FromForm] bool lower)
    {
        if (string.IsNullOrWhiteSpace(text))
            return Ok();

        if (lower) text = text.ToLower();

        var replacedText = _findReplaceService.Replace(text);
        
        Console.WriteLine();
        Console.WriteLine(replacedText);
        
        TextCopy.ClipboardService.SetText(replacedText);

        return Ok();
    }
}
