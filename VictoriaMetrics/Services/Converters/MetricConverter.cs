using System;
using System.Collections.Generic;
using VictoriaMetrics.Exceptions;
using VictoriaMetrics.Extensions;
using VictoriaMetrics.Models.Attributes;
using VictoriaMetrics.Models.Metrics;
using Field = VictoriaMetrics.Models.Attributes.Field;
using FieldObj = VictoriaMetrics.Models.Metrics.Field;
using Tag = VictoriaMetrics.Models.Attributes.Tag;
using TagObj = VictoriaMetrics.Models.Metrics.Tag;

namespace VictoriaMetrics.Services.Converters
{
    public class MetricConverter : IMetricConverter
    {
        /// <summary>
        /// Convert an Object into a metric
        /// </summary>
        /// <param name="toConvert"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="AttributeNotFound">Not a valid object to be transformed into a Metric</exception>
        public Metric ToMetric<T>(T toConvert) where T : class
        {
            var type = toConvert.GetType();
            if (!Attribute.IsDefined(type, typeof(Measurement)))
            {
                throw new AttributeNotFound($"{nameof(Measurement)} attribute not found on {type.Name}.");
            }

            var measurementAtt = (Measurement) Attribute.GetCustomAttribute(type, typeof(Measurement));

            var tags   = new Dictionary<string, TagObj>();
            var fields = new Dictionary<string, FieldObj>();

            DateTime? timestamp = null;

            foreach (var propertyInfo in type.GetProperties())
            {
                foreach (var attribute in propertyInfo.GetCustomAttributes(true))
                {
                    switch (attribute)
                    {
                        case Tag tag:
                            tags.Add(tag.Name, new TagObj
                            {
                                Name  = tag.Name ?? propertyInfo.Name.ToUnderscoreCase(),
                                Value = propertyInfo.GetValue(toConvert).ToString()
                            });
                            continue;
                        case Field field:
                            fields.Add(field.Name, new FieldObj
                            {
                                Name  = field.Name ?? propertyInfo.Name.ToUnderscoreCase(),
                                Value = (long) propertyInfo.GetValue(toConvert)
                            });
                            break;
                        case Timestamp time:
                            timestamp = (DateTime?) propertyInfo.GetValue(toConvert);
                            break;
                    }
                }
            }

            return new Metric
            {
                Name      = measurementAtt.Name ?? type.Name.ToUnderscoreCase(),
                Tags      = tags,
                Fields    = fields,
                Timestamp = timestamp
            };
        }
    }
}