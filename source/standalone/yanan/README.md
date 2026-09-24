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

`resources/noir-frames.zip`、`resources/stage-frames.zip` 和 `resources/crimson-frames.zip` 各包含 176 张 PNG 和 315 组网格，三套共 528 张 PNG、945 组网格；直接编译不需要 Python。资源报告与逐行动作预览位于仓库 `qa/evidence/`。自动验证命令为 `./verify.ps1 -WindowQa`；它把 EXE 单独复制到隔离目录后执行，不依赖工作区图片，并逐套检查皮肤切换和手机保持姿势的像素一致性。

按用户反馈，手机停留改为固定 `r22/c03`，停止 `c03–c05` 循环和插值；点击通过专用 `c03→c06` 网格进入起身。保留 c04/c05 资源以兼容 176 帧协议。待机与凝视统一画布高度和脚底，手机、睡眠的站立端点复用同皮肤中性站姿，坐姿不放大填满画布。
