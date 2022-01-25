using ConfigurationWizard.models;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Xml;

namespace ConfigurationWizard.Controllers
{
    [ApiController]
    [Route("[controller]")]

    public class JournalConfigController : ControllerBase
    {
        private readonly string journalConfigPath = @"C:\Users\Public\Documents\MIR\Journal\";
        private readonly string journalConfigName = "JournalConfig.xml";
        [HttpGet]
        public ConfigInfo Get()
        {
            if (!System.IO.File.Exists(journalConfigPath + journalConfigName))
                return null;

            using (FileStream SourceStream = System.IO.File.Open(journalConfigPath + journalConfigName, FileMode.Open))
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
               
                    return new ConfigInfo(arrayRes[0], arrayRes[1], arrayRes[2], arrayRes[3]);
                }
                

            }

            return null;
        }

        [HttpPatch]
        public bool Udpate(ConfigInfo prm)
        {
            try
            {
                DirectoryInfo dirInfo = new(journalConfigPath);

                if (System.IO.File.Exists(journalConfigPath))
                {
                    CreateJournalConfig(prm);
                }
                else
                {
                    Directory.CreateDirectory(journalConfigPath);
                    CreateJournalConfig(prm);
                }
            }
            catch
            {
                return false;
            }

            return true;
        }

        private void CreateJournalConfig(ConfigInfo prm)
        {
            XmlDocument xmlDoc = new();
            XmlNode rootNode = xmlDoc.CreateElement("Config");

            XmlAttribute attributeXsi = xmlDoc.CreateAttribute("xmlns:xsi");
            attributeXsi.Value = "http://www.w3.org/2001/XMLSchema-instance";
            rootNode.Attributes.Append(attributeXsi);

            XmlAttribute attributeXsd = xmlDoc.CreateAttribute("xmlns:xsd");
            attributeXsd.Value = "http://www.w3.org/2001/XMLSchema";
            rootNode.Attributes.Append(attributeXsd);
            xmlDoc.AppendChild(rootNode);

            XmlNode connectionString = xmlDoc.CreateElement("connectionString");
            connectionString.InnerText = $"Data Source={prm.HostName};Initial Catalog={prm.DbName};User ID={prm.UserName};Password={prm.Password}";
            rootNode.AppendChild(connectionString);

            XmlNode logLevel = xmlDoc.CreateElement("LogLevel");
            logLevel.InnerText = "Error";
            rootNode.AppendChild(logLevel);

            xmlDoc.Save(journalConfigPath + journalConfigName);
        }
    }
}
