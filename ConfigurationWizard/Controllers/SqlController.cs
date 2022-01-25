using ConfigurationWizard.models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace ConfigurationWizard.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SqlController : ControllerBase
    {
        private string CreateDatabaseQuery(string dbName)
        {
            return string.Format("CREATE DATABASE {0};", dbName);
        }



        private string EnableReadCommitedSnapshot(string dbName)

        {
            return $"ALTER DATABASE [{dbName}] SET READ_COMMITTED_SNAPSHOT ON WITH NO_WAIT;";

        }



        private string SetCollation(string dbName)

        {
            return $"ALTER DATABASE {dbName} COLLATE Cyrillic_General_CI_AS;";

        }

        private async Task<List<Databases>> GetAllDbNames(SqlConnection con)
        {

            var result = new List<Databases>();
            using (SqlCommand cmd = new("SELECT name from sys.databases", con))
            {
                using (IDataReader dr = await cmd.ExecuteReaderAsync())
                {
                    while (dr.Read())
                    {
                        result.Add(new(dr[0].ToString(), dr[0].ToString()));
                    }
                }
            }

            return result;
        }

        [HttpPost]
        [Route("/getDatabases")]
        public async Task<List<Databases>> GetDatabases(GetDatabasesParams prm)
        {

            {
                string conString = $"server={prm.DataSource};User Id={prm.UserId};pwd={prm.Password};";

                using (SqlConnection con = new(conString))
                {
                    try
                    {
                        await con.OpenAsync();

                        return await GetAllDbNames(con);
                    }
                    catch (SqlException e)
                    {
                        throw new Exception(e.Message);
                    }
                }
        
            }
        }

        [HttpPost]
        [Route("/checkSqlConnections")]
        public async Task<bool> CheckSqlConnections(GetDatabasesParams prm)
        {
            string conString = $"server={prm.DataSource};User Id={prm.UserId};pwd={prm.Password};";

            using (SqlConnection con = new(conString))
            {
                try
                {
                    await con.OpenAsync();
                    return true;

                }
                catch (SqlException e)
                {
                    return false;
                }
            }
        }

        [HttpPost]
        [Route("/checkDatabaseExist")]
        public async Task<bool> CheckDatabaseExist(ConfigInfo prm)
        {
            string conString = $"server={prm.HostName};User Id={prm.UserName};pwd={prm.Password};";

            try
            {
                using (SqlConnection con = new(conString))
                {
                    await con.OpenAsync();

                    var databases = await GetAllDbNames(con);

                    return databases.Any(x => x.Title == prm.DbName);
                }
            }
            catch (SqlException e)
            {
                return false;
            }
        }

        [HttpPost]
        [Route("/createNewDB")]
        public async Task<string> CreateNewDB(ConfigInfo prm)
        {
            string conString = $"server={prm.HostName};User Id={prm.UserName};pwd={prm.Password};";
            try
            {
                using (SqlConnection con = new(conString))
                {
                    await con.OpenAsync();
                    using SqlCommand cmd = new(CreateDatabaseQuery(prm.DbName) + EnableReadCommitedSnapshot(prm.DbName) + SetCollation(prm.DbName), con);
                    using IDataReader dr = await cmd.ExecuteReaderAsync();
                    return "";

                }

            }
            catch (SqlException e)
            {
                return "Ошибка при создание базы данных: "+e.Message;
            }
            
        }
    }
}

