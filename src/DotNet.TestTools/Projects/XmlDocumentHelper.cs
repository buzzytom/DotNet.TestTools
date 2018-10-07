using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace DotNet.TestTools.Projects
{
    public static class XmlDocumentHelper
    {
        public static XmlNode FirstOrDefault(this XmlNodeList collection, Func<XmlNode, bool> predicate)
        {
            return collection
                .Where(predicate)
                .FirstOrDefault();
        }

        public static IEnumerable<XmlNode> Where(this XmlNodeList collection, Func<XmlNode, bool> predicate)
        {
            for (int i = 0; i < collection.Count; i++)
            {
                if (predicate(collection[i]))
                    yield return collection[i];
            }
        }

        public static IEnumerable<XmlNode> SelectMany<TEntity>(this IEnumerable<TEntity> collection, Func<TEntity, XmlNodeList> selector)
        {
            foreach (TEntity entity in collection)
            {
                XmlNodeList nodes = selector(entity);
                for (int i = 0; i < nodes.Count; i++)
                    yield return nodes[i];
            }
        }

        public static string ReadAttribute(this XmlNode node, string name)
        {
            for (int i = 0; i < node.Attributes.Count; i++)
            {
                if (node.Attributes[i].Name == name)
                    return node.Attributes[i].Value;
            }
            return null;
        }
    }
}
