using System.Collections.Generic;
using System.Text.RegularExpressions;
using CNUSPACKER.FST;
using Microsoft.Extensions.Logging;

namespace CNUSPACKER.Packaging
{
    /// <summary>
    /// Applies content rules to file entries in the file system tree.
    /// </summary>
    public class ContentRulesService
    {
        private const long MaxContentLength = (long)(0xBFFFFFFFL * 0.975);

        private long _curContentSize;
        private Content _curContent;
        private Content _curContentFirst;

        private readonly ILogger<ContentRulesService> _logger;

        public ContentRulesService(ILogger<ContentRulesService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Applies a list of content rules to the file system tree.
        /// </summary>
        public void ApplyRules(FSTEntry root, Contents targetContents, List<ContentRule> rules)
        {
            _logger.LogInformation("Applying content rules...");

            foreach (var rule in rules)
            {
                var regex = new Regex(rule.Pattern, RegexOptions.IgnoreCase);
                _logger.LogInformation("Applying rule: {Pattern}", rule.Pattern);

                if (rule.ContentPerMatch)
                {
                    SetNewContentRecursive("", root, targetContents, rule.Details, regex);
                }
                else
                {
                    _curContent = targetContents.CreateNewContent(rule.Details);
                    _curContentFirst = _curContent;
                    _curContentSize = 0L;

                    bool matched = SetContentRecursive("", root, targetContents, rule.Details, regex);

                    if (!matched)
                    {
                        _logger.LogWarning("No file matched rule: {Pattern}, removing content.", rule.Pattern);
                        targetContents.DeleteContent(_curContent);
                    }
                }
            }
        }

        private Content SetNewContentRecursive(string path, FSTEntry current, Contents contents, ContentDetails details, Regex regex)
        {
            path += current.Filename + "/";
            Content result = null;

            if (current.Children.Count == 0 && regex.IsMatch(path))
            {
                result = contents.CreateNewContent(details);
            }

            foreach (var child in current.Children)
            {
                if (child.IsDirectory)
                {
                    result = SetNewContentRecursive(path, child, contents, details, regex) ?? result;
                }
                else
                {
                    var fullPath = path + child.Filename;
                    if (regex.IsMatch(fullPath))
                    {
                        var assigned = contents.CreateNewContent(details);
                        _logger.LogDebug("Assigned content {ContentId:X} to: {Path}", assigned.Id, fullPath);
                        child.SetContent(assigned);
                        result = assigned;
                    }
                }
            }

            if (result != null)
                current.SetContent(result);

            return result;
        }

        private bool SetContentRecursive(string path, FSTEntry current, Contents contents, ContentDetails details, Regex regex)
        {
            path += current.Filename + "/";
            bool matchFound = false;

            if (current.Children.Count == 0 && regex.IsMatch(path))
            {
                _logger.LogDebug("Assigned content {ContentId:X} ({Offset:X},{Size:X}) to: {Path}", _curContent.Id, _curContentSize, current.FileSize, path);
                current.SetContent(_curContent);
                return true;
            }

            foreach (var child in current.Children)
            {
                if (child.IsDirectory)
                {
                    matchFound |= SetContentRecursive(path, child, contents, details, regex);
                }
                else
                {
                    var fullPath = path + child.Filename;
                    if (regex.IsMatch(fullPath))
                    {
                        if (_curContentSize + child.FileSize > MaxContentLength)
                        {
                            _logger.LogInformation("Splitting content due to size limit. Creating new content block.");
                            _curContent = contents.CreateNewContent(details);
                            _curContentSize = 0;
                        }

                        _curContentSize += child.FileSize;
                        _logger.LogDebug("Assigned content {ContentId:X} ({Offset:X},{Size:X}) to: {Path}", _curContent.Id, _curContentSize, child.FileSize, fullPath);
                        child.SetContent(_curContent);
                        matchFound = true;
                    }
                }
            }

            if (matchFound)
                current.SetContent(_curContentFirst);

            return matchFound;
        }
    }
}
