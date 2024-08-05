using System.Text.Json;
using System.Text.RegularExpressions;

namespace SpeechToClipboard.Services.Implementation;

public class FindReplaceService : IFindReplaceService
{
    public static IFindReplaceService FromFile(string fileName)
    {
        var json = File.ReadAllText(fileName);
        
        var map = JsonSerializer.Deserialize<Dictionary<string, string>>(json)
                  ?? throw new NullReferenceException(fileName);

        var findReplaceList = map
            .Select(pair =>
            {
                var cleanFind = pair.Key.Replace(".", @"\.");
                
                // Use boundary to prevent partial word matches
                var findRegex = new Regex(
                    @"(^|\b)" + cleanFind + @"(\b|$)",
                    RegexOptions.Compiled | RegexOptions.IgnoreCase);

                return (findRegex, pair.Value);
            })
            .ToArray();
        
        return new FindReplaceService(findReplaceList);
    }
    
    private readonly (Regex Find, string Replace)[] _findReplaceList;

    private FindReplaceService((Regex Find, string Replace)[] findReplaceList) => _findReplaceList = findReplaceList;

    public string Replace(string input)
    {
        var output = input.Trim();

        foreach (var (find, replace) in _findReplaceList)
            output = find.Replace(output, replace);

        return output;
    }
}