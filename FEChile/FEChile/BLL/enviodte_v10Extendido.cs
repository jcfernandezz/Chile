using System.Xml.Serialization;

[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.sii.cl/SiiDte")]
[System.Xml.Serialization.XmlRootAttribute("DTE", Namespace = "http://www.sii.cl/SiiDte", IsNullable = false)]
public partial class DTEDefTypeExtendido : DTEDefType
{
    private string torefField;

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified, Namespace = "http://www.sii.cl/SiiDte EnvioDTE_v10.xsd")]
    public string schemaLocation
    {
        get
        {
            return this.torefField;
        }
        set
        {
            this.torefField = value;
        }
    }

    //[XmlAttribute("schemaLocation", Namespace = System.Xml.Schema.XmlSchema.InstanceNamespace)]
    //public string xsiSchemaLocation = @"http://www.sii.cl/SiiDte EnvioDTE_v10.xsd";
}

