﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MLTAscend.MVC.Models;
using MLTAscend.MVC.ViewModels;
using Newtonsoft.Json;
using dom = MLTAscend.Domain.Models;

namespace MLTAscend.MVC.Controllers
{
  public class UserController : Controller
  {
    // add route
    public IActionResult LogIn(InUser signIn)
    {
      if (ModelState.IsValid)
      {
        var uvm = new UserViewModel();
        var user = uvm.GetUsers().FirstOrDefault(u => u.Username == signIn.Username);

        if (user != null && signIn.Password == user.Password)
        {
          // add to session

          return View("LoggedIn");
        };
      }
      return RedirectToAction("SignUp", "Home");
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

          var user = uvm.GetUsers().Find(u => u.Username == signUp.Username);
          // add to session

          return View("LoggedIn");
        };
      }
      return RedirectToAction("SignUp", "Home");
    }
  }
}