using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DllRefChanger
{
    public class SolutionConfig
    {
        /// <summary>
        /// 解决方案sln文件的绝对路径
        /// </summary>
        public string AbsolutePath { get; set; }

        /// <summary>
        /// 解决方案名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// DLL名称，被替换的DLL名称
        /// </summary>
        public string DllName { get; set; }

        /// <summary>
        /// 源DLL版本，被替换的DLL版本
        /// </summary>
        public string SourceDllVersion { get; set; }
        
        /// <summary>
        /// 目标DLL的绝对路径
        /// </summary>
        public string NewFileAbsolutePath { get; set; }

    }
}
