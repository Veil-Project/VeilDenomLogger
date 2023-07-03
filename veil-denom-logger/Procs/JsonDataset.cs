using System;
using System.Data;
using System.Data.SqlClient;
using VeilBlockToDB.ModelsJson;

namespace VeilBlockToDB.Procs
{
    public class JsonDataset
    {
        public static bool InsertDenomEfficiency(long blockID)
        {
            var _dbVeilContext = new VeilContext();
            try
            {
                _dbVeilContext.Database.Connection.Open();
                for (var iDateRange = 3; iDateRange >= 1; iDateRange--)
                {
                    var cmd = _dbVeilContext.Database.Connection.CreateCommand();
                    cmd.CommandText = "[dbo].[spI_DenomEfficiency]";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@BlockID", blockID));
                    cmd.Parameters.Add(new SqlParameter("@BlockRange", iDateRange));
                    cmd.CommandTimeout = 600;

                    // execute the command
                    var iOutput = cmd.ExecuteScalar(); 
                }
                return true;
            }
            catch (Exception ex)
            {
                var o = ex.Message;
            }
            finally
            {
                _dbVeilContext.Database.Connection.Close();
            }
            return false;
        }

        public static MultiSeriesLineChart GetDenomEfficiencyDB(int dateRange, int movingAverage)
        {
            var _dbVeilContext = new VeilContext();
            try
            {
                _dbVeilContext.Database.Connection.Open();

                var cmd = _dbVeilContext.Database.Connection.CreateCommand();
                cmd.CommandText = "sp_GetDenomEfficiencyData";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("@DateRange", dateRange));
                cmd.Parameters.Add(new SqlParameter("@MovingAverage", movingAverage));
                cmd.CommandTimeout = 600;

                // execute the command
                var rdr = cmd.ExecuteReader();

                // iterate through results, printing each to console      
                var oLineGraph = new MultiSeriesLineChart();
                while (rdr.Read())
                {
                    var dtBlockTime = ((DateTime)rdr["BlockDate"]).ToString("dd MMM yyyy HH:mm:ss UTC");
                    oLineGraph.LastBlockTime = dtBlockTime;
                    oLineGraph.Series1.Add(new LineGraphDataPointDecimal((long)rdr["BlockID"], (decimal)rdr["Efficiency10"], dtBlockTime));
                    oLineGraph.Series2.Add(new LineGraphDataPointDecimal((long)rdr["BlockID"], (decimal)rdr["Efficiency100"], dtBlockTime));
                    oLineGraph.Series3.Add(new LineGraphDataPointDecimal((long)rdr["BlockID"], (decimal)rdr["Efficiency1000"], dtBlockTime));
                    oLineGraph.Series4.Add(new LineGraphDataPointDecimal((long)rdr["BlockID"], (decimal)rdr["Efficiency10000"], dtBlockTime));
                }
             
                return oLineGraph;
            }
            catch (Exception ex)
            {
                var o = ex.Message;
                return new MultiSeriesLineChart();
            }
            finally
            {
                _dbVeilContext.Database.Connection.Close();
            }
        }
    }
}
