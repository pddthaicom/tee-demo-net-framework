using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using MySql.Data;
using MySql.Data.MySqlClient;

namespace SQLHelp
{
    public class SQLHelper
    {
        private string conString;

        private string ConString
        {
            get
            {
                return conString;
            }
            set
            {
                conString = value;
            }
        }

        /// <summary>
        /// get Connection String
        /// </summary>
        /// <param name="connString"></param>
        public SQLHelper(string conString)
        {
            this.conString = conString;
        }
       
        /// <summary>
        /// ExecuteNonQuery
        /// </summary>
        /// <param name="cmd">sqlcommand</param>
        /// <returns></returns>
        public void ExecuteNonQuery(SqlCommand cmd)
        {
            SqlConnection con = null;
            try
            {
                con = new SqlConnection(this.ConString);
                con.Open();
                cmd.Connection = con;
                cmd.ExecuteNonQuery();
            }
            catch (SqlException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (cmd != null)
                {
                    cmd.Dispose();
                    cmd = null;
                }
                if (con != null)
                {
                    con.Close();
                    con.Dispose();
                    con = null;
                    GC.SuppressFinalize(this);
                }
            }
        }

        /// <summary>
        /// ExecuteNonQuery
        /// </summary>
        /// <param name="cmd">sqlcommand</param>
        /// <returns></returns>
        public void ExecuteNonQuery(MySqlCommand cmd)
        {
            MySqlConnection con = null;
            try
            {
                con = new MySqlConnection(this.ConString);
                con.Open();
                cmd.Connection = con;
                cmd.ExecuteNonQuery();
            }
            catch (MySqlException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (cmd != null)
                {
                    cmd.Dispose();
                    cmd = null;
                }
                if (con != null)
                {
                    con.Close();
                    con.Dispose();
                    con = null;
                    GC.SuppressFinalize(this);
                }
            }
        }

        /// <summary>
        /// Execute datatable
        /// </summary>
        /// <param name="dt">DataTable</param>
        /// <param name="cmd">object command</param>
        /// <returns></returns>
        public void ExecuteDataTable(ref DataTable dt, SqlCommand cmd)
        {
            SqlConnection con = null;
            SqlDataAdapter adap = null;

            try
            {
                con = new SqlConnection(this.conString);
                con.Open();
                cmd.Connection = con;
                adap = new SqlDataAdapter(cmd);
                adap.Fill(dt);
            }
            catch (SqlException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (adap != null)
                {
                    adap.Dispose();
                    adap = null;
                }
                if (cmd != null)
                {
                    cmd.Dispose();
                    cmd = null;
                }
                if (con != null)
                {
                    con.Close();
                    con.Dispose();
                    con = null;
                    GC.SuppressFinalize(this);
                }
            }
        }

        /// <summary>
        /// Execute Datatable
        /// </summary>
        /// <param name="cmd"></param>
        public DataTable ExecuteDataTable(SqlCommand cmd)
        {
            SqlConnection con = null;
            SqlDataAdapter adap = null;
            DataTable dt = new DataTable();

            try
            {
                con = new SqlConnection(this.conString);
                con.Open();
                cmd.Connection = con;
                adap = new SqlDataAdapter(cmd);
                adap.Fill(dt);
            }
            catch (SqlException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (adap != null)
                {
                    adap.Dispose();
                    adap = null;
                }
                if (cmd != null)
                {
                    cmd.Dispose();
                    cmd = null;
                }
                if (con != null)
                {
                    con.Close();
                    con.Dispose();
                    con = null;
                    GC.SuppressFinalize(this);
                }
            }
            return dt;
        }

        /// <summary>
        /// Execute Datatable
        /// </summary>
        /// <param name="cmd"></param>
        public DataTable ExecuteDataTable(MySqlCommand cmd)
        {
            MySqlConnection con = null;
            MySqlDataAdapter adap = null;
            DataTable dt = new DataTable();

            try
            {
                con = new MySqlConnection(this.conString);
                con.Open();
                cmd.Connection = con;
                adap = new MySqlDataAdapter(cmd);
                adap.Fill(dt);
            }
            catch (MySqlException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (adap != null)
                {
                    adap.Dispose();
                    adap = null;
                }
                if (cmd != null)
                {
                    cmd.Dispose();
                    cmd = null;
                }
                if (con != null)
                {
                    con.Close();
                    con.Dispose();
                    con = null;
                    GC.SuppressFinalize(this);
                }
            }
            return dt;
        }

        /// <summary>
        /// Execute DataSet
        /// </summary>
        /// <param name="ds">Object DataSet</param>
        /// <param name="cmd">Object Command</param>
        /// <returns></returns>
        public void ExecuteDataset(ref DataSet ds, SqlCommand cmd)
        {
            SqlConnection con = null;
            SqlDataAdapter adap = null;

            try
            {
                con = new SqlConnection(this.conString);
                con.Open();
                cmd.Connection = con;
                adap = new SqlDataAdapter(cmd);
                adap.Fill(ds);
            }
            catch (SqlException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (adap != null)
                {
                    adap.Dispose();
                    adap = null;
                }
                if (cmd != null)
                {
                    cmd.Dispose();
                    cmd = null;
                }
                if (con != null)
                {
                    con.Close();
                    con.Dispose();
                    con = null;
                    GC.SuppressFinalize(this);
                }
            }
        }

        /// <summary>
        /// Execute Scalar
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="cmd"></param>
        public Object ExecuteScalar(SqlCommand cmd)
        {
            SqlConnection con = null;
            Object obj = null;
            try
            {
                con = new SqlConnection(this.ConString);
                con.Open();
                cmd.Connection = con;
                obj = cmd.ExecuteScalar();
            }
            catch (SqlException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (cmd != null)
                {
                    cmd.Dispose();
                    cmd = null;
                }
                if (con != null)
                {
                    con.Close();
                    con.Dispose();
                    con = null;
                    GC.SuppressFinalize(this);
                }
            }
            return obj;
        }

        /// <summary>
        /// Execute Scalar
        /// </summary>
        /// <param name="obj">Object Object</param>
        /// <param name="cmd">Object Command</param>
        public void ExecuteScalar(ref Object obj,SqlCommand cmd)
        {
            SqlConnection con = null;
            try
            {
                con = new SqlConnection(this.ConString);
                con.Open();
                cmd.Connection = con;
                obj = cmd.ExecuteScalar();
            }
            catch (SqlException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (cmd != null)
                {
                    cmd.Dispose();
                    cmd = null;
                }
                if (con != null)
                {
                    con.Close();
                    con.Dispose();
                    con = null;
                    GC.SuppressFinalize(this);
                }
            }
        }

        public SqlDataReader ExecuteDataReader()
        {
            SqlConnection con = null;
            SqlCommand com = null;

            con = new SqlConnection(this.conString);
            con.Open();
            com = new SqlCommand("", con);

            return com.ExecuteReader(CommandBehavior.CloseConnection);
        }
    }
}

