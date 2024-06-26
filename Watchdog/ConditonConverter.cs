﻿using System;
using System.Collections.Generic;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Watchdog
{
    public class ConditionConverter : IYamlTypeConverter
    {
        public bool Accepts(Type type)
        {
            return typeof(ICondition).IsAssignableFrom(type);
        }

        public object ReadYaml(IParser parser, Type type)
        {
            parser.Consume<MappingStart>();
            var conditionType = string.Empty;
            var properties = new Dictionary<string, object>();

            while (!parser.TryConsume<MappingEnd>(out var _))
            {
                var propertyName = parser.Consume<Scalar>().Value;
                parser.TryConsume<Scalar>(out var value);
                if (propertyName == "Condicion")
                    conditionType = value.Value;
                else
                    properties[propertyName] = value.Value;
            }

            return ConditionFactory.CreateCondition(conditionType, properties);
        }

        public void WriteYaml(IEmitter emitter, object value, Type type)
        {
            throw new NotImplementedException("Writing YAML is not supported.");
        }
    }
    public static class ConditionFactory
    {
        public static ICondition CreateCondition(string conditionType, IDictionary<string, object> properties)
        {
            return conditionType switch
            {
                "Inactividad" => new FileInactiveFor
                {
                    TiempoLimite = TimeSpan.Parse((string)properties["TiempoLimite"]),
                    Unanime = properties.TryGetValue("Unanime", out object? value) && bool.Parse((string)value),
                },
                "UltimaLinea" => new NewLineContains
                {
                    Contiene = (string)properties["Contiene"],
                    Unanime = properties.TryGetValue("Unanime", out object? value) && bool.Parse((string)value),
                },
                _ => throw new ArgumentException($"Unsupported condition type: {conditionType}"),
            };
        }
    }
}