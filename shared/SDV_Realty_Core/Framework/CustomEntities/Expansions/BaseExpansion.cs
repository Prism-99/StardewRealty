using System.Xml.Serialization;


namespace SDV_Realty_Core.Framework.Expansions
{
    [XmlInclude(typeof(FarmExpansionLocation))]
    [XmlInclude(typeof(LocationExpansion))]
    //[XmlInclude(typeof(FromagerieLocation))]
    //[XmlInclude(typeof(BreadFactoryLocation))]
    public interface BaseExpansion
    {
        public bool Active { get; set; }
        public bool AutoAdd { get; set; }
        public int GridId { get; set; }
    }
}
