using countries.data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;


namespace countries.Models
{
    public class CountyList
    {
        private Uri data_url = new Uri("https://restcountries.com/v3.1/all");

        private string html_list ;

        private bool availible = false;
        
        private List<MyItemCity> items ;

        public bool DataIsReady { get { return availible; } }


        public CountyList(string? url)
        {
            //Данные не доступны
            availible = false;

            //Очистка списка
            items = new List<MyItemCity>();
            items.Clear();  

            if (url != null)
            {
                data_url = new Uri(url);
            }

            //список стран
            CountryItem[] listOfCounties = null; 

            bool update=!false;

            //ускоритель - кэш
            string cache_filename = System.IO.Path.GetTempPath() + System.IO.Path.PathSeparator + "web-sample-country.json.cache";
            if (System.IO.File.Exists(cache_filename) )
            {
                     //Кэш есть сброс значения
                     update = false;
                    
                    DateTime last_update_date= System.IO.File.GetLastWriteTime(cache_filename);
                    TimeSpan interval =  new TimeSpan( DateTime.Now.Ticks - last_update_date.Ticks);
                    if (interval.Minutes > 4)
                    {
                        update = true;    
                    }

                     //если не нужно скачивать то считываем из файла
                    if (!update)
                    {
                         LoadFromCacheFile(cache_filename);
                        
                    }
            }

           

            if (update) //update
             using (var httpClient = new HttpClient())
             {
                    try
                    {
                        var async_reader = httpClient.GetStreamAsync(data_url);
                        async_reader.Wait();

                        //Когда все будет закачено, будет обработка списка
                        async_reader.ContinueWith(x =>
                        {
                            //Наполнение информацией кэша
                            try
                            {
                                FileStream StreamFile = new FileStream(cache_filename, FileMode.CreateNew, FileAccess.Write);
                                x.Result.CopyTo(StreamFile);
                                StreamFile.Close();

                            }
                            catch { }

                            /// Считываем из кэша
                            LoadFromCacheFile(cache_filename);

                        }

                     );
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
             
             }
        }

        public async Task< List<MyItemCity>>  GetList()
        {
            //Ждем данные
            while (!availible)
            {
                Task.Delay(200);
            }
            return  items;
        }


        public async void LoadFromCacheFile(string filename)
        {

            JsonSerializerOptions options = new JsonSerializerOptions();
            options.DefaultBufferSize = 4096;
            options.PropertyNameCaseInsensitive = false;

            //Открываем и загружаем данные из файла
            //FileStream fs = new FileStream (filename,FileMode.Open, FileAccess.Read);

            byte[] data = await System.IO.File.ReadAllBytesAsync(filename);

            JsonDocument jdoc=  JsonDocument.Parse(data);
            List<CountryItem> listOfCounties = JsonSerializer.Deserialize<CountryItem[]>(jdoc, options).ToList<CountryItem>();

            items.Clear();
            items.Capacity = listOfCounties.Count;

            //Вычитка данных
            foreach (CountryItem c in listOfCounties)
            {

                // capital это массив
                //
                string? capital = "без столицы";
                //Пытаемся найти имя столицы в списке
                if (c.capital != null) { capital = c.capital.First<string>(); }
                if (capital == null) { capital = c.capital.Last<string>(); }
                capital = capital == null ? "без столицы" : capital;

                MyItemCity item = new MyItemCity(c.name.official, capital);
                items.Add(item);
            }

            jdoc.Dispose();
            availible = true;

        }

    }

     /// <summary>
     /// Вспомогательный объект
     /// </summary>
    public class MyItemCity
    {
        /// <summary>
        /// СТрана
        /// </summary>
        public string countryName { get; set; }

        /// <summary>
        /// Столица
        /// </summary>
        public  string Capital { get; set; }

        /// <summary>
        /// Столица
        /// </summary>
        public string Index { get; set; }

        public MyItemCity(string CN, string capital)
        { 
            Capital=capital;
            countryName = CN;

            byte[] data =  System.Text.Encoding.Default.GetBytes(capital + "/" + countryName);
           
            MD5 hash = MD5.Create();
            Index = System.Convert.ToBase64String(  hash.ComputeHash(data)).Replace("/","-"); //System.Text.Encoding.Default.GetString
            
            hash.Dispose();

        }


    }
}
