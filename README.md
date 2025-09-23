# Description

An [MsBuild custom target](https://duckduckgo.com) for NuGet packages that converts relative GitHub URLs in the README file to raw URLs.

# MSBuild Properties

| Property          | Default   | Required   | Description                                                    |
| ----------        |---------  |----------  | ---------------------------------                              |
| BaseReadme        | README.md | No         | The github readme relative path to transform                   |
| PackageReadmeFile |           | Yes.       | The output readme relative path. Nuget property                |
| RepositoryUrl     |           | Yes.       | A Github RepositoryUrl Nuget property                          |
| RepositoryBranch  | master    | No.        | The branch to use for the raw URL.                             |