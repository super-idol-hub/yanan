# Windows 运行时

`Program.cs` 提供 WinForms layered window、托盘、状态机、凝视、持久手机和睡眠、单实例、缩放、皮肤原子切换和当前用户自启。程序不联网。

在 Windows PowerShell 中运行：

```powershell
.\build.ps1 -SourceOnly
# 仓库已包含完整角色资源：
.\build.ps1 -FrameArchive .\resources\noir-frames.zip
```

源代码检查构建不包含图片，只能用于 `--contract-test`；不能当作可用角色交付。正式构建内嵌完整资源，输出一个 EXE。使用系统 .NET Framework C# 编译器，不需要 .NET SDK。

```powershell
.\bin\yanan-1.0.0.exe --contract-test contract-test.json
.\bin\yanan-1.0.0.exe --self-test self-test.json self-test-preview.png
.\bin\yanan-1.0.0.exe --qa-window 10000
```

资源归档：`frames/r00/c00.png` 至规范最后一帧，24 行共 176 张 528×808 RGBA PNG；`motion/` 下为 17×25 的 `XWM1` 双向位移网格。逻辑画布为 132×202。显示阶段使用单主体网格变形，不能把两幅完整人物直接交叉淡入淡出。

生成和转换工具仅在构建时使用 Python、Pillow、NumPy、OpenCV。`build_motion_fields.py` 从已审核关键帧计算网格，不生成新人物图。原始参考照片不参与运行时构建。

`resources/noir-frames.zip` 已包含全部 PNG 和网格，直接编译不需要 Python。资源报告与逐行动作预览位于仓库 `qa/evidence/`。自动验证命令为 `./verify.ps1 -WindowQa`；它把 EXE 单独复制到隔离目录后执行，不依赖工作区图片。
