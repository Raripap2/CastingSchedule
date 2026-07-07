using CastingSchedule.Domain;
using CastingSchedule.Domain.Models;
using CastingSchedule.Models;
using Microsoft.AspNetCore.Mvc.Formatters;
using MyDataBase;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CastingSchedule.Service
{
    public class ServiceGetNow
    {
        private readonly DataBase _database;
        private readonly SearchRoute _searchRoute;
        private readonly CastingInfoService _castingInfoService;
        private static readonly Random _random = new Random();

        private static readonly HashSet<string> _heatStatuses = new() { "Fact", "Plan", "Actual" };

        public ServiceGetNow(DataBase database, SearchRoute searchRoute, CastingInfoService castingInfoService)
        {
            _database = database;
            _searchRoute = searchRoute;
            _castingInfoService = castingInfoService;
        }

        public List<CastingSheduleModelTest> GetSQLRange(DateTime start, DateTime end)
        {

            Console.WriteLine($"SELECT * FROM pkg_planning_db_api.fget_host_schedule({start}, {end})");
            string sql = $"SELECT * FROM pkg_planning_db_api.fget_host_schedule({start}, {end})";
            var parameters = new Dictionary<string, object>
            {
                { "p_start", start},
                { "p_tend", end}
            };
            var result = _database.QueryList<CastingSheduleModelTest>(sql);

            return result;

        }
        public List<List<CastingSheduleModelTest>> GetSQL()
        {
            string sql = "SELECT * FROM V_SCH_FACT_OVERVIEW WHERE UNIT_NAME <> 'CCM'";
            var allData = _database.QueryList<CastingSheduleModelTest>(sql);

            var factResult = allData.Where(x => x.STATUS != "A").ToList();
            var planResult = _database.QueryList<CastingSheduleModelTest>(
                "SELECT * FROM V_SCH_PLAN_OVERVIEW WHERE UNIT_NAME <> 'CCM'");

            var allResult = new List<CastingSheduleModelTest>(factResult.Count + planResult.Count);
            allResult.AddRange(factResult);
            allResult.AddRange(planResult);

            allResult.Sort((x, y) => string.Compare(x.UNIT_NAME, y.UNIT_NAME, StringComparison.Ordinal));

            var actualResult = allData.Where(x => x.STATUS != "F").ToList();

            return new List<List<CastingSheduleModelTest>> { allResult, actualResult };
        }

        public List<CastingSheduleModelTest> Maping(List<Domain.Models.ScheduleItem> data)
        {
            var result = new List<CastingSheduleModelTest>(data.Count);
            var select = new HashSet<string>();

            foreach (var item in data)
            {
                var obj = new CastingSheduleModelTest();

                switch (item)
                {
                    case Heat heat:
                        MapHeat(heat, obj);
                        break;
                    case UnitOperation up:
                        MapUnitOperation(up, obj);
                        break;
                }

                if (!select.Contains(item.GetType().ToString()))
                {
                    select.Add(item.GetType().ToString());
                }

                obj.SEQ = _random.Next(1000, 5000);
                obj.DATE_START = item.StartTime;
                obj.DATE_END = item.EndTime;
                result.Add(obj);
            }

            return result;
        }
        private static void MapHeat(Heat heat, CastingSheduleModelTest obj)
        {
            obj.HEAT_NO = heat.HeatNum;
            obj.STEEL_GRADE = heat.SteelGradeDesc;
            obj.SeriesSerialNumber = heat.SeriesSerialNumber;
            obj.DaySerialNumber = heat.DaySerialNumber;
            obj.STATUS = heat.HeatStatus.ToString() switch
            {
                "Fact" => "F",
                "Plan" => "P",
                "Actual" => "A",
                _ => obj.STATUS
            };

            if (heat.UnitCode == "MNLZ1" || heat.UnitCode == "MNLZ2")
            {
                obj.UNIT_NAME = "CCM";
                obj.UNIT_NUM = heat.UnitCode == "MNLZ1" ? "1" : "2";
            }

            obj.TASK_NUM = heat.HeatTask;
        }

        private static void MapUnitOperation(UnitOperation up, CastingSheduleModelTest obj)
        {
            obj.OPERATION_TYPE = up.TypeOperationStr;
            if (up.UnitCode == "MNLZ1" || up.UnitCode == "MNLZ2")
            {
                obj.UNIT_NAME = "CCM";
                obj.UNIT_NUM = up.UnitCode == "MNLZ1" ? "1" : "2";
            }
        }

        public async Task<List<CastingSheduleModelTest>> PutDownTime(List<CastingSheduleModelTest> data)
        {
            if (data.Count < 2) return data;

            var OrderData = data
                .OrderBy(x => x.UNIT_NAME)
                .ThenBy(x => x.UNIT_NUM)
                .ThenBy(x => x.UNIT_POS ?? "")
                .ThenBy(x => x.DATE_START)
                .ToList();

            var result = new List<CastingSheduleModelTest>(data.Count);

            var keys = new string[OrderData.Count];
            for (int i = 0; i < OrderData.Count; i++)
            {
                var x = OrderData[i];
                keys[i] = $"{x.UNIT_NAME}{x.UNIT_NUM}{x.UNIT_POS}";
            }

            for (int i = 0; i < OrderData.Count - 1; i++)
            {
                if (keys[i] == keys[i + 1]) 
                {
                    var item = OrderData[i];
                    var item2 = OrderData[i + 1];
                    var minutesDiff = (item2.DATE_START - item.DATE_END).TotalMinutes;

                    if (minutesDiff >= 4)
                    {
                        result.Add(new CastingSheduleModelTest
                        {
                            DATE_START = item.DATE_END,
                            STATUS = "F",
                            DATE_END = item2.DATE_START,
                            UNIT_NAME = item.UNIT_NAME,
                            UNIT_NUM = item.UNIT_NUM,
                            UNIT_POS = item.UNIT_POS,
                            FullUnitName = keys[i],
                            OPERATION_TYPE = ""
                        });
                    }
                }
            }

            if (result.Count > 0)
            {
                OrderData.AddRange(result);
                return OrderData;
            }

            return OrderData;
        }

        public async Task<List<CastingSheduleModelTest>> GetNowData(DateTime _date)
        {
            var stopwatch = Stopwatch.StartNew();

            // Работа с dll
            var mnlzData = await _castingInfoService.GetTotalSchedule(0.4, 1.26, _date, _date.AddHours(-84));
            stopwatch.Stop();
            Console.WriteLine($"DLL {stopwatch.Elapsed.ToString()}");
            var resMNLZFromDll = Maping(mnlzData.ToList());
            var resMNLZ = await PutDownTime(resMNLZFromDll);

            // Работа с бд
            var sqlData = GetSQL();
            var HeatData = sqlData[0];
            //var resHeatData = await PutDownTime(HeatData);

            var ActualData = sqlData[1];
            var EditData = _searchRoute.Search(HeatData, resMNLZ, ActualData);
            return EditData;
        }

        public async Task<List<CastingSheduleModelTest>> GetDateRange(DateTime _date)
        {
            var mnlzData = await _castingInfoService.GetTotalSchedule(0.4, 1.26, _date, _date.AddHours(-84));
            var resMNLZFromDll = Maping(mnlzData.ToList());
            var resMNLZ = await PutDownTime(resMNLZFromDll);

            var sqlDate = GetSQLRange(_date, _date.AddHours(-84));

            var EditData = _searchRoute.Search(sqlDate, resMNLZ, null);

            return EditData;

        }
    }
}