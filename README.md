# Description

An [MsBuild custom target](https://learn.microsoft.com/en-us/nuget/concepts/msbuild-props-and-targets) for NuGet packages that converts relative GitHub or GitLab URLs in the README file to raw URLs.

[GitLab supported markup languages](https://docs.gitlab.com/user/project/repository/files/#supported-markup-languages)

[GitLab markdown](https://docs.gitlab.com/user/markdown/)

[Github about readmes](https://docs.github.com/en/repositories/managing-your-repositorys-settings-and-features/customizing-your-repository/about-readmes#relative-links-and-image-paths-in-markdown-files)

[Github markdown](https://github.github.com/gfm/)

[Nuget package readme](https://learn.microsoft.com/en-us/nuget/nuget-org/package-readme-on-nuget-org)


# MSBuild Properties

| Property          | Default   | Required   | Description                                                    |
| ----------        |---------  |----------  | ---------------------------------                              |
| BaseReadme        | README.md | No         | The readme relative path to transform                          |
| PackageReadmeFile |           | Yes.       | The output readme relative path. NuGet property                |
| RepositoryUrl     |           | Yes.       | A GitHub or GitLab RepositoryUrl NuGet property                |
| RepositoryBranch  | master    | No.        | The branch to use for the raw URL.                             |