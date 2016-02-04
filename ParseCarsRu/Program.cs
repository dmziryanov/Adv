using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Helpers;
using System.Web.Script.Serialization;
using DAL;
using HtmlAgilityPack;

namespace ParseCarsRu
{
    class Program
    {


        static void Main(string[] args)
        {
            CategoryRepository repository = new CategoryRepository();
            var _advRepository = new AdvRepository();
            var httpClient = new WebClient();
            int index = 0;

            foreach (var var in repository.GetBrands().ToList())
            {
                if (var.Name[0] < 'F') continue;
                foreach (var carvar in repository.GetModels(var.Id).ToList())
                {
                    HtmlDocument carhtml = new HtmlDocument();
                    HtmlDocument shorthtml = new HtmlDocument();

                    try
                    {
                        carhtml.LoadHtml(System.Text.Encoding.Default.GetString(httpClient.DownloadData(System.Net.WebUtility.HtmlEncode("http://www.cars.ru/find/marka/" + var.Name + "/" + carvar.Name))));
                    }
                    catch (Exception)
                    {
                        continue;
                    }

                    
                        var pages =
                            carhtml.DocumentNode.Descendants("div")
                                .Where(
                                    d =>
                                        d.Attributes.Contains("class") &&
                                        d.Attributes["class"].Value.Contains("b-pagination"))
                                .ToList();
                    


                    bool NextPageExists = true;
                    string pageref = "";
                    if (pages.Count > 1)
                    {
                        pageref = Program.pageref(pages[1].ChildNodes[1].ChildNodes[1]);
                        carhtml.LoadHtml(System.Text.Encoding.UTF8.GetString(httpClient.DownloadData(("http://www.cars.ru" + pageref.Replace("amp;", "")))));

                        pages = carhtml.DocumentNode.Descendants("div").Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Contains("b-pagination")).ToList();
                        pageref = Program.pageref(pages[1].ChildNodes[1].ChildNodes[(2) * 2 - 1]).Replace("amp;", "");
                        
                    }

                    HtmlDocument concretecarhtml = new HtmlDocument();

                    var currentPage = 0;

                    
                    while (NextPageExists)
                    {
                        currentPage++;
                        
                        if (currentPage > 1)
                        {
                            try
                            {
                                carhtml.LoadHtml(
                                    System.Text.Encoding.UTF8.GetString(
                                        httpClient.DownloadData(("http://www.cars.ru" + pageref))));
                                pages =
                                    carhtml.DocumentNode.Descendants("div")
                                        .Where(
                                            d =>
                                                d.Attributes.Contains("class") &&
                                                d.Attributes["class"].Value.Contains("b-pagination"))
                                        .ToList();
                                pageref =
                                    Program.returnNOde(pages[1].ChildNodes[1].ChildNodes, (currentPage + 1).ToString()).Replace("amp;", "");


                                ;
                            }
                            catch (Exception)
                            {
                               break;
                            }



                        }

                        var carblock = carhtml.DocumentNode.Descendants().Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Contains("advert-short-info")).ToList();
                        

                         foreach (var VAR in carblock)
                         {
                             CarAdvModel model = new CarAdvModel();
                             try
                             {
                                 concretecarhtml.LoadHtml(
                                     System.Text.Encoding.Default.GetString(
                                         httpClient.DownloadData(
                                             System.Net.WebUtility.HtmlEncode("http://www.cars.ru/" +
                                                                              VAR.ChildNodes[1].ChildNodes[1].Attributes[0].Value))));
                                 DateTime adsDate;
                                 DateTime.TryParse(VAR.ChildNodes[3].InnerText.Split(' ')[1], out adsDate);
                                 var sellerregion = VAR.ChildNodes[3].InnerText.Split(' ')[0];
                                 var carblock2 =
                                     concretecarhtml.DocumentNode.Descendants()
                                         .Where(
                                             d =>
                                                 d.Attributes.Contains("class") &&
                                                 d.Attributes["class"].Value.Contains("b-advert-auto-show"))
                                         .FirstOrDefault();
                                 var carblock3 =
                                     concretecarhtml.DocumentNode.Descendants()
                                         .Where(
                                             d =>
                                                 d.Attributes.Contains("class") &&
                                                 d.Attributes["class"].Value.Contains("auto-info"))
                                         .FirstOrDefault();
                                 var carblock4 =
                                     concretecarhtml.DocumentNode.Descendants()
                                         .Where(
                                             d =>
                                                 d.Attributes.Contains("class") &&
                                                 d.Attributes["class"].Value.Contains("auto-details auto-seller"))
                                         .FirstOrDefault();
                                 var complekt =
                                     concretecarhtml.DocumentNode.Descendants()
                                         .Where(
                                             d =>
                                                 d.Attributes.Contains("class") &&
                                                 d.Attributes["class"].Value.Contains(
                                                     "auto-show-column-left auto-options"))
                                         .FirstOrDefault();
                                 var info =
                                     concretecarhtml.DocumentNode.Descendants()
                                         .Where(
                                             d =>
                                                 d.Attributes.Contains("class") &&
                                                 d.Attributes["class"].Value.Contains("seller-warning"))
                                         .FirstOrDefault();



                                 var title = carblock2.ChildNodes[1].InnerText;


                                 var data = _advRepository.Get(0);
                                 model.Name = title;
                                 model.Category = 49;
                                 model.Currency = "руб";
                                 model.SellerId = 0;
                                 var mileAge = 0;
                                 var sMileAge =
                                     WebUtility.HtmlDecode(carblock3.ChildNodes[3].ChildNodes[3].ChildNodes[5].InnerText)
                                         .Replace("[\\x00-\\x1F]", "")
                                         .Split(' ');
                                 int.TryParse(Regex.Replace(sMileAge[1], "[^0-9]", ""), out mileAge);

                                 var sYear =
                                     WebUtility.HtmlDecode(carblock3.ChildNodes[3].ChildNodes[3].ChildNodes[3].InnerText)
                                         .Replace("[\\x00-\\x1F]", "")
                                         .Split(' ');
                                 int.TryParse(Regex.Replace(sMileAge[1], "[^0-9]", ""), out mileAge);
                                 model.MileAge = mileAge;
                                 model.Year = int.Parse(sYear[2]);
                                 model.customs = 1;
                                 model.PTS = "1";

                                 var sPrice = Regex.Replace(carblock3.ChildNodes[1].InnerText, "[^0-9]", "");
                                 double price = 0;
                                 double.TryParse(sPrice, out price);
                                 model.Price = price;
                                 //model.Price = int.Parse(contact.ChildNodes[3].InnerHtml.Trim().Replace(" ", "").Replace("рублей", ""));
                                 //var seller = contact.ChildNodes[13].Descendants("li").ToList();
                                 var sellername = carblock4.ChildNodes[3].ChildNodes[1].InnerText;
                                 model.SellerName = sellername;
                                 if (sellername.Contains("Авто"))
                                 {
                                     model.type = AdvType.Business;
                                 }
                                 else
                                     model.type = AdvType.Private;

                                 model.SellerPhone = carblock4.ChildNodes[3].ChildNodes[3].InnerText.Trim();
                                 //model.SellerPhone = Regex.Replace(model.SellerPhone, "[^0-9]", "");
                                 
                                 
/*                                 shorthtml.LoadHtml(
                                     System.Text.Encoding.Default.GetString(
                                         httpClient.DownloadData("http://htmlweb.ru/geo/api.php?html&telcod=" +
                                                                 model.SellerPhone +
                                                                 "&api_key=2a38aa8b8b9e978205c7c77b6d885101")));

                                 var sellerregion = shorthtml.DocumentNode.ChildNodes[0].ChildNodes[1].InnerText;*/

                                 model.Description = complekt.InnerHtml;


                                 model.SellerDistrict = sellerregion;
                                 var ShortName = sellerregion.Length > 6 ? sellerregion.Substring(6) : sellerregion;
                                 model.Created = adsDate;
                                 if (data._locations.Where(x => x.Name.Contains(ShortName)).FirstOrDefault() != null)
                                 {
                                     model.Location =
                                         data._locations.Where(x => x.Name.Contains(ShortName))
                                             .FirstOrDefault()
                                             .CityId;
                                     model.Country =
                                         data._locations.Where(x => x.Name.Contains(ShortName))
                                             .FirstOrDefault()
                                             .countryId;
                                 }
                                 else
                                 {
                                     model.Location = 22;
                                 }

                                 var imgs =
                                     concretecarhtml.DocumentNode.Descendants("ul")
                                         .Where(
                                             d =>
                                                 d.Attributes.Contains("class") &&
                                                 d.Attributes["class"].Value.Contains("ad-thumb-list"))
                                         .FirstOrDefault();
                                 string i;
                                 byte[] file;
                                 foreach (var img in imgs.ChildNodes)
                                 {

                                     if (img.InnerHtml.Contains("img"))
                                     {
                                         i = img.ChildNodes[1].ChildNodes[1].Attributes[0].Value.Replace("i100x75",
                                             "i600x450");
                                         try
                                         {
                                             file = httpClient.DownloadData(i);
                                         

                                         index++;
                                         model.Imgs.Add(new ImageFile()
                                         {
                                             Created = DateTime.Now,
                                             FileBody = file,
                                             FileName = "import" + index
                                         });
                                         }
                                         catch (Exception ex)
                                         {

                                         }
                                     }
                                 }

                                 _advRepository.SaveCar(model);
                                 Console.ForegroundColor = ConsoleColor.Gray;
                                 Console.Write(var.Name + "/" + carvar.Name + "/");
                                 Console.ForegroundColor = ConsoleColor.DarkCyan;
                                 Console.Write(model.Name);
                                 Console.ForegroundColor = ConsoleColor.Green;
                                 Console.WriteLine(" Ok");

                             }
                             catch (Exception)
                             {
                                
                                 Console.ForegroundColor = ConsoleColor.Gray;
                                 Console.Write(var.Name + "/" + carvar.Name);
                                 Console.ForegroundColor = ConsoleColor.DarkCyan;
                                 Console.Write(model.Name);
                                 Console.ForegroundColor = ConsoleColor.Red;
                                 Console.WriteLine(" Error");

                             }
                         }
                    }
                }
            }
        }

        private static string pageref(HtmlNode page)
        {
            string pageref;
            if (page.ChildNodes.Count > 1)
                pageref = page.ChildNodes[1].Attributes[0].Value;
            else
                pageref = page.ChildNodes[0].Attributes[0].Value;
            return pageref;
        }

        private static string returnNOde(HtmlNodeCollection childNodes, string продавец)
        {
            foreach (var VAR in childNodes)
            {
                if (VAR.InnerText.ToUpper().Contains(продавец.ToUpper()))
                    return VAR.ChildNodes[1].Attributes[0].Value;
                else
                    returnNOde(VAR.ChildNodes, продавец);
            }
            return null;
        }
    }

}
