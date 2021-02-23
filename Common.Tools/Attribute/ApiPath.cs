using System;
using System.Collections.Generic;

namespace Common.Tools.Attribute
{
    /// <inheritdoc />
    /// <summary>
    /// api路径别名 方法和类注解工具
    /// </summary>
    [AttributeUsage(AttributeTargets.All,Inherited = true)]
    public class ApiPath:System.Attribute
    {
        /// <summary>
        /// 别名
        /// </summary>
        public string Name { get; private set; }

        public ApiPath(string name)
        {
            Name = name;
        }

        public List<ApiPath> ApiPaths { get; set; } 
    }
}