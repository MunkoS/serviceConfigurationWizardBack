using ConfigurationWizard.models;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace ConfigurationWizard.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DispatcherConfigController : Controller
    {
        private readonly string dbConfigPath = @"C:\Program Files (x86)\MIR\Scada\Dispatcher\";
        private readonly string dbConfigName = "appsettings.Production.json";
        [HttpGet]
        public async Task<ConfigInfo> Get()
        {
            if (!System.IO.File.Exists(dbConfigPath + dbConfigName))
                return null;

            using (FileStream SourceStream = System.IO.File.Open(dbConfigPath + dbConfigName, FileMode.Open))
            {

                DispatherInfo configInfo = await JsonSerializer.DeserializeAsync<DispatherInfo>(SourceStream);
              
                if (configInfo != null)
                {
                    var connectinStringPrms = configInfo.ConnectionStrings.ScadaDb.Split(';');
                    var arrayRes = new string[4];

                    for (int index = 0; index < connectinStringPrms.Length; index++)
                    {
                        arrayRes[index] = connectinStringPrms[index].Split('=')[1];
                    }

                    return new ConfigInfo(arrayRes[0], arrayRes[3], arrayRes[1], arrayRes[2]);
                }
            }

            return null;
        }

        [HttpPatch]
        public async Task<bool> Udpate(ConfigInfo prm)
        {
            try
            {
                DirectoryInfo dirInfo = new(dbConfigPath);

                if (System.IO.File.Exists(dbConfigPath))
                {
                   await CreateConfig(prm);
                }
                else
                {
                    Directory.CreateDirectory(dbConfigPath);
                    await CreateConfig(prm);
                }
            }
            catch
            {
                return false;
            }

            return true;
        }

        private async Task CreateConfig(ConfigInfo prm)
        {
            using (FileStream fs = new FileStream(dbConfigPath + dbConfigName, FileMode.OpenOrCreate))
            {
                DispatherInfo configInfo = new DispatherInfo()
                {
                   Customer = new Customer("АО 'РСК Ямала' г. Салехард"),
                   ConnectionStrings = new ConnectionStrings($"Server={prm.HostName};User ID={prm.UserName};Password={prm.Password};Database={prm.DbName}"),
                   Auth = new Auth(true),
                   Kestrel = new Kestrel(new EndPoints(new HttpInfo("http://localhost:5101")))
                };
                await JsonSerializer.SerializeAsync<DispatherInfo>(fs, configInfo, new JsonSerializerOptions()
                {
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                });
          
            }
        }
    }
}
