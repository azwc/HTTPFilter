# 发版流程

本仓库用 GitHub Actions 自动构建，工作流在 [.github/workflows/build.yml](.github/workflows/build.yml)。
下面是从准备到发布、再到出问题时怎么救的完整流程。

## TL;DR

1. **首次（或改过工作流后）**：把 `.github/workflows/` 等改动提交并推到 `main`，见 [第 1 节](#1-提交并推送工作流首次--改过工作流后)。
2. **一次性**：仓库设置里把 Workflow permissions 改成 Read and write（见 [2.1](#21-开启-actions-写权限必做)）。
3. 打标签并推送：`git tag -a v1.0.0 -m "v1.0.0"` → `git push origin v1.0.0`。
4. 等 Actions 里的 `Build (win-x64)` 变绿（约 5–10 分钟）。
5. 打开 Actions 运行页底部，确认 Artifacts 里两个 zip。
6. 回仓库 Releases 页，确认自动生成的 Release 挂上两个 zip。
7. 下载 `self-contained` 包，解压运行，核对 `version.txt`。

---

## 1. 提交并推送工作流（首次 / 改过工作流后）

GitHub 只会执行**标签所指向的那个提交里**存在的工作流文件。所以顺序必须是：

```bash
git add .github/workflows/build.yml RELEASING.md .gitignore
git commit -m "ci: 添加 GitHub Actions 自动构建与发版流程"
git push origin main
```

- 推到 `main` 会先跑一次**普通构建**（只出 Artifacts、不发 Release），用它确认工作流本身没问题；
  这一步绿了再打标签，避免"发版失败还得反复删标签"。
- 如果你用 PR 流程：开分支 → 提 PR → 合进 `main` → 等 `main` 的构建绿 → 再打标签。
- **以后只改业务代码、不动工作流时，不需要这一步**，直接跳到 [第 5 节](#5-打标签并推送主流程) 打标签即可。
  但注意标签要指向包含最新代码的提交（通常就是 `main` 的最新提交）。

检查清单：`git status` 干净、`main` 上有 `.github/workflows/build.yml`、`main` 最近一次 Actions 运行是绿的。

---

## 2. 一次性准备

### 2.1 开启 Actions 写权限（必做）

`Settings` → `Actions` → `General` → 最下方 **Workflow permissions**

- 选 **Read and write permissions** → `Save`。

原因：工作流里的 `permissions: contents: write` 只能**收紧**、不能突破仓库设置。默认只读时，最后一步 `gh release create` 会以
`Resource not accessible by integration` / 403 失败，前面的编译打包都正常，只有发版那步挂。

### 2.2 其它前置

- 仓库的 Actions 功能处于启用状态（Fork 出来的仓库第一次要手动 Enable）。
- 在 **Fork** 里跑只能得到构建产物，发 Release 需要在原仓库有写权限；Fork 的 PR 拿不到写 token，属正常。
- 计费：公开仓库免费；私有仓库的 Windows runner 按 2 倍分钟计费，一次发版大约 5–10 分钟 × 2。

---

## 3. 版本号约定

| 项 | 规则 |
| --- | --- |
| 标签格式 | 必须 `v` 开头：`v1.0.0`、`v1.2.3`、`v1.2.3-beta.1` |
| 版本号来源 | 去掉前导 `v` 的标签名，用于 zip 名、Release 标题、包内 `version.txt` |
| 触发条件 | `push` 的标签匹配 `v*`；推 `1.0.0`（没有 v）**不会触发任何构建** |
| 预发布 | 标签里含 `-`（如 `v1.0.0-beta.1`）时自动加 `--prerelease`，不会顶掉 latest |
| 已发布标签 | **不要删除后重推同名标签、也不要移动标签**：下载链接、latest 会错乱；要改内容请发新版本号 |

产物命名示例（`v1.0.0`）：

```
HTTPFilter-1.0.0-win-x64-self-contained.zip       自包含，目标机无需装 .NET（推荐）
HTTPFilter-1.0.0-win-x64-framework-dependent.zip  需要 .NET 6 Desktop Runtime
```

---

## 4. 发版前检查

- [ ] 工作流文件已在 `main` 上，且 `main` 最近一次构建是绿的。
- [ ] 目标提交已合入 `main`。
- [ ] 关键功能自测：能启动捕获、能抓到 `/api/getGachaLog`、停止后系统代理恢复正常。
- [ ] 依赖或 `app.manifest` 有改动时，确认程序仍以管理员身份运行（`requireAdministrator`）。
- [ ] 确定版本号，并判断是否需要预发布。

可选：本地复现 CI 命令（需要 Windows + .NET 6 SDK），见 [第 12 节](#12-本地复现-ci-命令)。

---

## 5. 打标签并推送（主流程）

```bash
git switch main
git pull --ff-only

# 附注标签，消息就是版本说明
git tag -a v1.0.0 -m "v1.0.0"

# 只推这一个标签
git push origin v1.0.0
```

注意事项：

- 用 `git push origin v1.0.0`，**不要**顺手 `git push --tags`（会把本地所有历史标签一起推上去，可能触发多个发布）。
- 标签指向的提交里必须已经包含 `.github/workflows/build.yml`；指向工作流之前的老提交不会触发任何运行。
- 标签指向的提交可以在任何分支上，但建议指向 `main` 上的提交，避免 Release 里的自动提交列表混乱。
- 打错了先别推：`git tag -d v1.0.0` 删本地的即可。

---

## 6. 观察构建

进入 `Actions` → 左侧 `Build` → 点最新一次运行，job 名为 `Build (win-x64)`。

步骤顺序（与工作流一一对应）：

1. 检出代码
2. 安装 .NET 6 SDK
3. 打印 SDK 信息
4. 缓存 NuGet 包
5. 计算版本号 ← 这里能看到 `version` / `artifact` 两个值
6. 还原依赖（解决方案：HTTPFilter + Listener_Core）
7. 编译（Release，两个项目）
8. 发布 · 自包含（win-x64）
9. 发布 · 依赖运行时（win-x64）
10. 打包 ← 这里会打印两个 zip 的大小
11. 上传构建产物
12. 创建 / 更新 Release

耗时参考：首次（无缓存）约 6–10 分钟，`发布 · 自包含` 和 `打包` 最耗时；之后有 NuGet 缓存约 3–6 分钟。
整体超时上限 40 分钟（`timeout-minutes`）。

构建成功即可进行下一步；第 11 步的产物在运行页底部 **Artifacts**，保留 30 天。

---

## 7. 校验发布结果

1. 打开 `Releases` 页：应出现标题为 `v1.0.0` 的 Release，正文是根据提交自动生成的说明（可直接编辑）。
2. 资产应有 2 个 zip，名字与 [第 3 节](#3-版本号约定) 的格式一致。
3. 下载 `...-self-contained.zip` 解压，目录里应有：
   - `抽卡链接提取器.exe`
   - 若干原生 DLL（`wpfgfx_cor3.dll` 等，WPF 单文件发布不会把它们打进 exe）
   - `version.txt`，内容应为 `1.0.0`
4. 双击 exe：会弹 UAC（程序清单要求管理员权限），属正常；取消则无法工作。

给下载者的说明建议写进 Release 正文（或 README）：

- 首次运行会提示 **"Windows 已保护你的电脑"**（未签名 + SmartScreen），需点"更多信息 → 仍要运行"。
- 代理类工具容易被杀软误报，可提示加白名单。
- 程序运行时会修改系统 HTTP/HTTPS 代理；异常退出可能残留代理设置，需要在"Internet 选项 / 设置 → 代理"里手动关闭。

---

## 8. 其它发版方式

### 8.1 网页上先写说明再发

`Releases` → `Draft a new release` → 选择或新建 `v1.0.0` 标签 → 发布。
新建标签同样会触发 `push` 标签事件；工作流最后一步会 `gh release view` 命中已有 Release，然后 `--clobber` 覆盖上传两个 zip，不会重复建 Release。

### 8.2 只验证构建（不发版）

`Actions` → `Build` → `Run workflow`（手动触发）。
产物是 `HTTPFilter-dev-<短SHA>-win-x64-*.zip`，只留在 Actions Artifacts 里，不产生 Release。

### 8.3 重跑 / 换资产

- `Actions` 里该次运行右上角 **Re-run all jobs**：重新构建并覆盖同名资产（`--clobber`）。
- 只想换附件：`gh release upload v1.0.0 dist/*.zip --clobber`。

---

## 9. 故障排查

| 现象 | 原因 | 处理 |
| --- | --- | --- |
| 工作流文件推上去了，但 Actions 里看不到 `Build` | 文件路径不对（必须 `.github/workflows/*.yml`）；或 YAML 语法错误被 GitHub 拒绝 | 看仓库 `Actions` 页顶部是否有报错；确认路径与缩进 |
| 推标签后没有任何运行 | 标签不以 `v` 开头；标签指向的提交里没有工作流文件；Actions 被禁用；或在 Fork 里 | 检查标签名、`git show v1.0.0:.github/workflows/build.yml` 是否存在 |
| 最后一步报 403 / `Resource not accessible by integration` | 仓库 Workflow permissions 是只读 | 按 [2.1](#21-开启-actions-写权限必做) 改成 Read and write 后重跑 |
| `setup-dotnet` 无法解析 `6.0.x` | .NET 6 已 EOL，元数据/下载源变动 | 在 `build.yml` 里改成固定版本，如 `dotnet-version: '6.0.428'` |
| `NETSDK1045` / 找不到 `net6.0-windows` | 装成了 .NET 8/9 SDK，没有 6 | 确认 `安装 .NET 6 SDK` 步骤成功，必要时 `dotnet --list-sdks` 看日志 |
| `打包` 步骤超时或内存不足 | 自包含单文件较大（百 MB 级） | 把 `-CompressionLevel Optimal` 改成 `Fastest` |
| `gh: command not found` | runner 镜像变动 | 改用 `softprops/action-gh-release@v2` 发布 |
| zip 内中文文件名乱码 | 解压工具版本老 | 用资源管理器 / 新版 7-Zip、WinRAR 解压 |
| Release 正文只有标题、没有自动说明 | `--generate-notes` 首次发版找不到可对比的上一版本 | 正常，手动补写说明即可 |
| 编译有 CS1998/CS8618 警告 | `Listener_Core` 里 `async` 无 `await`、字段未初始化 | 只是警告，不影响构建 |

---

## 10. 撤回与补救

```bash
# 删远端 Release 并顺带删除标签（需要 gh 已登录）
gh release delete v1.0.0 --yes --cleanup-tag

# 删本地标签
git tag -d v1.0.0
```

- 删掉标签后重推同名标签会**重新触发**工作流，会重建 Release。
- 已经有人下载过的版本，不建议覆盖；请递增修订号发 `v1.0.1`，并在说明里注明替代关系。
- 仅删 Asset 不用删 Release：Release 页右上角编辑 → 删除附件，再上传新的。

---

## 11. 关于 exe 的版本号（已知限制）

`HTTPFilter.csproj` 设置了 `GenerateAssemblyInfo=false`，而 `AssemblyInfo.cs` 是 T4 的**预处理类**
（`TransformText()` 只在运行时生成字符串，编译期不会产生 `[assembly: AssemblyVersion]`）。
所以 exe 文件属性里的版本是默认值，**版本信息以包内 `version.txt` 和 Release 标题为准**。

如果希望 exe 属性跟随标签，需要一起改三处，我可以代做：

1. csproj 里去掉 `GenerateAssemblyInfo=false`（启用 SDK 自动生成），并把 `<AssemblyName>` 固定为
   `抽卡链接提取器`（现在写的是 `抽卡链接提取器$(AssemblyVersion)`，启用后会导致 exe 文件名带上版本号）；
2. 把 `[assembly: ThemeInfo]`、`[assembly: Guid]` 这些属性挪到普通 `.cs` 文件里（`ThemeInfo` 对样式查找有影响，
   `Guid` 可不要）；`AssemblyInfo.tt` / 现在的预处理类留着也不冲突，因为它不产生编译期属性；
3. 工作流 publish 时传 `-p:Version=${{ github.ref_name }}`、`-p:AssemblyVersion=...`、`-p:FileVersion=...`。

---

## 12. 本地复现 CI 命令

需要 Windows + .NET 6 SDK（`dotnet --list-sdks` 能看到 6.0.x）。

```powershell
dotnet restore HTTPFilter.sln
dotnet build HTTPFilter.sln -c Release --no-restore

dotnet publish HTTPFilter/HTTPFilter.csproj -c Release -r win-x64 `
  --self-contained true -p:PublishSingleFile=true `
  -p:DebugType=none -p:DebugSymbols=false -o publish/self-contained

dotnet publish HTTPFilter/HTTPFilter.csproj -c Release -r win-x64 `
  --self-contained false -p:DebugType=none -p:DebugSymbols=false `
  -o publish/framework-dependent

Compress-Archive -Path publish/self-contained/* -DestinationPath dist/local-test.zip
```

`publish/` 和 `dist/` 已在 `.gitignore` 里，不会误提交。

---

## 13. 发版检查清单

- [ ] 工作流文件已提交并推到 `main`，`main` 的构建为绿
- [ ] Workflow permissions 为 Read and write
- [ ] 版本号已确定，标签以 `v` 开头
- [ ] 只推目标标签，没有 `git push --tags`
- [ ] Actions 中 12 个步骤全绿
- [ ] Release 页有 2 个 zip，标题与标签一致
- [ ] 下载自包含包，`version.txt` 内容正确，exe 可正常启动
- [ ] Release 正文补充了 SmartScreen / 杀软 / 系统代理的注意事项
