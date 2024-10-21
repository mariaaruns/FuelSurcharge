using System;
using HtmlAgilityPack;
using System.Collections.Generic;
using PuppeteerSharp;
using System.IO;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.IO.Compression;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using Microsoft.Data.SqlClient;
using System.Data;
using PuppeteerSharp.Cdp;
using System.Configuration;
using Microsoft.Extensions.Configuration;
using Get_Weekly_fuelSurcharge;



public class Program
{
    static async Task Main(string[] args)
    {
        string conn = ConfigurationSettings.AppSettings["conn"].ToString();
        var todayFriday = DateTime.Now.DayOfWeek;
        var NextMonday = DateTime.Now.AddDays(3).Date.ToString("MM/dd/yyyy");
        bool IsThisWeekDataInserted =await IsNextModayDataExist(NextMonday,conn);

        while (!IsThisWeekDataInserted) {

            if (todayFriday is DayOfWeek.Sunday)
            {

                var htmls = "https://www.ups.com/us/en/support/shipping-support/shipping-costs-rates/fuel-surcharges.page";
                var options = new ChromeOptions();
                options.AddArgument("headless"); // Run Chrome in headless mode
                options.AddArgument("remote-allow-origins=*");
                options.AddArgument("--disable-blink-features=AutomationControlled");
                options.AddArgument("--user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.3");

                var driver = new ChromeDriver(options);

                driver.Navigate().GoToUrl(htmls);
                Console.WriteLine(driver.Title);

           //     var pageSource = driver.PageSource;
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                var element = wait.Until(ExpectedConditions.ElementExists(By.XPath("//div[@id='RevenueSurchargeHistory']")));


                var firstRow = driver.FindElement(By.XPath("//div[@id='RevenueSurchargeHistory']/table/tbody/tr[1]"));

                if (firstRow != null)
                {
                   // var FirstRowNodes = firstRow.GetAttribute("outerHTML");

                    var cells = firstRow.FindElements(By.TagName("td"));
                    
                    var fuelSurcharge = new FuelSurcharge
                    {
                        EffectiveStartDate = cells[0].Text,
                        DomesticAirSurcharge = cells[1].Text,
                        DomesticGroundSurcharge = cells[2].Text,
                        InternationalAirExportSurcharge = cells[3].Text,
                        InternationalAirImportSurcharge = cells[4].Text,
                        InternationalGroundExportImportSurcharge = cells[5].Text
                    };

                    if (fuelSurcharge is FuelSurcharge) 
                    {
                        IsThisWeekDataInserted = await InsertMondayFuelSurcharge(fuelSurcharge, conn);
                    }

                   /* foreach (var cell in cells)
                    {

                        Console.WriteLine(cell.Text);
                    }*/
                }
                else
                {
                    Console.WriteLine("No data found in the first row.");
                }

                driver.Quit();

                await Task.Delay(60000);
            }
            else 
            {
                IsThisWeekDataInserted = true;    
            }
        
        }
       // await FetchHtmlWithPuppeteerAsync();
       
    }

    public static async Task<bool> IsNextModayDataExist(string NextmondayDate,string connectionString)
    {
        var IsNextModayDataExist = false;
        try
        {
            using (SqlConnection conn = new SqlConnection(connectionString)) {
                await conn.OpenAsync();
                using (SqlCommand cmd = conn.CreateCommand()) {

                    cmd.CommandText = @"select top(1) EffectiveStartDate from FuelSurcharge order by Convert(Date,EffectiveStartDate)";
                    cmd.CommandTimeout = 120;
                    object obj = await cmd.ExecuteScalarAsync();
                    if (obj is not null && obj != DBNull.Value)
                    {
                        IsNextModayDataExist = NextmondayDate.Equals(obj.ToString(), StringComparison.OrdinalIgnoreCase);

                    }
                }
            }
        }
        catch(Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        
        return IsNextModayDataExist;
    }
    public static async Task<bool> InsertMondayFuelSurcharge( FuelSurcharge fuelSurcharge,string connectionString)
    {
        var IsdataInserted = false;
        try
        {
            using (SqlConnection conn = new SqlConnection(connectionString)) {
                await conn.OpenAsync();

                using (SqlCommand cmd = conn.CreateCommand()) 
                {   cmd.CommandText = @"Insert into FuelSurcharge values(@Effectivedate,@DomGrndSurcharge,@DomAirSurcharge,@IntAirExportSurcharge,@IntAirImportSurcharge,@IntGrndExpImpSurcharge)";
                    cmd.CommandTimeout = 120;
                    cmd.Parameters.AddWithValue("@Effectivedate", fuelSurcharge.EffectiveStartDate);
                    cmd.Parameters.AddWithValue("@DomGrndSurcharge", fuelSurcharge.DomesticGroundSurcharge);
                    cmd.Parameters.AddWithValue("@DomAirSurcharge", fuelSurcharge.DomesticAirSurcharge);
                    cmd.Parameters.AddWithValue("@IntAirExportSurcharge", fuelSurcharge.InternationalAirExportSurcharge);
                    cmd.Parameters.AddWithValue("@IntAirImportSurcharge", fuelSurcharge.InternationalAirImportSurcharge);
                    cmd.Parameters.AddWithValue("@IntGrndExpImpSurcharge", fuelSurcharge.InternationalGroundExportImportSurcharge);
                    int i = await cmd.ExecuteNonQueryAsync();
                    IsdataInserted = i > 0 ?  true: false;
                }

            }
        }
        catch(Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        return IsdataInserted;
    }
  
}
