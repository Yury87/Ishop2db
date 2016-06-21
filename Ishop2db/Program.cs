using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Data.SqlClient;
using sfdb;
using System.IO;

namespace Ishop2db
{
    class Program
    {
        static string[] files = Directory.GetFiles(@"\\s-ftp\samson-f\");
        static List<string> sqlinsert = new List<string>();
        static int count = files.Length;
        static int i = 0;
        static int threadcount = 0;
        static string[] FilePositions = new string[files.Length];
        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(DateTime.Now + " - программа запущена");
            while (i < count)
            {
                Console.WriteLine("Обрабатывается файл " + files[i]);
                Thread thread = new Thread(new ParameterizedThreadStart(SetIShop));
                thread.Start(files[i]);
              threadcount++;
                i++;
            }
            
          //  Console.ReadKey();
            
        }

        static void SetIShop(object filepath)
        {
            string file = (string)filepath;
            IShop sf = new IShop();
         /*   sf.FBConnectionStr = "User ID=sysdba;Password=masterkey;" +
               "Database=server/3052:e:\\iadb\\IAPTEKA.fdb; " +
               "DataSource=server;Charset=NONE;";*/
            sf.LogPath = @".\app.log";
            if (sf.ParseXmlBody(file, "ishop"))
            {
                Console.WriteLine("Получение даты файла " + file);
                string datetime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                string filedate = File.GetLastWriteTime(file).ToString("yyyy-MM-dd HH:mm:ss");
                Console.WriteLine("ЗАпись в бд дат");
                sf.ConnectMS("UPDATE [pharmacyterminal].[dbo].[ishopinfo] " +
                    "SET [updatedate]=Convert(datetime,'" + datetime + "', 121),"+
                    "[filedate]=Convert(datetime,'" + filedate + "',121),"+
                    "[fileupdate]=1 where [ishop]='" + file.Split('\\').Last() + "'");
                Console.WriteLine("Удаление инфы о файле" + file);
                while (sf.SelectSQLMS("SELECT * from [pharmacyterminal].[dbo].[productremains] where apt_id='" + file.Split('\\').Last() + "'") == 1)
                {
                    sf.ConnectMS("DELETE from [pharmacyterminal].[dbo].[productremains] where apt_id='" + file.Split('\\').Last() + "'");
                }
                string sql;
                Console.WriteLine("запись в базу" + file);
                foreach (string[] item in sf.Items)
                {
                    sql = "INSERT INTO [pharmacyterminal].[dbo].[productremains] ([position] ,[code] ,[name_product] ,[manufacturer] ,[country] ,[type] ,[options1001] ,[MNN] ,[price] ,[discount] ,[pre_order] ,[apt_id])" +
                              "VALUES";
                    sql += "(" + Convert.ToInt16(item[0]) + "," +
                                         Convert.ToInt32(item[1]) + ",'" +
                                         item[2].Replace('\'', ' ') + "','" +
                                         item[3].Replace('\'', ' ') + "','" +
                                         item[4].Replace('\'', ' ') + "','" +
                                         item[5].Replace('\'', ' ') + "','" +
                                         item[6].Replace('\'', ' ') + "','" +
                                         item[7].Replace('\'', ' ') + "'," +
                                         item[8].Replace(',', '.') + "," +
                                         item[9].Replace(',', '.') + "," +
                                         item[10] + ",'" + file.Split('\\').Last() + "')";
                    sf.ConnectMS(sql);
                 
                }
                sf.ConnectMS("UPDATE [pharmacyterminal].[dbo].[ishopinfo] " +
                    "SET fileupdate=0 where ishop='" + file.Split('\\').Last() + "'");
              
            }
                threadcount--;
                if (threadcount == 0)
                {
                    if (!File.Exists(@".\app.log")) { using (var fs = File.Create(@".\app.log")) { } 
                 }
               using (StreamWriter log = new StreamWriter(@".\app.log", true, Encoding.Default))
              {
                  log.WriteLine("#");
                  log.WriteLine(DateTime.Now.ToString() + " | Программа запущена");
                  log.WriteLine("#");
                  if (sf.error == "")
                  {
                      log.WriteLine("Ошибок нет");
                  }
                  log.WriteLine(sf.error);
                  log.WriteLine("#");
                  log.WriteLine(DateTime.Now.ToString() + " | Программа завершена");
                  log.WriteLine("#");
                  log.WriteLine("#");
              }
                   
                }
        }
        

    }

}
