# Intellisense can show ITaskItem.GetMetadata return value as string?.  Do not rely upon this for absence of metadata as will be empty string.

# How nuget packaging becomes part of the build process via DependsOnTargets
[Determine the target build order](https://learn.microsoft.com/en-us/visualstudio/msbuild/target-build-order?view=vs-2022#determine-the-target-build-order)
...
> 4) Before the target is executed or skipped, its DependsOnTargets targets are run, unless the Condition attribute is applied to the target and evaluates to false.
 Note
A target is considered skipped if it isn't executed because its output items are up-to-date (see incremental build). This check is done just before executing the tasks inside the target, and does not affect the order of execution of targets.



The [sdk imports](https://github.com/dotnet/sdk/blob/b9f441a35351d260b51e7c682cebcf318270dac7/src/Tasks/Microsoft.NET.Build.Tasks/sdk/Sdk.targets)
```xml
  <!-- Import targets from NuGet.Build.Tasks.Pack package/Sdk -->
  <PropertyGroup Condition="'$(NuGetBuildTasksPackTargets)' == '' AND '$(ImportNuGetBuildTasksPackTargetsFromSdk)' != 'false'">
    <NuGetBuildTasksPackTargets Condition="'$(IsCrossTargetingBuild)' == 'true'">$(MSBuildThisFileDirectory)..\..\NuGet.Build.Tasks.Pack\buildCrossTargeting\NuGet.Build.Tasks.Pack.targets</NuGetBuildTasksPackTargets>
    <NuGetBuildTasksPackTargets Condition="'$(IsCrossTargetingBuild)' != 'true'">$(MSBuildThisFileDirectory)..\..\NuGet.Build.Tasks.Pack\build\NuGet.Build.Tasks.Pack.targets</NuGetBuildTasksPackTargets>
    <ImportNuGetBuildTasksPackTargetsFromSdk>true</ImportNuGetBuildTasksPackTargetsFromSdk>
  </PropertyGroup>

  <Import Project="$(NuGetBuildTasksPackTargets)"
          Condition="Exists('$(NuGetBuildTasksPackTargets)') AND '$(ImportNuGetBuildTasksPackTargetsFromSdk)' == 'true'"/>

```

[NuGet.Build.Tasks.Pack.target](https://github.com/NuGet/NuGet.Client/blob/594fe417f2b3c7adfd970a91986f50d46780c8d9/src/NuGet.Core/NuGet.Build.Tasks/NuGet.Build.Tasks.Pack.targets)

```xml
<PropertyGroup>
  <!-- omitting for clarity -->
  <GenerateNuspecDependsOn>_LoadPackInputItems; _GetTargetFrameworksOutput; _WalkEachTargetPerFramework; _GetPackageFiles; $(GenerateNuspecDependsOn)</GenerateNuspecDependsOn>
</PropertyGroup>
<PropertyGroup Condition="'$(NoBuild)' == 'true' or '$(GeneratePackageOnBuild)' == 'true'">
  <GenerateNuspecDependsOn>$(GenerateNuspecDependsOn)</GenerateNuspecDependsOn>
</PropertyGroup>

<PropertyGroup Condition="'$(NoBuild)' != 'true' and '$(GeneratePackageOnBuild)' != 'true'">
  <GenerateNuspecDependsOn>Build;$(GenerateNuspecDependsOn)</GenerateNuspecDependsOn>
</PropertyGroup>

  <Target Name="GenerateNuspec"
          Condition="'$(IsPackable)' == 'true' AND '$(PackageReferenceCompatibleProjectStyle)' == 'true'"
          Inputs="@(NuGetPackInput)" Outputs="@(NuGetPackOutput)"
          DependsOnTargets="$(GenerateNuspecDependsOn);_CalculateInputsOutputsForPack;_GetProjectReferenceVersions;_InitializeNuspecRepositoryInformationProperties">

    <!-- omitting for clarity, calls PackTask  -->

    </Target>
```

The GenerateNuspec is included from
```xml
<PropertyGroup>
  <!-- omitting for clarity -->
  <PackDependsOn>$(BeforePack); _IntermediatePack; GenerateNuspec; $(PackDependsOn)</PackDependsOn>
</PropertyGroup>
<Target Name="Pack" DependsOnTargets="$(PackDependsOn)">
  <IsPackableFalseWarningTask Condition="'$(IsPackable)' == 'false' AND '$(WarnOnPackingNonPackableProject)' == 'true'"/>
</Target>
```

# How source control information is made available to the build - **important for the ref**

GenerateNuspec depends on _InitializeNuspecRepositoryInformationProperties
```xml
<Target Name="GenerateNuspec"
  Condition="'$(IsPackable)' == 'true' AND '$(PackageReferenceCompatibleProjectStyle)' == 'true'"
  Inputs="@(NuGetPackInput)" Outputs="@(NuGetPackOutput)"
  DependsOnTargets="$(GenerateNuspecDependsOn);_CalculateInputsOutputsForPack;_GetProjectReferenceVersions;_InitializeNuspecRepositoryInformationProperties">
```
__InitializeNuspecRepositoryInformationProperties depends on InitializeSourceControlInformation which is just a marker target
[marker target](https://github.com/dotnet/sourcelink/blob/d19562d86335814367a0eb67e05500f659b16d26/src/SourceLink.Common/buildMultiTargeting/Microsoft.SourceLink.Common.targets#L3)

[InitializeSourceControlInformation.targets](https://github.com/dotnet/sourcelink/blob/d19562d86335814367a0eb67e05500f659b16d26/src/SourceLink.Common/build/InitializeSourceControlInformation.targets)
It is this target via its DependsOnTargets that supplies the source control information.  **It is conditional**
```xml
  <!--
    Triggers InitializeSourceControlInformationFromSourceControlManager target defined by a source control package Microsoft.Build.Tasks.{Git|Tfvc|...}.
    
    Notes: No error is reported if InitializeSourceControlInformation is not defined.
  -->
  <Target Name="_InitializeSourceControlInformationFromSourceControlManager"
          DependsOnTargets="InitializeSourceControlInformationFromSourceControlManager;_SourceLinkHasSingleProvider;$(SourceControlManagerUrlTranslationTargets);SourceControlManagerPublishTranslatedUrls"
          BeforeTargets="InitializeSourceControlInformation"
          Condition="'$(EnableSourceControlManagerQueries)' == 'true'" />
```
(InitializeSourceControlInformationFromSourceControlManager)[https://github.com/dotnet/sourcelink/blob/d19562d86335814367a0eb67e05500f659b16d26/src/Microsoft.Build.Tasks.Git/build/Microsoft.Build.Tasks.Git.targets#L20]
Supplies the proerties ( of interest )
ScmRepositoryUrl => PrivateRepositoryUrl => RepositoryUrl if not set and PublishRepositoryUrl is true
SourceRevisionId => RepositoryCommit
SourceBranchName => RepositoryBranch if RepositoryBranch not set and PublishRepositoryUrl is true

```
  <Target Name="InitializeSourceControlInformationFromSourceControlManager">
    <!--
      Reports a warning if the given project doesn't belong to a repository under source control,
      unless the targets were implicily imported from an SDK without a package reference.
    -->
    <Microsoft.Build.Tasks.Git.LocateRepository
      Path="$(MSBuildProjectDirectory)"
      RemoteName="$(GitRepositoryRemoteName)"
      ConfigurationScope="$(GitRepositoryConfigurationScope)"
      NoWarnOnMissingInfo="$(PkgMicrosoft_Build_Tasks_Git.Equals(''))">

      <Output TaskParameter="RepositoryId" PropertyName="_GitRepositoryId" />
      <Output TaskParameter="Url" PropertyName="ScmRepositoryUrl" />
      <Output TaskParameter="Roots" ItemName="SourceRoot" />
      <Output TaskParameter="RevisionId" PropertyName="SourceRevisionId" Condition="'$(SourceRevisionId)' == ''" />
      <Output TaskParameter="BranchName" PropertyName="SourceBranchName" />
    </Microsoft.Build.Tasks.Git.LocateRepository>

    <PropertyGroup>
      <RepositoryType Condition="'$(RepositoryType)' == ''">git</RepositoryType>
    </PropertyGroup>
  </Target>

```
This can supply the PrivateRepositoryUrl from ScmRepositoryUrl 
```xml
	  <Target Name="SourceControlManagerPublishTranslatedUrls">
	    <PropertyGroup>
	      <!--
	        If the project already sets RepositoryUrl use it. Such URL is considered final and translations are not applied.
	      -->
	      <PrivateRepositoryUrl Condition="'$(PrivateRepositoryUrl)' == ''">$(RepositoryUrl)</PrivateRepositoryUrl>
	      <PrivateRepositoryUrl Condition="'$(PrivateRepositoryUrl)' == ''">$(ScmRepositoryUrl)</PrivateRepositoryUrl>
	    </PropertyGroup>
	
	    <ItemGroup>
	      <SourceRoot Update="@(SourceRoot)">
	        <RepositoryUrl Condition="'%(SourceRoot.RepositoryUrl)' == ''">%(SourceRoot.ScmRepositoryUrl)</RepositoryUrl>
	      </SourceRoot>
	    </ItemGroup>
  </Target>

```
Finally we see how, if we do not specify, the RepositoryUrl and RepositoryBranch will be available to GenerateNuspec if **PublishRepositoryUrl is true**.
The RepositoryCommit does not require PublishRepositoryUrl.
**It is conditional.**

```xml
  <!--
    Initialize Repository* properties from properties set by a source control package, if available in the project.
  -->
  <Target Name="_InitializeNuspecRepositoryInformationProperties"
          DependsOnTargets="InitializeSourceControlInformation"
          Condition="'$(SourceControlInformationFeatureSupported)' == 'true'">
    <PropertyGroup>
      <!-- The project must specify PublishRepositoryUrl=true in order to publish the URL or branch, in order to prevent inadvertent leak of internal data. -->
      <RepositoryUrl Condition="'$(RepositoryUrl)' == '' and '$(PublishRepositoryUrl)' == 'true'">$(PrivateRepositoryUrl)</RepositoryUrl>
      <RepositoryCommit Condition="'$(RepositoryCommit)' == ''">$(SourceRevisionId)</RepositoryCommit>
      <RepositoryBranch Condition="'$(RepositoryBranch)' == '' and '$(PublishRepositoryUrl)' == 'true' and '$(SourceBranchName)' != ''">$(SourceBranchName)</RepositoryBranch>
    </PropertyGroup>
  </Target>

```

and the sdk imports the targets providing the functionality above.
[sdk import](https://github.com/dotnet/sdk/blob/b9f441a35351d260b51e7c682cebcf318270dac7/src/Tasks/Microsoft.NET.Build.Tasks/targets/Microsoft.NET.Sdk.targets#L1373)
```xml
 <Import Project="$(MSBuildThisFileDirectory)Microsoft.NET.Sdk.SourceLink.targets" Condition="'$(SuppressImplicitGitSourceLink)' != 'true'" />
```

[Microsoft.NET.Sdk.SourceLink.targets](https://github.com/dotnet/sdk/blob/b9f441a35351d260b51e7c682cebcf318270dac7/src/Tasks/Microsoft.NET.Build.Tasks/targets/Microsoft.NET.Sdk.SourceLink.targets#L27)
```xml
<Project xmlns="http://schemas.microsoft.com/developer/msbuild/2003">

  <!-- C++ projects currently do not import Microsoft.NET.Sdk.props. -->
  <Import Project="$(MSBuildThisFileDirectory)Microsoft.NET.Sdk.SourceLink.props" Condition="'$(_SourceLinkPropsImported)' != 'true'"/>

  <PropertyGroup>
    <!-- Workaround for https://github.com/Microsoft/msbuild/issues/3294. -->
    <_SourceLinkSdkSubDir>build</_SourceLinkSdkSubDir>
    <_SourceLinkSdkSubDir Condition="'$(IsCrossTargetingBuild)' == 'true'">buildMultiTargeting</_SourceLinkSdkSubDir>

    <!-- Workaround for https://github.com/dotnet/sdk/issues/36585 (Desktop XAML targets do not produce correct #line directives) -->
    <EmbedUntrackedSources Condition="'$(EmbedUntrackedSources)' == '' and '$(ImportFrameworkWinFXTargets)' != 'true'">true</EmbedUntrackedSources>
  </PropertyGroup>

  <Import Project="$(MSBuildThisFileDirectory)..\..\Microsoft.Build.Tasks.Git\build\Microsoft.Build.Tasks.Git.targets"/>
  <Import Project="$(MSBuildThisFileDirectory)..\..\Microsoft.SourceLink.Common\$(_SourceLinkSdkSubDir)\Microsoft.SourceLink.Common.targets"/>
  <Import Project="$(MSBuildThisFileDirectory)..\..\Microsoft.SourceLink.GitHub\build\Microsoft.SourceLink.GitHub.targets"/>
  <Import Project="$(MSBuildThisFileDirectory)..\..\Microsoft.SourceLink.GitLab\build\Microsoft.SourceLink.GitLab.targets"/>
  <Import Project="$(MSBuildThisFileDirectory)..\..\Microsoft.SourceLink.AzureRepos.Git\build\Microsoft.SourceLink.AzureRepos.Git.targets"/>
  <Import Project="$(MSBuildThisFileDirectory)..\..\Microsoft.SourceLink.Bitbucket.Git\build\Microsoft.SourceLink.Bitbucket.Git.targets"/>

</Project>

```
[Microsoft.NET.Sdk.SourceLink.props](https://github.com/dotnet/sdk/blob/b9f441a35351d260b51e7c682cebcf318270dac7/src/Tasks/Microsoft.NET.Build.Tasks/targets/Microsoft.NET.Sdk.SourceLink.props)
Imports the Microsoft.SourceLink.Common.props that sets the conditional props
```xml
<Project xmlns="http://schemas.microsoft.com/developer/msbuild/2003">

  <PropertyGroup>
    <!-- Suppress implicit SourceLink inclusion if any Microsoft.SourceLink package is referenced. -->
    <SuppressImplicitGitSourceLink Condition="'$(PkgMicrosoft_SourceLink_Common)' != ''">true</SuppressImplicitGitSourceLink>
    <_SourceLinkPropsImported>true</_SourceLinkPropsImported>
  </PropertyGroup>

  <ImportGroup Condition="'$(SuppressImplicitGitSourceLink)' != 'true'">
    <Import Project="$(MSBuildThisFileDirectory)..\..\Microsoft.Build.Tasks.Git\build\Microsoft.Build.Tasks.Git.props"/>
    <Import Project="$(MSBuildThisFileDirectory)..\..\Microsoft.SourceLink.Common\build\Microsoft.SourceLink.Common.props"/>
    <Import Project="$(MSBuildThisFileDirectory)..\..\Microsoft.SourceLink.GitHub\build\Microsoft.SourceLink.GitHub.props"/>
    <Import Project="$(MSBuildThisFileDirectory)..\..\Microsoft.SourceLink.GitLab\build\Microsoft.SourceLink.GitLab.props"/>
    <Import Project="$(MSBuildThisFileDirectory)..\..\Microsoft.SourceLink.AzureRepos.Git\build\Microsoft.SourceLink.AzureRepos.Git.props"/>
    <Import Project="$(MSBuildThisFileDirectory)..\..\Microsoft.SourceLink.Bitbucket.Git\build\Microsoft.SourceLink.Bitbucket.Git.props"/>
  </ImportGroup>

</Project>
```

If not an SDK style project then you can add the nuget package [Microsoft.Build.Tasks.Git](https://www.nuget.org/packages/Microsoft.Build.Tasks.Git).
This is included with [Microsoft.SourceLink.GitHub](https://www.nuget.org/packages/Microsoft.SourceLink.GitHub/) and [Microsoft.SourceLink.GitLab](https://www.nuget.org/packages/Microsoft.SourceLink.GitLab/).


# How the target runs in the correct place, so as to generate PackageReadmeFile in time.

```xml
     <!-- 1. Always define a dummy target to anchor things if BeforePack is not defined -->
    <Target Name="BeforePackHook_NugetReadmeGithubRelativeToRaw" />

    <!-- 2. If BeforePack is not defined, point it to BeforePackHook_NugetReadmeGithubRelativeToRaw -->
    <PropertyGroup>
      <BeforePack Condition="'$(BeforePack)' == ''">BeforePackHook_NugetReadmeGithubRelativeToRaw</BeforePack>
    </PropertyGroup>

    <!-- or instead _GetPackageFiles -->
    <Target Name="ReadmeRewrite_ForPack"
          BeforeTargets="$(BeforePack)"
          DependsOnTargets="_InitializeNuspecRepositoryInformationProperties"
          Condition="'$(IsPackable)' != 'false'">

          <!-- omitting for clarity  -->

        <!-- Run the ReadmeRewriterTask....... -->

        <ItemGroup>
            <None Include="$(PackageReadmeFile)" Pack="true" PackagePath=""/>
        </ItemGroup>
     </Target>
```

## Why BeforeTargets="$(BeforePack)"

The Pack target DependsOnTargets
`<PackDependsOn>$(BeforePack); _IntermediatePack; GenerateNuspec; $(PackDependsOn)</PackDependsOn>`
It is GenerateNuspec that calls the PackTask to create the nupkg file so if we BeforeTargets BeforePack then are in time to supply the generated readme file.

Could have instead used _GetPackageFiles - omitted elements for clarity
```xml
  <Target Name="_GetPackageFiles" Condition="$(IncludeContentInPack) == 'true'">
    <ItemGroup>
      <_PackageFiles Include="@(None)" Condition=" %(None.Pack) == 'true' ">
        <BuildAction Condition="'%(None.BuildAction)' == ''">None</BuildAction>
      </_PackageFiles>
    </ItemGroup>
  </Target>
```
This runs before GenerateNuspec as seen in the GenerateNuspecDependsOn
```xml
    <GenerateNuspecDependsOn>_LoadPackInputItems; _GetTargetFrameworksOutput; _WalkEachTargetPerFramework; _GetPackageFiles; $(GenerateNuspecDependsOn)</GenerateNuspecDependsOn>
```

## Why DependsOnTargets="_InitializeNuspecRepositoryInformationProperties"

That target sets RepositoryUrl, RepositoryBranch and RepositoryCommit if not already set and PublishRepositoryUrl is true.

This has been tested in the integration tests by manually creating the .git directory with the relevant files as required by the [LocateRepository task](https://github.com/dotnet/sourcelink/blob/d19562d86335814367a0eb67e05500f659b16d26/src/Microsoft.Build.Tasks.Git/LocateRepository.cs#L11)



-----
The [packing the readme advice](https://learn.microsoft.com/en-us/nuget/reference/errors-and-warnings/nu5039)
> When creating a package from an MSBuild project file, make sure to reference the readme file in the project, as follows:
```xml
<Project Sdk="Microsoft.NET.Sdk">
	<PropertyGroup>
		<PackageReadmeFile>readme.md</PackageReadmeFile>
	</PropertyGroup>
	<ItemGroup>
		<None Include="docs\readme.md" Pack="true" PackagePath=""/>
	</ItemGroup>
</Project>
```