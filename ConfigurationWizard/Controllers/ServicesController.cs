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
        private bool CheckTcpIpClient(string hostName, int port)
        {
            try
            {
                TcpClient tcpClient = new();
                tcpClient.Connect(hostName, port);
                var result = tcpClient.Connected;
                tcpClient.Close();
                return result;
            }
            catch (SocketException)
            {
                return false;
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

            if (rowWithIndex.Any(x => splitRow[x.Position].Contains($"{(serviceName == ServicesName.MirJournalService ? "Mir.Journal.Service" : serviceName)}.exe")))
            {
                return "";
            }
            else
            {
                var result = "порт " + port.ToString() + " занят другим процессом:";
                rowWithIndex.ToList().ForEach(row =>
                {
                    if (splitRow[row.Position].Length > 0)
                    {
                        result += " " + splitRow[row.Position];
                    }
                });
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
            var service = scServices.FirstOrDefault(x => x.ServiceName == serviceName.ToString());
            if (service != null)
            {

                if (service.Status != ServiceControllerStatus.Running)
                {
                    return $"Служба {serviceName.ToString()} не активна!";
                }

                foreach(var port in ports)
                {
                    if (!CheckTcpIpClient(hostName, port))
                    {
                        return $"Соединение с портом {port} не установлено!";
                    }
                }
/*
                if (!CheckTcpIpClient(hostName, 7070))
                {
                    return "Соединение с портом 7070 не установлено!";
                }

                if (!CheckTcpIpClient(hostName, 4568))
                {
                    return "Соединение с портом 4568 не установлено!";
                }
*/
                var allProcess = GetProcessCmd(serviceName);


                foreach (var port in ports)
                { 
                    var checkPort = CheckListeningServicePort(allProcess, port, serviceName);
                        if (checkPort.Length > 0)
                        {
                            return checkPort;
                        }
                }

             /*   var port7070 = CheckListeningDasPort(allProcess, 7070);
                var port4568 = CheckListeningDasPort(allProcess, 4568);
                if (port7070.Length > 0)
                {
                    return port7070;
                }


                if (port4568.Length > 0)
                {
                    return port4568;
                }*/

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
                var currentService = scServices.FirstOrDefault(x => x.ServiceName == service.ToString());
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
