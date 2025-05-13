# Slotty Package Improvements TODO

A comprehensive list of improvements to implement for the Slotty package based on .NET best practices.

## 🔥 High Priority Items

### Release Management
- [ ] **Create first release to publish to NuGet**
  - [x] Add your logo as `Slotty/icon.png` (128x128px PNG recommended)
  - [x] Uncomment the icon reference in `Slotty.csproj` 
  - [ ] Create Git tag `v1.0.0` to trigger first release
  - [ ] Push tag to GitHub: `git push origin v1.0.0`
  - [ ] Verify package appears on NuGet.org after GitHub Action completes

### Core Architecture Improvements
- [x] **Add input validation and content sanitization**
  - [x] Add argument validation for slot names and content
  - [x] Add validation for slot naming conventions
  - [x] ~~Implement HTML sanitization~~ *Decision: Content sanitization is developer's responsibility*

- [x] **Implement proper error handling**
  - [x] Create custom exception types (`SlotNotFoundException`, `InvalidSlotNameException`)
  - [x] Replace generic exceptions with specific ones where appropriate  
  - [x] Add proper error messages for development vs production
  - [x] Add configurable validation modes (Silent, Log, Throw)

- [x] **Add comprehensive validation and configuration system**
  - [x] Create `SlottyOptions` configuration class with simplified single ValidationMode option
  - [x] Add `SlotValidationMode` enum (Silent, Log, Throw)
  - [x] Add slot usage tracking functionality (enabled when ValidationMode != Silent)
  - [x] Add orphaned fills validation
  - [x] Add slot name pattern validation with `SlotNameValidator` class
  - [x] Support for `:before` and `:after` slot extensions

- [ ] **Add comprehensive integration tests**
  - [ ] Create integration tests using TestHost
  - [ ] Test TagHelper rendering in various scenarios
  - [ ] Test development mode rendering
  - [ ] Test error conditions and edge cases

## 🚀 Medium Priority Items

### Performance & Code Quality
- [ ] **Performance optimizations**
  - [ ] Replace string concatenation with StringBuilder in TagHelpers
  - [ ] Use StringSegment or ReadOnlySpan<char> for string operations
  - [ ] Implement content caching for repeated renders
  - [ ] Add BenchmarkDotNet performance tests

- [x] **Enhanced package metadata**
  - [ ] Add package icon (place `icon.png` in `Slotty/` folder and uncomment in .csproj)
  - [x] Add PackageReleaseNotes and other metadata
  - [x] Enable EnablePackageValidation
  - [x] Implement semantic versioning with MinVer or GitVersion

- [ ] **Code analysis and quality tools**
  - [ ] Add StyleCop.Analyzers
  - [ ] Enable TreatWarningsAsErrors
  - [ ] Add SonarAnalyzer.CSharp
  - [ ] Improve nullable reference type annotations

- [ ] **Logging integration**
  - [ ] Add ILogger to TagHelpers
  - [ ] Log slot rendering for debugging
  - [ ] Add performance logging in development mode

### Project Structure
- [ ] **Add essential configuration files**
  - [ ] Create `Directory.Build.props` for shared MSBuild properties
  - [x] Add `global.json` to pin .NET SDK version
  - [ ] Add `.editorconfig` for consistent coding standards
  - [x] Create `CHANGELOG.md` for version history
  - [ ] Add `CONTRIBUTING.md` for contributor guidelines

- [x] **CI/CD Pipeline**
  - [x] Create GitHub Actions for automated testing
  - [x] Add automated package publishing to NuGet
  - [ ] Add security scanning with CodeQL
  - [ ] Test on multiple .NET versions

## 🎯 Low Priority Items

### Advanced Features
- [x] **Configuration system**
  - [x] Create `SlottyOptions` class for configuration
  - [x] Support custom slot naming conventions (via `SlotNameValidator`)
  - [x] ~~Add configurable content sanitization options~~ *Decision: Content sanitization is developer's responsibility*
  - [ ] Add options for development mode behavior

- [ ] **Modern .NET practices**
  - [ ] Make content retrieval async where beneficial
  - [ ] Use IAsyncEnumerable<T> for large content collections
  - [ ] Implement IDisposable for content stores if needed
  - [ ] Consider object pooling for frequently created objects

- [ ] **Enhanced developer experience**
  - [ ] Improve CSS with better custom properties
  - [ ] Add TypeScript definitions for JavaScript
  - [ ] Improve accessibility (ARIA labels, keyboard navigation)
  - [ ] Consider browser extension for slot visualization

### Documentation & Build
- [ ] **Enhanced documentation**
  - [ ] Create API documentation with DocFX
  - [ ] Add inline documentation generation for NuGet
  - [ ] Improve README with more examples
  - [ ] Add troubleshooting guide

- [ ] **Build optimization**
  - [ ] Enable Source Link for debugging
  - [ ] Add deterministic builds
  - [ ] Include symbols packages
  - [ ] Optimize for NativeAOT compatibility

## 📋 Code-Specific Improvements

### SlotContentStore Refactoring
- [ ] Move from static class to interface-based service
- [ ] Add thread-safety considerations
- [ ] Implement proper disposal patterns
- [ ] Add content validation and sanitization

### TagHelper Improvements
- [ ] Add proper async/await patterns
- [ ] Implement better error messages
- [ ] Add performance optimizations
- [ ] Improve development mode rendering

### Testing Enhancements
- [ ] Add more edge case tests
- [ ] Test concurrent access scenarios
- [ ] Add performance regression tests
- [ ] Test with different hosting environments

## 🏁 Completed Items

### ✅ Enhanced Package Metadata & Versioning
- [x] **Enhanced NuGet package metadata** - Added comprehensive package information, licensing, URLs, and validation
- [x] **Automatic versioning with MinVer** - Implemented GitVersion-based semantic versioning
- [x] **Source Link support** - Added debugging support with GitHub source linking
- [x] **Package validation** - Enabled NuGet package validation and compatibility checks
- [x] **Deterministic builds** - Configured for reproducible builds in CI/CD
- [x] **Symbol packages** - Added .snupkg symbol package generation
- [x] **Target Framework Strategy** - Package targets .NET 6.0 for max compatibility, Example targets .NET 9.0 for latest features

### ✅ CI/CD Pipeline Setup
- [x] **GitHub Actions workflow** - Updated to latest action versions (v4)
- [x] **Automated testing** - Tests run on all PRs and main branch
- [x] **NuGet package validation** - Automated validation using Meziantou tool
- [x] **Automated publishing** - Publishes to NuGet on releases and version tags
- [x] **Global.json configuration** - Pinned .NET SDK version for consistency

### ✅ Project Configuration
- [x] **Global.json** - Added SDK version pinning
- [x] **CHANGELOG.md** - Created changelog following Keep a Changelog format

### ✅ Core Architecture Improvements  
- [x] **Converted static SlotContentStore to DI-based service** - Created `ISlotContentStore` interface, implemented as scoped service, updated all TagHelpers to use dependency injection
- [x] **Added comprehensive input validation** - Added proper argument validation for slot names and content with meaningful error messages  
- [x] **Enhanced test coverage** - Updated all tests to use new DI-based approach and added extensive validation tests (77 total tests)
- [x] **Implemented custom exception types** - Created `SlotNotFoundException` and `InvalidSlotNameException` for specific error handling scenarios
- [x] **Added configurable validation system** - Created simplified `SlottyOptions` configuration with single `ValidationMode` option, slot usage tracking, and orphaned fills validation
- [x] **Enhanced ServiceCollectionExtensions** - Added overloads for configuring Slotty with custom options or configuration sections
- [x] **Created dedicated slot name validator** - Implemented `SlotNameValidator` class with regex pattern `^[a-zA-Z][a-zA-Z0-9_-]*(?::(?:before|after))?$` supporting `:before` and `:after` extensions only
- [x] **Simplified validation behavior** - Slot name validation always enforced; tracking and orphaned fills detection only when ValidationMode != Silent
- [x] **Maintained backward compatibility** - All existing code continues to work unchanged with new validation system

---

## Notes
- Items are organized by priority: High → Medium → Low
- Check off items as they are completed
- Move completed items to the "Completed Items" section
- Add specific implementation notes or decisions as comments
- Update this file as new requirements or improvements are identified

### Recent Implementation Notes
- **Validation System (v1.1.0)**: Implemented comprehensive validation with `SlotValidationMode` options:
  - `Silent` (default): Basic slot name validation only - maintains backward compatibility
  - `Log`: Full validation with warning logs for orphaned fills  
  - `Throw`: Full validation with exceptions for orphaned fills
- **Slot Name Conventions**: Always enforced pattern `^[a-zA-Z][a-zA-Z0-9_-]*(?::(?:before|after))?$`
- **Content Sanitization**: Explicitly excluded - developers maintain full control over content 