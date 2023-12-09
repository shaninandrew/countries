using countries.data;
using Microsoft.Extensions.Options;
using System.Net.Http;
using System.Text.Json;
using static System.Net.WebRequestMethods;


namespace countries.Models
{
    public class GetDatails
    {

        /// <summary>
        /// Публичный доступ к закачке
        /// </summary>
        public  CountryItem GetCountry{ get { return _Country; } }

        /// <summary>
        /// Скрытая инфа
        /// </summary>
        private CountryItem _Country;


        private string  data_url ="";

        /// <summary>
        /// Передайте имя страны
        /// </summary>
        /// <param name="name">Имя страны, а не столицы</param>
        public GetDatails(string name)
        {
            //https://restcountries.com/v3.1/name/aruba?fullText=true
            // ask
            data_url = $"https://restcountries.com/v3.1/name/{name}?fullText=true";
            Task <countries.data.CountryItem>  t= GetItem();
            t.Wait();
            _Country =t.Result;
        }

        public  async Task<CountryItem> GetItem()
        { 
            using (var httpClient = new HttpClient())
            {

                JsonSerializerOptions options = new JsonSerializerOptions();
                options.DefaultBufferSize = 4096;
                options.PropertyNameCaseInsensitive = false;


                List<CountryItem> coll = await httpClient.GetFromJsonAsync<List<countries.data.CountryItem>>(data_url);
                if (coll == null)
                {
                    return null;
                }
                
                return coll.First();

            }   //using

        }//GetDetails
    }
}
