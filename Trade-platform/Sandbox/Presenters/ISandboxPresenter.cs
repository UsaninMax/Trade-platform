﻿namespace TradePlatform.Sandbox.Presenters
{
    public interface ISandboxPresenter
    {
        void Execute();
        void StopExecution();
        bool IsActive();
    }
}
