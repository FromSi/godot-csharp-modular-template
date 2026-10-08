using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Game.Game.Common.Service.JsonConverter;

public class JsonConverterStateService : IJsonConverterStateService
{
    private readonly Dictionary<string, Type> _registry = new();
    private readonly JsonSerializerOptions _options;

    public JsonConverterStateService()
    {
        _options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            Converters = { new Vector2JsonConverter() }
        };
    }

    public void Register(Type type)
    {
        if (!string.IsNullOrEmpty(type.Name))
        {
            _registry[type.Name] = type;
        }
    }

    public string Serialize(List<object?> collections)
    {
        var result = new List<object>();

        foreach (var item in collections)
        {
            switch (item)
            {
                case IDictionary { Count: 0 }:
                    result.Add(new Dictionary<string, object> { ["_cls"] = "", ["_arr"] = new List<object>() });
                    continue;
                case IDictionary dict:
                {
                    var values = dict.Values.Cast<object>().Where(value => value != null).ToList();

                    if (values.Count == 0)
                    {
                        result.Add(new Dictionary<string, object> { ["_cls"] = "", ["_arr"] = new List<object>() });
                        continue;
                    }

                    var firstValue = values[0];
                    var clsName = firstValue.GetType().Name;

                    if (!_registry.ContainsKey(clsName))
                        continue;

                    result.Add(new Dictionary<string, object> { ["_cls"] = clsName, ["_arr"] = values });
                    break;
                }
                case null:
                    result.Add(new Dictionary<string, object?> { ["_cls"] = "", ["_obj"] = null });
                    break;
                default:
                {
                    var clsName = item.GetType().Name;

                    if (!_registry.ContainsKey(clsName))
                        continue;

                    result.Add(new Dictionary<string, object> { ["_cls"] = clsName, ["_obj"] = item });
                    break;
                }
            }
        }

        return JsonSerializer.Serialize(result, _options);
    }

    public List<object?> Deserialize(string json)
    {
        var entries = JsonSerializer.Deserialize<List<JsonElement>>(json, _options);

        if (entries == null)
            return [];

        var result = new List<object?>();

        foreach (var entry in entries)
        {
            var clsName = entry.GetProperty("_cls").GetString() ?? "";

            if (entry.TryGetProperty("_arr", out var arrEl))
            {
                if (clsName == "")
                {
                    result.Add(new Dictionary<int, object?>());
                    continue;
                }

                if (!_registry.TryGetValue(clsName, out var type))
                    continue;

                var dictResult = new Dictionary<int, object?>();

                foreach (var d in arrEl.EnumerateArray())
                {
                    var obj = JsonSerializer.Deserialize(d.GetRawText(), type, _options);
                    if (obj == null) continue;

                    var idProperty = type.GetProperty("Id");
                    if (idProperty != null)
                    {
                        var id = (int)idProperty.GetValue(obj)!;
                        dictResult[id] = obj;
                    }
                }

                result.Add(dictResult);
            }
            else if (entry.TryGetProperty("_obj", out var objEl))
            {
                if (objEl.ValueKind == JsonValueKind.Null)
                {
                    result.Add(null);
                    continue;
                }

                if (clsName == "" || !_registry.TryGetValue(clsName, out var type))
                    continue;

                var obj = JsonSerializer.Deserialize(objEl.GetRawText(), type, _options);
                result.Add(obj);
            }
        }

        return result;
    }
}
