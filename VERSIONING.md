# Automatic Versioning Guide

This project uses [MinVer](https://github.com/adamralph/minver) for automatic semantic versioning based on Git history.

## How It Works

MinVer automatically determines the version based on:
1. **Git tags** - Version tags in the format `v1.2.3`
2. **Commit count** - Number of commits since the last version tag
3. **Branch name** - Different behavior for main vs feature branches
4. **Git commit SHA** - Added as metadata for pre-release versions

## Version Format

- **Release versions**: `1.2.3` (when building from a version tag)
- **Pre-release versions**: `1.2.4-alpha.0.5+abc1234` (when building from commits after a tag)

## Creating Releases

### Option 1: GitHub Releases (Recommended)
1. Go to your GitHub repository
2. Click "Releases" → "Create a new release"
3. Create a new tag (e.g., `v1.0.0`)
4. Fill in release title and description
5. Publish the release
6. GitHub Actions will automatically build and publish to NuGet

### Option 2: Git Tags
```bash
# Create and push a version tag
git tag v1.0.0
git push origin v1.0.0
```

## Version Increments

Follow [Semantic Versioning](https://semver.org/):

- **MAJOR** (`v2.0.0`) - Breaking changes
- **MINOR** (`v1.1.0`) - New features (backwards compatible)
- **PATCH** (`v1.0.1`) - Bug fixes (backwards compatible)

## Examples

```bash
# Current state: Last tag was v1.0.0, 3 commits ahead
# MinVer will generate: 1.0.1-alpha.0.3+abc1234

# After tagging v1.1.0:
git tag v1.1.0
git push origin v1.1.0
# MinVer will generate: 1.1.0 (exact version)

# Next commit after v1.1.0:
# MinVer will generate: 1.1.1-alpha.0.1+def5678
```

## Build Locally

To see what version MinVer would generate:

```bash
dotnet build --verbosity normal
# Look for: "MinVer: Using version X.Y.Z"
```

## Configuration

MinVer is configured in `Slotty.csproj`:
- No custom configuration needed for basic usage
- Version is automatically included in NuGet package metadata
- Works with deterministic builds in CI/CD

## First Release

For the very first release:
1. Make sure all code is ready
2. Create a release with tag `v1.0.0`
3. MinVer will use this as the baseline for all future versions

## Troubleshooting

- **Missing version tags**: Ensure tags are in `v*` format (e.g., `v1.0.0`)
- **Local vs CI differences**: CI uses `fetch-depth: 0` to get full Git history
- **Pre-release versions**: Normal for development builds between releases 