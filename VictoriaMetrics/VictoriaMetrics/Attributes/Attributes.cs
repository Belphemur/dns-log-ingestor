using System;

namespace VictoriaMetrics.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class Timestamp : Attribute
    {
        
    }
    
    [AttributeUsage(AttributeTargets.Property)]
    public class Tag : Attribute
    {
        public string Name { get; set; }
    }
    
    [AttributeUsage(AttributeTargets.Property)]
    public class Value : Attribute
    {
        public string Name { get; set; }
    }
    
}