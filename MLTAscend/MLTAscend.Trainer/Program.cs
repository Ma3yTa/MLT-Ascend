﻿using Microsoft.ML;
using MLTAscend.Trainer.Trainers;
using System;

namespace MLTAscend.Trainer
{
   class Program
   {
      static void Main(string[] args)
      {
         Console.WriteLine("Hello World!");
         MLContext mlContext = new MLContext(seed: 1);

         PredictionModelHelper.TrainAndSaveModel(mlContext, "Trainer/daily_MSFT.csv");
      }
   }
}
