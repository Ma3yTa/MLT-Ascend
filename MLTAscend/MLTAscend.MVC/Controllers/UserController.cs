﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MLTAscend.MVC.Models;
using MLTAscend.MVC.ViewModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using dom = MLTAscend.Domain.Models;
using System.Net.Http.Formatting;

namespace MLTAscend.MVC.Controllers
{
   public class UserController : Controller
   {
      private static readonly HttpClient HttpClient = InitClient();

      private static HttpClient InitClient()
      {
         return new HttpClient();
      }

      // add route
      public IActionResult LogIn(InUser signIn)
      {
         if (ModelState.IsValid)
         {
            var uvm = new UserViewModel();
            var user = uvm.GetUsers().FirstOrDefault(u => u.Username == signIn.Username);

            if (user != null && signIn.Password == user.Password)
            {
               HttpContext.Session.SetString("user", JsonConvert.SerializeObject(user));

               return RedirectToAction("index", "Home");
            }
         }
         return View("../Home/_LogIn");
      }

      // add route
      public IActionResult SignUp(UpUser signUp)
      {
         if (ModelState.IsValid)
         {
            var uvm = new UserViewModel();
            var users = uvm.GetUsers();

            if (!users.Exists(u => u.Username == signUp.Username) && signUp.Password == signUp.Confirm)
            {
               uvm.SignUp(signUp.Name, signUp.Username, signUp.Password);

               var user = uvm.GetUsers().FirstOrDefault(u => u.Username == signUp.Username);
               HttpContext.Session.SetString("user", JsonConvert.SerializeObject(user));

               return RedirectToAction("index", "Home");
            }
         }
         return View("../Home/_SignUp");
      }

      public IActionResult LogOut()
      {
         HttpContext.Session.Clear();

         return View("../Home/Index");
      }

      [Route("[controller]/History/{sort?}")]
      public IActionResult History(string sort)
      {
         var uvm = new UserViewModel();

         dom.User user;
         Log log;

         var _user = HttpContext.Session.GetString("user");
         if (_user != null)
         {
            user = JsonConvert.DeserializeObject<dom.User>(_user);

            log = new Log()
            {
               Predictions = uvm.GetPredictionsByUser(user.Username)
            };
         }         
         else
         {
            log = new Log()
            {
               Predictions = uvm.GetPredictionsByUser("anonymous")
            };
         }

         if (TempData["inverse"] == null)
         {
            TempData["inverse"] = false;
         }

         var inverse = (bool)TempData["inverse"];

         switch (sort)
         {
            case "CreationDate":
               if (inverse)
               {
                  log.Predictions.Sort((x, y) => ((DateTime)y.GetType().GetProperty(sort).GetValue(y)).CompareTo((DateTime)x.GetType().GetProperty(sort).GetValue(x)));
               }
               else
               {
                  log.Predictions.Sort((y, x) => ((DateTime)y.GetType().GetProperty(sort).GetValue(y)).CompareTo((DateTime)x.GetType().GetProperty(sort).GetValue(x)));
               }
               TempData["inverse"] = !inverse;
               break;
            case "CompanyName":
            case "Ticker":
               if (inverse)
               {
                  log.Predictions.Sort((x, y) => ((string)y.GetType().GetProperty(sort).GetValue(y)).CompareTo((string)x.GetType().GetProperty(sort).GetValue(x)));
               }
               else
               {
                  log.Predictions.Sort((y, x) => ((string)y.GetType().GetProperty(sort).GetValue(y)).CompareTo((string)x.GetType().GetProperty(sort).GetValue(x)));
               }
               TempData["inverse"] = !inverse;
               break;
            case "OneDayPred":
            case "OneWeekPred":
            case "OneMonthPred":
            case "ThreeMonthPred":
            case "OneYearPred":
               if (inverse)
               {
                  log.Predictions.Sort((x, y) => ((double)y.GetType().GetProperty(sort).GetValue(y)).CompareTo((double)x.GetType().GetProperty(sort).GetValue(x)));
               }
               else
               {
                  log.Predictions.Sort((y, x) => ((double)y.GetType().GetProperty(sort).GetValue(y)).CompareTo((double)x.GetType().GetProperty(sort).GetValue(x)));
               }
               TempData["inverse"] = !inverse;
               break;
            default:
               log.Predictions.Sort((x, y) => y.CreationDate.CompareTo(x.CreationDate));
               TempData["inverse"] = !inverse;
               break;
         }

         return View("../User/History", log);
      }

      public async Task<IActionResult> Predict(Ticker ticker)
      {
         var uvm = new UserViewModel();
         var _data = await GetTickerData(ticker);
         ViewBag.tickerData = _data;

         var _dayData = _data.LastOrDefault();
         _dayData.CompanyName = await GetCompanyName(ticker);
         _dayData.Ticker = ticker.Symbol;
         ViewBag.tickerDay = _dayData;

         var _user = HttpContext.Session.GetString("user");
         if (_user != null)
         {
            var user = JsonConvert.DeserializeObject<dom.User>(_user);
            ViewBag.Prediction = uvm.CreateStockData(_dayData, user.Username);
         }
         else
         {
            ViewBag.Prediction = uvm.CreateStockData(_dayData, "anonymous");
         }
         var news = await GetTickerNews(ticker);
         ViewBag.news = news;

         return View("../User/Predict");
      }

      public async Task<IEnumerable<Symbol>> GetTickerData(Ticker ticker)
      {
         try
         {
            var url = $"https://api.iextrading.com/1.0/stock/{ticker.Symbol}/chart/1y?filter=date,high,low,open,close,volume";

            HttpResponseMessage response = await HttpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var responseBody = await response.Content.ReadAsStringAsync();

            var tickerData = JsonConvert.DeserializeObject<IEnumerable<Symbol>>(responseBody);

            return tickerData;
         }
         catch (HttpRequestException hre)
         {
            throw new HttpRequestException("Could not retrieve ticker", hre);
         }
      }

      public async Task<string> GetCompanyName(Ticker ticker)
      {
         try
         {
            var url = $"https://api.iextrading.com/1.0/stock/{ticker.Symbol}/company?filter=companyName";

            HttpResponseMessage response = await HttpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var responseBody = await response.Content.ReadAsStringAsync();

            var companyName = JsonConvert.DeserializeAnonymousType(responseBody, new { companyName = "" }).companyName;

            return companyName;
         }
         catch (HttpRequestException hre)
         {
            throw new HttpRequestException("Could not retrieve company name", hre);
         }
      }

      public async Task<IEnumerable<News>> GetTickerNews(Ticker ticker)
      {
         try
         {
            var url = $"https://api.iextrading.com/1.0/stock/{ticker.Symbol}/news/last/5";

            HttpResponseMessage response = await HttpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var responseBody = await response.Content.ReadAsStringAsync();

            var newsData = JsonConvert.DeserializeObject<IEnumerable<News>>(responseBody);

            return newsData;
         }
         catch (HttpRequestException hre)
         {
            throw new HttpRequestException("Could not retrieve company news", hre);
         }
      }
   }
}