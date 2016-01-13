using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Configuration;
using System.Net.Mail;
using System.Text;
using DAL;
using HtmlAgilityPack;

namespace Download
{
    class Program
    {
        static void Main(string[] args)
        {

            var lines = new List<String>();
            var httpClient = new WebClient();

            HtmlDocument html = new HtmlDocument();
            html.LoadHtml(
                System.Text.Encoding.UTF8.GetString(
                    httpClient.DownloadData("http://usedcars.ru/catalog/passenger/AC/CL")));
                                            
            var carblock = html.DocumentNode.Descendants("div").Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Contains("block cars")).FirstOrDefault();
            if (returnNOde(carblock.ChildNodes, "Последние объявления") != null) Console.WriteLine("");
            

            HtmlDocument brandhtml = new HtmlDocument();
            brandhtml.LoadHtml(System.Text.Encoding.UTF8.GetString(httpClient.DownloadData("http://usedcars.ru/catalog/passenger/")));
            var brands = brandhtml.DocumentNode.Descendants("div").Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Contains("brands")).FirstOrDefault();
            var refs = brands.Descendants("a");

            Dictionary<string, List<string>> bnames = new Dictionary<string, List<string>>();

            foreach (var var in refs)
            {
                if (var.Attributes.Contains("href"))
                {
                    var l = new List<string>();
                    bnames.Add(var.InnerText, l);
                    HtmlDocument carhtml = new HtmlDocument();
                    carhtml.LoadHtml(System.Text.Encoding.UTF8.GetString(httpClient.DownloadData("http://usedcars.ru" + var.Attributes[0].Value)));
                    var cars = carhtml.DocumentNode.Descendants("div")
                            .Where(
                                d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Contains("brands m"))
                            .FirstOrDefault();
                    var carrefs = cars.Descendants("a");
                    foreach (var carvar in carrefs)
                    {
                        if (carvar.Attributes.Contains("href"))
                            l.Add(carvar.InnerText);
                    }
                }
            }

            
            foreach (var brand in bnames.Keys)
            {

                foreach (var modelcar in bnames[brand])
                {
                    try
                    {

                        html = new HtmlDocument();
                        html.LoadHtml(
                            System.Text.Encoding.UTF8.GetString(
                                httpClient.DownloadData("http://usedcars.ru/catalog/passenger/" + brand + "/" + modelcar +
                                                        "/")));
                        carblock = html.DocumentNode.Descendants("div").Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Contains("block cars")).FirstOrDefault();
                        
                        if (returnNOde(carblock.ChildNodes, "Последние объявления") != null) continue;


                        var findclasses = html.DocumentNode.Descendants("div").Where(d =>
                            d.Attributes.Contains("class") && d.Attributes["class"].Value.Contains("cars-list")
                            );

                        var findcars = findclasses.First().Descendants("div").Where(d =>
                            d.Attributes.Contains("class") && d.Attributes["class"].Value.Contains("car")
                            );

                        var _advRepository = new AdvRepository();

                        var index = 0;
                        foreach (var car in findcars)
                        {
                            
                            var r = car.ChildNodes[1].Attributes[1].Value;
                            var carhtml = new HtmlDocument();

                            carhtml.LoadHtml(
                                System.Text.Encoding.UTF8.GetString(httpClient.DownloadData("http://usedcars.ru/" + r)));
                            var imgs =
                                carhtml.DocumentNode.Descendants("div")
                                    .Where(
                                        d =>
                                            d.Attributes.Contains("class") &&
                                            d.Attributes["class"].Value.Contains("img-previews"))
                                    .FirstOrDefault();
                            var info =
                                carhtml.DocumentNode.Descendants("div")
                                    .Where(
                                        d =>
                                            d.Attributes.Contains("class") &&
                                            d.Attributes["class"].Value.Contains("about"))
                                    .FirstOrDefault();
                            var allinfohtml =
                                carhtml.DocumentNode.Descendants("div")
                                    .Where(
                                        d =>
                                            d.Attributes.Contains("class") &&
                                            d.Attributes["class"].Value.Contains("about"))
                                    .FirstOrDefault();

                            allinfohtml.ChildNodes[5].Remove();

                            var contact =
                                carhtml.DocumentNode.Descendants("div")
                                    .Where(
                                        d =>
                                            d.Attributes.Contains("class") &&
                                            d.Attributes["class"].Value.Contains("complekt"))
                                    .FirstOrDefault();

                            CarAdvModel model = new CarAdvModel();
                            var data = _advRepository.Get(0);
                            model.Category = 49;
                            model.Currency = "руб";
                            model.SellerId = 0;
                            model.Price =
                                int.Parse(contact.ChildNodes[3].InnerHtml.Trim().Replace(" ", "").Replace("рублей", ""));
                            var seller = contact.ChildNodes[13].Descendants("li").ToList();
                            var sellername = "";
                            var sellerregion = "";
                            var sellerphone = "";
                            if (seller.Count > 3)
                            {
                                var s = returnNOde(contact.ChildNodes, "продавец");

                                if (s == null)
                                {
                                    model.Location = 22;
                                    model.type = AdvType.Business;
                                    sellername =
                                        returnNOde(contact.ChildNodes, "Регион");

                                }
                                else
                                {
                                    sellername =
                                        s.Split(new string[] {"\r\n", "\n"}, StringSplitOptions.None)[1].Replace(
                                            "Продавец: ", "");
                                    sellerregion =
                                        returnNOde(contact.ChildNodes, "Регион")
                                            .Split(new string[] {"\r\n", "\n"}, StringSplitOptions.None)[3].Replace(
                                                "Регион: ", "")
                                            .Trim();
                                    sellerphone =
                                        returnNOde(contact.ChildNodes, "Телефон")
                                            .Split(new string[] {"\r\n", "\n"}, StringSplitOptions.None)[4].Replace(
                                                "Телефон: ", "")
                                            .Trim();
                                }
                            }
                            else
                            {
                                sellername = seller[0].InnerText;
                                sellerregion = seller[1].InnerText.Replace("Регион:", "").Trim();
                                sellerphone = seller[2].InnerText.Replace("Телефон:", "").Trim();
                            }

                            model.Name = "New";
                            model.Description = "New";
                            model.SellerName = sellername;
                            model.SellerPhone = sellerphone;
                            model.Location =
                                data._locations.Where(x => x.Name.Contains(sellerregion)).FirstOrDefault().CityId;
                            model.Country =
                                data._locations.Where(x => x.Name.Contains(sellerregion)).FirstOrDefault().countryId;
                            model.Name = info.ChildNodes[1].InnerText.Replace("Информация о", "");


                            var table = info.ChildNodes[3];
                            int mileage;

                            if (table.ChildNodes[3].ChildNodes[1].InnerText.Contains("Б/У"))
                                model.condition = AdvCondition.Used;
                            else model.condition = AdvCondition.New;
                            model.Description = allinfohtml.InnerText;

                            int year;
                            if (int.TryParse(table.ChildNodes[4].ChildNodes[1].InnerText, out year))
                                model.Year = year;
                            else model.Year = year;
                            model.PTS = "1";

                            if (int.TryParse(table.ChildNodes[6].ChildNodes[1].InnerText.Split(' ')[0], out mileage))
                                model.MileAge = mileage;

                            string i = "";

                            if (imgs != null)
                                foreach (var img in imgs.ChildNodes)
                                {
                                    if (img.InnerHtml.Contains("img"))
                                    {
                                        i = img.ChildNodes[0].Attributes[0].Value.Replace("min_", "");
                                        var file = httpClient.DownloadData(i);

                                        index++;
                                        model.Imgs.Add(new ImageFile()
                                        {
                                            Created = DateTime.Now,
                                            FileBody = file,
                                            FileName = "import" + index
                                        });
                                    }
                                }

                            _advRepository.SaveCar(model);
                        }

                    }
                        /*   while (content.IndexOf("<cars-list>") > 0)
               {
                           content = content.Substring(content.IndexOf("<article>") + 11,content.Length - content.IndexOf("<article>") - 11);
                           j = content.Substring(content.IndexOf("shops/"),content.IndexOf('"' + ">") - content.IndexOf("shops/"));
                           var content2 = httpClient.DownloadString("http://parts.dmir.ru/msk/" + j);var email = content2.Substring(content2.IndexOf("data-isCompany="),
                               content2.IndexOf("</a>", content2.IndexOf("data-isCompany=")) -content2.IndexOf("data-isCompany="));
                           email = email.Substring(22, email.Length - 22);
                           lines.Add(email);
               }*/
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }

        private static string returnNOde(HtmlNodeCollection childNodes, string продавец)
        {
            foreach (var VAR in childNodes)
            {
                if (VAR.InnerText.ToUpper().Contains(продавец.ToUpper()))
                    return VAR.InnerText;
                else
                    returnNOde(VAR.ChildNodes, продавец);
            }
            return null;
        }
    }
}
