# BetterDiscordAutoInstaller

BetterDiscord 自动安装器。在 Discord 启动时自动下载并安装 BetterDiscord，无需手动操作。

## 工作原理

本项目是一个 .NET Framework 类库，通过 AppDomainManager 注入机制在 Discord 的 `Update.exe` 进程中加载运行。具体流程如下：

1. **AppDomainManager 注入**: `Update.exe.config` 将 `BetterDiscordInstaller.Injector` 注册为 AppDomainManager。当 `Update.exe` 启动 .NET 运行时，CLR 会自动加载 `BetterDiscordInstaller.dll` 并调用 `InitializeNewDomain` 方法。

2. **Harmony 方法补丁**: 在 `InitializeNewDomain` 中，程序使用 [Harmony](https://github.com/pardeike/Harmony) 对 `Process.Start` 方法进行运行时补丁。

3. **拦截 Discord 启动**: 当 `Update.exe` 通过 `--processStart Discord.exe` 启动 Discord 主程序时，补丁会拦截该 `Process.Start` 调用。

4. **下载 BetterDiscord**: 程序检查 BetterDiscord 的 asar 文件是否已存在。如果不存在，则从 GitHub Release 下载最新的 `betterdiscord.asar`。

5. **注入 require 语句**: 在 Discord 的 `modules/discord_desktop_core-1/discord_desktop_core/index.js` 文件开头插入 `require("path/to/betterdiscord.asar")`，使 Discord 在启动时加载 BetterDiscord。

## 使用方法

1. 将 `BetterDiscordInstaller.dll` 和 `Update.exe.config` 复制到 Discord 安装根目录（即 `Update.exe` 所在目录）。

   > Discord 默认安装路径通常为 `%LocalAppData%\Discord`，但不同版本（如 PTB、Canary）路径可能不同。请确保放入正确的目录。

2. 正常启动 Discord。

3. BetterDiscord 将在首次启动时自动下载并安装。后续启动会自动跳过已完成安装的检测。

## 从源码构建

### 需求

- [.NET SDK](https://dotnet.microsoft.com/download) 9.0 或更高版本
- Discord 桌面客户端

### 构建步骤

```bash
git clone https://github.com/FrostyTwilight/BetterDiscordAutoInstaller.git
cd BetterDiscordAutoInstaller
dotnet build BetterDiscordInstaller/BetterDiscordInstaller.csproj -c Release
```

构建产物位于 `BetterDiscordInstaller/bin/Release/net472/` 目录下：

- `BetterDiscordInstaller.dll` — 核心注入 DLL
- `Update.exe.config` — AppDomainManager 注册配置

> **说明**: 该项目目标框架为 .NET Framework 4.7.2 (`net472`)。.NET SDK 9.0+ 支持跨目标框架构建，可以正常编译此项目。

## 许可证

本项目基于 MIT License 开源。

Copyright (c) 2026 FrostyTwilight

详见 [LICENSE](LICENSE) 文件。

## 免责声明

本工具为第三方独立项目，与 Discord Inc. 及 BetterDiscord 项目无关。使用本工具可能违反 Discord 的服务条款，请自行承担风险。作者不对因使用本工具造成的任何后果负责。
