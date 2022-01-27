using Microsoft.AspNetCore.Mvc;
using System.ServiceProcess;
using System.Linq;
using System.Net.Sockets;
using System;
using System.Diagnostics;
using ConfigurationWizard.models;

namespace ConfigurationWizard.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ServicesController : Controller
    {

        private string GetServiceName(ServicesName servicesName)
        {
            switch (servicesName)
            {
                case ServicesName.DAService:
                    {
                        return "DAService";
                    }
                case ServicesName.MirJournalService:
                    {
                        return "MirJournalService";
                    }
                case ServicesName.Dispatcher:
                    {
                        return "Mir.Scada.Dispatcher.Api";
                    }
                case ServicesName.Editor:
                    {
                        return "Mir.Scada.Editor.Api";
                    }

            }

            return "";
        }

        private string GetNetTcpName(ServicesName servicesName)
        {
            switch (servicesName)
            {
                case ServicesName.DAService:
                    {
                        return "DAService.exe";
                    }
                case ServicesName.MirJournalService:
                    {
                        return "Mir.Journal.Service.exe";
                    }
                case ServicesName.Dispatcher:
                    {
                        return "Mir.Scada.Dispatcher.Api.exe";
                    }
                case ServicesName.Editor:
                    {
                        return "Mir.Scada.Editor.Api.exe";
                    }

            }

            return "";
        }

        private string CheckTcpIpClient(string hostName, int port)
        {
            try
            {
                TcpClient tcpClient = new();
                tcpClient.Connect(hostName, port);
                var result = tcpClient.Connected;
                tcpClient.Close();
                return "";
            }
            catch (SocketException e)
            {
                return e.Message;
            }
        }

        private string CheckListeningServicePort(string allProcess, int port, ServicesName serviceName )
        {
            var splitRow = allProcess.Split("\r\n");
            var rowWithIndex = splitRow.Select((item, index) =>
            new
            {
                ItemName = item,
                Position = index - 1
            }).Where(x => x.ItemName.Contains(":" + port.ToString()) && x.ItemName.Contains("LISTENING"));

            var stringServiceName = GetNetTcpName(serviceName);

            if (rowWithIndex.Any(x => splitRow[x.Position].Contains(stringServiceName)))
            {
                return "";
            }
            else
            {
                var result = "порт " + port.ToString() + " занят другим процессом:";
                result += " " + splitRow[rowWithIndex.FirstOrDefault().Position];
                return result;
            }
        }

        private string GetProcessCmd(ServicesName serviceName)
        {
            try
            {
                using Process p = new();
                // set start info
                p.StartInfo = new ProcessStartInfo("cmd.exe")
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = false,
                    Arguments = "/C netstat -anb"
                };
                p.Start();
                p.WaitForExit(1000);
                string output = p.StandardOutput.ReadToEnd();
                return output;

            }
            catch (Exception ex)
            {
                return $"Ошибка при проверке тестового конракта {serviceName}" + ex.Message;
            }
        }

        [HttpPost]
        [Route("/checkSerice")]
        public string CheckSerice(int[] ports, ServicesName serviceName, string hostName = "localhost")
        {
            var scServices = ServiceController.GetServices();
            var stringServiceName = GetServiceName(serviceName);
            var service = scServices.FirstOrDefault(x => x.ServiceName == stringServiceName);
            if (service != null)
            {

                if (service.Status != ServiceControllerStatus.Running)
                {
                    return $"Служба {serviceName.ToString()} не активна!";
                }

                foreach(var port in ports)
                {
                    var errorMessage = CheckTcpIpClient(hostName, port);
                    if (errorMessage.Length > 0)
                    {
                        return $"Соединение с портом {port} не установлено!: {errorMessage}";
                    }
                }

                var allProcess = GetProcessCmd(serviceName);

                if(allProcess.Contains("Запрошенная операция требует повышения"))
                {
                    return "Запустите программу в режиме администратора!";
                }

                foreach (var port in ports)
                { 
                    var checkPort = CheckListeningServicePort(allProcess, port, serviceName);
                        if (checkPort.Length > 0)
                        {
                            return checkPort;
                        }
                }

                return "";

            }
            else
            {
                return $"Служба {serviceName.ToString()} не найдена!";
            }

        }

        [HttpGet]
        [Route("/restartService")]
        public string RestartService(ServicesName service)
        {
            try
            {
                var scServices = ServiceController.GetServices();
                var stringServiceName = GetServiceName(service);
                var currentService = scServices.FirstOrDefault(x => x.ServiceName == stringServiceName);
                if (currentService != null)
                {
                    int timeoutMilliseconds = 10000;
                    int millisec1 = Environment.TickCount;
                    TimeSpan timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds);
              
                    currentService.Stop();
                    currentService.WaitForStatus(ServiceControllerStatus.Stopped, timeout);

                    int millisec2 = Environment.TickCount;
                    timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds - (millisec2 - millisec1));

                    currentService.Start();
                    currentService.WaitForStatus(ServiceControllerStatus.Running, timeout);
                    return "";

                }
                return $"Не удалось перезапустить службу {service} так как она не найдена!";
            }

            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}
