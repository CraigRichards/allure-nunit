﻿using System;
using NUnit.Framework;

namespace NUnit.Allure.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
    public class AllureOwnerAttribute : NUnitAttribute
    {
        public AllureOwnerAttribute(string owner)
        {
            Owner = owner;
        }

        internal string Owner { get; }
    }
}