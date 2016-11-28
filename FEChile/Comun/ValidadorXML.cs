using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Text;

namespace Comun
{

public class ValidadorXML
{
    private int _iErr = 0;             // Validation Error Count
    private string _sMsj = "";        // Validation Error Message
    private XmlSchemaSet sc;                // Esquema

    public string sMsj
    {
        get { return _sMsj; }
    }
    public int iErr
    {
        get { return _iErr; }
    }

    public ValidadorXML(String URLArchivoXSD)
    {
        // Create the XmlSchemaSet class.
        sc = new XmlSchemaSet();
        try
        {
            // Add the schema to the collection.
            sc.Add(null, URLArchivoXSD);
        }
        catch
        {
            _sMsj = "No se encontró el esquema en el URL: " + URLArchivoXSD;
            _iErr++;
        }
    }

    // Display any warnings or errors.
    private void ValidationCallBack(object sender, ValidationEventArgs args)
    {
        if (args.Severity == XmlSeverityType.Warning)
            _sMsj = "No se encontró el esquema. No se pudo validar el archivo xml. Verifique la configuración. " + args.Message;
            //Console.WriteLine("\tWarning: Matching schema not found.  No validation occurred." + args.Message);
        else
            _sMsj = args.Message;
            //Console.WriteLine("\tValidation error: " + args.Message);
        _iErr++;
    }

    public bool ValidarXSD(XmlDocument archivoXml)
    {
        _iErr = 0;
        _sMsj = "";

        XmlNodeReader nodeReader = new XmlNodeReader(archivoXml);

        // Set the validation settings.
        XmlReaderSettings settings = new XmlReaderSettings();
        settings.ValidationType = ValidationType.Schema;
        settings.Schemas = sc;
        settings.ValidationEventHandler += new ValidationEventHandler (ValidationCallBack);

        try
        {
            // Create the XmlReader object.
            XmlReader reader = XmlReader.Create(nodeReader, settings);
            // Parse the file. 
            while (reader.Read()) ;
            reader.Close();
        }
        catch (Exception eXsd)
        {
            _sMsj = "Excepción al validar el documento XML. Revise los datos del documento. [ValidadorXML.ValidarXSD] " + eXsd.Message;
            _iErr++;
        }
        return _iErr == 0;
    }
}



}
