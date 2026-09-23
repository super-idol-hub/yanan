# 人物与制作来源

人物为 **颜安**，演员、歌手，2000 年 10 月 27 日出生。检索日期：2026-09-23。

| 来源 | 用途 | 是否随项目分发 |
|---|---|---|
| [爱奇艺人物页](https://www.iq.com/actor-info/yan-6567833148885905?lang=zh_cn) | 核对姓名、职业、生日，正面脸部参考 | 仅链接，不分发原始照片 |
| [颜安 YA 微博](https://weibo.com/u/6383278820) | 核对人物身份及公开活动 | 仅链接 |
| [米拍公开造型文章](https://www.mipai.com.cn/article/MjgxOTAy) | 正面、侧面、发型与身材参考；内容为转载，非官方授权证明 | 仅链接，不分发原始照片 |

公开可访问不表示获得再分发授权。原始照片仅保留在仓库外的本地参考目录，不进入 Git、EXE、ZIP 或 CI。

首发“黑白日常”服装是本项目提出并经用户确认的无品牌服装方案，不宣称对应某次真实活动。身份锚点通过内置 ImageGen 生成；用户已明确回复“确认这个形象和服装，继续制作动作”。全部动作继续使用该锚点作为身份与服装基准。

工程基于组织已有 Windows 角色实现进行适配：[上游源码](https://github.com/super-idol-hub/xiaoluhan/tree/cefbda0496af2d9fe05dea8c424b8ecde337db40/source/standalone/xiaoluhan)。仅复用运行时和动作网格算法，不复用上游人物图片、图标或发布包；同一组织维护者授权创建本项目。上游未附独立代码许可证，不能据此推断任意第三方再许可。

规范基线：[3c58458](https://github.com/super-idol-hub/idol-windows-character-guides/tree/3c58458b7ee0713ccebbe0ef92d0ce7b99db3164)。构建时使用 Python、Pillow（HPND）、NumPy（BSD）、OpenCV（Apache-2.0）；运行时仅使用 Windows/.NET Framework，不捆绑这些构建工具。
