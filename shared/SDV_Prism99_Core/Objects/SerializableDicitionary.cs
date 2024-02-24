using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;



namespace Prism99_Core.Objects
{
    /// <summary>
    /// Base on https://weblogs.asp.net/pwelter34/444961
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// 
#if false
    [XmlRoot("dictionary")]
    [Serializable]
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, IXmlSerializable
    {
        // XmlSerializer.Deserialize() will create a new Object, and then call ReadXml()
        // So cannot use instance field, use class field.

        public static string itemTag = "item";
        public static string keyTag = "key";
        public static string valueTag = "value";

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            if (reader.IsEmptyElement)
                return;

            var keySerializer = new XmlSerializer(typeof(TKey));
            var valueSerializer = new XmlSerializer(typeof(TValue));

            reader.ReadStartElement();

            // IsStartElement() will call MoveToContent()
            // reader.MoveToContent();
            while (reader.IsStartElement(itemTag))
            {
                reader.ReadStartElement(itemTag);

                reader.ReadStartElement(keyTag);
                TKey key = (TKey)keySerializer.Deserialize(reader);
                reader.ReadEndElement();

                reader.ReadStartElement(valueTag);
                TValue value = (TValue)valueSerializer.Deserialize(reader);
                reader.ReadEndElement();


                reader.ReadEndElement();
                this.Add(key, value);
                // IsStartElement() will call MoveToContent()
                // reader.MoveToContent();
            }
            reader.ReadEndElement();
        }
        public void WriteXml(XmlWriter writer)
        {
            var keySerializer = new XmlSerializer(typeof(TKey));
            //var valueSerializer = new XmlSerializer(typeof(TValue));

            var aor = new XmlAttributeOverrides();
            var Attribs = new XmlAttributes();
            Attribs.XmlElements.Add(new XmlElementAttribute("XmlInclude", typeof(FarmExpansionLocation)));
            Attribs.XmlElements.Add(new XmlElementAttribute("XmlInclude", typeof(LocationExpansion)));
            Attribs.XmlElements.Add(new XmlElementAttribute("XmlInclude", typeof(GreenSlime)));
            Attribs.XmlElements.Add(new XmlElementAttribute("XmlInclude", typeof(FromagerieLocation)));
#if v16
            // TODO: fix this
//Attribs.XmlElements.Add(new XmlElementAttribute("XmlInclude", typeof(BuildingData)));
#endif
            aor.Add(typeof(GameLocation), "Models", Attribs);

            XmlSerializer valueSerializer = new XmlSerializer(typeof(TValue), aor);

            //XmlSerializer valueSerializer = new XmlSerializer(typeof(TValue), new Type[1]
            //     {
            //typeof(GreenSlime)
            //     });

            //XmlSerializer valueSerializer = new XmlSerializer(typeof(TValue), new Type[24]
            //       {
            //typeof(Tool),
            //typeof(Duggy),
            //typeof(Ghost),
            //typeof(GreenSlime),
            //typeof(LavaCrab),
            //typeof(RockCrab),
            //typeof(ShadowGuy),
            //typeof(Child),
            //typeof(Pet),
            //typeof(Dog),
            //typeof(Cat),
            //typeof(Horse),
            //typeof(SquidKid),
            //typeof(Grub),
            //typeof(Fly),
            //typeof(DustSpirit),
            //typeof(Bug),
            //typeof(BigSlime),
            //typeof(BreakableContainer),
            //typeof(MetalHead),
            //typeof(ShadowGirl),
            //typeof(Monster),
            //typeof(JunimoHarvester),
            //typeof(TerrainFeature)
            //       });


            foreach (var kvp in this)
            {
                writer.WriteStartElement(itemTag);

                writer.WriteStartElement(keyTag);
                keySerializer.Serialize(writer, kvp.Key);
                writer.WriteEndElement();

                writer.WriteStartElement(valueTag);
                valueSerializer.Serialize(writer, kvp.Value);
                writer.WriteEndElement();

                writer.WriteEndElement();
            }
        }
    }
#endif

    [XmlRoot("dictionary")]
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, IXmlSerializable
    {
        public struct ChangeArgs
        {
            public readonly ChangeType Type;

            public readonly TKey Key;

            public readonly TValue Value;

            public ChangeArgs(ChangeType type, TKey k, TValue v)
            {
                Type = type;
                Key = k;
                Value = v;
            }
        }

        public delegate void ChangeCallback(object sender, ChangeArgs args);

        private static XmlSerializer _keySerializer;

        private static XmlSerializer _valueSerializer;

        public event ChangeCallback CollectionChanged;

        static SerializableDictionary()
        {
            _keySerializer = GetSerializer(typeof(TKey));
            _valueSerializer = GetSerializer(typeof(TValue));

        }
        private static XmlSerializer GetSerializer(Type tType)
        {
            List<Type> extraTypes = new List<Type> { };
            //
            //  check for dga
            //if (FEFramework.helper.ModRegistry.IsLoaded("spacechase0.DynamicGameAssets"))
            //{
            //    //
            //    //  add possible dga types
            //    //
            //    extraTypes.Add(Type.GetType("DynamicGameAssets.Game.CustomBasicFurniture, DynamicGameAssets"));
            //    extraTypes.Add(Type.GetType("DynamicGameAssets.Game.CustomBedFurniture, DynamicGameAssets"));
            //    extraTypes.Add(Type.GetType("DynamicGameAssets.Game.CustomBigCraftable, DynamicGameAssets"));
            //    extraTypes.Add(Type.GetType("DynamicGameAssets.Game.CustomCrop, DynamicGameAssets"));
            //    extraTypes.Add(Type.GetType("DynamicGameAssets.Game.CustomFence, DynamicGameAssets"));
            //    extraTypes.Add(Type.GetType("DynamicGameAssets.Game.CustomFruitTree, DynamicGameAssets"));
            //    extraTypes.Add(Type.GetType("DynamicGameAssets.Game.CustomGiantCrop, DynamicGameAssets"));
            //    extraTypes.Add(Type.GetType("DynamicGameAssets.Game.CustomObject, DynamicGameAssets"));
            //    extraTypes.Add(Type.GetType("DynamicGameAssets.Game.CustomStorageFurniture, DynamicGameAssets"));
            //    extraTypes.Add(Type.GetType("DynamicGameAssets.Game.CustomTVFurniture, DynamicGameAssets"));
            //}
            return new XmlSerializer(tType, extraTypes.ToArray());
        }
        public new void Add(TKey key, TValue value)
        {
            base.Add(key, value);
            OnCollectionChanged(this, new ChangeArgs(ChangeType.Add, key, value));
        }

        public new bool Remove(TKey key)
        {
            if (TryGetValue(key, out var val))
            {
                base.Remove(key);
                OnCollectionChanged(this, new ChangeArgs(ChangeType.Remove, key, val));
                return true;
            }
            return false;
        }

        public new void Clear()
        {
            base.Clear();
            OnCollectionChanged(this, new ChangeArgs(ChangeType.Clear, default(TKey), default(TValue)));
        }

        private void OnCollectionChanged(object sender, ChangeArgs args)
        {
            this.CollectionChanged?.Invoke(sender ?? this, args);
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            bool isEmptyElement = reader.IsEmptyElement;
            reader.Read();
            if (!isEmptyElement)
            {
                while (reader.NodeType != XmlNodeType.EndElement)
                {
                    reader.ReadStartElement("item");
                    reader.ReadStartElement("key");
                    TKey key = (TKey)_keySerializer.Deserialize(reader);
                    reader.ReadEndElement();
                    reader.ReadStartElement("value");
                    TValue value = (TValue)_valueSerializer.Deserialize(reader);
                    reader.ReadEndElement();
                    base.Add(key, value);
                    reader.ReadEndElement();
                    reader.MoveToContent();
                }
                reader.ReadEndElement();
            }
        }

        public void WriteXml(XmlWriter writer)
        {
            foreach (TKey key in base.Keys)
            {
                writer.WriteStartElement("item");
                writer.WriteStartElement("key");
                _keySerializer.Serialize(writer, key);
                writer.WriteEndElement();
                writer.WriteStartElement("value");
                TValue value = base[key];
                _valueSerializer.Serialize(writer, value);
                writer.WriteEndElement();
                writer.WriteEndElement();
            }
        }
    }

}