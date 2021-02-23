using System;

namespace Common.Tools.Attribute
{
    [AttributeUsage(AttributeTargets.All, Inherited = true)]
    public class Alias: System.Attribute
    {
        public string Name { get; set; }

    }
}