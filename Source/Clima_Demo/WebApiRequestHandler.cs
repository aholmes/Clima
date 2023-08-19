using Meadow.Foundation.Web.Maple;
using Meadow.Foundation.Web.Maple.Routing;
using System;
using System.Collections.Generic;
using System.Text;


namespace Clima_Demo
{
    public class WebApiRequestHandler: RequestHandlerBase
    {
        public override bool IsReusable => true;

        [HttpGet("/climate")]
        public IActionResult Climate()
        {
            return new JsonResult(new
            {
                Clima_Demo.Climate.Bme688,
                Clima_Demo.Climate.SolarVoltage,
                Clima_Demo.Climate.Anemometer,
                Clima_Demo.Climate.RainGuage,
                Clima_Demo.Climate.Windvane,
                Clima_Demo.Climate.Scd40,
            });
        }
    }
}
