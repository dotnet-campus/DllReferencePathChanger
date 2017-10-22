# VS DLL引用替换插件

Download:   
[https://marketplace.visualstudio.com/items?itemName=Jasongrass.DLLReferencePathChangeAuto](https://marketplace.visualstudio.com/items?itemName=Jasongrass.DLLReferencePathChangeAuto)  
  
Visual Studio Extensions
[https://visualstudiogallery.msdn.microsoft.com/](https://visualstudiogallery.msdn.microsoft.com/)  
  
## 使用

加载要替换的DLL文件路径。

点击`替换`可以将当前解决方案中的引用的同名DLL文件替换为选定的DLL文件。

点击`撤销`可撤销替换操作。

## 1、HintPath Dll 替换

HintPath引用替换 是替换csproj中的引用路径，一般用于替换Nuget引用。

具体实现：

这种替换方式本质上是对工程文件csproj中的DLL引用信息做更改。

替换后重新编译解决方案，目标DLL就会引用新的DLL文件。

撤销操作是恢复对csproj文件的更改。

**注意** 

这里的撤销操作依赖于git的checkout命令，使用checkout命令恢复对csproj文件的更改。所以，这里要求在替换之前，csproj不能有未提交的更改。

另外，解决方案必须使用git进行管理，否则撤销将无法进行。

## 2、 文件替换

文件替换 是简单地对Debug目录下的DLL文件进行替换。

撤销操作是利用备份的Debug目录下的DLL文件进行撤销，如果备份文件丢失，将无法撤销。

## 应用场景：

* 通过替换DLL为其它解决方案Debug目录下的DLL，可以在调试时，调整到引用DLL的代码内部调试。

