using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using FirebirdSql.Data.FirebirdClient;
using System.IO;
using System.Xml;

namespace sfdb
{
    public class SFIA
    {
        public string FBConnectionStr { get; set; }
        public string MSConnectionStr {get; set;}
        public string LogPath { get; set; }
        public bool adate { get; set; }
        public string error = "";

        public SFIA()
        {
            FBConnectionStr = "User ID=sysdba;Password=masterkey;" +
               "Database=server/3052:e:\\iadb\\IAPTEKA.fdb; " +
               "DataSource=server;Charset=NONE;";
            LogPath = @".\app.log";
            adate = true;
            MSConnectionStr = "data source=DBSRV01\\SQL01;persist security info=True;" +
                "user id=infoterminal;password=9FPqY5ofy2Y8;"+
                "MultipleActiveResultSets=True;Connection Timeout=60";
        }
        /// <summary>
        /// Запрос к БД Firebird
        /// </summary>
        /// <param name="sql">Текст запроса</param>
        /// <returns>FbDataReader</returns>
     /*   public FbDataReader RunFBQuery(string sql)
        {
            string ConnectionString = FBConnectionStr;
            FbConnection addDetailsConnection = new FbConnection(ConnectionString);
            try
            {
                addDetailsConnection.Open();
            }catch
            {
                WriteLog("Ошибка подключения к БД");   
            }
            FbCommand readCommand =
          new FbCommand(sql, addDetailsConnection);
            try
            {
                FbDataReader myreader = readCommand.ExecuteReader();
                myreader.Close();
                addDetailsConnection.Close();
                return myreader;
            }
            catch
            {
                WriteLog("Ошибка выполнения запроса к БД");
            }
        }*/
        public void ConnectMS(string sql)
        {
            SqlConnection myConnection = new SqlConnection(MSConnectionStr);
            try
            {
                myConnection.Open();
            }
            catch (Exception e)
            {
                WriteLog(e.ToString());
                Console.WriteLine(e.ToString());
            }
            try
            {
                SqlCommand myCommand = new SqlCommand(sql, myConnection);
                myCommand.ExecuteNonQuery();
                myConnection.Close();
            }
            catch (Exception e)
            {
                WriteLog(e.ToString());
                Console.WriteLine(e.ToString());
                myConnection.Close();
            }
        }
        /// <summary>
        /// Запись лога в текстовый файл в папку с исполняемым файлов
        /// </summary>
        /// <param name="value">Строка для записи в лог</param>
        public void WriteLog(string value)
        {
            error += error + value + "\r\n";
            /*  if (!File.Exists(LogPath)){using (var fs = File.Create(LogPath)){}}
             using (StreamWriter log = new StreamWriter(LogPath, true, Encoding.Default))
              {
                  if (adate){log.WriteLine(DateTime.Now.ToString() + " | " + value);}
                  else{log.WriteLine(value);}
              }*/

        }
    }

    public class IShop : SFIA
    {
        public string[] Files { get; set; }
        public List<string[]> Items = new List<string[]>();
        public bool ParseXmlHeader(string path, string type)
        {
            return true;
        }

        public bool ParseXmlBody(string path, string type)
        {
            if(type=="ishop")
            {
                XmlDocument doc = CheckXml(path);
                XmlNodeList list = doc.GetElementsByTagName("Позиция");
                int count = list.Count;
                if (count == 0)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Битый файл - " + path);
                    Console.ForegroundColor = ConsoleColor.White;
                    return false;
                }
                else
                {
                    foreach (XmlNode position in list)
                    {
                        string[] arr = new string[11];
                        arr[0] = position.Attributes[0].Value;
                        arr[1] = position.ChildNodes[0].Attributes[0].Value;
                        arr[2] = position.ChildNodes[1].Attributes[0].Value;
                        arr[3] = position.ChildNodes[2].Attributes[0].Value;
                        arr[4] = position.ChildNodes[3].Attributes[0].Value;
                        arr[5] = position.ChildNodes[4].Attributes[0].Value;
                        arr[6] = position.ChildNodes[5].Attributes[0].Value;
                        arr[7] = position.ChildNodes[6].Attributes[0].Value;
                        arr[8] = position.ChildNodes[7].Attributes[0].Value;
                        arr[9] = position.ChildNodes[8].Attributes[0].Value;
                        arr[10] = position.ChildNodes[9].Attributes[0].Value;
                        //Console.WriteLine(path +" --- " +Items.Count);
                        Items.Add(arr);
                    }
                    return true;
                }
            }
            return false;
        }

        public int SelectSQLMS(string sql)
        {
            SqlConnection myConnection = new SqlConnection(MSConnectionStr);
            try
            {
                myConnection.Open();
            }
            catch (Exception e)
            {
                WriteLog(e.ToString());
                Console.WriteLine(e.ToString());
            }
            try
            {
                SqlCommand myCommand = new SqlCommand(sql, myConnection);
                SqlDataReader reader =  myCommand.ExecuteReader();

                if (reader.HasRows)
                {
                    myConnection.Close();
                    return 1;
                    
                }
                else
                {
                    myConnection.Close();
                    return 0;
                    
                }
            }
            catch (Exception e)
            {
                WriteLog(e.ToString());
                Console.WriteLine(e.ToString());
                myConnection.Close();
            }
            return 0;
        }

        private XmlDocument CheckXml(string path)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(path);
                return doc;
            }
            catch
            {
                WriteLog("Файл - " + path.Split('\\').Last() + " не является XML");
                return new XmlDocument();
            }
            
        }

    }
}
