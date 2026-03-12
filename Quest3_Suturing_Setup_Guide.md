# Quest 3 縫合場景修改指南

## 概述
本指南說明如何將Suturing場景從OpenHaptics/VRPN設備修改為支持Quest 3 VR頭顯。

## 前置條件

### 1. Unity XR設置
在Unity中安裝以下Package：
1. **Window → Package Manager**
2. 安裝以下包：
   - `XR Plugin Management`
   - `Oculus XR Plugin` 或 `OpenXR Plugin`
   - `XR Interaction Toolkit` (可選，用於更高級的交互)

### 2. 啟用XR
1. **Edit → Project Settings → XR Plug-in Management**
2. 勾選 **Oculus** 或 **OpenXR**
3. 配置Oculus設置（如果使用Oculus插件）

## 場景修改步驟

### 步驟1: 創建XR Rig

1. 在Suturing場景中，替換或修改Main Camera：
   ```
   創建新的GameObject層級結構：
   
   XR Origin
   ├── Camera Offset
   │   ├── Main Camera (添加TrackedPoseDriver組件)
   │   ├── LeftHandController
   │   │   ├── UnityDrivenDevice (腳本)
   │   │   ├── Quest3InputHandler (腳本)
   │   │   └── Tool Visual (可選的視覺模型)
   │   └── RightHandController
   │       ├── UnityDrivenDevice (腳本)
   │       ├── Quest3InputHandler (腳本)
   │       └── Tool Visual (可選的視覺模型)
   ```

2. 添加XR Origin：
   - GameObject → XR → XR Origin (如果安裝了XR Interaction Toolkit)
   - 或手動創建上述結構

### 步驟2: 配置控制器跟蹤

對於每個控制器GameObject（LeftHandController / RightHandController）：

1. 添加 **Tracked Pose Driver** 組件（或Unity自動添加）
   - Left Controller: 設置 `Tracking Type = Controller`，`Device Position/Rotation = Left Controller`
   - Right Controller: 設置 `Tracking Type = Controller`，`Device Position/Rotation = Right Controller`

### 步驟3: 設置設備驅動

找到原Suturing場景中的工具GameObject（通常連接到VRPN或OpenHaptics設備）：

#### 原始設置（使用VRPN）
```
Needle Tool GameObject
├── VrpnDevice 組件 ❌
├── VrpnDeviceManager 引用 ❌
└── RigidController
    └── device = VrpnDevice
```

#### 新設置（Quest 3）
```
RightHandController
├── UnityDrivenDevice 組件 ✅
├── Quest3InputHandler 組件 ✅
└── RigidController
    └── device = UnityDrivenDevice
```

### 步驟4: 配置UnityDrivenDevice

1. 在控制器GameObject上添加 `UnityDrivenDevice` 組件
2. 該組件會自動讀取GameObject的Transform
3. 無需額外配置

### 步驟5: 配置RigidController

在Inspector中調整RigidController參數（因為Quest 3沒有力反馈）：

```
Device: 拖入同一GameObject上的UnityDrivenDevice
Controlled Object: 拖入要控制的iMSTK Rigid對象

==== Spring Parameters ====
Linear Ks: 100-500 (降低，原觸覺設備可能是5000+)
Linear Kd: 50-100
Angular Ks: 1000-10000 (降低，原觸覺設備可能是100000+)
Angular Kd: 100-500

==== Haptic Parameters ====
Force Scale: 0.0 (Quest 3無力反饋)
Use Force Smoothing: false
Use Critical Damping: true

==== Transform Parameters ====
Translation Scaling: 1.0 (Unity使用米，iMSTK使用厘米，可能需要調整)
Attachment Point: (0, 0, 0)
```

### 步驟6: 添加輸入控制

1. 在控制器GameObject上添加 `Quest3InputHandler` 組件
2. 配置：
   - **Controller Node**: 選擇 `LeftHand` 或 `RightHand`
3. 使用該組件獲取按鈕狀態（在其他腳本中）：
   ```csharp
   Quest3InputHandler inputHandler = GetComponent<Quest3InputHandler>();
   if (inputHandler.IsTriggerPressed())
   {
       // 抓取縫合針
   }
   ```

### 步驟7: 調整縫合交互

根據您的Suturing場景設置，可能需要調整：

1. **針抓取邏輯**: 使用Quest3InputHandler的trigger按鈕
2. **視覺反饋**: 由於沒有觸覺反饋，增強視覺和聲音反饋
3. **碰撞反應**: 調整iMSTK碰撞參數使其更明顯

## 場景配置示例

完整的單手控制器配置：

```
RightHandController
├── Transform
│   └── (XR自動更新位置和旋轉)
├── UnityDrivenDevice
├── Quest3InputHandler
│   └── Controller Node: RightHand
├── RigidController
│   ├── device: UnityDrivenDevice
│   ├── rigid: [拖入縫合針的Rigid對象]
│   ├── linearKs: 200
│   ├── linearKd: 50
│   ├── angularKs: 5000
│   ├── angularKd: 200
│   ├── forceScale: 0
│   └── translationScaling: 100 (Unity米→iMSTK厘米)
└── NeedleVisual (可選)
    └── MeshRenderer
```

## 雙手設置（推薦用於縫合）

對於複雜的縫合操作，建議設置雙手控制：

### 左手 - 組織固定/鑷子
```
LeftHandController
├── UnityDrivenDevice
├── Quest3InputHandler (Controller Node: LeftHand)
├── RigidController
│   └── device: LeftHandUnityDrivenDevice
│   └── rigid: [鑷子Rigid對象]
└── ForcepsModel
```

### 右手 - 縫合針
```
RightHandController
├── UnityDrivenDevice
├── Quest3InputHandler (Controller Node: RightHand)
├── RigidController
│   └── device: RightHandUnityDrivenDevice
│   └── rigid: [縫合針Rigid對象]
└── NeedleHolderModel
```

## 測試和調整

### 初次測試檢查清單
- [ ] Quest 3已連接並被Unity識別
- [ ] 控制器在場景視圖中正確跟蹤
- [ ] UnityDrivenDevice正在更新位置
- [ ] RigidController接收到設備輸入
- [ ] iMSTK工具對象正確響應
- [ ] 按鈕輸入正確觸發

### 常見問題

**問題1: 工具移動太快或太慢**
- 調整 `RigidController.translationScaling` (通常100-200)
- 調整 `RigidController.linearKs` (50-500)

**問題2: 工具旋轉不穩定**
- 增加 `angularKd` (阻尼)
- 啟用 `useCriticalDamping`

**問題3: 控制器未跟蹤**
- 檢查XR Plug-in Management是否正確啟用
- 確認Quest 3在PC上正確連接（Link或Air Link）

**問題4: 工具與手柄不同步**
- 檢查UnityDrivenDevice在Update中正確執行
- 檢查iMSTK的SimulationManager正在運行

## 性能優化建議

1. **降低物理精度**（如果幀率低）：
   - 在SimulationManager中調整時間步長
   - 降低PBD迭代次數

2. **簡化視覺模型**：
   - 使用更低多邊形的工具模型
   - 考慮使用Level of Detail (LOD)

3. **優化Quest 3設置**：
   - 在Oculus/OpenXR設置中調整解析度
   - 啟用Fixed Foveated Rendering

## UnityController場景參考

您可以參考 `Assets/Scenes/UnityController.unity` 場景：
- 它展示了如何使用UnityDrivenDevice
- 查看其GameObject結構和組件配置

## 參考文檔

- [UnityDrivenDevice.cs](Assets/Scripts/Devices/UnityDrivenDevice.cs) - Unity驅動的設備實現
- [RigidController.cs](Assets/Scripts/Controllers/RigidController.cs) - 剛體控制器
- [Quest3InputHandler.cs](Assets/Scripts/Devices/Quest3InputHandler.cs) - Quest 3輸入處理

## 限制說明

**Quest 3相比觸覺設備的限制：**
- ❌ 無力反饋 - 無法感受到組織阻力
- ❌ 精度較低 - 手持控制器不如固定的觸覺設備精確
- ❌ 無觸覺提示 - 需要依賴視覺和聲音反饋
- ✅ 更自然的手部動作
- ✅ 沉浸式VR體驗
- ✅ 移動自由度更高

## 需要幫助？

如果遇到問題：
1. 檢查Unity Console中的錯誤信息
2. 確認iMSTK日誌輸出
3. 參考iMSTK-Unity文檔: https://imstk-unity.readthedocs.io/
4. 在Kitware Discourse論壇提問: https://discourse.kitware.com/c/imstk/

---

**创建日期**: 2026年3月2日  
**適用版本**: iMSTK-Unity 2.0, Unity 2021.3+, Quest 3
