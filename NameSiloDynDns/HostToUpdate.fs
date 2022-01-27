namespace NameSiloDynDns

open System

[<CLIMutable>]
type HostToUpdate =
    { Host: string
      Domain: string
      Hours: int
      Minutes: int
      Seconds: int
      RetryAttempts: int }

    member this.UpdateTimeSpan =
        TimeSpan(this.Hours, this.Minutes, this.Seconds)

    member this.RetryTimeSpan =
        if this.RetryAttempts < 0 then
            TimeSpan.Zero
        else
            TimeSpan.FromTicks(
                this.UpdateTimeSpan.Ticks
                / int64 (this.RetryAttempts + 1)
            )
