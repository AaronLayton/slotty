# 🎯 Slotty

<div align="center">

![Slotty Logo](Slotty/icon.png)

**A modern, flexible content slot system for ASP.NET Core that replaces the limitations of `@RenderSection`**

[![NuGet Version](https://img.shields.io/nuget/v/Slotty?style=flat-square&logo=nuget)](https://www.nuget.org/packages/Slotty)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Slotty?style=flat-square&logo=nuget)](https://www.nuget.org/packages/Slotty)
[![Build Status](https://img.shields.io/github/actions/workflow/status/AaronLayton/slotty/ci.yml?branch=main&style=flat-square&logo=github)](https://github.com/AaronLayton/slotty/actions)
[![License](https://img.shields.io/github/license/AaronLayton/slotty?style=flat-square)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-6%2B-purple?style=flat-square&logo=dotnet)](https://dotnet.microsoft.com)

</div>

---

## 🚀 Why Slotty?

Tired of the rigid, error-prone `@RenderSection` system? **Slotty** brings modern content management to ASP.NET Core with a flexible, component-based approach that just works.

### The Problem with `@RenderSection`

```csharp
// ❌ Old way: Rigid, error-prone, limited
@RenderSection("Scripts", required: false)
@RenderSection("Styles", required: false)

// Must define sections in exact layout hierarchy
// No dynamic content injection
// Poor error handling
// Limited extensibility
```

### The Slotty Solution

```html
<!-- ✅ New way: Flexible, powerful, intuitive -->
<slot name="head"></slot>
<slot name="scripts"></slot>

<!-- Multiple content blocks, extensions, fallbacks -->
<slot name="sidebar">
    <div>Default sidebar content</div>
</slot>
```

## ✨ Features

- 🎯 **Multiple Content Sources** - Add content to slots from anywhere in your application
- 🔧 **Slot Extensions** - Automatic `:before` and `:after` extension points
- 🛡️ **Type Safety** - Built-in validation with helpful error messages
- 🎨 **Development Tools** - Visual slot debugging in development mode
- 📦 **Zero Configuration** - Works out of the box with sensible defaults
- 🚀 **High Performance** - Request-scoped content management
- 🔄 **Backward Compatible** - Migrate gradually from `@RenderSection`

## 📋 Table of Contents

- [Installation](#-installation)
- [Quick Start](#-quick-start)
- [Configuration](#-configuration)
- [Basic Usage](#-basic-usage)
- [Advanced Examples](#-advanced-examples)
- [Development Tools](#-development-tools)
- [Migration Guide](#-migration-guide)
- [API Reference](#-api-reference)
- [Contributing](#-contributing)

## 📦 Installation

### Package Manager Console
```powershell
Install-Package Slotty
```

### .NET CLI
```bash
dotnet add package Slotty
```

### PackageReference
```xml
<PackageReference Include="Slotty" Version="1.0.0" />
```

## ⚡ Quick Start

### 1. Register Services

```csharp
// Program.cs
builder.Services.AddSlotty();
app.UseSlotty(); // For automatic dev tools injection
```

### 2. Define Slots in Layout

```html
<!-- Views/Shared/_Layout.cshtml -->
<!DOCTYPE html>
<html>
<head>
    <slot name="head">
        <!-- Fallback content -->
        <title>Default Title</title>
    </slot>
</head>
<body>
    <header>
        <slot name="header"></slot>
    </header>
    
    <main>
        @RenderBody()
    </main>
    
    <slot name="scripts"></slot>
</body>
</html>
```

### 3. Fill Slots from Views

```html
<!-- Views/Home/Index.cshtml -->
<fill slot="head">
    <title>Home Page</title>
    <meta name="description" content="Welcome to our site">
</fill>

<fill slot="header">
    <nav>Navigation content</nav>
</fill>

<h1>Welcome!</h1>

<fill slot="scripts">
    <script src="~/js/home.js"></script>
</fill>
```

## ⚙️ Configuration

### Basic Configuration

```csharp
// Use default settings (recommended)
builder.Services.AddSlotty();
```

### Advanced Configuration

```csharp
// Program.cs - Programmatic configuration
builder.Services.AddSlotty(options =>
{
    options.ValidationMode = SlottyValidationMode.Log; // Silent, Log, Throw
    options.DevToolsInjection = SlottyDevToolsInjectionMode.Auto; // Auto, Always, Never
});
```

### Configuration via appsettings.json

```json
{
  "Slotty": {
    "ValidationMode": "Log",
    "DevToolsInjection": "Auto"
  }
}
```

```csharp
// Program.cs
builder.Services.AddSlotty(builder.Configuration.GetSection("Slotty"));
```

### Validation Modes

| Mode | Description | Use Case |
|------|-------------|----------|
| `Silent` | Basic slot name validation only | Production environments |
| `Log` | Validation with warning logs | Staging/testing environments |
| `Throw` | Strict validation with exceptions | Development environments |

## 📖 Basic Usage

### Simple Slots

```html
<!-- Define a slot -->
<slot name="sidebar"></slot>

<!-- Fill the slot -->
<fill slot="sidebar">
    <div class="widget">Content here</div>
</fill>
```

### Slots with Fallbacks

```html
<!-- Slot with default content -->
<slot name="breadcrumbs">
    <nav>
        <a href="/">Home</a>
    </nav>
</slot>

<!-- Override when needed -->
<fill slot="breadcrumbs">
    <nav>
        <a href="/">Home</a> > 
        <a href="/products">Products</a> > 
        <span>iPhone</span>
    </nav>
</fill>
```

### Multiple Content Blocks

```html
<!-- Multiple fills for the same slot -->
<fill slot="styles">
    <link rel="stylesheet" href="~/css/components.css">
</fill>

<fill slot="styles">
    <link rel="stylesheet" href="~/css/page-specific.css">
</fill>

<!-- Both stylesheets will be rendered -->
<slot name="styles"></slot>
```

### Slot Extensions

Every slot automatically gets `:before` and `:after` extension points:

```html
<!-- Layout -->
<slot name="content"></slot>

<!-- Page fills -->
<fill slot="content:before">
    <div class="alert">Important notice!</div>
</fill>

<fill slot="content">
    <h1>Main Content</h1>
</fill>

<fill slot="content:after">
    <div class="related-links">Related: ...</div>
</fill>
```

## 🔥 Advanced Examples

### Dynamic E-commerce Layout

```html
<!-- _Layout.cshtml -->
<!DOCTYPE html>
<html>
<head>
    <slot name="head">
        <title>My Store</title>
    </slot>
    <slot name="meta"></slot>
</head>
<body>
    <header>
        <slot name="header:before"></slot>
        <slot name="navigation"></slot>
        <slot name="header:after"></slot>
    </header>
    
    <div class="container">
        <aside>
            <slot name="sidebar">
                <div>Default sidebar</div>
            </slot>
        </aside>
        
        <main>
            <slot name="content:before"></slot>
            @RenderBody()
            <slot name="content:after"></slot>
        </main>
    </div>
    
    <footer>
        <slot name="footer"></slot>
    </footer>
    
    <slot name="scripts:before"></slot>
    <slot name="scripts"></slot>
    <slot name="scripts:after"></slot>
</body>
</html>
```

### Product Page with Rich Content

```html
<!-- Views/Products/Details.cshtml -->
<fill slot="head">
    <title>@Model.Name - My Store</title>
</fill>

<fill slot="meta">
    <meta name="description" content="@Model.Description">
    <meta property="og:title" content="@Model.Name">
    <meta property="og:image" content="@Model.ImageUrl">
</fill>

<fill slot="header:before">
    <div class="sale-banner">🔥 Flash Sale: 50% Off!</div>
</fill>

<fill slot="navigation">
    <nav>
        <a href="/">Home</a> > 
        <a href="/products">Products</a> > 
        @Model.Category.Name
    </nav>
</fill>

<fill slot="sidebar">
    <div class="filters">
        <h3>Filter Products</h3>
        <!-- Filter UI -->
    </div>
    
    <div class="recent-products">
        <h3>Recently Viewed</h3>
        <!-- Recent products -->
    </div>
</fill>

<!-- Main product content -->
<div class="product-details">
    <h1>@Model.Name</h1>
    <div class="price">$@Model.Price</div>
    <!-- Product details -->
</div>

<fill slot="content:after">
    <section class="related-products">
        <h2>You Might Also Like</h2>
        <!-- Related products -->
    </section>
    
    <section class="reviews">
        <h2>Customer Reviews</h2>
        <!-- Reviews -->
    </section>
</fill>

<fill slot="scripts">
    <script src="~/js/product-gallery.js"></script>
    <script src="~/js/add-to-cart.js"></script>
</fill>
```

### Conditional Content with View Components

```html
<!-- Fill slots from View Components -->
<fill slot="sidebar:before">
    @await Component.InvokeAsync("ShoppingCart")
</fill>

<fill slot="footer">
    @if (User.Identity.IsAuthenticated)
    {
        @await Component.InvokeAsync("UserAccount")
    }
    else
    {
        @await Component.InvokeAsync("LoginPrompt")
    }
</fill>
```

### Partial Views with Slots

```html
<!-- _ProductCard.cshtml -->
<div class="product-card">
    <slot name="product-image">
        <img src="@Model.DefaultImage" alt="@Model.Name">
    </slot>
    
    <div class="product-info">
        <h3>@Model.Name</h3>
        <div class="price">$@Model.Price</div>
        
        <slot name="product-actions">
            <button class="btn btn-primary">Add to Cart</button>
        </slot>
    </div>
    
    <slot name="product-badges"></slot>
</div>

<!-- Usage -->
@Html.Partial("_ProductCard", product)

<fill slot="product-badges">
    @if (product.IsOnSale)
    {
        <span class="badge sale">SALE</span>
    }
</fill>

<fill slot="product-actions">
    <button class="btn btn-primary" data-product="@product.Id">
        Quick Add - $@product.Price
    </button>
    <button class="btn btn-secondary">
        <i class="heart"></i> Wishlist
    </button>
</fill>
```

## 🛠️ Development Tools

### Automatic Dev Tools

In development mode, Slotty automatically injects visual debugging tools:

```html
<!-- Automatically injected in development -->
<style>
    .slotty-slot-wrapper { border: 2px dashed #007bff; margin: 2px; }
    .slotty-slot-label { background: #007bff; color: white; font-size: 12px; }
</style>

<script>
    // Interactive slot debugging
    console.log('Slotty dev tools loaded');
</script>
```

### Head-Safe Debugging

Slots in `<head>` use HTML comments for valid markup:

```html
<head>
    <!-- SLOTTY: head (HAS CONTENT) -->
    <title>My Page</title>
    <meta charset="utf-8">
    <!-- /SLOTTY: head -->
</head>
```

### Configuration

```csharp
// Control dev tools injection
builder.Services.AddSlotty(options =>
{
    options.DevToolsInjection = SlottyDevToolsInjectionMode.Always; // Always show
    // or SlottyDevToolsInjectionMode.Never;  // Never show
    // or SlottyDevToolsInjectionMode.Auto;   // Development only (default)
});
```

## 🔄 Migration Guide

### From @RenderSection

**Before:**
```html
<!-- Layout -->
@RenderSection("Scripts", required: false)
@RenderSection("Styles", required: false)

<!-- Page -->
@section Scripts {
    <script src="~/js/page.js"></script>
}

@section Styles {
    <link rel="stylesheet" href="~/css/page.css">
}
```

**After:**
```html
<!-- Layout -->
<slot name="scripts"></slot>
<slot name="styles"></slot>

<!-- Page -->
<fill slot="scripts">
    <script src="~/js/page.js"></script>
</fill>

<fill slot="styles">
    <link rel="stylesheet" href="~/css/page.css">
</fill>
```

### Benefits of Migration

| @RenderSection | Slotty | Benefit |
|----------------|--------|---------|
| Single content per section | Multiple fills per slot | ✅ Flexible content composition |
| Must be defined in hierarchy | Fill from anywhere | ✅ True component architecture |
| Required/optional only | Rich validation modes | ✅ Better error handling |
| No extensibility | `:before` and `:after` extensions | ✅ Built-in extensibility |
| No default content | Fallback content support | ✅ Progressive enhancement |

## 📚 API Reference

### TagHelpers

#### `<slot>`
Defines a content slot that can be filled by `<fill>` tags.

**Attributes:**
- `name` (required): The unique name of the slot

**Example:**
```html
<slot name="sidebar">
    <div>Default content</div>
</slot>
```

#### `<fill>`
Adds content to a named slot.

**Attributes:**
- `slot` (required): The name of the slot to fill

**Example:**
```html
<fill slot="sidebar">
    <div>Custom sidebar content</div>
</fill>
```

### Configuration

#### `SlottyOptions`

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `ValidationMode` | `SlottyValidationMode` | `Silent` | Validation behavior |
| `DevToolsInjection` | `SlottyDevToolsInjectionMode` | `Auto` | Dev tools injection mode |

#### `SlottyValidationMode`

| Value | Description |
|-------|-------------|
| `Silent` | Basic validation only |
| `Log` | Validation with warning logs |
| `Throw` | Strict validation with exceptions |

#### `SlottyDevToolsInjectionMode`

| Value | Description |
|-------|-------------|
| `Auto` | Inject in development environment only |
| `Always` | Always inject dev tools |
| `Never` | Never inject dev tools |

### Service Registration

```csharp
// Basic registration
services.AddSlotty();

// With configuration
services.AddSlotty(options => { /* configure */ });

// From configuration section
services.AddSlotty(configuration.GetSection("Slotty"));
```

### Middleware Registration

```csharp
// Enable automatic dev tools injection
app.UseSlotty();
```

## 🤝 Contributing

We love contributions! Please see our [Contributing Guide](CONTRIBUTING.md) for details.

### Development Setup

```bash
git clone https://github.com/AaronLayton/slotty.git
cd slotty
dotnet restore
dotnet build
dotnet test
```

### Running Examples

```bash
dotnet run --project Slotty.Example
```

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🌟 Support

- ⭐ **Star this repo** if you find it useful
- 🐛 **Report bugs** via [GitHub Issues](https://github.com/AaronLayton/slotty/issues)
- 💡 **Request features** via [GitHub Discussions](https://github.com/AaronLayton/slotty/discussions)
- 📖 **Documentation** at [GitHub Wiki](https://github.com/AaronLayton/slotty/wiki)

---

<div align="center">

Made with ❤️ by [Aaron Layton](https://github.com/AaronLayton) • Follow on [X](https://x.com/aaronlayton)

**[⬆ Back to Top](#-slotty)**

</div> 