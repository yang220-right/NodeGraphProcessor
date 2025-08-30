# Timeline节点 - Unity Timeline风格功能

## 概述
Timeline节点现在提供了与Unity原生Timeline高度一致的功能，包括完整的轨道系统、剪辑管理、混合模式等。

## 核心功能

### 1. 轨道系统 (TimelineTrack)
- **多种轨道类型**：
  - Animation (动画轨道)
  - Audio (音频轨道)
  - Event (事件轨道)
  - Control (控制轨道)
  - Video (视频轨道)
  - Cinemachine (相机轨道)
  - Activation (激活轨道)
  - Material (材质轨道)
  - Particle (粒子轨道)
  - Custom (自定义轨道)

- **轨道属性**：
  - 轨道索引、名称、类型
  - 启用/禁用状态
  - 颜色、音量、偏移、长度
  - 混合模式 (Override/Additive/Multiply/Blend)
  - 权重、优先级
  - 锁定、静音、独奏状态
  - 循环播放
  - 动画曲线

### 2. 剪辑系统 (TimelineClip)
- **剪辑属性**：
  - 名称、开始时间、持续时间
  - 剪辑类型、资源引用
  - 混合模式、权重
  - 偏移、缩放
  - 循环、锁定、静音、独奏
  - 动画曲线
  - 事件列表

- **剪辑类型**：
  - Animation (动画剪辑)
  - Audio (音频剪辑)
  - Video (视频剪辑)
  - Event (事件剪辑)
  - Control (控制剪辑)
  - Material (材质剪辑)
  - Particle (粒子剪辑)
  - Custom (自定义剪辑)

### 3. 事件系统 (ClipEvent)
- **事件类型**：
  - Custom (自定义事件)
  - Animation (动画事件)
  - Audio (音频事件)
  - Particle (粒子事件)
  - Material (材质事件)
  - Activation (激活事件)
  - Deactivation (停用事件)

### 4. 混合模式
- **轨道混合模式**：
  - Override (覆盖)
  - Additive (叠加)
  - Multiply (乘法)
  - Blend (混合)

- **剪辑混合模式**：
  - Override (覆盖)
  - Additive (叠加)
  - Multiply (乘法)
  - Blend (混合)

## 使用方法

### 基本操作
```csharp
// 添加轨道
timelineNode.AddTrack();

// 添加剪辑
timelineNode.AddClip();

// 添加剪辑到指定轨道
timelineNode.AddClipToTrack(0, 5f, 3f);

// 删除剪辑
timelineNode.DeleteClip(0, 0);

// 移动剪辑
timelineNode.MoveClip(0, 0, 10f);

// 调整剪辑持续时间
timelineNode.ResizeClip(0, 0, 5f);
```

### 轨道控制
```csharp
// 设置轨道独奏
timelineNode.SetTrackSolo(0, true);

// 设置轨道静音
timelineNode.SetTrackMute(0, true);

// 设置轨道锁定
timelineNode.SetTrackLock(0, true);
```

### 查询功能
```csharp
// 获取当前时间的所有剪辑
var clipsAtTime = timelineNode.GetClipsAtCurrentTime();

// 获取轨道的有效剪辑
var validClips = timelineNode.GetValidClips(0);

// 检查剪辑冲突
bool hasConflict = timelineNode.HasClipConflict(0, 5f, 3f);
```

## 高级功能

### 1. 时间轴控制
- 播放、暂停、停止
- 跳转到指定时间
- 循环播放
- 播放速度控制

### 2. 关键帧系统
- 关键帧创建和管理
- 插值模式选择
- 缓入缓出时间设置

### 3. 时间轴显示
- 时间刻度显示
- 播放头显示
- 轨道和剪辑可视化
- 缩放和偏移控制

## 与Unity Timeline的对应关系

| Unity Timeline | 我们的Timeline节点 |
|----------------|-------------------|
| Timeline Asset | TimelineNode |
| Track | TimelineTrack |
| Clip | TimelineClip |
| Signal | ClipEvent |
| Track Mixer | TrackBlendMode |
| Clip Mixer | ClipBlendMode |
| Timeline Window | TimelineNodeView |

## 扩展建议

### 1. 添加更多轨道类型
- 可以继承TimelineTrack创建自定义轨道
- 实现特定的轨道行为

### 2. 自定义剪辑类型
- 继承TimelineClip创建专用剪辑
- 添加特定于项目的剪辑属性

### 3. 事件系统扩展
- 实现更多事件类型
- 添加事件参数验证
- 支持事件链式调用

### 4. 性能优化
- 实现剪辑池管理
- 添加LOD系统
- 优化大量剪辑的渲染

## 注意事项

1. **轨道索引**：确保轨道索引的唯一性和连续性
2. **时间冲突**：添加剪辑前检查时间冲突
3. **资源引用**：剪辑资源需要正确设置
4. **性能考虑**：大量剪辑可能影响性能
5. **序列化**：复杂数据结构需要正确的序列化支持

## 示例场景

### 游戏过场动画
- 使用多个轨道控制角色动画、相机移动、音效播放
- 通过事件系统触发游戏逻辑

### 角色动画混合
- 使用Additive混合模式叠加多个动画
- 通过权重控制混合强度

### 音视频同步
- 音频和视频轨道同步播放
- 使用事件系统控制播放状态

这个Timeline节点现在提供了与Unity原生Timeline非常接近的功能，可以满足大多数时间轴相关的开发需求。

