
using CastingSchedule.Models;
using CastingSchedule.Service;
using Microsoft.AspNetCore.Mvc;
using MyDataBase;
using OfficeOpenXml.Data.Connection;
using Newtonsoft.Json;
using System.Runtime.Intrinsics.Arm;
using System.Security.AccessControl;
using System.Security.Cryptography.Xml;
using System.Numerics;
using Microsoft.AspNetCore.Localization.Routing;
using CastingSchedule.Domain;
using System.Threading.Tasks;
using System.Diagnostics;
using CastingShedule.Service;
using System.Data;

namespace CastingSchedule.Controllers
{
    [ApiController]
    [Route("/API/[Controller]")]
    public class CastingSheduleController : ControllerBase
    {
        private readonly ServiceGetNow _serviceGetNow;
        private readonly CastingInfoService _castingInfoService;
        private readonly ICacheService _cacheService;
        private readonly ILogger<CastingSheduleController> _logger;
        private readonly TaskService _taskService;

        public CastingSheduleController(ServiceGetNow serviceGetNow, CastingInfoService castingInfoService,
            ICacheService cacheService, ILogger<CastingSheduleController> logger,
            TaskService taskService)
        {
            _serviceGetNow = serviceGetNow;
            _castingInfoService = castingInfoService;
            _cacheService = cacheService;
            _logger = logger;
            _taskService = taskService;
        }


        [HttpGet("/DLL")]
        public async Task<ActionResult> DLL()
        {
                DateTime now = DateTime.Now;
                var result = await _castingInfoService.GetUnitsAsync(0.4, 1.26, now, now.AddDays(-3), 20);
                
                if (result != null)
                {
                    var json = JsonConvert.SerializeObject(result);
                    return Ok(json);
                }
                return Ok();
            
        }

        [HttpGet("/nowDateNew")]
        public async Task<ActionResult> NowDateNew()
        {
            //try
            //{
                var stopwatch = Stopwatch.StartNew();

                var now = DateTime.Now;

                var cacheKey = $"NowData_{now:yyyyMMddHHmm}";

                var result = await _cacheService.GetOrSetAsync(cacheKey, async () =>
                {
                    var sw = Stopwatch.StartNew();
                    var data = await _serviceGetNow.GetNowData(now);
                    sw.Stop();
                    _logger.LogInformation("Data loaded from DB, took {Elapsed}ms", sw.ElapsedMilliseconds);
                    return data;
                }, seconds: 2);

                stopwatch.Stop();
                Console.WriteLine(stopwatch.Elapsed.ToString());

                return Ok(new
                {
                    data = result,
                    centerDate = now.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                });
            //}
            //catch (Exception ex)
            //{
            //    return BadRequest(new { message = ex.Message });
            //}
        }

        [HttpGet("/dateRange")]
        public async Task<ActionResult> DateRande(string date)
        {
            try
            {
                DateTime _date;
                try
                {
                    _date = DateTime.Parse(date);
                    _logger.LogInformation($"Data {_date}");
                    var result = await _serviceGetNow.GetDateRange(_date);

                    return Ok(new
                    {
                        data = result
                    });
                }
                catch (Exception ex)
                {
                    return BadRequest(new { message = $"Неверный формат даты: \n Нужный формат YYYY-MM-DDT00:00:00", EX = ex.Message });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new {message = ex.Message});
            }
        }

        [HttpPut("/DropHeat")]
        public async Task<ActionResult> DropHeat(dropHeatModel data)
        {
            try
            {
                // Метод UPDATE в бд

                DateTime now = DateTime.Now;

                var cacheKey = $"NowData_{now:yyyyMMddHHmm}";
                var result = await _cacheService.GetOrSetAsync(cacheKey, async () =>
                {
                    var sw = Stopwatch.StartNew();
                    var data = await _serviceGetNow.GetNowData(now);
                    sw.Stop();
                    _logger.LogInformation("Data loaded from DB, took {Elapsed}ms", sw.ElapsedMilliseconds);
                    return data;
                }, seconds: 2);

                return Ok(new
                {
                    data = result,
                    centerDate = now.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new {message = ex.Message});
            }
        }

        [HttpGet("/GetTask")]
        public async Task<ActionResult> GetTask([FromQuery] int page)
        {
            try
            {
                var result = await _taskService.GetTask(page);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
