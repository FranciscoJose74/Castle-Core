# Castle Core

<img align="right" src="docs/images/castle-logo.png">

Castle Core provides common Castle Project abstractions including logging services. It also features **Castle DynamicProxy** a lightweight runtime proxy generator, and **Castle DictionaryAdapter**.

See the [documentation](docs/README.md).

## Releases

[![NuGet](https://img.shields.io/nuget/v/Castle.Core.svg)](https://www.nuget.org/packages/Castle.Core/)

See the [Releases](https://github.com/castleproject/Core/releases).

Debugging symbols are available in symbol packages in the AppVeyor build artifacts since version 4.1.0. For example, [here are the artifacts for 4.1.0](https://ci.appveyor.com/project/castleproject/core/build/4.1.0/artifacts).

## License

Castle Core is &copy; 2004-2022 Castle Project. It is free software, and may be redistributed under the terms of the [Apache 2.0](http://opensource.org/licenses/Apache-2.0) license.

## Contributing

Browse the [contributing section](https://github.com/castleproject/Home#its-community-driven) of our _Home_ repository to get involved.

## Building

| Platforms       | NuGet Feed |
|-----------------|------------|
| Windows & Linux | [Preview Feed](https://ci.appveyor.com/nuget/core-0mhe40ifodk8)

### On Windows

```
build.cmd
```

Compilation requires a C# 9 compiler, an up-to-date .NET Core SDK, and MSBuild 15+ (which should be included in the former).

Running the unit tests additionally requires the .NET Framework 4.6.2+ as well as the .NET Core 2.1, 3.1 and 6.0 runtimes to be installed. (If you do not have all of those installed, you can run the tests for a specific target framework using `dotnet test -f <framework>`.)

These requirements should be covered by Visual Studio 2022 and the .NET 6 SDK.

### On Linux

```
./build.sh
```

Compilation requires an up-to-date .NET Core SDK.

Running the unit tests additionally requires the .NET Core 3.1 and 6.0 runtimes to be installed, as well as either Docker or Mono. For the latter, we recommend Mono 5.10+, though older versions (4.6.1+) might still work as well.

:information_source: **Mono runtime support:** Castle Core runs with minor limitations and defects on Mono 4.0.2+ (however 4.6.1+ is highly recommended, or 5.10+ if your code uses new C# 7.x language features such as `in` parameters).

We test against up-to-date Mono versions in order to fix known defects as soon as possible. Because of this, if you are using an older Mono version than our Continuous Integration (CI) build, you might see some unit tests fail.

For known Mono defects, check [our issue tracker](https://github.com/castleproject/Core/issues?utf8=%E2%9C%93&q=is%3Aissue%20is%3Aopen%20mono), as well as unit tests marked with `[Platform(Exclude = "Mono", ...)]` in the source code.

### Conditional Compilation Symbols

The following conditional compilation symbols (vertical) are currently defined for each of the build configurations (horizontal):

Symbol                              | .NET 4.6.2         | .NET Standard 2.x and .NET 6
----------------------------------- | ------------------ | ----------------------------
`FEATURE_APPDOMAIN`                 | :white_check_mark: | :no_entry_sign:
`FEATURE_ASSEMBLYBUILDER_SAVE`      | :white_check_mark: | :no_entry_sign:
`FEATURE_SERIALIZATION`             | :white_check_mark: | :no_entry_sign:
`FEATURE_SYSTEM_CONFIGURATION`      | :white_check_mark: | :no_entry_sign:

* `FEATURE_APPDOMAIN` - enables support for features that make use of an AppDomain in the host.
* `FEATURE_ASSEMBLYBUILDER_SAVE` - enabled support for saving the dynamically generated proxy assembly.
* `FEATURE_SERIALIZATION` - enables support for serialization of dynamic proxies and other types.
* `FEATURE_SYSTEM_CONFIGURATION` - enables features that use System.Configuration and the ConfigurationManager.
