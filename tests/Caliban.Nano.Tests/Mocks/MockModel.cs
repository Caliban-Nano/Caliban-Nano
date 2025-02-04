﻿using Caliban.Nano.Data;

namespace Caliban.Nano.Tests.Mocks
{
    internal sealed class MockModel : Model
    {
        public bool Value1
        {
            get => Get<bool>();
            set => Set(value);
        }

        public bool? Value2
        {
            get => Get<bool?>();
            set => Set(value, "Value2", "Value1");
        }

        public void SetHasChanged(bool value) => HasChanged = value;
    }
    internal sealed class MockModelRepository : Model.Repository
    {
        public void SetHasChanged(bool value) => HasChanged = value;
    }
}
