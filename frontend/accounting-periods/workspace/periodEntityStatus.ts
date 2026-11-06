/**
 * Where an account or fund ended an accounting period relative to its goal. NotFullyFunded marks a fund that ended
 * within range without receiving its full contribution.
 */
enum PeriodEntityStatus {
  BelowMinimum = "BelowMinimum",
  AboveMaximum = "AboveMaximum",
  NotFullyFunded = "NotFullyFunded",
  WithinRange = "WithinRange",
  NoGoal = "NoGoal",
}

export default PeriodEntityStatus;
