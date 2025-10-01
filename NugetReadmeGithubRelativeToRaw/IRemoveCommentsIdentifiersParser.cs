using System.Collections.Generic;
using NugetReadmeGithubRelativeToRaw.Rewriter;

namespace NugetReadmeGithubRelativeToRaw
{
    internal interface IRemoveCommentsIdentifiersParser
    {
        RemoveCommentIdentifiers? Parse(string? removeCommentIdentifiers, IAddError addErrors);
    }
}