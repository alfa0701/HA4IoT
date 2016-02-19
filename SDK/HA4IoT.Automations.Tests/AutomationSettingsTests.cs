﻿using System;
using System.Diagnostics;
using Windows.Data.Json;
using FluentAssertions;
using HA4IoT.Core.Settings;
using HA4IoT.Tests.Mockups;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace HA4IoT.Automations.Tests
{
    [TestClass]
    public class AutomationSettingsTests
    {
        [TestMethod]
        public void Create_Setting()
        {
            bool valueChangedFired = false;

            var setting = new Setting<int>(1);
            setting.ValueChanged += (s, e) => valueChangedFired = true;

            setting.Value.ShouldBeEquivalentTo(1);
            setting.IsValueSet.ShouldBeEquivalentTo(false);

            IJsonValue jsonValue = setting.ExportToJsonObject();
            jsonValue.ValueType.ShouldBeEquivalentTo(JsonValueType.Number);
            jsonValue.GetNumber().ShouldBeEquivalentTo(1.0M);

            setting.Value = 2;
            setting.Value.ShouldBeEquivalentTo(2);
            setting.IsValueSet.ShouldBeEquivalentTo(true);
            valueChangedFired.ShouldBeEquivalentTo(true);

            setting.DefaultValue.ShouldBeEquivalentTo(1);

            jsonValue = setting.ExportToJsonObject();
            jsonValue.ValueType.ShouldBeEquivalentTo(JsonValueType.Number);
            jsonValue.GetNumber().ShouldBeEquivalentTo(2.0M);

            jsonValue = JsonValue.CreateNumberValue(3);
            setting.ImportFromJsonValue(jsonValue);

            setting.Value.ShouldBeEquivalentTo(3);
        }
        
        [TestMethod]
        public void Serialize_AutomationSettings()
        {
            var settings = new RollerShutterAutomationSettings(AutomationIdFactory.EmptyId, new TestHttpRequestController(), new TestLogger());
            settings.MaxOutsideTemperatureForAutoClose.Value = 24.5F;
            settings.DoNotOpenBefore.Value = TimeSpan.Parse("07:30");

            JsonObject jsonObject = settings.ExportToJsonObject();
            Debug.WriteLine(jsonObject.Stringify());

            settings.MaxOutsideTemperatureForAutoClose.Value = 9F;
            settings.MinOutsideTemperatureForDoNotOpen.Value = 10F;
            settings.DoNotOpenBefore.Value = null;
            
            settings.ImportFromJsonObject(jsonObject);

            settings.DoNotOpenBefore.Value.ShouldBeEquivalentTo(TimeSpan.Parse("07:30"));
            settings.MinOutsideTemperatureForDoNotOpen.Value.ShouldBeEquivalentTo(null);
            settings.MaxOutsideTemperatureForAutoClose.Value.ShouldBeEquivalentTo(24.5F);
        }
    }
}