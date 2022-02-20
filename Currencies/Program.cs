using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;

namespace Currencies
{
    class Program
    {
        static void Main(string[] args)
        {
            WebClient _client = new WebClient();

            NumberFormatInfo customNumFormat = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
            customNumFormat.NumberGroupSeparator = "";
            customNumFormat.NumberDecimalSeparator = ".";

            //Consumo el primer servicio
            var json = new WebClient().DownloadString("https://api.mercadolibre.com/currencies/");

            //Elimino los archivos en caso de que existan
            File.Delete("C:\\salida.txt");
            File.Delete("C:\\currency_conversions.csv");
            dynamic dynJson = JsonConvert.DeserializeObject<dynamic>(json);
            File.AppendAllText("C:\\currency_conversions.csv", "currency_base,currency_quote,ratio,rate,inv_rate,creation_date,valid_until"+Environment.NewLine);

            foreach (var item in dynJson)
            {
                // Utilizo el campo id para realizar la segunda consulta
                try
                {
                    JObject _segundaLectura = JObject.Parse(_client.DownloadString(string.Concat("https://api.mercadolibre.com/currency_conversions/search?from=", item.id, "&to=USD")));
                    item.todolar = _segundaLectura;

                    //Voy alimentando el archivo .csv
                    File.AppendAllText("C:\\currency_conversions.csv", string.Concat(_segundaLectura["currency_base"].ToString(), ",", _segundaLectura["currency_quote"].ToString(), ",", string.Format(customNumFormat, "{0:N}", _segundaLectura["ratio"]), ",", string.Format(customNumFormat, "{0:N}", _segundaLectura["rate"]), ",", string.Format(customNumFormat, "{0:N}", _segundaLectura["inv_rate"]), ",", _segundaLectura["creation_date"].ToString(), ",", _segundaLectura["valid_until"].ToString())+Environment.NewLine);
                }
                catch
                {
                    item.todolar = "Conversion no disponible";
                }
            }
            File.WriteAllText("C:\\salida.txt", dynJson.ToString());
        }
    }
}
