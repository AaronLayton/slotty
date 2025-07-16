# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- Enhanced NuGet package metadata
- Automatic versioning using MinVer
- Source Link support for debugging
- Package validation and symbols
- Deterministic builds

### Changed
- Improved package description and tags
- Updated project URLs and metadata
- Updated test project to target .NET 9.0 for better CI compatibility
- Set example project as non-packable to prevent packaging warnings

### Fixed
- Fixed GitHub Actions CI workflow by adding missing checkout steps for global.json access
- Fixed NuGet package validation by switching to embedded symbols instead of separate .snupkg files

## [1.0.0] - 2024-12-27

### Added
- Initial release of Slotty package
- Slot-based content injection using `<slot>` and `<fill>` TagHelpers
- Support for multiple content blocks per slot
- Fallback content support for empty slots
- Per-request content storage using HttpContext.Items
- **Custom Exception Types**
  - `SlotNotFoundException` for missing slot references
  - `InvalidSlotNameException` for invalid slot naming
- **Validation System**
  - Three validation modes: `Silent`, `Log`, `Throw`
  - Slot name pattern validation with regex support
  - Orphaned fills detection (fills without corresponding slots)
  - Usage tracking for defined vs filled slots
- **Development Tools**
  - Automatic CSS/JS injection middleware
  - Context-aware dev mode wrappers (HTML comments for head slots, div wrappers for body slots)
  - Three injection modes: `Auto` (dev only), `Always`, `Never`
- **Configuration System**
  - Multiple configuration approaches: programmatic, appsettings.json, and default
  - Dependency injection integration with `AddSlotty()` and `UseSlotty()`
  - Options pattern support with `IOptions<SlottyOptions>`
- **Slot Extensions**
  - Support for `:before` and `:after` slot modifiers
  - Slot name validation with pattern `^[a-zA-Z][a-zA-Z0-9_-]*(?::(?:before|after))?$`
- **Developer Experience**
  - Comprehensive unit tests (92 tests covering all functionality)
  - Full XML documentation with examples
  - Development mode visualization with CSS and JavaScript
  - Detailed logging for slot registration and content addition

### Features
- HTML-like TagHelper syntax: `<slot name="header" />` and `<fill slot="header">content</fill>`
- Support for slot extensions: `<slot name="header:before" />`
- Nested slot support for complex layouts
- Integration with ASP.NET Core Razor views and MVC
- Cross-platform compatibility (.NET 6.0+)
- Backward compatibility - silent mode maintains existing behavior
- Performance optimized with minimal overhead in production
- Head-safe development tools that don't break HTML validation

[Unreleased]: https://github.com/AaronLayton/slotty/compare/v1.0.0...HEAD
[1.0.0]: https://github.com/AaronLayton/slotty/releases/tag/v1.0.0 