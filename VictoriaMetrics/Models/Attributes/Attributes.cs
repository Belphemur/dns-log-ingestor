#nullable enable
using System;

namespace VictoriaMetrics.Models.Attributes
{
    public abstract class NamedAttribute : Attribute
    {
        public string? Name { get; }

        protected NamedAttribute(string name = null)
        {
            Name = name;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class Measurement : NamedAttribute
    {
        public Measurement(string name = null) : base(name)
        {
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class Timestamp : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class Tag : NamedAttribute
    {
        public Tag(string name = null) : base(name)
        {
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class Field : NamedAttribute
    {
        public Field(string name = null) : base(name)
        {
        }
    }
}