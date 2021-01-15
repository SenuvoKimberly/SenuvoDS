using DirectScale.Disco.Extension.Hooks.Autoships;
using senuvo.Api;
using System;
using System.Collections.Generic;

namespace Hooks
{
    public interface IDaily
    {
        DailyRunHookResponse Invoke(DailyRunHookRequest request, Func<DailyRunHookRequest, DailyRunHookResponse> func);
        ResultDailyRunAfter DailyRunAfter();
        List<int> FinalizeNonAcceptedDistributors();
        List<int> FinalizeNonAcceptedOrder();
    }
}