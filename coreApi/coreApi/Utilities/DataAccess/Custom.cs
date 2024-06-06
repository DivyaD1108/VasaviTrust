using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace coreApi.DataAccess
{
    public class Custom
    {
        public static SqlParameter AddParam(SqlDbType ParamType, string ParamName, string ParamValue, int length, bool paramType = false)
        {
            SqlParameter prm = new SqlParameter("@" + ParamName, ParamType, length);
            if (paramType)
            {
                prm.Direction = ParameterDirection.Output;
                prm.Value = DBNull.Value;
            }
            else
            {
                prm.Direction = ParameterDirection.Input;
                prm.Value = ParamValue;
            }
            return prm;
        }
    }
}
