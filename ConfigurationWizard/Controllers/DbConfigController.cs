using ConfigurationWizard.models;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Xml;

namespace ConfigurationWizard.Controllers
{
    [ApiController]
    [Route("[controller]")]

    public class DbConfigController : ControllerBase
    {
        private readonly string dbConfigPath = @"C:\Users\Public\Documents\MIR\Sunrise\";
        private readonly string dbConfigName = "Dbconnect.xml";
        [HttpGet]
        public DbConfig Get()
        {
            if (!System.IO.File.Exists(dbConfigPath + dbConfigName))
                return null;

            using (FileStream SourceStream = System.IO.File.Open(dbConfigPath + dbConfigName, FileMode.Open))
            {
                XmlDocument xmldoc = new();
                xmldoc.Load(SourceStream);
                var connectionString = xmldoc.GetElementsByTagName("connectionString")[0];
                if (connectionString != null)
                {
                  var connectinStringPrms = connectionString.InnerText.Split(';');
                    var arrayRes = new string[4];

                    for (int index = 0; index < connectinStringPrms.Length; index++)
                    {
                        arrayRes[index] = connectinStringPrms[index].Split('=')[1];
                    }
               
                    return new DbConfig(arrayRes[0], arrayRes[1], arrayRes[2], arrayRes[3]);
                }
                

            }

            return null;
        }

        [HttpPatch]
        public bool Udpate(DbConfig prm)
        {
            try
            {
                DirectoryInfo dirInfo = new(dbConfigPath);

                if (System.IO.File.Exists(dbConfigPath))
                {
                    CreateDbConfig(prm);
                }
                else
                {
                    Directory.CreateDirectory(dbConfigPath);
                    CreateDbConfig(prm);
                }
            }
            catch
            {
                return false;
            }

            return true;
        }

        private void CreateDbConfig(DbConfig prm)
        {
            XmlDocument xmlDoc = new();
            XmlNode rootNode = xmlDoc.CreateElement("Dbconfig");

            XmlAttribute attributeXsi = xmlDoc.CreateAttribute("xmlns:xsi");
            attributeXsi.Value = "http://www.w3.org/2001/XMLSchema-instance";
            rootNode.Attributes.Append(attributeXsi);

            XmlAttribute attributeXsd = xmlDoc.CreateAttribute("xmlns:xsd");
            attributeXsd.Value = "http://www.w3.org/2001/XMLSchema";
            rootNode.Attributes.Append(attributeXsd);
            xmlDoc.AppendChild(rootNode);

            XmlNode providerName = xmlDoc.CreateElement("providerName");
            providerName.InnerText = "Sql";
            rootNode.AppendChild(providerName);

            XmlNode connectionString = xmlDoc.CreateElement("connectionString");
            connectionString.InnerText = $"Data Source={prm.HostName};Initial Catalog={prm.DbName};User ID={prm.UserName};Password={prm.Password}";
            rootNode.AppendChild(connectionString);

            xmlDoc.Save(dbConfigPath + dbConfigName);
        }
    }
}
