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
        ArgumentNullException.ThrowIfNull(data);
        var element = SerializeObject(data, typeof(T).Name);
        return element.ToString(GetSaveOptions());
    }

    public string Format<T>(IEnumerable<T> data)
    {
        ArgumentNullException.ThrowIfNull(data);
        var items = data.ToList();
        var root = new XElement(XmlFormatterConstants.ItemsRoot);

        foreach (var item in items)
        {
            root.Add(SerializeObject(item, XmlFormatterConstants.ItemElement));
        }

        var document = new XDocument(root);
        return document.ToString(GetSaveOptions());
    }

    public string FormatError(string errorCode, string message, string? details = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(errorCode);
        ArgumentException.ThrowIfNullOrEmpty(message);
        var root = new XElement(XmlFormatterConstants.ErrorRoot,
            new XElement(XmlFormatterConstants.ErrorCode, errorCode),
            new XElement(XmlFormatterConstants.Message, message),
            new XElement(XmlFormatterConstants.Details, details ?? string.Empty),
            new XElement(XmlFormatterConstants.Timestamp, DateTime.UtcNow.ToString(XmlFormatterConstants.DateFormatRoundtrip))
        );

        return root.ToString(GetSaveOptions());
    }

    public string FormatMessage(Message message)
    {
        ArgumentNullException.ThrowIfNull(message);
        var element = new XElement(XmlFormatterConstants.Message,
            new XElement(XmlFormatterConstants.Id, message.MessageId),
            new XElement(XmlFormatterConstants.Content, message.Content),
            new XElement(XmlFormatterConstants.UserId, message.UserId),
            new XElement(XmlFormatterConstants.ChatId, message.ChatId),
            new XElement(XmlFormatterConstants.CreatedAt, message.CreatedAt.ToString(XmlFormatterConstants.DateFormatRoundtrip)),
            new XElement(XmlFormatterConstants.Type, message.Type.ToString())
        );

        return element.ToString(GetSaveOptions());
    }

    public string FormatMessages(IEnumerable<Message> messages)
    {
        ArgumentNullException.ThrowIfNull(messages);
        var root = new XElement(XmlFormatterConstants.MessagesRoot);

        foreach (var msg in messages)
        {
            root.Add(new XElement(XmlFormatterConstants.Message,
                new XElement(XmlFormatterConstants.Id, msg.MessageId),
                new XElement(XmlFormatterConstants.Content, msg.Content),
                new XElement(XmlFormatterConstants.UserId, msg.UserId),
                new XElement(XmlFormatterConstants.ChatId, msg.ChatId),
                new XElement(XmlFormatterConstants.CreatedAt, msg.CreatedAt.ToString(XmlFormatterConstants.DateFormatRoundtrip)),
                new XElement(XmlFormatterConstants.Type, msg.Type.ToString())
            ));
        }

        root.SetAttributeValue(XmlFormatterConstants.CountAttribute, root.Elements(XmlFormatterConstants.Message).Count());
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