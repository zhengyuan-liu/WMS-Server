# WMS-Server
A server which implements OGC Web Map Service (WMS) (http://www.opengeospatial.org/standards/wms) by C#


## 一、什么是WMS
WMS（Web Map Service，Web地图服务）是OGC（Open GIS Consortium，开放地理信息系统协会）制定的一种能够从地理信息动态生成具有地理空间位置数据的地图图像的服务标准。WMS标准将由地理信息图示表达的“地图”定义为计算机屏幕上显示的数字图像文件，因此WMS产生的地图一般以图像格式提供，如PNG、GIF 或BMP；或按SVG（Scalable Vector Graphics）或WebCGM（Web Computer Graphics Metafile）格式提供基于矢量的图形元素。
WMS标准定义了三个基本操作：第一个操作是GetCapabilities，用于返回服务级元数据，它是对服务信息内容和要求参数的一种描述；第二个操作是GetMap，用于返回一个地图图像，其地理空间参考和大小参数是明确定义的；可选的第三个操作是GetFeatureInfo，返回显示在地图上的某些具体要素的信息。本文只实现WMS的两个必选操作。
用户可以通过使用标准的网络浏览器或地图客户端（如Gaia）以统一资源定位符（Uniform Resources Locators，URL）的形式发出请求来调用网络地图服务的操作。URL的内容取决于被请求的操作的要求。特别是当请求一幅地图时，URL需要指出地图所要显示的地理信息（图层）、地图的覆盖范围、指定的空间参照系以及输出图像的宽度和高度；当利用同样的地理信息参数和输出范围（Boundary Box，BBOX）产生两幅或多幅地图时，其结果可以准确地被叠加以产生复合地图。

## 二、WMS的capability.xml
在实现WMS时，首先要需要根据OGC制定的WMS实现规范写一个capability的xml文档，里面提供了WMS的服务级元数据，包括服务信息内容和要求参数等。在用户向服务器发送GetCapabilities请求时，服务器返回此xml文档，用户通过阅读这个xml文档可以了解到WMS提供了哪些数据、具体实现了规范中的哪些功能等。而诸如Gaia等地图客户端会通过分析capability.xml自动得到提供的图层和实现的格式、样式等（图1）。

![](https://raw.githubusercontent.com/zhengyuan-liu/WMS-Server/master/demo/1.png)

<p align = "center">图1 Gaia客户端对于capability的分析</p>

capability.xml有两个一级标签，分别是<Service>和<Capability>。其中<Service>标签记录了此WMS服务的名字、关键词等基本信息，同时给出了服务提供人的联系信息，包括所在地、单位、E-mail等，方便用户在需要时与服务提供人联系。
<Capability>标签是capability.xml的核心内容。标签下有三个主要子标签<Request>、<Exception>和<Layer>。<Request>标签记录了WMS所支持的请求的内容，包括返回的格式和请求URL前缀。<Exception>标签说明了异常的格式。<Layer>标签先记录了WMS整体上支持的坐标系和边界范围（BoundingBox），再详细记录了每个空间数据图层的关键词、坐标系、边界范围、可以选择的样式<style>等。

## 三、Shapefile的读取与成图
WMS对于GetMap请求的响应是根据用户所请求的空间数据图层和地理范围，从空间数据动态生成具有指定地理范围的地图图像。因此如何将空间数据（本文实现的WMS的空间数据格式为Shapefile）渲染成地图图像，即实现Shapefile文件的读取与成图是实现GetMap的关键。
本文通过实现一个与Shapefile文件相对应的Shapefile类和与Shapefile文件中记录的几何对象相对应的FeatureClass类，实现Shapefile的读取与成图。为了方便统一处理，FeatureClass类包括了点要素类PointFeature、线要素类PolylineFeature和面要素类PolygonFeature的集合（List）。此部分（shp读取命名空间）的依赖项关系图如下：

<div  align="center"> 
<img src="https://raw.githubusercontent.com/zhengyuan-liu/WMS-Server/master/demo/2.png" width = "425" height = "200"/>
</div>

<p align = "center">图2 shp读取命名空间依赖项关系图</p>

### 1. Shapefile的格式与读取
Shapefile是 ESRI 提供的一种矢量数据格式，它没有拓扑信息。一个Shapefile由一组文件组成，其中必要的基本文件包括坐标文件（.shp）、索引文件（.shx）和属性文件（.dbf）三个文件。本文只实现坐标文件（.shp）的读取，根据坐标文件的内容就可以画出Shapefile的图形。
坐标文件(.shp)用于记录空间坐标信息。它由文件头和实体信息两部分构成。坐标文件的文件头是一个长度固定（100 bytes）的记录段，存储了文件长度、Shapefile文件所记录的几何类型、几何类型的空间范围等基本信息。实体信息记录了几何实体的坐标等信息。
需要指出的是Shapefile文件中数据的位序有Little（小尾）和big（大尾）之分，二者的区别在于它们字节排列的顺序相反。通常情况下数据的位序都是Little，对于位序为 big 的数据，如果想得到它的真实数值需要将它的位序转换成Little，转换原理就是交换字节的顺序，代码如下：

    /// <summary>
    /// 大尾整数转小尾整数
    /// </summary>
    /// <param name="big">大尾整数</param>
    /// <returns>小尾整数</returns>
    public static int ReverseByte(int big)
    {
        byte[] bytes = BitConverter.GetBytes(big);
        ExchangeByte(ref bytes[0], ref bytes[3]);
        ExchangeByte(ref bytes[1], ref bytes[2]);
        int little = BitConverter.ToInt32(bytes, 0);
        return little;
    }

其中ExchangeByte函数的用于交换两个字节的值，代码如下：

    public static void ExchangeByte(ref byte b1, ref byte b2)
    {
        byte temp;
        temp = b1;
        b1 = b2;
        b2 = temp;
    }

Shapefile文件所支持的几何类型包括点、线、面、多点、多线、多面等，一个Shapefile文件只能记录一种几何类型。对于不同的几何类型，文件头的内容和格式相同，因此读取文件头的代码是一样的。但由于不同的几何类型存储的内容和方式不同，需要各自单独处理。总之按照Shapefile的文件格式和几何类型的存储方式逐个数据读取即可，由于篇幅原因这里不再详细说明。

### 2. Shapefile的成图
Shapefile成图就是根据读取的Shapefile生成的FeatureClass类绘制成一个Graphic，并通过Graphic生成一个Bitmap（内存图）过程。点要素的绘制可以直接使用Graphic类的DrawRectangle和FillRectangle方法（把点表现为一个小正方形），线要素的绘制可以直接使用Graphic类的DrawLines方法，面要素的绘制可以直接使用Graphic类的DrawPolygon和FillPolygon方法。通过可以使用不同类型的边界，并填充不同的颜色，可以表示不同的style。
成图的另一个关键问题是坐标变换，即将实际的坐标系（WGS84）转化为像素坐标系。由于对于本文实现的WMS提供的空间数据范围较小（一个北大的范围），所以直接对地理坐标相对于像素坐标系做线性拉伸即可。代码如下：

    public Point GetBMPPoint(BBOX boundarybox, int width, int height)
    {
        double x = width * (this.x - boundarybox.xmin) / (boundarybox.xmax - boundarybox.xmin);
        double y = height * (this.y - boundarybox.ymin) / (boundarybox.ymax - boundarybox.ymin);
        Point bmpPoint = new Point((int)x, height - (int)y);
        return bmpPoint;
    }

由于绘制的bitmap的范围（大小，即Width*Height）是根据GetMap请求中的boundarybox确定的，所以坐标范围之外的图形和坐标会落在bitmap范围之外不被绘入bitmap之中，所以无需做额外裁剪处理。

## 四、WMS服务器的实现
在完成了capability.xml和实现了Shapefile的读取与成图后，剩下的工作就是建立WMS服务器了。WMS服务器（WMSServer命名空间）的依赖项关系图如下：

<div  align="center"> 
<img src="https://raw.githubusercontent.com/zhengyuan-liu/WMS-Server/master/demo/3.png" width = "350" height = "250"/>
</div>
    
<p align = "center">图3 WMSServer命名空间依赖项关系图</p>


将WMS的GetCapability请求和GetMap请求分别抽象为CapabilityRequest类（图4）和MapRequest类（图5），并根据请求字符串完成类的构造。WMS类中实现了GetCapabilityData和GetMap两个静态方法。GetCapability请求的处理和响应比较简单，实际上只需将capability.xml返回即可，WMS类中的GetCapabilityData静态方法就是以UTF8编码的形式返回capability.xml的字节数组。而GetMap请求的处理和响应则比较复杂，下面详细论述。

<div  align="center"> 
<img src="https://raw.githubusercontent.com/zhengyuan-liu/WMS-Server/master/demo/4.png" width = "400" height = "250"/>
</div>

<p align = "center">图4 CapabilityRequest依赖项关系图</p>

<div  align="center"> 
<img src="https://raw.githubusercontent.com/zhengyuan-liu/WMS-Server/master/demo/5.png" width = "400" height = "100"/>
</div>

<p align = "center">图5 MapRequest依赖项关系图</p>

GetMap的请求所包含的必选参数如下表所示：
表| GetMap请求的必选参数
请求参数|	说明
VERSION= 1.3.0|	请求版本.
REQUEST=GetMap|	请求名称.
LAYERS=layer_list|	以逗号隔开的一个或多个图层列表。
STYLES=style_list|	以逗号隔开的请求图层的一个渲染样式的列表。
CRS=namespace:identifier|	空间参照系。
BBOX=minx,miny,maxx,maxy|	以CRS单位表示的边框边角 (左下角，右上角)。
WIDTH=output_width|	以像元表示的地图图像宽度。.
HEIGHT=output_height|	以像元表示的地图图像高度。
FORMAT=output_format|	地图输出格式。.

MapRequest类的构造函数将请求字符串按上表分解为各个参数，完成MapRequest类的构造。
WMS的GetMap静态方法根据MapRequest对象中的请求参数，调用shp读取命名空间中的Shapefile和FeatureClass类，读取请求图层对应的Shapefile并生成一张Bitmap（内存图）。Bitmap的宽和高与请求的Width和Height相同，格式也与请求的Format相同。
WMSListener类的主要内容就是一个TcpListener，负责监听浏览器/客户端发出的WMS请求，并通过WMSThreadHandler接收和响应请求，并返回相应内容。WMSThreadHandler的依赖项关系图如下：

<div  align="center"> 
<img src="https://raw.githubusercontent.com/zhengyuan-liu/WMS-Server/master/demo/6.png" width = "425" height = "100"/>
</div>

<p align = "center">图6 WMSThreadHandler依赖项关系图</p>

为方便统一处理，WMSThreadHandler类中既包含了一个MapRequest又包含了一个CapabilityRequest。WMSThreadHandler中的GetRequest方法用于获取和解析请求字符串是GetMap还是GetCapability；GetResponceData方法用于从WMS的静态方法中获取返回的数据流，如果是GetCapability则返回capability.xml数据流，如果是GetMap则返回绘制完成的内存图数据流；SendResponce方法用于将GetResponceData得到数据流发送给浏览器或客户端。
至此，一个基本的WMS服务器就完成了。

## 五、WMS服务器的测试
使用Gaia作为客户端进行测试。新建一个Web Map Service，在输入WMS名称和URL之后双击新建的WMS，Gaia就向发出服务器发出GetCapability请求（图7），并自动分析WMS支持的图层数据及相应的数据格式和图层样式等（图1）。

![](https://raw.githubusercontent.com/zhengyuan-liu/WMS-Server/master/demo/7.png)

<p align = "center">图7 服务器接收到的GetCapability请求</p>

添加各数据图层，并选择合适的样式。客户端向服务器端发送GetMap请求，接收服务器返回的指定格式的Bitmap并将其显示在屏幕上。显示了全部图层的北大地图如图9所示。

![](https://raw.githubusercontent.com/zhengyuan-liu/WMS-Server/master/demo/8.png)

<p align = "center">图8 服务器接收到的GetMap请求</p>

![](https://raw.githubusercontent.com/zhengyuan-liu/WMS-Server/master/demo/9.png)

<p align = "center">图9 Gaia显示的WMS返回的多图层北大地图</p>

拖动、缩放地图，客户端又向服务器发送了不同参数的GetMap请求，反映了WMS地图生成的动态性。


