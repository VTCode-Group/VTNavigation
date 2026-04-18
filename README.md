<div align="center">

# VTNavigation

**基于八叉树体素化的 3D 寻路系统**

专为 Unity 引擎打造的高性能三维空间导航解决方案

[![Unity](https://img.shields.io/badge/Unity-2022.3.50f1-black?logo=unity)](https://unity.com/)
[![Language](https://img.shields.io/badge/C%23-9.0-68217A?logo=csharp)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![License](https://img.shields.io/badge/Platform-Windows%2064bit-blue)]()

</div>

---

## 概览

VTNavigation 是一套完全自主实现的 **3D 体素导航系统**，不依赖 Unity 内置 NavMesh，通过**八叉树空间划分**将三维场景离散化为体素网格，并在此基础上执行 **A\* 寻路算法**。系统采用独创的 **VTCode 空间编码**方案，以单 `UInt32` 高效表示三维坐标与层级信息，实现极速的邻域查询与体素判定。

### 核心优势

| 特性 | 说明 |
|---|---|
| **纯自研八叉树引擎** | 基于 LOC（Location Encoding）的 10 层八叉树，支持三角形 / AABB 精确切割 |
| **高效空间编码 VTCode** | 单 `UInt32` 编码 X/Y/Z 坐标 + 层级深度，位运算实现 O(1) 邻域跳转 |
| **A\* 体素寻路** | 基于最小堆优先队列的 A\* 实现，支持跨子场景无缝寻路 |
| **双模式路径平滑** | 射线投射贪心简化 + 三阶贝塞尔曲线插值 |
| **大世界分块支持** | VTSceneGroup 按 X×Y×Z 网格拆分子场景，支持流式加载与引用计数 |
| **完整烘焙管线** | Editor 工具窗口，支持 Mesh / BoxCollider / MeshCollider / Terrain 全源采集 |
| **异步寻路** | 基于 `Task.Run` + 协程的异步路径查询接口 |

---

## 系统架构

```
VTNavigation
├── OCTree/              # 八叉树核心
│   ├── OCTree.cs        # 八叉树数据结构与操作（分裂/合并/射线检测/邻域查询）
│   ├── VTCode.cs        # 自动生成的 LOC 空间编解码（位运算加速）
│   ├── HashCode.cs      # 哈希编码封装，坐标↔Bounds双向转换
│   └── OCTreeUtil.cs    # 八叉树辅助工具（子节点定位/状态位操作/空间缩放）
│
├── Navigation/          # 寻路核心
│   ├── NavigationHelper.cs   # A* 寻路实现 + 射线投射路径简化
│   ├── NavigationService.cs  # 寻路服务（同步/异步接口）
│   ├── NavigationAgent.cs    # 导航代理 MonoBehaviour
│   ├── INavService.cs        # 寻路服务接口
│   ├── IMap.cs / IMapGroup.cs # 地图抽象层接口
│   └── AStarNode.cs          # A* 节点（含跨子场景坐标）
│
├── Scene/               # 场景管理
│   ├── VTScene.cs       # 单个子场景（封装 OCTree + 世界/树空间转换）
│   ├── VTSceneGroup.cs  # 子场景组（网格管理 + 跨场景寻路协调）
│   └── VTSceneUtil.cs   # 场景空间转换扩展方法 + 三角形采集
│
├── Geometry/            # 几何计算
│   ├── GeometryUtil.cs       # SAT 三角形-AABB 碰撞检测 / Bounds 运算
│   ├── Triangle.cs           # 三角形数据结构
│   ├── EarClippingHelper.cs  # 耳切法多边形三角化
│   ├── BezierCurve.cs        # 贝塞尔曲线
│   ├── Circle.cs / Box2D.cs  # 2D 几何图元
│   └── RayCastResult.cs      # 射线检测结果
│
├── Editor/              # 编辑器工具
│   ├── VTNavigationEditorWindow.cs  # 烘焙配置 + 可视化编辑面板
│   ├── BakeWork.cs / BakeTask.cs    # 多任务并行烘焙管线
│   └── BakeTreeProgress.cs          # 烘焙进度可视化
│
├── Drawer/              # 可视化绘制
├── Debugger/            # 运行时调试绘制
├── Common/              # 基础数据结构（HeapQueue / DList）
├── Util/                # 工具类（IO / 数学 / 路径平滑）
├── Serivces/            # 服务定位器（Service Locator 模式）
└── Demo/                # 演示场景
```

---

## 核心技术解析

### 1. VTCode 空间编码系统

VTCode 是本系统的基石。它将三维空间坐标 (X, Y, Z) 和八叉树层级 (Layer) 编码到单个 `UInt32` 中：

```
Bit 31 (未使用)  │  Y坐标 (10bit)  │  Z坐标 (10bit)  │  X坐标 (10bit)  │  层级标志 (1bit)
```

层级通过最低位中 **单个置位比特** 的位置来表示（`ONE << layer`），解码时提取最右置位比特即可得到层级值（0~10）。

- **10 层深度**：最精细层可表示 1024³ = **10.7 亿**个体素
- **O(1) 邻域跳转**：通过位运算直接计算相邻节点编码（左/右/上/下/前/后）
- **O(1) 层级升降**：父节点→子节点 / 子节点→父节点均为位操作
- **溢出检测**：自动检测坐标越界，用于跨子场景边界判定

> VTCode 由代码生成器自动产出，所有编解码逻辑零运行时开销。

### 2. 八叉树体素化（OCTree）

```
场景三角面 ──SAT碰撞检测──▶ 八叉树递归分裂 ──▶ 叶节点阻塞标记
```

- **三角形切割**：使用 **SAT（分离轴定理）** 精确检测三角形与 AABB 的重叠
- **自适应分辨率**：从根节点 (1024³) 递归细分至最小体素尺寸 (由 `minVoxelSize` 控制)
- **节点状态压缩**：每个父节点用一个 `byte` 记录 8 个子节点的阻塞状态（`0xFF` = 全阻塞）
- **自动合并**：当 8 个子节点全部阻塞时，自动向上合并为单个父节点标记
- **序列化**：二进制读写，自定义 `.vtscene` / `.vtgroup` 文件格式

### 3. A\* 寻路算法

```
起点世界坐标 → 体素哈希 → 最小堆优先队列 → 6邻域扩展 → 跨场景无缝过渡 → 路径回溯
```

**权重评估函数**：

```csharp
weight = dist(current, start) + 2.0 × dist(current, target) - 4.0 × voxelSize
```

- **跨场景寻路**：通过 `IMapGroup.ToNextHashCode()` 自动处理子场景边界跨越
- **多分辨率支持**：`ToMaxWalkableArea()` 将精细体素提升至最大可行走区域，减少搜索节点数
- **射线平滑**：后处理阶段使用二分搜索 + 射线投射贪心去除冗余拐点

### 4. 大世界分块（VTSceneGroup）

| 概念 | 说明 |
|---|---|
| **VTScene** | 单个八叉树场景，最大尺寸 = 1024 × voxelSize |
| **VTSceneGroup** | 按 X×Y×Z 网格组合多个 VTScene，实现无限扩展 |
| **引用计数** | `ReferenceSubScene` / `UnReferenceSubScene` 支持流式加载管理 |
| **跨场景邻域** | 坐标溢出时自动映射到相邻子场景的对应体素 |

### 5. 烘焙管线

```
┌─────────────┐    ┌───────────────┐    ┌──────────────┐    ┌──────────┐
│ 场景几何采集  │───▶│ 按子场景分配   │───▶│ 并行八叉树烘焙 │───▶│ 序列化输出 │
│ Mesh/Collider│    │ SAT 碰撞筛选  │    │ BakeTask×N   │    │ .vtscene │
│ /Terrain     │    │               │    │              │    │ .vtgroup │
└─────────────┘    └───────────────┘    └──────────────┘    └──────────┘
```

- **全源采集**：MeshFilter / BoxCollider / MeshCollider / Terrain 高度图
- **按需分配**：每个子场景只接收与之重叠的三角形，避免无效计算
- **编辑器可视化**：Scene 视图内嵌参数面板，拖拽手柄调整场景原点，实时预览 AABB 分区
- **配置加载**：支持从已有的 `.vtgroup` 文件回溯烘焙参数

---

## 快速开始

### 烘焙导航数据

1. 打开编辑器窗口：`Tools → VT Navigation → Editor Window`
2. 点击 **Enter Edit Mode**，在 Scene 视图中调整参数：
   - `SceneOrigin`：场景原点（支持拖拽手柄）
   - `CustomVoxelSize`：体素尺寸（0.2 ~ 2.0）
   - `X/Y/Z SceneCount`：子场景网格数
   - `MinBlockLayer`：最小阻塞层级（1~4，过滤微小障碍）
3. 点击 **Save** 保存配置
4. 选择输出目录，点击 **Bake**
5. 烘焙产物：
   - `{SceneName}.vtgroup` — 场景组元数据
   - `{SceneName}_{x}_{y}_{z}.vtscene` — 各子场景八叉树数据

### 运行时寻路

```csharp
// 1. 加载导航数据
VTSceneGroup sceneGroup = new VTSceneGroup();
sceneGroup.ReadAllFromFile("Assets/Resources/Maze.vtgroup");

// 2. 注册寻路服务
ServiceLocator.Instance.AddService<INavService>(new NavigationService());

// 3. 查询路径
INavService navService = ServiceLocator.Instance.GetService<INavService>();
List<Vector3> path = navService.QueryPath(sceneGroup, startPos, targetPos, smooth: true);

// 4. 异步寻路（协程）
List<Vector3> asyncPath = new List<Vector3>();
yield return navService.QueryPathAsync(sceneGroup, startPos, targetPos, asyncPath, smooth: true);
```

### 使用导航代理

```csharp
// NavigationAgent 组件挂载到寻路角色上
NavigationAgent agent = GetComponent<NavigationAgent>();
agent.m_MoveSpeed = 5.0f;
agent.m_StopDistance = 0.1f;

// 设置目标并开始移动
agent.SetDestination(targetPosition);
```

---

## 自定义文件格式

| 扩展名 | 说明 |
|---|---|
| `.vtscene` | 单个子场景数据：体素尺寸 + 场景原点 + 场景尺寸 + 八叉树节点（二进制） |
| `.vtgroup` | 场景组元数据：网格维度 (X×Y×Z) + 体素尺寸 + 场景包围盒 |

---

## 依赖

| 依赖 | 版本 | 说明 |
|---|---|---|
| Unity | 2022.3.50f1 | 目标引擎版本 |
| TextMeshPro | 3.0.6 | UI 文本渲染 |
| Timeline | 1.6.4 | 时间轴 |
| Visual Scripting | 1.7.8 | 可视化脚本 |

> **零外部依赖**：寻路核心、八叉树引擎、几何运算全部自主实现，无需安装任何第三方包。

---

## 项目特征总结

```
✦ 纯 C# 自研八叉树引擎，不依赖 Unity NavMesh
✦ VTCode 位编码方案，UInt32 一键表达空间坐标 + 层级
✦ SAT 精确碰撞检测，支持任意三角形网格体素化
✦ A* 寻路 + 跨场景无缝衔接
✦ 射线简化 + 贝塞尔平滑双重路径优化
✦ 大世界网格分块 + 引用计数流式加载
✦ Editor 可视化烘焙工具，全源几何采集
✦ 异步寻路接口（Task + 协程）
✦ 服务定位器模式，易于扩展替换
✦ 运行时调试可视化
```

---

## 许可证

Copyright © VTCodeGroup. All rights reserved.
