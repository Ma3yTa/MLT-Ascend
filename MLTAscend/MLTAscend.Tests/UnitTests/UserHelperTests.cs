﻿using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using dom = MLTAscend.Domain.Models;
using dat = MLTAscend.Data.Helpers;

namespace MLTAscend.Tests.UnitTests
{
    public class UserHelperTests
    {
        private dom.User sut { get; set; }
        private dom.User ExistUser;
        private dom.Prediction pr;
        public dat.UserHelper UserHelper { get; set; }
        public dat.PredictionHelper ph { get; set; }

        public UserHelperTests()
        {
            ExistUser = new dom.User()
            {
                Name = "fred",
                Username = "belottef",
                Password = "peoples"
            };

            sut = new dom.User()
            {
                Name = "john",
                Username = "jacob",
                Password = "jingle"
            };

            pr = new dom.Prediction()
            {
                Ticker = "ryry"
            };

            UserHelper = new dat.UserHelper(new Data.InMemoryDbContext());
            ph = new dat.PredictionHelper(new Data.InMemoryDbContext());

            UserHelper.SetUser(ExistUser);
            ph.SetPrediction(pr, ExistUser.Username);
        }

        [Fact]
        public void Test_SetUser_Fail()
        {
            Assert.False(UserHelper.SetUser(ExistUser));
        }

        [Fact]
        public void Test_SetUser_Succeed()
        {
            Assert.True(UserHelper.SetUser(sut));
        }

        [Fact]
        public void Test_GetUserByUsername()
        {
            var actual = UserHelper.GetUserByUsername(ExistUser.Username);

            Assert.True(actual.Username == ExistUser.Username);
        }

        [Fact]
        public void Test_GetUserPredictions()
        {
            var actual = UserHelper.GetUserPredictions(ExistUser.Username);

            Assert.True(actual.Count > 0);
            Assert.True(actual[0].Ticker == "ryry");
        }

        [Fact]
        public void Test_GetUsers()
        {
            var actual = UserHelper.GetUsers();

            Assert.True(actual.Count > 0);
            Assert.True(actual[0].Username == ExistUser.Username);
        }
    }
}
