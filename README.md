# DicomView.Behavior

一个DICOM控件demo，核心思路是利用 `Behaviors` 将交互逻辑解耦为可复用的行为单元。同时使用controller管理behavior的互斥(如鼠标左键功能的调节窗宽窗位和绘制roi)，每个behavior只需着力于自身功能逻辑的开发，易扩展和维护。

## 演示
![演示](./Display.mp4)
## 两种实现方式

主窗口左右两列展示两种 Behavior 拼装模式：

### 方式一：Behavior 直接绑定（右列）

每个控件独立挂载 Behavior，通过 `IsActive` 属性绑定和 `BehaviorActiveConverter` 实现互斥：

```xml
<ItemsControl x:Name="images2" Grid.Row="0" Grid.Column="1" ItemsSource="{Binding Images2}" Width="600" Height="700">
            <i:Interaction.Behaviors>
                <behaviors:ItemsDragBehavior ItemsMoved="ItemsMoved2" IsActive="{Binding LeftButton, Mode=TwoWay, Converter={StaticResource behaviorActiveConverter}, ConverterParameter={x:Static behaviors:BehaviorType.Move}}"/>
            </i:Interaction.Behaviors>
            <ItemsControl.ItemsPanel>
                <ItemsPanelTemplate>
                    <UniformGrid Rows="2" Columns="2"/>
                </ItemsPanelTemplate>
            </ItemsControl.ItemsPanel>
            <ItemsControl.ItemTemplate>
                <DataTemplate>
                    <view:DicomImageShell ROISource="{Binding ROIs}" Loaded="DicomImageShell_Loaded">
                        <i:Interaction.Behaviors>
                            <behaviors:ItemsROIBehavior ROIType="{Binding DataContext.CurrentROI, Mode=TwoWay, RelativeSource={RelativeSource AncestorType={x:Type ItemsControl}}}" AddROIAction="AddROI2" IsActive="{Binding DataContext.LeftButton,Mode=TwoWay, RelativeSource={RelativeSource Mode=FindAncestor, AncestorType={x:Type ItemsControl}}, Converter={StaticResource behaviorActiveConverter}, ConverterParameter={x:Static behaviors:BehaviorType.ROI}}"/>
                            <behaviors:ItemsAdjustWindowBehavior AdjustWindowAction="TestAdjustWindow2" IsActive="{Binding DataContext.LeftButton,Mode=TwoWay, RelativeSource={RelativeSource Mode=FindAncestor, AncestorType={x:Type ItemsControl}}, Converter={StaticResource behaviorActiveConverter}, ConverterParameter={x:Static behaviors:BehaviorType.WindowLevel}}"/>
                            <behaviors:MoveROIBehavior/>
                        </i:Interaction.Behaviors>
                    </view:DicomImageShell>
                </DataTemplate>
            </ItemsControl.ItemTemplate>
        </ItemsControl>
```

- 由于每个控件都附加了behavior，大量列表时修改激活的behavior会通知所有子项。

### 方式二：Controller 集中管理（左列）

模仿behavior的思路创建behaviorCore类型，通过 `DicomImagesBehaviorCoreController` 来管理core的激活与注销。
同时core不是附加到所有子项，而是在鼠标点击时动态附加(目前是在子项的mousedown事件上来修改附加元素，二次开发也可通过Hittest等其他方式来确定附加元素)：

```xml
<ItemsControl>
    <i:Interaction.Behaviors>
        <controllers:DicomImagesBehaviorCoreController>
            <controllers:DicomImagesBehaviorCoreController.BehaviorCores>
                <behaviorCores:ItemsAdjustWindowCore AdjustWindowAction="TestAdjustWindow"/>
                <behaviorCores:ItemsROIBehaviorCore AddROIAction="AddROI"/>
                <behaviorCores:ItemsDragBehaviorCore ItemsMoved="ItemsMoved"/>
            </controllers:DicomImagesBehaviorCoreController.BehaviorCores>
        </controllers:DicomImagesBehaviorCoreController>
    </i:Interaction.Behaviors>
</ItemsControl>
```

- core实现了ITomenuItem接口，load后会自动生成右键菜单项
- 也支持将`IsActive`属性绑定到ViewModel，同时关闭`AutoActive`来增加性能

## 已实现(Behavior)功能

| 功能 | 说明 |绑定按键|
|------|------|------|
| **窗宽窗位调节** | 鼠标拖拽实时调整灰度映射 |Left
| **图像拖拽排序** | 通过拖拽重排图像位置 |Left
| **直线ROI绘制** | 会测量实际长度(mm) |Left
| **ROI 拖拽** | 拖动已绘制的 ROI 形状 |Left

修改窗宽窗位、绘制roi都会作用到Items中的所有项(通过对应的Action触发)

## 使用说明
DicomImageShell控件需求IDicomImage接口作为DataContext，接口定义如下：
```
internal interface IDicomImage
    {
        public double WindowWidth { get; set; }
        public double WindowCenter { get; set; }
        public double SpacingX { get; }
        public double SpacingY { get; }
        public ushort Cols { get; }
        public ushort Rows { get; }

        public BitmapSource DicomImage { get; }
        public void AdjustWindow(Vector vector);
    }
```
所以目前只支持普通的断层图像，其他类型的Dicom文件(如增强图、动态心电图等)有待开发
Demo使用开源的 `fo-dicom` 库来读取Dicom文件。可以按需自定义其他读取方式，创建Model并实现IDicomImage接口就可以使用

## 补充
FO-DicomOverride目录中的类是对fo-dicom库中相关类的重写，主要目的是为了增加调节窗宽窗位的性能。源库中修改窗宽窗位步骤很多且每次都会new一个新的WriteableBitmap，修改后删除了一些步骤并且将new改成了修改后台像素的方式来渲染。


