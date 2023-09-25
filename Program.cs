using System.ServiceProcess;

namespace BSSMessagingConsoleT24SVC
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            //ServiceBase[] ServicesToRun;
            //ServicesToRun = new ServiceBase[]
            //{
            //        new Service1(),

            //};
            //ServiceBase.Run(ServicesToRun);

            //#region Requirement Test
            Service1 service = new Service1();
            service.Start();
            //#endregion

            //#if DEBUG
            //            Service1 myService = new Service1();
            //            myService.OnDebug();
            //            System.Threading.Thread.Sleep(System.Threading.Timeout.Infinite);
            //#else
            //                                                System.ServiceProcess.ServiceBase[] ServicesToRun;
            //                                                ServicesToRun = new System.ServiceProcess.ServiceBase[] 
            //                                                { 
            //                                                    new Service1(),

            //                                                };
            //                                                System.ServiceProcess.ServiceBase.Run(ServicesToRun);
            //#endif



        }
    }
}
