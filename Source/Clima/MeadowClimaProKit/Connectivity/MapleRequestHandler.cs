using CommonContracts.Models;
using Meadow.Foundation;
using Meadow.Foundation.Web.Maple.Server;
using Meadow.Foundation.Web.Maple.Server.Routing;
using MeadowClimaProKit.Controller;
using MeadowClimaProKit.Database;
using System;
using System.Linq;
using System.Collections.Generic;

namespace MeadowClimaProKit.Connectivity
{
    public class MapleRequestHandler : RequestHandlerBase
    {
        public MapleRequestHandler() { }

        [HttpGet]
        public void GetClimateLogs()
        {
            Console.WriteLine("Maple: handling request.");

            LedController.Instance.SetColor(Color.Magenta);

            Console.WriteLine("Maple: reading database.");
            var logs = DatabaseManager.Instance.GetAllClimateReadings().Select(o => o.Value);

            Console.WriteLine("Maple: database read.");
            var data = new List<ClimateModel>();
            foreach (var log in logs)
            {
                data.Add(new ClimateModel()
                {
                    Date = log.DateTime.ToString(),
                    Temperature = log.Temperature.ToString(),
                    Pressure = log.Pressure.ToString(),
                    Humidity = log.Humidity.ToString(),
                    WindDirection = log.WindDirection.ToString(),
                    WindSpeed = log.WindSpeed.ToString()
                });
            }

            Console.WriteLine("Maple: sending response.");
            Console.WriteLine($"Maple: response data: {Newtonsoft.Json.JsonConvert.SerializeObject(data)}");

            LedController.Instance.SetColor(Color.Green);

            Context.Response.ContentType = ContentTypes.Application_Json;
            Context.Response.StatusCode = 200;
            Send(data);
        }
    }
}