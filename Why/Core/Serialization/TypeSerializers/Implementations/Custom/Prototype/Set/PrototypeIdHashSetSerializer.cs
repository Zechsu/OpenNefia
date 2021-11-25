using System.Collections.Generic;
using Why.Core.IoC;
using Why.Core.Prototypes;
using Why.Core.Serialization.Manager;
using Why.Core.Serialization.Manager.Result;
using Why.Core.Serialization.Markdown;
using Why.Core.Serialization.Markdown.Sequence;
using Why.Core.Serialization.Markdown.Validation;
using Why.Core.Serialization.Markdown.Value;
using Why.Core.Serialization.TypeSerializers.Interfaces;

namespace Why.Core.Serialization.TypeSerializers.Implementations.Custom.Prototype.Set
{
    public class PrototypeIdHashSetSerializer<TPrototype> : ITypeSerializer<HashSet<string>, SequenceDataNode> where TPrototype : class, IPrototype
    {
        private readonly PrototypeIdSerializer<TPrototype> _prototypeSerializer = new();

        public ValidationNode Validate(ISerializationManager serializationManager, SequenceDataNode node, IDependencyCollection dependencies, ISerializationContext? context = null)
        {
            var list = new List<ValidationNode>();

            foreach (var dataNode in node.Sequence)
            {
                if (dataNode is not ValueDataNode value)
                {
                    list.Add(new ErrorNode(dataNode, $"Cannot cast node {dataNode} to ValueDataNode."));
                    continue;
                }

                list.Add(_prototypeSerializer.Validate(serializationManager, value, dependencies, context));
            }

            return new ValidatedSequenceNode(list);
        }

        public DeserializationResult Read(ISerializationManager serializationManager, SequenceDataNode node, IDependencyCollection dependencies, bool skipHook, ISerializationContext? context = null)
        {
            var set = new HashSet<string>();
            var mappings = new List<DeserializationResult>();

            foreach (var dataNode in node.Sequence)
            {
                var result = _prototypeSerializer.Read(
                    serializationManager,
                    (ValueDataNode) dataNode,
                    dependencies,
                    skipHook,
                    context);

                set.Add((string) result.RawValue!);
                mappings.Add(result);
            }

            return new DeserializedCollection<HashSet<string>, string>(set, mappings,
                elements => new HashSet<string>(elements));
        }

        public DataNode Write(ISerializationManager serializationManager, HashSet<string> value, bool alwaysWrite = false, ISerializationContext? context = null)
        {
            var list = new List<DataNode>();

            foreach (var str in value)
            {
                list.Add(_prototypeSerializer.Write(serializationManager, str, alwaysWrite, context));
            }

            return new SequenceDataNode(list);
        }

        public HashSet<string> Copy(ISerializationManager serializationManager, HashSet<string> source, HashSet<string> target, bool skipHook, ISerializationContext? context = null)
        {
            return new(source);
        }
    }
}
