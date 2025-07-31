# Xperience Community: Admin Extensions

[![Nuget](https://img.shields.io/nuget/v/XperienceCommunity.AdminExtensions)](https://www.nuget.org/packages/XperienceCommunity.AdminExtensions)

## Description

This package provides useful extensions to the Kentico Xperience administration interface. The extensions are automatically registered once the package is installed, enhancing the admin experience with additional functionality and improved workflows.

## Library Version Matrix

| Xperience Version | Library Version |
|-------------------|-----------------|
| >= 30.6.0         | >= 1.0.0        |

> **Note:** The latest version that has been tested is 30.6.0

## ⚙️ Package Installation

Add the package to your application using the .NET CLI

```bash
dotnet add package XperienceCommunity.AdminExtensions
```

## 🚀 Quick Start

No additional configuration is required! Once the package is installed, the admin extensions will be automatically registered and available in your Kentico Xperience administration interface.

## ⚙️ Configuration

The package supports optional configuration to customize certain features. Add the following section to your `appsettings.json`:

```json
{
  "XperienceCommunityAdminExtensions": {
    "ContentHubListPageSize": 100
  }
}
```

### Configuration Options

| Setting | Description | Default Value |
|---------|-------------|---------------|
| `ContentHubListPageSize` | Sets the number of items displayed per page in the Content Hub list | `50` |

## ✨ Features

### Event Log Enhancements

**Clear Event Log Button**: Adds a convenient "Clear" button to the Event Log page header, allowing administrators to quickly clear all event log entries with a single click.

### Content Hub Enhancements

**Custom Page Size**: Allows configuration of the number of items displayed per page in the Content Hub list. This helps administrators manage large content repositories more efficiently by customizing the page size to their preference.

### Content Type List Enhancements

**Content Type Filtering**: Adds advanced filtering capabilities to the Content Type list page, allowing administrators to filter content types by their usage:
- **Website** - Content types used for website pages
- **Reusable** - Content types used for reusable content items
- **Email** - Content types used for email campaigns
- **Headless** - Content types used for headless/API content delivery

The filter supports multi-selection, enabling administrators to view content types across multiple usage categories simultaneously.

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## 📄 License

This project is licensed under the MIT License.