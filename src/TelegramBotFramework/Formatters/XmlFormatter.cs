#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace TelegramBotFramework.Formatters;

using System.Xml.Linq;
using TelegramBotFramework.Models;

/// <summary>
/// Formats data as XML output for exports and interoperability.
/// Handles proper XML escaping and hierarchical structures.
/// </summary>
public sealed class XmlFormatter : IOutputFormatter, IXmlFormatter
{
    private readonly bool _pretty;

    public XmlFormatter(bool pretty = true)
    {
        _pretty = pretty;
    }

    /// <summary>
    /// Gets the pretty-print setting of this formatter.
    /// </summary>
    /// <returns>The pretty-print setting.</returns>
    public bool GetPretty()
    {
        return _pretty;
    }

    public string Format<T>(T data)
    {
        var element = SerializeObject(data, typeof(T).Name);
        return element.ToString(GetSaveOptions());
    }

    public string Format<T>(IEnumerable<T> data)
    {
        var items = data.ToList();
        var root = new XElement("items");

        foreach (var item in items)
        {
            root.Add(SerializeObject(item, "item"));
        }

        var document = new XDocument(root);
        return document.ToString(GetSaveOptions());
    }

    public string FormatError(string errorCode, string message, string? details = null)
    {
        var root = new XElement("error",
            new XElement("code", errorCode),
            new XElement("message", message),
            new XElement("details", details ?? string.Empty),
            new XElement("timestamp", DateTime.UtcNow.ToString("O"))
        );

        return root.ToString(GetSaveOptions());
    }

    public string FormatMessage(Message message)
    {
        var element = new XElement("message",
            new XElement("id", message.MessageId),
            new XElement("content", message.Content),
            new XElement("userId", message.UserId),
            new XElement("chatId", message.ChatId),
            new XElement("createdAt", message.CreatedAt.ToString("O")),
            new XElement("type", message.Type.ToString())
        );

        return element.ToString(GetSaveOptions());
    }

    public string FormatMessages(IEnumerable<Message> messages)
    {
        var root = new XElement("messages");

        foreach (var msg in messages)
        {
            root.Add(new XElement("message",
                new XElement("id", msg.MessageId),
                new XElement("content", msg.Content),
                new XElement("userId", msg.UserId),
                new XElement("chatId", msg.ChatId),
                new XElement("createdAt", msg.CreatedAt.ToString("O")),
                new XElement("type", msg.Type.ToString())
            ));
        }

        root.SetAttributeValue("count", root.Elements("message").Count());
        return root.ToString(GetSaveOptions());
    }

    /// <summary>
    /// Recursively serializes an object to XML element.
    /// </summary>
    private XElement SerializeObject(object? obj, string elementName)
    {
        var element = new XElement(elementName);

        if (obj  is null)
            return element;

        var type = obj.GetType();
        var properties = type.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

        foreach (var prop in properties)
        {
            if (!prop.CanRead)
                continue;

            var value = prop.GetValue(obj);

            if (value  is null)
            {
                element.Add(new XElement(prop.Name));
            }
            else if (value is string || value.GetType().IsPrimitive)
            {
                element.Add(new XElement(prop.Name, value));
            }
            else if (value is System.Collections.IEnumerable enumerable && !(value is string))
            {
                var collectionElement = new XElement(prop.Name);
                foreach (var item in enumerable)
                {
                    collectionElement.Add(SerializeObject(item, "item"));
                }
                element.Add(collectionElement);
            }
            else
            {
                element.Add(SerializeObject(value, prop.Name));
            }
        }

        return element;
    }

    private SaveOptions GetSaveOptions()
    {
        return _pretty ? SaveOptions.None : SaveOptions.DisableFormatting;
    }
}