# VS DLL引用替换插件

Download:   
[https://marketplace.visualstudio.com/items?itemName=Jasongrass.DLLReferencePathChangeAuto](https://marketplace.visualstudio.com/items?itemName=Jasongrass.DLLReferencePathChangeAuto)  
  
Visual Studio Extensions
[https://visualstudiogallery.msdn.microsoft.com/](https://visualstudiogallery.msdn.microsoft.com/)  

## 使用
  
* [使用指南](http://jgrass.cc/2017-10-auto-dll-reference-change-tool/)   

<br/>

## 1、工程引用替换
与 HintPath Dll 替换类似，也是对 csproj 文件相关的引用项进行修改，将引用方式由 dll 引用替换为 工程引用，
不同的是，除了替换 csproj 文件之外，还需要在解决方案 sln 文件中插入对引入工程的引用，这里使用的是 dotnet.exe 工具，来自 .net core sdk，
如果没有，需要安装才能使用。  
[.net core sdk 下载链接](https://www.microsoft.com/net/download/windows)  

## 2、HintPath Dll 替换

HintPath引用替换 是替换csproj中的引用路径，一般用于替换Nuget引用。

具体实现：

这种替换方式本质上是对工程文件csproj中的DLL引用信息做更改。

替换后重新编译解决方案，目标DLL就会引用新的DLL文件。

撤销操作是恢复对csproj文件的更改。

**注意** 

这里的撤销操作依赖于git的checkout命令，使用checkout命令恢复对csproj文件的更改。所以，这里要求在替换之前，csproj不能有未提交的更改。

另外，解决方案必须使用git进行管理，否则撤销将无法进行。

## 3、 文件替换

文件替换 是简单地对Debug目录下的DLL文件进行替换。

撤销操作是利用备份的Debug目录下的DLL文件进行撤销，如果备份文件丢失，将无法撤销。

<br/>

## 代码使用说明：
  需要为 DllRefChanger 和 DllRefChangerSettingView 两个工程添加签名才能正确编译。  
   
  ![](https://github.com/JasonGrass/DllReferencePathChanger/blob/master/Img/20171023221524.png)   
  
