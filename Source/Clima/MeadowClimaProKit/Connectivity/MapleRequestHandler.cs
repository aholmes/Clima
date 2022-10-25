//using CommonContracts.Models;
//using Meadow.Foundation;
//using Meadow.Foundation.Web.Maple;
//using Meadow.Foundation.Web.Maple.Routing;
//using MeadowClimaProKit.Controller;
//using MeadowClimaProKit.Database;
//using System;
//using System.Linq;
//using System.Collections.Generic;
//using System.Text.Json;

//namespace MeadowClimaProKit.Connectivity
//{
//    public class MapleRequestHandler : RequestHandlerBase
//    {
//        public MapleRequestHandler() { }

//        [HttpGet("/getclimalogs")]
//        public void GetClimateLogs()
//        {
//            try
//            {
//                Console.WriteLine("Maple: handling request.");

//                LedController.Instance.SetColor(Color.Magenta);

//                Console.WriteLine("Maple: reading database.");
//                var logs = DatabaseManager.Instance.GetAllClimateReadings().Select(o => o.Value);

//                Console.WriteLine("Maple: database read.");
//                var data = new List<ClimateModel>();
//                foreach (var log in logs)
//                {
//                    data.Add(new ClimateModel()
//                    {
//                        Date = log.DateTime.ToString(),
//                        Temperature = log.Temperature.ToString(),
//                        Pressure = log.Pressure.ToString(),
//                        Humidity = log.Humidity.ToString(),
//                        WindDirection = log.WindDirection.ToString(),
//                        WindSpeed = log.WindSpeed.ToString()
//                    });
//                }

//                Console.WriteLine("Maple: sending response.");
//                //Console.WriteLine($"Maple: response data: {JsonSerializer.Serialize(data)}");

//                LedController.Instance.SetColor(Color.Green);

//                Context.Response.ContentType = ContentTypes.Application_Json;
//                Context.Response.StatusCode = 200;
//                Send(data);
//            }
//            catch(Exception e)
//            {
//                Console.WriteLine($"Maple error: {e.Message}");
//            }

//        }
//    }
//}
using CommonContracts.Models;
using Meadow.Foundation;
using Meadow.Foundation.Web.Maple;
using Meadow.Foundation.Web.Maple.Routing;
using MeadowClimaProKit.Controller;
using MeadowClimaProKit.Database;
using System.Collections.Generic;

namespace MeadowClimaProKit.Connectivity
{
    public class MapleRequestHandler : RequestHandlerBase
    {
        public MapleRequestHandler() { }

        [HttpGet("/getclimalogs")]
        public IActionResult GetClimateLogs()
        {
            LedController.Instance.SetColor(Color.Magenta);

            var logs = DatabaseManager.Instance.GetAllClimateReadings();

            var data = new List<ClimateModel>();
            foreach (var log in logs)
            {
                data.Add(new ClimateModel()
                {
                    Date = log.Value.DateTime.ToString(),
                    Temperature = log.Value.Temperature.ToString(),
                    Pressure = log.Value.Pressure.ToString(),
                    Humidity = log.Value.Humidity.ToString(),
                    WindDirection = log.Value.WindDirection.ToString(),
                    WindSpeed = log.Value.WindSpeed.ToString()
                });
            }

            LedController.Instance.SetColor(Color.Green);
            return new JsonResult(data);
        }
    }
}