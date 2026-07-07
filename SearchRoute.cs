using CastingSchedule.Controllers;
using CastingSchedule.Models;
using System.Linq.Expressions;
using System.Text.Json;

namespace CastingSchedule.Service
{
    public class SearchRoute
    {
        private readonly ILogger<CastingSheduleController> _logger;

        public SearchRoute(ILogger<CastingSheduleController> logger)
        {
            _logger = logger;
        }

        public List<CastingSheduleModelTest> Search(List<CastingSheduleModelTest> data, List<CastingSheduleModelTest> mnlzData,
            List<CastingSheduleModelTest> ActualData
            )
        {
            try
            {
                Dictionary<string, string> map = new Dictionary<string, string>
                {
                    { "VD11", "УВС-1" },
                    { "VD12", "УВС-2" },
                    { "CCM1", "МНЛЗ-1" },
                    { "CCM2", "МНЛЗ-2" },
                    { "EAF1", "ГМП-1" },
                    { "EAF2", "ГМП-2" },
                    { "LF11", "УКП-1А" },
                    { "LF12", "УКП-1Б" },
                    { "LF21", "УКП-2А" },
                    { "LF22", "УКП-2Б" }
                };

                Dictionary<string, int> mapTime = new Dictionary<string, int>
                {
                    { "VD", 40 },
                    { "LF", 60 },
                    { "EAF", 60 }
                };

                var result = new List<CastingSheduleModelTest>();

                var ActData = ActualData != null
                    ? ActualData.OrderBy(x => x.UNIT_NAME + x.UNIT_NUM + x.UNIT_POS).ToList()
                    : new List<CastingSheduleModelTest>();

                var NewActData = new List<CastingSheduleModelTest>();

                for (int i = 1; i < ActData.Count; i++)
                {
                    var item1 = ActData[i - 1];
                    var item2 = ActData[i];
                    var FullName1 = item1.UNIT_NAME + item1.UNIT_NUM + item1.UNIT_POS;
                    var FullName2 = item2.UNIT_NAME + item2.UNIT_NUM + item2.UNIT_POS;

                    if (item1.DATE_START.AddMinutes(mapTime[item1.UNIT_NAME]) > item2.DATE_START && FullName1 == FullName2)
                    {
                        item1.DATE_END = item2.DATE_START;
                    }
                    else
                    {
                        item1.DATE_END = item1.DATE_START.AddMinutes(mapTime[item1.UNIT_NAME]);
                    }
                    NewActData.Add(item1);
                }

                Dictionary<string, mindata> mnlz1 = new Dictionary<string, mindata>();
                Dictionary<string, mindata> mnlz2 = new Dictionary<string, mindata>();
                Dictionary<string, object> stats = new Dictionary<string, object>();

                foreach (var item in mnlzData)
                {
                    if (item.HEAT_NO == null || item.HEAT_NO == "") continue;

                    if (item.UNIT_NAME == "CCM" && item.UNIT_NUM == "1")
                    {
                        mnlz1[item.HEAT_NO] = new mindata
                        {
                            SeriesSerialNumber = item.SeriesSerialNumber,
                            DaySerialNumber = item.DaySerialNumber,
                            STEEL_GRADE = item.STEEL_GRADE,
                        };
                    }
                    else if (item.UNIT_NAME == "CCM" && item.UNIT_NUM == "2")
                    {
                        mnlz2[item.HEAT_NO] = new mindata
                        {
                            SeriesSerialNumber = item.SeriesSerialNumber,
                            DaySerialNumber = item.DaySerialNumber,
                            STEEL_GRADE = item.STEEL_GRADE
                        };
                    }
                }

                var Data = data.Concat(NewActData).OrderBy(x => x.DATE_START).ToList();
                var newData = Data.Concat(mnlzData).OrderBy(x => x.DATE_START).ToList();

                foreach (var item in newData)
                {
                    if (!map.ContainsKey(item.UNIT_NAME + item.UNIT_NUM + item.UNIT_POS)) continue;
                    item.FullUnitName = map[item.UNIT_NAME + item.UNIT_NUM + item.UNIT_POS];
                    int route = 0;
                    var _tData = new mindata();

                    if (item.HEAT_NO != null && item.HEAT_NO != "")
                    {
                        if (mnlz1.ContainsKey(item.HEAT_NO))
                        {
                            _tData = mnlz1[item.HEAT_NO];
                            route = 1;
                            item.SeriesSerialNumber = _tData.SeriesSerialNumber;
                            item.STEEL_GRADE = _tData.STEEL_GRADE;
                            item.DaySerialNumber = _tData.DaySerialNumber;
                        }
                        else if (mnlz2.ContainsKey(item.HEAT_NO))
                        {
                            _tData = mnlz2[item.HEAT_NO];
                            route = 2;
                            item.SeriesSerialNumber = _tData.SeriesSerialNumber;
                            item.STEEL_GRADE = _tData.STEEL_GRADE;
                            item.DaySerialNumber = _tData.DaySerialNumber;
                        }
                    }
                    else
                    {
                        if (item.UNIT_NAME + item.UNIT_NUM == "CCM1") route = 1;
                        else if (item.UNIT_NAME + item.UNIT_NUM == "CCM2") route = 2;
                    }



                    if (item.STATUS == "A")
                    {
                        switch (item.UNIT_NAME)
                        {
                            case "VD":
                                item.DATE_END = item.DATE_START.AddMinutes(40);
                                break;

                            case "LF":
                                item.DATE_END = item.DATE_START.AddMinutes(60);
                                break;

                            case "EAF":
                                item.DATE_END = item.DATE_START.AddMinutes(60);
                                break;
                        }

                        if (item.DATE_END < DateTime.Now)
                        {
                            item.DATE_END = DateTime.Now;
                        }
                        var mnlzItem = mnlzData.Where(x => item.HEAT_NO == x.HEAT_NO).FirstOrDefault();
                        if (mnlzItem != null && item.DATE_END > mnlzItem.DATE_START && item.UNIT_NAME != "CCM")
                        {
                            item.DATE_END = mnlzItem.DATE_START;
                        }
                        if ((item.DATE_END - item.DATE_START).TotalMinutes > 120 && item.UNIT_NAME == "VD")
                        {
                            item.DATE_END = item.DATE_START;
                        }

                    }
                    item.route = route;
                    result.Add(item);


                    
                }
                
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return new List<CastingSheduleModelTest>();
            }
        }
    }
}
