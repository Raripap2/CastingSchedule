using CastingShedule.Models;
using MyDataBase;
using System.Data;

namespace CastingShedule.Service
{
    public class TaskService
    {
        private readonly DataBase _dataBase;
        public TaskService(DataBase dataBase) 
        { 
            _dataBase = dataBase;
        }

        public async Task<List<TaskModel>> GetSQL(int page)
        {
            int offset = page * 15;
            string sql = @$"
SELECT * FROM v_heat_task_bindings
OFFSET {offset} ROWS
FETCH NEXT 15 ROWS ONLY
";
            var result = _dataBase.QueryList<TaskModel>(sql);
            return result;
        }

        public async Task<List<TaskModel>> GetTask(int page)
        {
            var result = await GetSQL(page);
            return result;
        }
    }
}
