# Description

An [MsBuild custom target](https://learn.microsoft.com/en-us/nuget/concepts/msbuild-props-and-targets) for NuGet packages that 

1. Converts relative GitHub or GitLab URLs in the README file to raw URLs.
2. Allows removal or replacement of parts of the README file.

[GitLab supported markup languages](https://docs.gitlab.com/user/project/repository/files/#supported-markup-languages)

[GitLab markdown](https://docs.gitlab.com/user/markdown/)

[Github about readmes](https://docs.github.com/en/repositories/managing-your-repositorys-settings-and-features/customizing-your-repository/about-readmes#relative-links-and-image-paths-in-markdown-files)

[Github markdown](https://github.github.com/gfm/)

[Nuget package readme](https://learn.microsoft.com/en-us/nuget/nuget-org/package-readme-on-nuget-org)


# MSBuild Properties

| Property                 | Default   | Required                     | Nuget Property | Description                                                       |
| --------------------     |---------  |----------------------------- | ---------------|-----------------------------------------------                    |
| BaseReadme               | README.md | No                           |                | The readme relative path to transform                             |
| PackageReadmeFile        |           | Yes.                         |                | The output readme relative path. NuGet property                   |
|                          |           |                              |                |                                                                   |
|                          |           |                              |                | A GitHub or GitLab repository url ( .git ) - order of precedence  |
| ReadmeRepositoryUrl      |           | Not if RepositoryUrl         |                |                                                                   |
| RepositoryUrl            |           | Not if ReadmeRepositoryUrl   |      Yes       |                                                                   |
|                          |           |                              |                                                                                    |
|                          |           |                              |                | The ref part of the generated absolute url in order of precedence |
| RepositoryRef            | master    | No.                          |                |                                                                   |
| RepositoryCommit         | master    | No.                          |      Yes       |                                                                   |
| RepositoryBranch         | master    | No.                          |      Yes       |                                                                   |
|                          |           |                              |                |                                                                   |
| RemoveCommentIdentifiers |           | No.                          | The format is - *startidentifier*;*endidentifier*                                  |

Of the ref MSBuild properties, RepositoryCommit is probably what you should be used.
Note that with SDK style projects the RepositoryUrl, RepositoryCommit and RepositoryBranch properties are automatically populated from the .git directory 
if you set the MSBuild PublishRepositoryUrl property to True.

Non SDK style you can add the nuget package [Microsoft.Build.Tasks.Git](https://www.nuget.org/packages/Microsoft.Build.Tasks.Git).
This is included with [Microsoft.SourceLink.GitHub](https://www.nuget.org/packages/Microsoft.SourceLink.GitHub/) and [Microsoft.SourceLink.GitLab](https://www.nuget.org/packages/Microsoft.SourceLink.GitLab/).


# Removal / replacement

Removal or replacement can be specified in two ways.
If you wish to mark the BaseReadme with comments to identify parts to remove then you can use RemoveCommentIdentifiers.

e.g
```xml
<RemoveCommentIdentifiers>remove-start;remove-end</RemoveCommentIdentifiers>
```

```md
This is visible
<!-- remove-start 1 -->
This is removed
<!-- remove-end 1 -->
This is also visible
<!-- remove-start 2 -->
This is removed
<!-- remove-end 2 -->
This too is visible
```

This is the regex used internally.
```cs
    public static Regex CreateRegex(string commentIdentifier)
    {
        var startPattern = @"<!--\s*" + Regex.Escape(commentIdentifier) + @"\b[^>]*-->";
        return new Regex(startPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
    }
```
Although the end identifier is required, if you want to remove remaining then you can omit from the BaseReadme.

```md
This is visible
<!-- remove-start -->
This is removed
```

If you want to use regexes instead of comments or you want to do replacements then you need to use the msbuild item ReadmeRemoveReplace.
For each section that you want to remove or replace you need to add an item.

The available metadata is :

| Metadata         | Required | Description                                                                                                         |
| ---------------- | -------- | ------------------------------------------------------------------------------------------------------------------- |
| CommentOrRegex   | Yes      | Either 'Comment' or 'Regex'. If 'Comment' then the same regex will be used as per RemoveCommentIdentifiers          |
| Start            | Yes      | A comment identifier or regex for the start of a section to be removed or replaced.                                 |
| End              | No       | A comment identifier or regex for the end of a section. If null then removes or replaces to the end.                |
| ReplacementText  | No       | For supplying inline, not from a file.                                                                              |


You can supply replacement text in two ways. Either inline using the ReplacementText metadata or from a file using the Include attribute.
If there is no replacement text then the matching section is removed.

There is also a special marker that can be used in replacement text. If replacement text contains "{readme_marker}" that will be replaced with the absolute url to the repository BaseReadme.

e.g
```md
Common to repo readme and nuget readme

# GitHub or GitLab eyes only

this will be replaced
```

Inline replacement text ( note the Include attribute is required even if not using a file )

```xml
<ReadmeRemoveReplace Include="1">
  <CommentOrRegex>Regex</CommentOrRegex>
  <Start>GitHub or GitLab eyes only</Start>
  <ReplacementText>For full details [see]({readme_marker})</ReplacementText>>
</ReadmeRemoveReplace>
```


# Failing the build

The relative BaseReadme does not exist.

The BaseReadme contains html.

The BaseReadme has absolute image urls that are not from [Allowed domains](https://learn.microsoft.com/en-us/nuget/nuget-org/package-readme-on-nuget-org#allowed-domains-for-images-and-badges)

The BaseReadme has relative links that do not exist in the project.

The RepositoryUrl or ReadmeRepositoryUrl property is not a valid GitHub or GitLab url.

The RemoveCommentIdentifiers property is in an invalid format.

ReadmeRemoveReplace required metadata is missing.

ReadmeRemoveReplace Start and End are the same.