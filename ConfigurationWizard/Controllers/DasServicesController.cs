using Microsoft.AspNetCore.Mvc;
using System.ServiceProcess;
using System.Linq;
using System.Net.Sockets;
using System;
using System.Diagnostics;

namespace ConfigurationWizard.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DasServicesController : Controller
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

        private string CheckListeningDasPort(string allProcess, int port)
        {
            var splitRow = allProcess.Split("\r\n");
            var rowWithIndex = splitRow.Select((item, index) =>
            new
            {
                ItemName = item,
                Position = index
            }).Where(x => x.ItemName.Contains(":" + port.ToString()) && x.ItemName.Contains("LISTENING"));

            if (rowWithIndex.Any(x => splitRow[x.Position - 1].Contains("DAService.exe")))
            {
                return "";
            }
            else
            {
                var result = "порт" + port.ToString() + " занят другим процессом:";
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

        private string GetProcessCmd()
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
                return "Ошибка при проверке тестового конракта Das" + ex.Message;
            }
        }

        [HttpGet]
        [Route("/checkDasSerice")]
        public string CheckDasSerice(string hostName = "localhost")
        {
            var scServices = ServiceController.GetServices();
            var dasService = scServices.FirstOrDefault(x => x.ServiceName == "DAService");
            if (dasService != null)
            {
                if (dasService.Status != ServiceControllerStatus.Running)
                {
                    return "Служба DasService не активна!";
                }

                if (!CheckTcpIpClient(hostName, 7070))
                {
                    return "Соединение с портом 7070 не установлено!";
                }

                if (!CheckTcpIpClient(hostName, 4568))
                {
                    return "Соединение с портом 4568 не установлено!";
                }

                var allProcess = GetProcessCmd();

                var port7070 = CheckListeningDasPort(allProcess, 7070);
                var port4568 = CheckListeningDasPort(allProcess, 4568);
                if (port7070.Length > 0)
                {
                    return port7070;
                }


                if (port4568.Length > 0)
                {
                    return port4568;
                }

                return "";

            }
            else
            {
                return "Служба DasService не найдена!";
            }

        }

        [HttpGet]
        [Route("/restartDasService")]
        public string RestartDasService()
        {
            try
            {
                var scServices = ServiceController.GetServices();
                var dasService = scServices.FirstOrDefault(x => x.ServiceName == "DAService");
                if (dasService != null)
                {
                    int timeoutMilliseconds = 10000;
                    int millisec1 = Environment.TickCount;
                    TimeSpan timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds);

                    dasService.Stop();
                    dasService.WaitForStatus(ServiceControllerStatus.Stopped, timeout);

                    int millisec2 = Environment.TickCount;
                    timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds - (millisec2 - millisec1));

                    dasService.Start();
                    dasService.WaitForStatus(ServiceControllerStatus.Running, timeout);
                    return "";

                }
                return "Не удалось перезапустить службу DAService так как она не найдена!";
            }

            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}
