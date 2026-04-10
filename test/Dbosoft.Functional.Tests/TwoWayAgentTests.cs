using System;
using System.Threading.Tasks;
using Dbosoft.Functional;
using Xunit;

namespace Dbosoft.Functional.Tests;

public class TwoWayAgentTests
{
    [Fact]
    public async Task Tell_from_synchronous_continuation_does_not_deadlock()
    {
        // Use the sync (non-async) TwoWayAgent constructor.
        // In the sync handler, SetResult is called directly on the ActionBlock thread.
        // If RunContinuationsAsynchronously is not properly configured on the TCS,
        // a synchronous continuation calling Tell() will deadlock because:
        //   1. SetResult runs the continuation inline on the ActionBlock thread
        //   2. The continuation posts a new message and waits for its result
        //   3. The ActionBlock can't process the new message - it's single-threaded
        //      and the current handler hasn't returned yet
        var agent = Agent.Start<int, string, string>(
            0,
            (state, msg) => (state + 1, msg + "_reply"));

        var deadlockDetected = false;
        var result2 = (string?)null;

        // Attach a synchronous continuation that calls Tell on the same agent.
        // ExecuteSynchronously forces it to run on the thread that calls SetResult.
        var task = agent.Tell("msg1").ContinueWith(t =>
        {
            // This runs on the ActionBlock's thread if continuations are synchronous
            try
            {
                var innerTask = agent.Tell("msg2");
                // Use a short timeout - if this doesn't complete quickly, we're deadlocked
                #pragma warning disable xUnit1031 // blocking is intentional to reproduce the deadlock
                if (!innerTask.Wait(TimeSpan.FromSeconds(3)))
                #pragma warning restore xUnit1031
                    deadlockDetected = true;
                else
                    result2 = innerTask.Result;
            }
            catch (Exception)
            {
                deadlockDetected = true;
            }

            return t.Result;
        }, TaskContinuationOptions.ExecuteSynchronously);

        var completed = await Task.WhenAny(task, Task.Delay(TimeSpan.FromSeconds(5)));

        Assert.True(completed == task,
            "Outer Tell() did not complete within timeout - likely deadlocked");
        Assert.Equal("msg1_reply", task.Result);
        Assert.False(deadlockDetected,
            "Inner Tell() deadlocked - TaskCompletionSource is not configured with RunContinuationsAsynchronously");
        Assert.Equal("msg2_reply", result2);
    }
}
